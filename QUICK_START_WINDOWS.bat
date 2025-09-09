@echo off
echo ========================================
echo GameTable Manager Pro - Quick Start
echo ========================================
echo.

REM Check if .NET 8 is installed
dotnet --version >nul 2>&1
if %errorlevel% neq 0 (
    echo .NET 8 is not installed. Please run the PowerShell version instead.
    echo.
    echo For automatic .NET 8 installation, run:
    echo QUICK_START_WINDOWS.ps1
    echo.
    echo Or download .NET 8 manually from:
    echo https://dotnet.microsoft.com/download/dotnet/8.0
    echo.
    pause
    exit /b 1
)

echo ✓ .NET 8 is installed
echo.

REM Build the application
echo Building GameTable Manager Pro...
dotnet build GameTableManagerPro.sln --configuration Release --nologo

if %errorlevel% neq 0 (
    echo ERROR: Build failed!
    echo.
    pause
    exit /b 1
)

echo ✓ Build successful
echo.

REM Run the application
echo Starting GameTable Manager Pro...
echo.

cd GameTableManagerPro\bin\Release\net8.0-windows
start GameTableManagerPro.exe

echo.
echo Application started! Check your taskbar for the window.
echo.
echo If the application doesn't start, you can manually run:
echo GameTableManagerPro\bin\Release\net8.0-windows\GameTableManagerPro.exe
echo.
pause
