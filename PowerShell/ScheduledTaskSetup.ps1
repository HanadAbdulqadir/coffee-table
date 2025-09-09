<#
.SYNOPSIS
    Scheduled Task Setup for Coffee Table Gaming Systems
.DESCRIPTION
    Configures scheduled tasks for daily updates, maintenance, and auto-start.
    Designed for domain environments with admin privileges.
.PARAMETER TestMode
    Run in test mode without making actual changes
.PARAMETER LogPath
    Custom path for log file
#>
param(
    [switch]$TestMode = $false,
    [string]$LogPath = "C:\Temp\ScheduledTaskSetup_Log_$(Get-Date -Format 'yyyyMMdd_HHmmss').txt"
)

$ErrorActionPreference = "Stop"
$global:TaskStatus = @{
    Success = $true
    Errors = @()
    TasksConfigured = @()
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
    $adminRole = [Security.Principal.WindowsBuiltInRole]::Administrator
    return ([Security.Principal.WindowsPrincipal]$currentUser).IsInRole($adminRole)
}

function Register-ScheduledTaskWithRetry {
    param(
        [string]$TaskName,
        [Microsoft.PowerShell.ScheduledTask.ScheduledTaskAction]$Action,
        [Microsoft.PowerShell.ScheduledTask.ScheduledTaskTrigger]$Trigger,
        [Microsoft.PowerShell.ScheduledTask.ScheduledTaskSettingsSet]$Settings,
        [string]$Description,
        [int]$MaxRetries = 3
    )
    
    $retryCount = 0
    while ($retryCount -lt $MaxRetries) {
        try {
            if (-not $TestMode) {
                # Check if task already exists
                $existingTask = Get-ScheduledTask -TaskName $TaskName -ErrorAction SilentlyContinue
                if ($existingTask) {
                    Unregister-ScheduledTask -TaskName $TaskName -Confirm:$false
                    Write-Log "Removed existing task: $TaskName"
                }
                
                Register-ScheduledTask -TaskName $TaskName `
                    -Action $Action `
                    -Trigger $Trigger `
                    -Settings $Settings `
                    -User "SYSTEM" `
                    -RunLevel Highest `
                    -Description $Description `
                    -Force
            }
            
            Write-Log "✓ Scheduled task configured: $TaskName"
            $global:TaskStatus.TasksConfigured += $TaskName
            return $true
        }
        catch {
            $retryCount++
            if ($retryCount -eq $MaxRetries) {
                throw "Failed to register task '$TaskName' after $MaxRetries attempts: $($_.Exception.Message)"
            }
            Write-Log "Attempt $retryCount failed for task '$TaskName', retrying in 2 seconds..." "WARNING"
            Start-Sleep -Seconds 2
        }
    }
}

function Configure-DailyUpdateTask {
    try {
        Write-Log "Configuring daily update task..."
        
        $taskName = "CoffeeTableDailyUpdate"
        $updateScript = "C:\Temp\DailyAutoUpdate.ps1"
        
        $action = New-ScheduledTaskAction -Execute "PowerShell.exe" `
            -Argument "-ExecutionPolicy Bypass -File `"$updateScript`""
        
        $trigger = New-ScheduledTaskTrigger -Daily -At 2AM
        $trigger.StartBoundary = (Get-Date -Hour 2 -Minute 0 -Second 0).ToString("s")
        
        $settings = New-ScheduledTaskSettingsSet `
            -AllowStartIfOnBatteries `
            -DontStopIfGoingOnBatteries `
            -RestartCount 3 `
            -RestartInterval (New-TimeSpan -Minutes 5) `
            -ExecutionTimeLimit (New-TimeSpan -Hours 2) `
            -MultipleInstances IgnoreNew
        
        $description = "Daily update for coffee table gaming system. Runs health checks, software updates, asset synchronization, and maintenance."
        
        Register-ScheduledTaskWithRetry -TaskName $taskName `
            -Action $action `
            -Trigger $trigger `
            -Settings $settings `
            -Description $description
        
        return $true
    }
    catch {
        $errorMsg = "Failed to configure daily update task: $($_.Exception.Message)"
        Write-Log $errorMsg "ERROR"
        $global:TaskStatus.Errors += $errorMsg
        return $false
    }
}

function Configure-FrontendAutoStartTask {
    try {
        Write-Log "Configuring frontend auto-start task..."
        
        $taskName = "AutoStartArcadeFrontend"
        $frontendPath = "C:\Games\AttractMode\AttractMode.exe"
        
        if (-not (Test-Path $frontendPath)) {
            Write-Log "Frontend executable not found, skipping auto-start configuration" "WARNING"
            return $true
        }
        
        $action = New-ScheduledTaskAction -Execute $frontendPath
        $trigger = New-ScheduledTaskTrigger -AtLogOn
        
        $settings = New-ScheduledTaskSettingsSet `
            -AllowStartIfOnBatteries `
            -DontStopIfGoingOnBatteries `
            -ExecutionTimeLimit (New-TimeSpan -Hours 24) `
            -MultipleInstances IgnoreNew
        
        $description = "Auto-start arcade frontend on user login. Launches AttractMode for immediate gaming access."
        
        Register-ScheduledTaskWithRetry -TaskName $taskName `
            -Action $action `
            -Trigger $trigger `
            -Settings $settings `
            -Description $description
        
        return $true
    }
    catch {
        $errorMsg = "Failed to configure frontend auto-start task: $($_.Exception.Message)"
        Write-Log $errorMsg "ERROR"
        $global:TaskStatus.Errors += $errorMsg
        return $false
    }
}

function Configure-WeeklyMaintenanceTask {
    try {
        Write-Log "Configuring weekly maintenance task..."
        
        $taskName = "CoffeeTableWeeklyMaintenance"
        
        $action = New-ScheduledTaskAction -Execute "PowerShell.exe" `
            -Argument "-Command `"Get-ChildItem C:\Temp -Recurse -File | Where-Object { `$_.CreationTime -lt (Get-Date).AddDays(-7) } | Remove-Item -Force; Get-ChildItem C:\Games\Logs -Recurse -File | Where-Object { `$_.CreationTime -lt (Get-Date).AddDays(-30) } | Remove-Item -Force`""
        
        $trigger = New-ScheduledTaskTrigger -Weekly -DaysOfWeek Sunday -At 3AM
        $trigger.StartBoundary = (Get-Date -Hour 3 -Minute 0 -Second 0).ToString("s")
        
        $settings = New-ScheduledTaskSettingsSet `
            -AllowStartIfOnBatteries `
            -DontStopIfGoingOnBatteries `
            -ExecutionTimeLimit (New-TimeSpan -Minutes 30) `
            -MultipleInstances IgnoreNew
        
        $description = "Weekly maintenance for coffee table system. Cleans up temporary files and old logs."
        
        Register-ScheduledTaskWithRetry -TaskName $taskName `
            -Action $action `
            -Trigger $trigger `
            -Settings $settings `
            -Description $description
        
        return $true
    }
    catch {
        $errorMsg = "Failed to configure weekly maintenance task: $($_.Exception.Message)"
        Write-Log $errorMsg "ERROR"
        $global:TaskStatus.Errors += $errorMsg
        return $false
    }
}

function Configure-HealthMonitorTask {
    try {
        Write-Log "Configuring health monitor task..."
        
        $taskName = "CoffeeTableHealthMonitor"
        $monitorScript = "C:\Temp\HealthMonitor.ps1"
        
        # Create basic health monitor script if it doesn't exist
        if (-not (Test-Path $monitorScript) -and -not $TestMode) {
            $healthScriptContent = @'
# Basic health monitor script
try {
    $health = @{
        Timestamp = Get-Date
        DiskSpace = (Get-PSDrive C).FreeSpace / 1GB
        Memory = (Get-CimInstance Win32_OperatingSystem).FreePhysicalMemory / 1MB
        Services = @(Get-Service -Name "Winmgmt", "EventLog", "Schedule" | Where-Object { $_.Status -ne "Running" })
    }
    
    if ($health.DiskSpace -lt 5 -or $health.Memory -lt 500 -or $health.Services.Count -gt 0) {
        # Could send email notification here
        Write-Warning "Health check failed: Disk: $($health.DiskSpace)GB free, Memory: $($health.Memory)MB free, Services down: $($health.Services.Count)"
        exit 1
    }
    
    exit 0
}
catch {
    Write-Error "Health monitor error: $($_.Exception.Message)"
    exit 1
}
'@
            Set-Content -Path $monitorScript -Value $healthScriptContent
        }
        
        $action = New-ScheduledTaskAction -Execute "PowerShell.exe" `
            -Argument "-ExecutionPolicy Bypass -File `"$monitorScript`""
        
        $trigger = New-ScheduledTaskTrigger -Daily -At 6AM
        $trigger.StartBoundary = (Get-Date -Hour 6 -Minute 0 -Second 0).ToString("s")
        
        $settings = New-ScheduledTaskSettingsSet `
            -AllowStartIfOnBatteries `
            -DontStopIfGoingOnBatteries `
            -ExecutionTimeLimit (New-TimeSpan -Minutes 5) `
            -MultipleInstances IgnoreNew
        
        $description = "Daily health monitoring for coffee table system. Checks disk space, memory, and critical services."
        
        Register-ScheduledTaskWithRetry -TaskName $taskName `
            -Action $action `
            -Trigger $trigger `
            -Settings $settings `
            -Description $description
        
        return $true
    }
    catch {
        $errorMsg = "Failed to configure health monitor task: $($_.Exception.Message)"
        Write-Log $errorMsg "ERROR"
        $global:TaskStatus.Errors += $errorMsg
        return $false
    }
}

function Test-TaskConfiguration {
    try {
        Write-Log "Testing task configurations..."
        
        $tasksToTest = @(
            "CoffeeTableDailyUpdate",
            "AutoStartArcadeFrontend", 
            "CoffeeTableWeeklyMaintenance",
            "CoffeeTableHealthMonitor"
        )
        
        foreach ($taskName in $tasksToTest) {
            $task = Get-ScheduledTask -TaskName $taskName -ErrorAction SilentlyContinue
            if ($task) {
                Write-Log "✓ Task verified: $taskName"
            } else {
                Write-Log "Task not found: $taskName" "WARNING"
            }
        }
        
        return $true
    }
    catch {
        Write-Log "Task verification failed: $($_.Exception.Message)" "ERROR"
        return $false
    }
}

# Main execution
try {
    Start-Transcript $LogPath
    Write-Log "=== Starting Scheduled Task Setup ==="
    
    # Check admin privileges
    if (-not (Test-Admin)) {
        throw "Administrator privileges required. Please run as Administrator."
    }
    
    # Configure tasks
    $configurationSteps = @(
        { Configure-DailyUpdateTask }
        { Configure-FrontendAutoStartTask }
        { Configure-WeeklyMaintenanceTask }
        { Configure-HealthMonitorTask }
    )
    
    foreach ($step in $configurationSteps) {
        if (-not (& $step)) {
            $global:TaskStatus.Success = $false
        }
    }
    
    # Test configuration
    if (-not $TestMode) {
        Test-TaskConfiguration
    }
    
    # Final status
    if ($global:TaskStatus.Success) {
        Write-Log "=== Scheduled task setup completed successfully ==="
        Write-Log "Tasks configured: $($global:TaskStatus.TasksConfigured -join ', ')"
        
        # Display task schedule summary
        Write-Log "`nTask Schedule Summary:"
        Write-Log "• Daily Update: 2:00 AM daily"
        Write-Log "• Frontend Auto-Start: On user login" 
        Write-Log "• Weekly Maintenance: 3:00 AM every Sunday"
        Write-Log "• Health Monitor: 6:00 AM daily"
        
        exit 0
    } else {
        Write-Log "=== Scheduled task setup completed with errors ==="
        Write-Log "Errors: $($global:TaskStatus.Errors -join '; ')"
        exit 1
    }
}
catch {
    $errorMsg = "Fatal task setup error: $($_.Exception.Message)"
    Write-Log $errorMsg "ERROR"
    exit 1
}
finally {
    Stop-Transcript
}
