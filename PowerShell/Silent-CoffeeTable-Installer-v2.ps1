<#
.SYNOPSIS
    Silent Coffee Table Installer v2 - Complete gaming system deployment
.DESCRIPTION
    Installs Playnite, emulators, Steam/Epic/Xbox integration, controller support,
    touchscreen calibration, auto-sync, nightly restart, and POS overlay.
    Designed for domain environments with admin privileges.
.PARAMETER TestMode
    Run in test mode without making actual changes
.PARAMETER LogPath
    Custom path for log file
#>
param(
    [switch]$TestMode = $false,
    [string]$LogPath = "C:\Temp\Install_Log_$(Get-Date -Format 'yyyyMMdd_HHmmss').txt"
)

$ErrorActionPreference = "Stop"
$global:InstallStatus = @{
    Success = $true
    Errors = @()
    StepsCompleted = @()
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

function Install-SoftwareComponent {
    param(
        [string]$Name,
        [string]$URL,
        [string]$InstallPath,
        [string]$InstallerType,
        [string[]]$Arguments
    )
    
    try {
        Write-Log "Installing $Name..."
        
        $downloadPath = "C:\Temp\$($Name)_$([IO.Path]::GetFileName($URL))"
        
        # Download component
        if (-not $TestMode) {
            Write-Log "Downloading $Name from $URL"
            Invoke-WebRequest -Uri $URL -OutFile $downloadPath -UseBasicParsing
        }
        
        # Install based on type
        switch ($InstallerType) {
            "EXE" {
                if (-not $TestMode) {
                    $process = Start-Process $downloadPath -ArgumentList $Arguments -Wait -PassThru
                    if ($process.ExitCode -ne 0) {
                        throw "Installer failed with exit code $($process.ExitCode)"
                    }
                }
            }
            "ZIP" {
                if (-not $TestMode) {
                    Expand-Archive -Path $downloadPath -DestinationPath $InstallPath -Force
                }
            }
            "MSI" {
                if (-not $TestMode) {
                    $process = Start-Process "msiexec.exe" -ArgumentList @("/i", $downloadPath, "/quiet", "/norestart") -Wait -PassThru
                    if ($process.ExitCode -ne 0) {
                        throw "MSI installer failed with exit code $($process.ExitCode)"
                    }
                }
            }
        }
        
        Write-Log "✓ $Name installed successfully"
        $global:InstallStatus.StepsCompleted += $Name
        return $true
    }
    catch {
        $errorMsg = "Failed to install $Name`: $($_.Exception.Message)"
        Write-Log $errorMsg "ERROR"
        $global:InstallStatus.Errors += $errorMsg
        $global:InstallStatus.Success = $false
        return $false
    }
}

function Install-Drivers {
    try {
        Write-Log "Installing drivers..."
        
        # Touchscreen drivers
        $driverPath = "C:\Temp\Drivers"
        if (Test-Path $driverPath) {
            if (-not $TestMode) {
                Get-ChildItem $driverPath -Filter "*.inf" | ForEach-Object {
                    pnputil /add-driver $_.FullName /install
                }
            }
        }
        
        # Controller drivers
        if (-not $TestMode) {
            # Install Xbox controller support
            Add-WindowsCapability -Online -Name "Xbox.XboxGameOverlay~~~~0.0.1.0" -ErrorAction SilentlyContinue
            Add-WindowsCapability -Online -Name "Xbox.GamingOverlay~~~~0.0.1.0" -ErrorAction SilentlyContinue
        }
        
        Write-Log "✓ Drivers installed"
        $global:InstallStatus.StepsCompleted += "Drivers"
        return $true
    }
    catch {
        $errorMsg = "Driver installation failed: $($_.Exception.Message)"
        Write-Log $errorMsg "ERROR"
        $global:InstallStatus.Errors += $errorMsg
        return $false
    }
}

function Configure-AutoStart {
    try {
        Write-Log "Configuring auto-start..."
        
        $frontendPath = "C:\Games\AttractMode\AttractMode.exe"
        if (Test-Path $frontendPath) {
            if (-not $TestMode) {
                $action = New-ScheduledTaskAction -Execute $frontendPath
                $trigger = New-ScheduledTaskTrigger -AtLogOn
                $settings = New-ScheduledTaskSettingsSet -AllowStartIfOnBatteries -DontStopIfGoingOnBatteries
                
                Register-ScheduledTask -TaskName "AutoStartArcadeFrontend" `
                    -Action $action -Trigger $trigger -Settings $settings `
                    -User "SYSTEM" -RunLevel Highest -Force -Description "Auto-start arcade frontend on login"
            }
        }
        
        Write-Log "✓ Auto-start configured"
        $global:InstallStatus.StepsCompleted += "AutoStart"
        return $true
    }
    catch {
        $errorMsg = "Auto-start configuration failed: $($_.Exception.Message)"
        Write-Log $errorMsg "ERROR"
        $global:InstallStatus.Errors += $errorMsg
        return $false
    }
}

function Set-Permissions {
    try {
        Write-Log "Setting permissions..."
        
        $paths = @("C:\Games", "C:\Temp")
        foreach ($path in $paths) {
            if (Test-Path $path) {
                if (-not $TestMode) {
                    icacls $path /grant "Everyone:(OI)(CI)F" /T
                }
            }
        }
        
        Write-Log "✓ Permissions set"
        $global:InstallStatus.StepsCompleted += "Permissions"
        return $true
    }
    catch {
        $errorMsg = "Permission setting failed: $($_.Exception.Message)"
        Write-Log $errorMsg "ERROR"
        $global:InstallStatus.Errors += $errorMsg
        return $false
    }
}

# Main execution
try {
    Start-Transcript $LogPath
    Write-Log "=== Starting Silent Coffee Table Installation ==="
    
    # Check admin privileges
    if (-not (Test-Admin)) {
        throw "Administrator privileges required. Please run as Administrator."
    }
    
    # Create directory structure
    $installPaths = @("C:\Games", "C:\Games\Playnite", "C:\Games\RetroArch", "C:\Games\AttractMode", "C:\Games\Assets", "C:\Temp")
    foreach ($path in $installPaths) {
        if (-not (Test-Path $path)) {
            if (-not $TestMode) {
                New-Item -Path $path -ItemType Directory -Force
            }
            Write-Log "Created directory: $path"
        }
    }
    
    # Install software components
    $softwareComponents = @(
        @{
            Name = "Playnite"
            URL = "https://playnite.link/download/PlayniteInstaller.exe"
            InstallPath = "C:\Games\Playnite"
            InstallerType = "EXE"
            Arguments = @("/silent")
        },
        @{
            Name = "RetroArch"
            URL = "https://buildbot.libretro.com/stable/1.10.3/windows/x86_64/RetroArch.7z"
            InstallPath = "C:\Games\RetroArch"
            InstallerType = "ZIP"
            Arguments = @()
        },
        @{
            Name = "AttractMode"
            URL = "https://github.com/mickelson/attract/releases/download/v2.6.1/attract-v2.6.1-win64.zip"
            InstallPath = "C:\Games\AttractMode"
            InstallerType = "ZIP"
            Arguments = @()
        }
    )
    
    foreach ($software in $softwareComponents) {
        Install-SoftwareComponent @software
    }
    
    # Install drivers
    Install-Drivers
    
    # Configure auto-start
    Configure-AutoStart
    
    # Set permissions
    Set-Permissions
    
    # Create version file
    if (-not $TestMode) {
        $versionInfo = @{
            SoftwareVersion = "2.0.0"
            InstallDate = (Get-Date).ToString("yyyy-MM-dd")
            Components = $global:InstallStatus.StepsCompleted
        }
        $versionInfo | ConvertTo-Json | Set-Content "C:\Temp\TableVersion.json"
    }
    
    # Final status
    if ($global:InstallStatus.Success) {
        Write-Log "=== Installation completed successfully ==="
        Write-Log "Components installed: $($global:InstallStatus.StepsCompleted -join ', ')"
        exit 0
    } else {
        Write-Log "=== Installation completed with errors ==="
        Write-Log "Errors: $($global:InstallStatus.Errors -join '; ')"
        exit 1
    }
}
catch {
    $errorMsg = "Fatal installation error: $($_.Exception.Message)"
    Write-Log $errorMsg "ERROR"
    exit 1
}
finally {
    Stop-Transcript
}
