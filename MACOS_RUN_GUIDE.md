# Running GameTable Manager Pro on macOS

## ⚠️ Important Considerations

GameTable Manager Pro is a **Windows WPF application** built with .NET 8 for Windows. However, there are ways to work with it on macOS:

## Option 1: Virtualization (Recommended)

### Using Parallels Desktop
1. Install **Parallels Desktop** on your Mac
2. Create a Windows 11 VM
3. Install .NET 8 SDK in the Windows VM
4. Clone the repository in the VM
5. Build and run the application normally

### Using VMware Fusion
1. Install **VMware Fusion**
2. Set up a Windows 11 virtual machine
3. Follow the same steps as Parallels

### Using UTM (Free Alternative)
1. Download **UTM** from https://mac.getutm.app/
2. Create a Windows VM
3. Install Windows and required dependencies

## Option 2: Cross-Platform Development Setup

### Prerequisites
```bash
# Install .NET 8 SDK for macOS
# Download from: https://dotnet.microsoft.com/download/dotnet/8.0

# Verify installation
dotnet --version

# Install PowerShell for macOS
# Download from: https://github.com/PowerShell/PowerShell

# Verify PowerShell
pwsh --version
```

### Working with the Codebase on macOS

#### 1. Code Editing and Version Control
```bash
# Clone the repository on macOS
git clone https://github.com/HanadAbdulqadir/coffee-table.git
cd coffee-table

# You can edit all C# code, XAML files, and PowerShell scripts on macOS
# The solution will open in VS Code or Rider
```

#### 2. Building the Solution (Cross-Platform Parts)
```bash
# You can build the class library components
dotnet build GameTableManagerPro.sln

# However, the WPF application itself requires Windows to run
```

#### 3. PowerShell Script Development
```bash
# You can develop and test PowerShell scripts on macOS
# Note: Some Windows-specific commands may not work

# Run PowerShell scripts with:
pwsh -File ./PowerShell/ScriptName.ps1
```

### Limitations on macOS

1. **WPF Application**: Cannot run directly on macOS (Windows-only technology)
2. **Windows-Specific Features**: Some PowerShell commands require Windows
3. **UI Testing**: Cannot test the actual WPF user interface
4. **Windows Dependencies**: System.Management.Automation.dll is Windows-specific

## Option 3: Remote Development

### Using Windows Server or Cloud VM
1. Set up a Windows Server on Azure/AWS
2. Use VS Code Remote Development extension
3. Develop remotely on the Windows machine
4. Test the application on the Windows environment

### Using GitHub Codespaces
1. Set up a Windows-based Codespace
2. Develop in the cloud-based Windows environment
3. Test the application remotely

## Option 4: Cross-Platform Rearchitecture (Advanced)

### If you want macOS native support:
1. **Migrate to MAUI**: .NET MAUI supports macOS and Windows
2. **Use Avalonia**: Cross-platform XAML framework
3. **Web Frontend**: Blazor WebAssembly with .NET backend
4. **Separate UI Layer**: Keep business logic cross-platform

### Migration Steps to MAUI
```xml
<!-- Change project type in GameTableManagerPro.csproj -->
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFrameworks>net8.0-android;net8.0-ios;net8.0-maccatalyst</TargetFrameworks>
    <TargetFrameworks Condition="$([MSBuild]::IsOSPlatform('windows'))">
      $(TargetFrameworks);net8.0-windows10.0.19041.0
    </TargetFrameworks>
    <UseMaui>true</UseMaui>
    <SingleProject>true</SingleProject>
  </PropertyGroup>
</Project>
```

## Current macOS-Compatible Components

### ✅ Works on macOS:
- **C# Business Logic**: All ViewModels and Services
- **Entity Framework Core**: Database operations
- **PowerShell Scripts**: Most scripting logic (with pwsh)
- **Configuration Files**: JSON settings and app config
- **Build System**: dotnet build for libraries

### ❌ Requires Windows:
- **WPF User Interface**: XAML views and windows
- **Windows-Specific APIs**: System.Management.Automation
- **Windows Forms Dialogs**: FolderBrowserDialog, etc.
- **Windows-Specific Features**: Registry access, etc.

## Development Workflow for macOS Users

### 1. Code on macOS
```bash
# Develop and commit code on macOS
git add .
git commit -m "Feature implemented"
git push origin main
```

### 2. Test on Windows
- Use a Windows VM or remote machine
- Pull the latest code
- Build and test the WPF application
- Test PowerShell scripts in Windows PowerShell

### 3. Continuous Integration
```yaml
# GitHub Actions workflow
name: Build and Test
on: [push, pull_request]

jobs:
  build-windows:
    runs-on: windows-latest
    steps:
    - uses: actions/checkout@v3
    - name: Setup .NET
      uses: actions/setup-dotnet@v3
      with:
        dotnet-version: '8.0.x'
    - name: Build
      run: dotnet build --configuration Release
    - name: Test
      run: dotnet test --configuration Release
```

## PowerShell Script Compatibility

### Windows-Only Commands (Need Alternatives)
```powershell
# These require Windows:
Get-Service
Get-WmiObject
Get-CimInstance
Register-ScheduledTask
New-ScheduledTaskAction
```

### Cross-Platform Alternatives
```powershell
# Use platform-agnostic approaches
# For scheduled tasks, consider:
- Cron jobs on macOS/Linux
- Task Scheduler on Windows
- Cross-platform task runners
```

## Recommended Setup for macOS Developers

### Minimal Setup
1. **.NET 8 SDK** on macOS
2. **PowerShell 7+** (pwsh)
3. **Git** for version control
4. **VS Code** with C# extensions

### Full Development Setup
1. **Parallels Desktop** with Windows 11 VM
2. **.NET 8 SDK** in Windows VM
3. **Visual Studio 2022** in Windows VM
4. **Shared folders** between macOS and Windows

### Cloud Development Setup
1. **Windows VM** on Azure/AWS
2. **VS Code Remote Development**
3. **GitHub Codespaces** with Windows image

## Troubleshooting macOS Development

### Common Issues
1. **File Path Differences**: Use `[System.IO.Path]::Combine()`
2. **Line Endings**: Git config for cross-platform compatibility
3. **Permissions**: macOS file permission differences
4. **Dependencies**: Windows-specific NuGet packages

### Solutions
```bash
# Set git config for cross-platform
git config --global core.autocrlf true

# Use cross-platform path handling in code
var path = Path.Combine("folder", "file.txt");

# Check for OS platform in code
if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
{
    // Windows-specific code
}
else
{
    // macOS/Linux alternative
}
```

## Future Cross-Platform Roadmap

### Phase 1: Business Logic Cross-Platform
- Separate business logic into .NET Standard library
- Keep platform-specific UI layers

### Phase 2: UI Framework Migration
- Evaluate MAUI vs Avalonia vs Blazor
- Plan gradual migration strategy

### Phase 3: Full Cross-Platform
- Single codebase for all platforms
- Platform-specific implementations where needed

## Immediate Next Steps

1. **Set up Windows VM** using Parallels/UTM
2. **Install development tools** in Windows environment
3. **Clone repository** in Windows VM
4. **Build and test** the WPF application
5. **Develop on macOS**, test on Windows VM

## Support Resources

- .NET macOS Documentation: https://docs.microsoft.com/dotnet/core/install/macos
- PowerShell macOS: https://docs.microsoft.com/powershell/scripting/install/installing-powershell-on-macos
- Parallels Desktop: https://www.parallels.com/
- UTM: https://mac.getutm.app/

---

**Note**: While you can develop the code on macOS, the WPF application itself requires Windows to run. The recommended approach is to use a Windows virtual machine for testing and execution while developing on macOS.
