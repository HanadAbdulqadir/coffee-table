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
    Write-Host "ERROR: .NET 8 is not installed!" -ForegroundColor Red
    Write-Host "Please download from: https://dotnet.microsoft.com/download/dotnet/8.0" -ForegroundColor Yellow
    Write-Host ""
    pause
    exit 1
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
