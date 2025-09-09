# GameTable Manager Pro - Complete Deployment System

A comprehensive solution for managing gaming table deployments, featuring both a WPF management application and a PowerShell deployment system.

## Project Structure

```
.
├── PowerShell/                    # Complete deployment system
│   ├── Silent-CoffeeTable-Installer-v2.ps1
│   ├── DailyAutoUpdate.ps1
│   ├── ScheduledTaskSetup.ps1
│   ├── NetworkDeploy.ps1
│   └── README.md
├── setup-git-remote.ps1          # Git remote setup helper
└── README.md                     # This file
```

## What's Included

### 1. PowerShell Deployment System (Complete ✅)

A robust, enterprise-ready deployment system for gaming tables:

- **Silent Installer**: Complete silent installation of Playnite, RetroArch, AttractMode
- **Daily Updates**: Automated health monitoring and maintenance
- **Scheduled Tasks**: Fully configured automation
- **Network Deployment**: Parallel deployment to multiple tables
- **Domain Support**: Full integration with domain environments

### 2. WPF Management Application (Foundation Ready ⚡)

A modern .NET 8 WPF application with:

- **MVVM Architecture**: Using CommunityToolkit.MVVM
- **Entity Framework Core**: SQLite database with data models
- **Dependency Injection**: Modern application structure
- **Basic UI Framework**: Navigation and layout foundation

## Getting Started

### PowerShell Deployment System

1. **Review the deployment scripts** in the `PowerShell/` directory
2. **Read the documentation** in `PowerShell/README.md`
3. **Test the system** using the `-TestMode` parameter
4. **Deploy to tables** using `NetworkDeploy.ps1`

### WPF Application Development

The application foundation is ready for further development:

```csharp
// Core architecture is implemented:
- MVVM pattern with CommunityToolkit.MVVM
- Entity Framework Core with SQLite
- Dependency injection setup
- Basic UI structure
```

## Git Repository Setup

Your code is committed to a local Git repository. To push to a remote:

1. **Run the setup helper**:
   ```powershell
   .\setup-git-remote.ps1
   ```

2. **Follow the instructions** for your preferred Git platform (GitHub, GitLab, Azure DevOps)

3. **Push your code**:
   ```bash
   git push -u origin master
   ```

## Next Steps

### Immediate Use
- The PowerShell deployment system is complete and ready for production use
- Test with the `-TestMode` parameter first
- Deploy to your gaming tables using the network deployment script

### Application Development
The WPF application foundation is ready for:
- Dashboard module implementation
- Deployment wizard interface
- Health monitoring integration
- UI/UX enhancements

## Technical Specifications

- **.NET Version**: 8.0
- **Architecture**: MVVM with dependency injection
- **Database**: SQLite with Entity Framework Core
- **UI Framework**: WPF with modern styling
- **Deployment**: PowerShell 5.1+ compatible
- **Domain Support**: Full Active Directory integration

## Support

For deployment issues:
1. Check log files in `C:\Temp\`
2. Review script documentation
3. Use `-TestMode` for dry runs

For development questions:
- Review the existing C# code structure
- Check Entity Framework data models
- Examine the MVVM implementation

## License

This project is provided as-is for gaming table management and deployment.
