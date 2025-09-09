#!/bin/bash

# GameTable Manager Pro - Quick Start (macOS)
echo "========================================"
echo "GameTable Manager Pro - Quick Start"
echo "========================================"
echo ""

# Check if .NET 8 is installed
if command -v dotnet &> /dev/null; then
    DOTNET_VERSION=$(dotnet --version)
    echo "✓ .NET $DOTNET_VERSION is installed"
else
    echo ".NET 8 is not installed. Installing automatically..."
    echo ""
    
    # Check if Homebrew is installed
    if ! command -v brew &> /dev/null; then
        echo "Homebrew not found. Installing Homebrew first..."
        /bin/bash -c "$(curl -fsSL https://raw.githubusercontent.com/Homebrew/install/HEAD/install.sh)"
        
        # Add Homebrew to PATH
        if [ -f "/opt/homebrew/bin/brew" ]; then
            eval "$(/opt/homebrew/bin/brew shellenv)"
        elif [ -f "/usr/local/bin/brew" ]; then
            eval "$(/usr/local/bin/brew shellenv)"
        fi
    fi
    
    # Install .NET 8 via Homebrew
    echo "Installing .NET 8 via Homebrew..."
    brew install dotnet
    
    # Verify installation
    if command -v dotnet &> /dev/null; then
        DOTNET_VERSION=$(dotnet --version)
        echo "✓ .NET $DOTNET_VERSION installed successfully"
    else
        echo "ERROR: .NET installation failed"
        echo "Please install .NET 8 manually from: https://dotnet.microsoft.com/download/dotnet/8.0"
        echo ""
        read -p "Press any key to continue..."
        exit 1
    fi
    echo ""
fi

echo ""

# Build the application
echo "Building GameTable Manager Pro..."
dotnet build GameTableManagerPro.sln --configuration Release --nologo

if [ $? -ne 0 ]; then
    echo "ERROR: Build failed!"
    echo ""
    read -p "Press any key to continue..."
    exit 1
fi

echo "✓ Build successful"
echo ""

# Run the application
echo "Starting GameTable Manager Pro..."
echo ""

APP_PATH="GameTableManagerPro/bin/Release/net8.0-windows/GameTableManagerPro.exe"
if [ -f "$APP_PATH" ]; then
    echo "Application built successfully!"
    echo ""
    echo "To run the application on macOS, you'll need to:"
    echo "1. Install Wine or CrossOver to run Windows applications"
    echo "2. Or use a Windows virtual machine"
    echo ""
    echo "The application file is located at:"
    echo "$APP_PATH"
    echo ""
    echo "For development, you can also run:"
    echo "dotnet run --project GameTableManagerPro"
else
    echo "ERROR: Application not found at expected path!"
    echo "Please check the build output."
fi

echo ""
read -p "Press any key to continue..."
