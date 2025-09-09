<#
.SYNOPSIS
    Daily Auto Update for Coffee Table Gaming Systems
.DESCRIPTION
    Enhanced daily update script with health monitoring, asset synchronization,
    touchscreen calibration, and comprehensive logging. Designed for domain environments.
.PARAMETER TestMode
    Run in test mode without making changes
.PARAMETER ForceUpdate
    Force update even if already performed today
.PARAMETER LogPath
    Custom path for log file
#>
param(
    [switch]$TestMode = $false,
    [switch]$ForceUpdate = $false,
    [string]$LogPath = "C:\Temp\CoffeeTable_DailyUpdate_Log_$(Get-Date -Format 'yyyyMMdd_HHmmss').txt"
)

$ErrorActionPreference = "Stop"
$global:UpdateStatus = @{
    Success = $true
    Errors = @()
    StepsCompleted = @()
    HealthStatus = @{}
}

function Write-Log {
    param([string]$Message, [string]$Level = "INFO")
    $timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
    $logEntry = "[$timestamp] [$Level] $Message"
    Write-Host $logEntry
    Add-Content -Path $LogPath -Value $logEntry
}

function Test-TableHealth {
    $health = @{
        Timestamp = (Get-Date).ToString("yyyy-MM-dd HH:mm:ss")
        Storage = @{
            FreeSpaceGB = [math]::Round((Get-PSDrive C).FreeSpace / 1GB, 2)
            TotalSpaceGB = [math]::Round((Get-PSDrive C).Used + (Get-PSDrive C).Free / 1GB, 2)
            Status = (Get-PSDrive C).FreeSpace -gt 10GB
        }
        Memory = @{
            FreeMB = [math]::Round((Get-CimInstance Win32_OperatingSystem).FreePhysicalMemory / 1KB, 2)
            TotalMB = [math]::Round((Get-CimInstance Win32_OperatingSystem).TotalVisibleMemorySize / 1KB, 2)
            Status = (Get-CimInstance Win32_OperatingSystem).FreePhysicalMemory -gt 1GB
        }
        Network = @{
            MasterPCPing = Test-NetConnection -ComputerName "MasterPC" -Port 445 -InformationLevel Quiet -ErrorAction SilentlyContinue
            InternetAccess = Test-NetConnection -ComputerName "8.8.8.8" -InformationLevel Quiet -ErrorAction SilentlyContinue
            Status = $false
        }
        Services = @{
            Running = @()
            Stopped = @()
        }
    }
    
    $health.Network.Status = $health.Network.MasterPCPing -or $health.Network.InternetAccess
    
    # Check critical services
    $criticalServices = @("Winmgmt", "EventLog", "Schedule")
    foreach ($service in $criticalServices) {
        $svc = Get-Service -Name $service -ErrorAction SilentlyContinue
        if ($svc -and $svc.Status -eq "Running") {
            $health.Services.Running += $service
        } else {
            $health.Services.Stopped += $service
        }
    }
    
    $global:UpdateStatus.HealthStatus = $health
    return $health
}

function Invoke-HealthRemediation {
    param($HealthStatus)
    
    try {
        Write-Log "Performing health remediation..."
        
        # Clean up temp files if storage is low
        if (-not $HealthStatus.Storage.Status) {
            Write-Log "Low disk space detected, cleaning temp files..."
            if (-not $TestMode) {
                Get-ChildItem "C:\Temp" -Recurse -File | 
                    Where-Object { $_.CreationTime -lt (Get-Date).AddDays(-7) } | 
                    Remove-Item -Force -ErrorAction SilentlyContinue
                
                # Clear temporary internet files
                Remove-Item -Path "$env:TEMP\*" -Recurse -Force -ErrorAction SilentlyContinue
                Remove-Item -Path "$env:LOCALAPPDATA\Temp\*" -Recurse -Force -ErrorAction SilentlyContinue
            }
            Write-Log "✓ Temporary files cleaned"
        }
        
        # Restart critical services if stopped
        foreach ($service in $HealthStatus.Services.Stopped) {
            Write-Log "Attempting to start service: $service"
            if (-not $TestMode) {
                try {
                    Start-Service -Name $service -ErrorAction Stop
                    Write-Log "✓ Service $service started"
                }
                catch {
                    Write-Log "Failed to start service $service: $($_.Exception.Message)" "WARNING"
                }
            }
        }
        
        $global:UpdateStatus.StepsCompleted += "HealthRemediation"
        return $true
    }
    catch {
        Write-Log "Health remediation failed: $($_.Exception.Message)" "ERROR"
        return $false
    }
}

function Update-SoftwareComponents {
    try {
        Write-Log "Checking for software updates..."
        
        $config = @{
            InstallerRemote = "\\MasterPC\CoffeeTableUpdates\Silent-CoffeeTable-Installer-v2.ps1"
            LocalInstaller = "C:\Temp\Silent-CoffeeTable-Installer-v2.ps1"
            VersionFile = "C:\Temp\TableVersion.json"
        }
        
        # Check if update is needed
        $currentVersion = if (Test-Path $config.VersionFile) {
            Get-Content $config.VersionFile | ConvertFrom-Json
        } else {
            @{ SoftwareVersion = "1.0.0"; LastUpdate = (Get-Date).AddDays(-1).ToString() }
        }
        
        $lastUpdate = [DateTime]$currentVersion.LastUpdate
        if (-not $ForceUpdate -and (Get-Date).Date -eq $lastUpdate.Date) {
            Write-Log "Update already performed today. Use -ForceUpdate to override."
            return $true
        }
        
        # Check for newer installer
        if (Test-Path $config.InstallerRemote) {
            $remoteModified = (Get-Item $config.InstallerRemote).LastWriteTime
            $localModified = if (Test-Path $config.LocalInstaller) { 
                (Get-Item $config.LocalInstaller).LastWriteTime 
            } else { 
                [DateTime]::MinValue 
            }
            
            if ($remoteModified -gt $localModified -or $ForceUpdate) {
                Write-Log "New installer version available, updating..."
                if (-not $TestMode) {
                    Copy-Item -Path $config.InstallerRemote -Destination $config.LocalInstaller -Force
                    # Run the updated installer
                    $installerArgs = @(
                        "-ExecutionPolicy", "Bypass",
                        "-File", "`"$($config.LocalInstaller)`""
                    )
                    if ($TestMode) {
                        $installerArgs += "-TestMode"
                    }
                    Start-Process "powershell.exe" -ArgumentList $installerArgs -Wait -Verb RunAs
                }
                Write-Log "✓ Software components updated"
            } else {
                Write-Log "Software components are up to date"
            }
        }
        
        $global:UpdateStatus.StepsCompleted += "SoftwareUpdate"
        return $true
    }
    catch {
        Write-Log "Software update failed: $($_.Exception.Message)" "ERROR"
        $global:UpdateStatus.Errors += "SoftwareUpdate: $($_.Exception.Message)"
        return $false
    }
}

function Sync-GameAssets {
    try {
        Write-Log "Synchronizing game assets..."
        
        $config = @{
            Source = "\\MasterPC\CoffeeTableUpdates\GameAssets\*"
            Destination = "C:\Games\Assets\"
            LogFile = "C:\Temp\AssetSync_Log_$(Get-Date -Format 'yyyyMMdd_HHmmss').txt"
        }
        
        if (Test-Path $config.Source) {
            if (-not $TestMode) {
                # Create destination if it doesn't exist
                if (-not (Test-Path $config.Destination)) {
                    New-Item -Path $config.Destination -ItemType Directory -Force
                }
                
                # Robocopy for reliable file synchronization
                $robocopyArgs = @(
                    $config.Source.Replace('\*', ''),
                    $config.Destination,
                    "/MIR", "/NP", "/R:3", "/W:5", "/LOG:$($config.LogFile)", "/TEE"
                )
                
                $process = Start-Process "robocopy.exe" -ArgumentList $robocopyArgs -Wait -PassThru -NoNewWindow
                
                if ($process.ExitCode -ge 8) {
                    throw "Asset synchronization failed with exit code $($process.ExitCode)"
                }
            }
            Write-Log "✓ Game assets synchronized"
        } else {
            Write-Log "Asset source not available, skipping synchronization" "WARNING"
        }
        
        $global:UpdateStatus.StepsCompleted += "AssetSync"
        return $true
    }
    catch {
        Write-Log "Asset synchronization failed: $($_.Exception.Message)" "ERROR"
        $global:UpdateStatus.Errors += "AssetSync: $($_.Exception.Message)"
        return $false
    }
}

function Invoke-TouchCalibration {
    try {
        Write-Log "Running touchscreen calibration..."
        
        $calibrationTool = "C:\Temp\TouchCalibration.exe"
        if (Test-Path $calibrationTool) {
            if (-not $TestMode) {
                $process = Start-Process $calibrationTool -Verb RunAs -PassThru
                # Wait for calibration with timeout
                if (-not $process.WaitForExit(30000)) {
                    $process.Kill()
                    throw "Touch calibration timed out"
                }
                if ($process.ExitCode -ne 0) {
                    throw "Touch calibration failed with exit code $($process.ExitCode)"
                }
            }
            Write-Log "✓ Touchscreen calibration completed"
        } else {
            Write-Log "Touch calibration tool not found, skipping" "WARNING"
        }
        
        $global:UpdateStatus.StepsCompleted += "TouchCalibration"
        return $true
    }
    catch {
        Write-Log "Touch calibration failed: $($_.Exception.Message)" "ERROR"
        $global:UpdateStatus.Errors += "TouchCalibration: $($_.Exception.Message)"
        return $false
    }
}

function Update-LEDLighting {
    try {
        Write-Log "Updating LED lighting profile..."
        
        $ledTool = "C:\Temp\OpenRGB\OpenRGB.exe"
        if (Test-Path $ledTool) {
            if (-not $TestMode) {
                $process = Start-Process $ledTool -ArgumentList "--profile CafeGaming" -Wait -PassThru
                if ($process.ExitCode -ne 0) {
                    throw "LED profile update failed with exit code $($process.ExitCode)"
                }
            }
            Write-Log "✓ LED lighting profile updated"
        } else {
            Write-Log "LED control tool not found, skipping" "WARNING"
        }
        
        $global:UpdateStatus.StepsCompleted += "LEDUpdate"
        return $true
    }
    catch {
        Write-Log "LED update failed: $($_.Exception.Message)" "ERROR"
        $global:UpdateStatus.Errors += "LEDUpdate: $($_.Exception.Message)"
        return $false
    }
}

# Main execution
try {
    Start-Transcript $LogPath
    Write-Log "=== Starting Daily Coffee Table Update ==="
    
    # Perform health check
    $healthStatus = Test-TableHealth
    Write-Log "Health status: $(if ($healthStatus.Storage.Status -and $healthStatus.Memory.Status -and $healthStatus.Network.Status) {'HEALTHY'} else {'NEEDS ATTENTION'})"
    
    # Perform remediation if needed
    if (-not ($healthStatus.Storage.Status -and $healthStatus.Memory.Status -and $healthStatus.Network.Status)) {
        Invoke-HealthRemediation $healthStatus
    }
    
    # Execute update steps
    $updateSteps = @(
        { Update-SoftwareComponents }
        { Sync-GameAssets }
        { Invoke-TouchCalibration }
        { Update-LEDLighting }
    )
    
    foreach ($step in $updateSteps) {
        if (-not (& $step)) {
            $global:UpdateStatus.Success = $false
        }
    }
    
    # Update version information
    if (-not $TestMode -and $global:UpdateStatus.Success) {
        $versionInfo = @{
            SoftwareVersion = "2.0.0"
            LastUpdate = (Get-Date).ToString("yyyy-MM-dd HH:mm:ss")
            HealthStatus = $healthStatus
            UpdateSteps = $global:UpdateStatus.StepsCompleted
        }
        $versionInfo | ConvertTo-Json -Depth 3 | Set-Content "C:\Temp\TableVersion.json"
    }
    
    # Final status report
    if ($global:UpdateStatus.Success) {
        Write-Log "=== Daily update completed successfully ==="
        Write-Log "Steps completed: $($global:UpdateStatus.StepsCompleted -join ', ')"
        Write-Log "Health status: Free space: $($healthStatus.Storage.FreeSpaceGB)GB, Free memory: $($healthStatus.Memory.FreeMB)MB"
        exit 0
    } else {
        Write-Log "=== Daily update completed with errors ==="
        Write-Log "Errors: $($global:UpdateStatus.Errors -join '; ')"
        Write-Log "Health status may need attention"
        exit 1
    }
}
catch {
    $errorMsg = "Fatal update error: $($_.Exception.Message)"
    Write-Log $errorMsg "ERROR"
    exit 1
}
finally {
    Stop-Transcript
}
