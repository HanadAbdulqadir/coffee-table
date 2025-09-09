# GameTable Manager Pro - Deployment Guide

## Overview

GameTable Manager Pro is a comprehensive Windows application for managing gaming table deployments. This guide covers the complete deployment process, from building the application to production deployment.

## Table of Contents

1. [System Requirements](#system-requirements)
2. [Build Process](#build-process)
3. [Testing](#testing)
4. [Packaging](#packaging)
5. [Installation](#installation)
6. [Configuration](#configuration)
7. [Deployment Scripts](#deployment-scripts)
8. [Troubleshooting](#troubleshooting)

## System Requirements

### Development Environment
- **Operating System**: Windows 10/11
- **.NET SDK**: 8.0 or later
- **IDE**: Visual Studio 2022 or VS Code
- **PowerShell**: 5.1 or later
- **Git**: For version control

### Production Environment
- **Operating System**: Windows 10/11
- **.NET Runtime**: 8.0 or later
- **PowerShell**: 5.1 or later
- **Storage**: 500MB free space
- **Memory**: 4GB RAM minimum
- **Network**: For multi-table deployments

## Build Process

### Manual Build
```bash
# Clone the repository
git clone https://github.com/HanadAbdulqadir/coffee-table.git
cd coffee-table

# Restore NuGet packages
dotnet restore

# Build the solution
dotnet build --configuration Release

# Publish the application
dotnet publish GameTableManagerPro.sln --configuration Release --output ./publish
```

### Automated Build
Use the deployment script:
```powershell
.\Deploy-GameTableManagerPro.ps1 -Configuration Release -CreateInstaller
```

### Build Output
The build process generates:
- `GameTableManagerPro.exe` - Main executable
- `*.dll` - Application dependencies
- `*.json` - Configuration files
- `PowerShell/` - Deployment scripts

## Testing

### Unit Tests
```bash
# Run all tests
dotnet test

# Run specific test project
dotnet test GameTableManagerPro.Tests
```

### Test Categories
- **Service Tests**: Database and PowerShell services
- **ViewModel Tests**: MVVM pattern validation
- **Integration Tests**: Module interaction testing
- **UI Tests**: User interface validation

### Test Coverage
- Database operations: CRUD, migrations, backups
- PowerShell integration: Script execution, error handling
- Navigation: View switching and state management
- Configuration: Settings persistence and validation

## Packaging

### Deployment Package
The deployment package includes:
- Application binaries and dependencies
- PowerShell deployment scripts
- Documentation and user guides
- Configuration templates

### Creating Packages
```powershell
# Create basic package
.\Deploy-GameTableManagerPro.ps1 -Configuration Release

# Create package with installer
.\Deploy-GameTableManagerPro.ps1 -Configuration Release -CreateInstaller

# Prepare for GitHub release
.\Deploy-GameTableManagerPro.ps1 -Configuration Release -DeployToGitHub
```

### Package Structure
```
Deploy/
├── Build/          # Compiled binaries
├── Package/        # Deployment package
├── Installer/      # Installer package
└── Release/        # GitHub release package
```

## Installation

### Silent Installation
```powershell
# Run silent installer
.\Install.ps1 -Silent

# Custom installation path
.\Install.ps1 -InstallPath "D:\Applications\GameTableManagerPro"
```

### Manual Installation
1. Extract the deployment package
2. Run `GameTableManagerPro.exe`
3. Configure application settings
4. Set up database connection

### Post-Installation
- Configure database connection strings
- Set up asset storage paths
- Configure user accounts and permissions
- Set up automated backup schedules

## Configuration

### Application Settings
Located in `appsettings.json`:
```json
{
  "Database": {
    "ConnectionString": "Data Source=GameTableManagerPro.db"
  },
  "Notifications": {
    "Enabled": true,
    "Email": "admin@example.com"
  },
  "Deployment": {
    "DefaultTemplate": "Standard",
    "BackupEnabled": true
  }
}
```

### Database Configuration
- **SQLite**: Default local database
- **SQL Server**: Enterprise deployments
- **Backup Location**: Configured backup directory
- **Migration Strategy**: Automatic or manual

### User Management
- **Administrator**: Full system access
- **Operator**: Limited deployment access
- **Audit Logging**: User activity tracking
- **Role-Based Access**: Permission management

## Deployment Scripts

### PowerShell Scripts Overview

#### 1. Silent-CoffeeTable-Installer-v2.ps1
- Silent installation of gaming table software
- Installs Playnite, emulators, and drivers
- Configures auto-start and touch calibration

#### 2. DailyAutoUpdate.ps1
- Automated daily maintenance
- Software updates and asset synchronization
- Health checks and performance monitoring

#### 3. ScheduledTaskSetup.ps1
- Creates automated scheduled tasks
- Configures daily update schedules
- Sets up maintenance windows

#### 4. NetworkDeploy.ps1
- Multi-table network deployment
- Bulk configuration management
- Centralized update distribution

### Script Usage Examples

```powershell
# Deploy to single table
.\NetworkDeploy.ps1 -Tables "Table01"

# Deploy to multiple tables
.\NetworkDeploy.ps1 -Tables "Table01", "Table02", "Table03"

# Force update on all tables
.\NetworkDeploy.ps1 -Tables (Get-Content .\tables.txt) -ForceUpdate
```

### Script Configuration
Edit script parameters for:
- Target table hostnames/IP addresses
- Deployment package locations
- Update schedules and intervals
- Notification settings

## Troubleshooting

### Common Issues

#### Build Errors
- **Missing .NET SDK**: Install .NET 8.0 SDK
- **NuGet package restore**: Run `dotnet restore`
- **Reference errors**: Check project dependencies

#### Runtime Errors
- **Database connection**: Verify SQLite file permissions
- **PowerShell execution**: Enable script execution policy
- **File access**: Check directory permissions

#### Deployment Issues
- **Network connectivity**: Verify target table accessibility
- **Firewall rules**: Allow PowerShell remoting
- **Credentials**: Use appropriate authentication

### Log Files
- **Application Logs**: `%APPDATA%\GameTableManagerPro\logs`
- **Deployment Logs**: `C:\Temp\Deploy_Log_*.txt`
- **Database Logs**: SQLite transaction logs

### Support Resources
- GitHub Issues: https://github.com/HanadAbdulqadir/coffee-table/issues
- Documentation: Project README and this guide
- Community Support: GitHub discussions

## Maintenance

### Regular Tasks
- Database backups (automated or manual)
- Log file rotation and cleanup
- Software updates and patching
- Performance monitoring

### Backup Strategy
- Daily automated backups
- Monthly full backups
- Off-site backup storage
- Backup verification procedures

### Update Procedure
1. Test updates in development environment
2. Backup production database
3. Deploy updated application
4. Verify functionality
5. Update documentation

## Security Considerations

### Access Control
- Role-based user permissions
- Secure credential storage
- Audit logging of all operations
- Regular security reviews

### Network Security
- Secure PowerShell remoting
- Firewall configuration
- Network segmentation
- Encrypted communications

### Data Protection
- Database encryption
- Secure file storage
- Backup encryption
- Access logging

## Performance Optimization

### Database Optimization
- Regular index maintenance
- Query optimization
- Connection pooling
- Cache configuration

### Application Performance
- Async/await patterns
- Memory management
- UI virtualization
- Background processing

### Monitoring
- Performance counters
- Resource usage tracking
- Response time monitoring
- Error rate tracking

## Appendix

### File Structure
```
GameTableManagerPro/
├── Models/           # Data models
├── ViewModels/       # MVVM view models
├── Views/            # User interface
├── Services/         # Business logic
├── Data/             # Database context
├── Converters/       # WPF value converters
└── PowerShell/       # Deployment scripts
```

### Dependencies
- CommunityToolkit.Mvvm
- Microsoft.EntityFrameworkCore.Sqlite
- Microsoft.Extensions.DependencyInjection
- System.Management.Automation

### Version History
- v1.0.0: Initial release with all core features
- Future: Planned enhancements and updates

---

For additional support, refer to the GitHub repository or create an issue for specific problems.
