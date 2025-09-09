<#
.SYNOPSIS
    Network Deployment for Coffee Table Gaming Systems
.DESCRIPTION
    Deploys the complete coffee table gaming system to multiple target machines
    across a domain network. Handles parallel deployment, progress tracking,
    and comprehensive error reporting.
.PARAMETER TestMode
    Run in test mode without making actual changes
.PARAMETER Tables
    Array of table hostnames or IP addresses to deploy to
.PARAMETER Credential
    PSCredential for domain authentication
.PARAMETER LogPath
    Custom path for log file
#>
param(
    [switch]$TestMode = $false,
    [string[]]$Tables = @("Table01", "Table02", "Table03", "Table04", "Table05", "Table06", "Table07", "Table08", "Table09", "Table10"),
    [System.Management.Automation.PSCredential]$Credential,
    [string]$LogPath = "C:\Temp\NetworkDeploy_Log_$(Get-Date -Format 'yyyyMMdd_HHmmss').txt"
)

$ErrorActionPreference = "Stop"
$global:DeploymentStatus = @{
    TotalTables = 0
    Successful = 0
    Failed = 0
    TableStatus = @{}
    StartTime = Get-Date
}

function Write-Log {
    param([string]$Message, [string]$Level = "INFO")
    $timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
    $logEntry = "[$timestamp] [$Level] $Message"
    Write-Host $logEntry
    Add-Content -Path $LogPath -Value $logEntry
}

function Test-Admin {
    $currentUser = [Security.Principal.WindowsIdentity]::GetCurrent()
    $adminRole = [Security.PowerShell.ScheduledTask.WindowsBuiltInRole]::Administrator
    return ([Security.PowerShell.ScheduledTask.WindowsPrincipal]$currentUser).IsInRole($adminRole)
}

function Test-TableConnectivity {
    param([string]$Table)
    
    try {
        Write-Log "Testing connectivity to $Table..."
        
        # Test basic network connectivity
        if (-not (Test-Connection -ComputerName $Table -Count 1 -Quiet -ErrorAction SilentlyContinue)) {
            throw "No network connectivity to $Table"
        }
        
        # Test administrative share access
        if (-not (Test-Path "\\$Table\C$" -ErrorAction SilentlyContinue)) {
            throw "Cannot access administrative share on $Table"
        }
        
        Write-Log "✓ Connectivity test passed for $Table"
        return $true
    }
    catch {
        Write-Log "Connectivity test failed for $Table`: $($_.Exception.Message)" "ERROR"
        return $false
    }
}

function Copy-DeploymentPackage {
    param([string]$Table)
    
    try {
        Write-Log "Copying deployment package to $Table..."
        
        $sourcePath = "\\MasterPC\CoffeeTableUpdates\*"
        $destinationPath = "\\$Table\C`$\Temp\CoffeeTableUpdates\"
        
        if (-not $TestMode) {
            # Create destination directory
            if (-not (Test-Path $destinationPath)) {
                New-Item -Path $destinationPath -ItemType Directory -Force
            }
            
            # Use Robocopy for reliable file copy
            $robocopyArgs = @(
                $sourcePath.Replace('\*', ''),
                $destinationPath,
                "/MIR", "/NP", "/R:3", "/W:5", "/LOG+:C:\Temp\DeploymentCopy_$Table.log", "/TEE"
            )
            
            $process = Start-Process "robocopy.exe" -ArgumentList $robocopyArgs -Wait -PassThru -NoNewWindow
            
            if ($process.ExitCode -ge 8) {
                throw "File copy failed with exit code $($process.ExitCode)"
            }
        }
        
        Write-Log "✓ Deployment package copied to $Table"
        return $true
    }
    catch {
        Write-Log "Failed to copy deployment package to $Table`: $($_.Exception.Message)" "ERROR"
        return $false
    }
}

function Invoke-RemoteInstallation {
    param([string]$Table)
    
    try {
        Write-Log "Starting remote installation on $Table..."
        
        $installScript = "C:\Temp\CoffeeTableUpdates\Silent-CoffeeTable-Installer-v2.ps1"
        
        if (-not $TestMode) {
            # Execute installation remotely using PowerShell remoting
            $session = New-PSSession -ComputerName $Table -Credential $Credential -ErrorAction Stop
            
            $installResult = Invoke-Command -Session $session -ScriptBlock {
                param($ScriptPath, $TestMode)
                
                try {
                    # Copy script locally for execution
                    $localScript = "C:\Temp\Install.ps1"
                    Copy-Item $ScriptPath $localScript -Force
                    
                    # Execute with elevated privileges
                    $process = Start-Process "powershell.exe" `
                        -ArgumentList "-ExecutionPolicy Bypass -File `"$localScript`" -TestMode:`$$TestMode" `
                        -Wait -PassThru -Verb RunAs
                    
                    return @{
                        Success = $process.ExitCode -eq 0
                        ExitCode = $process.ExitCode
                    }
                }
                catch {
                    return @{
                        Success = $false
                        Error = $_.Exception.Message
                    }
                }
            } -ArgumentList $installScript, $TestMode
            
            Remove-PSSession $session
            
            if (-not $installResult.Success) {
                throw "Remote installation failed: $($installResult.Error)"
            }
        }
        
        Write-Log "✓ Remote installation completed on $Table"
        return $true
    }
    catch {
        Write-Log "Remote installation failed on $Table`: $($_.Exception.Message)" "ERROR"
        return $false
    }
}

function Configure-RemoteScheduledTasks {
    param([string]$Table)
    
    try {
        Write-Log "Configuring scheduled tasks on $Table..."
        
        $taskScript = "C:\Temp\CoffeeTableUpdates\ScheduledTaskSetup.ps1"
        
        if (-not $TestMode) {
            $session = New-PSSession -ComputerName $Table -Credential $Credential -ErrorAction Stop
            
            $taskResult = Invoke-Command -Session $session -ScriptBlock {
                param($ScriptPath, $TestMode)
                
                try {
                    # Copy script locally for execution
                    $localScript = "C:\Temp\TaskSetup.ps1"
                    Copy-Item $ScriptPath $localScript -Force
                    
                    # Execute task setup
                    $process = Start-Process "powershell.exe" `
                        -ArgumentList "-ExecutionPolicy Bypass -File `"$localScript`" -TestMode:`$$TestMode" `
                        -Wait -PassThru -Verb RunAs
                    
                    return $process.ExitCode -eq 0
                }
                catch {
                    return $false
                }
            } -ArgumentList $taskScript, $TestMode
            
            Remove-PSSession $session
            
            if (-not $taskResult) {
                throw "Scheduled task configuration failed"
            }
        }
        
        Write-Log "✓ Scheduled tasks configured on $Table"
        return $true
    }
    catch {
        Write-Log "Failed to configure scheduled tasks on $Table`: $($_.Exception.Message)" "ERROR"
        return $false
    }
}

function Get-DeploymentStatus {
    $elapsed = (Get-Date) - $global:DeploymentStatus.StartTime
    $progress = if ($global:DeploymentStatus.TotalTables -gt 0) {
        [math]::Round(($global:DeploymentStatus.Successful + $global:DeploymentStatus.Failed) / $global:DeploymentStatus.TotalTables * 100, 2)
    } else { 0 }
    
    return @{
        Elapsed = $elapsed.ToString("hh\:mm\:ss")
        Progress = $progress
        Successful = $global:DeploymentStatus.Successful
        Failed = $global:DeploymentStatus.Failed
        Remaining = $global:DeploymentStatus.TotalTables - ($global:DeploymentStatus.Successful + $global:DeploymentStatus.Failed)
    }
}

function Deploy-ToTable {
    param([string]$Table)
    
    $tableStatus = @{
        Table = $Table
        Steps = @()
        Success = $true
        Error = $null
        StartTime = Get-Date
    }
    
    try {
        Write-Log "=== Starting deployment to $Table ==="
        
        # Step 1: Test connectivity
        if (-not (Test-TableConnectivity $Table)) {
            throw "Connectivity test failed"
        }
        $tableStatus.Steps += "Connectivity"
        
        # Step 2: Copy deployment package
        if (-not (Copy-DeploymentPackage $Table)) {
            throw "Package copy failed"
        }
        $tableStatus.Steps += "PackageCopy"
        
        # Step 3: Remote installation
        if (-not (Invoke-RemoteInstallation $Table)) {
            throw "Installation failed"
        }
        $tableStatus.Steps += "Installation"
        
        # Step 4: Configure scheduled tasks
        if (-not (Configure-RemoteScheduledTasks $Table)) {
            throw "Task configuration failed"
        }
        $tableStatus.Steps += "TaskConfiguration"
        
        $tableStatus.Success = $true
        $global:DeploymentStatus.Successful++
        Write-Log "✓ Deployment to $Table completed successfully"
        
    }
    catch {
        $tableStatus.Success = $false
        $tableStatus.Error = $_.Exception.Message
        $global:DeploymentStatus.Failed++
        Write-Log "✗ Deployment to $Table failed: $($_.Exception.Message)" "ERROR"
    }
    finally {
        $tableStatus.EndTime = Get-Date
        $tableStatus.Duration = ($tableStatus.EndTime - $tableStatus.StartTime).ToString("hh\:mm\:ss")
        $global:DeploymentStatus.TableStatus[$Table] = $tableStatus
    }
    
    return $tableStatus
}

# Main execution
try {
    Start-Transcript $LogPath
    Write-Log "=== Starting Network Deployment ==="
    
    # Check admin privileges
    if (-not (Test-Admin)) {
        throw "Administrator privileges required. Please run as Administrator."
    }
    
    # Validate tables parameter
    if ($Tables.Count -eq 0) {
        throw "No tables specified for deployment. Use -Tables parameter or modify default list."
    }
    
    $global:DeploymentStatus.TotalTables = $Tables.Count
    Write-Log "Target tables: $($Tables -join ', ')"
    Write-Log "Total tables to deploy: $($Tables.Count)"
    
    # Get credentials if not provided
    if (-not $Credential) {
        $Credential = Get-Credential -Message "Enter domain credentials for deployment" -ErrorAction Stop
    }
    
    # Deploy to tables in parallel
    $jobs = @()
    foreach ($table in $Tables) {
        $jobs += Start-Job -Name "Deploy_$table" -ScriptBlock ${function:Deploy-ToTable} -ArgumentList $table
    }
    
    # Monitor deployment progress
    $completed = 0
    while ($jobs | Where-Object { $_.State -eq "Running" }) {
        $status = Get-DeploymentStatus
        Write-Log "Deployment progress: $($status.Progress)% - Successful: $($status.Successful), Failed: $($status.Failed), Elapsed: $($status.Elapsed)"
        
        # Wait for jobs with timeout
        $completedJobs = Wait-Job -Job $jobs -Timeout 30
        $completed += ($completedJobs | Measure-Object).Count
        
        # Process completed jobs
        foreach ($job in $completedJobs) {
            $result = Receive-Job -Job $job
            Write-Log "Job $($job.Name) completed: $(if ($result.Success) {'SUCCESS'} else {'FAILED'})"
        }
        
        Start-Sleep -Seconds 5
    }
    
    # Get final results
    $results = Get-Job | Receive-Job
    Remove-Job -Job $jobs -Force
    
    # Generate deployment report
    $finalStatus = Get-DeploymentStatus
    Write-Log "=== Deployment Summary ==="
    Write-Log "Total time: $($finalStatus.Elapsed)"
    Write-Log "Successful: $($finalStatus.Successful)/$($finalStatus.TotalTables)"
    Write-Log "Failed: $($finalStatus.Failed)/$($finalStatus.TotalTables)"
    
    # Detailed table status
    Write-Log "`nDetailed Table Status:"
    foreach ($table in $Tables) {
        $status = $global:DeploymentStatus.TableStatus[$table]
        if ($status.Success) {
            Write-Log "✓ $table : SUCCESS ($($status.Duration))"
        } else {
            Write-Log "✗ $table : FAILED - $($status.Error)"
        }
    }
    
    # Final exit code
    if ($finalStatus.Failed -eq 0) {
        Write-Log "=== Network deployment completed successfully ==="
        exit 0
    } else {
        Write-Log "=== Network deployment completed with errors ==="
        exit 1
    }
}
catch {
    $errorMsg = "Fatal deployment error: $($_.Exception.Message)"
    Write-Log $errorMsg "ERROR"
    exit 1
}
finally {
    # Cleanup any remaining jobs
    Get-Job | Remove-Job -Force -ErrorAction SilentlyContinue
    Stop-Transcript
}
