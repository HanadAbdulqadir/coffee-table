# GameTable Manager Pro - Quick Start (PowerShell)
Write-Host "========================================" -ForegroundColor Green
Write-Host "GameTable Manager Pro - Quick Start" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
Write-Host ""

# Check if .NET 8 is installed
try {
    $dotnetVersion = dotnet --version
    Write-Host "✓ .NET $dotnetVersion is installed" -ForegroundColor Green
}
catch {
    Write-Host ".NET 8 is not installed. Installing automatically..." -ForegroundColor Yellow
    Write-Host ""
    
    # Automatic .NET 8 installation
    Write-Host "Checking system architecture..." -ForegroundColor Cyan
    
    if ([Environment]::Is64BitOperatingSystem) {
        $dotnetUrl = "https://download.visualstudio.microsoft.com/download/pr/6fa74b18-09b0-4b9f-8d65-86a0f5df39dc/09b93c2b0d7b3deec95d91f6fa0c1d8a/dotnet-sdk-8.0.401-win-x64.exe"
        $installer = "dotnet-sdk-8.0.401-win-x64.exe"
        Write-Host "64-bit system detected. Using x64 installer." -ForegroundColor Cyan
    }
    else {
        $dotnetUrl = "https://download.visualstudio.microsoft.com/download/pr/01d579d7-2fdb-41f8-8103-d91ed1c158a6/0ee22e8ad19d96ad69c68b2a4cc0d2c1/dotnet-sdk-8.0.401-win-x86.exe"
        $installer = "dotnet-sdk-8.0.401-win-x86.exe"
        Write-Host "32-bit system detected. Using x86 installer." -ForegroundColor Cyan
    }

    # Download installer
    Write-Host "Downloading .NET 8 SDK..." -ForegroundColor Cyan
    try {
        Invoke-WebRequest -Uri $dotnetUrl -OutFile $installer -ErrorAction Stop
        Write-Host "✓ Download completed" -ForegroundColor Green
    }
    catch {
        Write-Host "ERROR: Failed to download .NET 8 SDK" -ForegroundColor Red
        Write-Host "Please check your internet connection and try again." -ForegroundColor Yellow
        Write-Host ""
        pause
        exit 1
    }

    # Run silent install
    Write-Host "Installing .NET 8 SDK silently..." -ForegroundColor Cyan
    try {
        $process = Start-Process -FilePath $installer -ArgumentList "/quiet /norestart" -Wait -PassThru -ErrorAction Stop
        
        if ($process.ExitCode -eq 0) {
            Write-Host "✓ .NET 8 installed successfully" -ForegroundColor Green
        }
        else {
            Write-Host "WARNING: Installation completed with exit code $($process.ExitCode)" -ForegroundColor Yellow
        }
    }
    catch {
        Write-Host "ERROR: Installation failed" -ForegroundColor Red
        Write-Host "Please install .NET 8 manually from: https://dotnet.microsoft.com/download/dotnet/8.0" -ForegroundColor Yellow
        Write-Host ""
        pause
        exit 1
    }

    # Cleanup
    Remove-Item $installer -Force -ErrorAction SilentlyContinue

    # Verify installation
    Write-Host "Verifying .NET installation..." -ForegroundColor Cyan
    try {
        $version = & dotnet --version
        if ($version) {
            Write-Host "✓ .NET $version installed successfully" -ForegroundColor Green
        }
        else {
            Write-Host "WARNING: .NET installed but not detected in PATH" -ForegroundColor Yellow
            Write-Host "You may need to restart your terminal or computer." -ForegroundColor Yellow
        }
    }
    catch {
        Write-Host "WARNING: Unable to verify .NET installation automatically" -ForegroundColor Yellow
        Write-Host "Please restart your computer and run this script again." -ForegroundColor Yellow
    }
    
    Write-Host ""
}

Write-Host ""

# Build the application
Write-Host "Building GameTable Manager Pro..." -ForegroundColor Yellow
dotnet build GameTableManagerPro.sln --configuration Release --nologo

if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR: Build failed!" -ForegroundColor Red
    Write-Host ""
    pause
    exit 1
}

Write-Host "✓ Build successful" -ForegroundColor Green
Write-Host ""

# Run the application
Write-Host "Starting GameTable Manager Pro..." -ForegroundColor Yellow
Write-Host ""

$appPath = "GameTableManagerPro\bin\Release\net8.0-windows\GameTableManagerPro.exe"
if (Test-Path $appPath) {
    Start-Process $appPath
    Write-Host "Application started! Check your taskbar for the window." -ForegroundColor Green
    Write-Host ""
    Write-Host "If the application doesn't start, you can manually run:"
    Write-Host $appPath -ForegroundColor Cyan
}
else {
    Write-Host "ERROR: Application not found at expected path!" -ForegroundColor Red
    Write-Host "Please check the build output." -ForegroundColor Yellow
}

Write-Host ""
pause
