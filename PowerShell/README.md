# Coffee Table Gaming System - PowerShell Deployment Suite

A comprehensive PowerShell-based deployment system for managing gaming table installations, updates, and maintenance across a domain network.

## Overview

This deployment suite provides automated installation, daily updates, scheduled maintenance, and network-wide deployment capabilities for gaming table systems. Designed for domain environments with up to 10 gaming tables.

## File Structure

```
PowerShell/
├── Silent-CoffeeTable-Installer-v2.ps1    # Main silent installer
├── DailyAutoUpdate.ps1                     # Enhanced daily update script
├── ScheduledTaskSetup.ps1                  # Scheduled task configuration
├── NetworkDeploy.ps1                       # Network deployment script
└── README.md                              # This documentation
```

## Script Descriptions

### 1. Silent-CoffeeTable-Installer-v2.ps1

**Purpose**: Complete silent installation of gaming system components

**Features**:
- Installs Playnite, RetroArch, and AttractMode
- Configures touchscreen and controller drivers
- Sets up directory structure and permissions
- Configures auto-start on login
- Comprehensive error handling and logging

**Usage**:
```powershell
.\Silent-CoffeeTable-Installer-v2.ps1 [-TestMode]
```

### 2. DailyAutoUpdate.ps1

**Purpose**: Daily maintenance and update operations

**Features**:
- Health monitoring (disk space, memory, network, services)
- Software component updates
- Game asset synchronization
- Touchscreen calibration
- LED lighting control
- Self-repair capabilities

**Usage**:
```powershell
.\DailyAutoUpdate.ps1 [-TestMode] [-ForceUpdate]
```

### 3. ScheduledTaskSetup.ps1

**Purpose**: Configures automated scheduled tasks

**Features**:
- Daily update task (2:00 AM)
- Frontend auto-start on login
- Weekly maintenance (Sunday 3:00 AM)
- Health monitoring (6:00 AM daily)
- Retry logic and error handling

**Usage**:
```powershell
.\ScheduledTaskSetup.ps1 [-TestMode]
```

### 4. NetworkDeploy.ps1

**Purpose**: Network-wide deployment to multiple gaming tables

**Features**:
- Parallel deployment to up to 10 tables
- Progress tracking and status reporting
- Comprehensive error handling
- Credential-based domain authentication
- Deployment validation and verification

**Usage**:
```powershell
.\NetworkDeploy.ps1 [-TestMode] [-Tables @("Table1","Table2")] [-Credential $cred]
```

## Deployment Process

### 1. Preparation
1. Place all deployment files in `\\MasterPC\CoffeeTableUpdates\`
2. Ensure network shares are accessible
3. Verify domain credentials have admin rights on target tables

### 2. Initial Deployment
```powershell
# Run from MasterPC
.\NetworkDeploy.ps1 -Tables @("Table01","Table02","Table03")
```

### 3. Daily Operations
- Scheduled tasks automatically run daily updates at 2:00 AM
- Health monitoring runs at 6:00 AM daily
- Weekly maintenance runs Sundays at 3:00 AM

### 4. Manual Updates
```powershell
# Force update on specific table
Invoke-Command -ComputerName Table01 -ScriptBlock {
    C:\Temp\DailyAutoUpdate.ps1 -ForceUpdate
}
```

## Configuration

### Network Settings
- **Master PC**: `\\MasterPC\CoffeeTableUpdates\`
- **Target Tables**: Table01-Table10 (modify in NetworkDeploy.ps1)
- **Domain**: Requires domain admin credentials

### File Locations
- **Installation Directory**: `C:\Games\`
- **Temporary Files**: `C:\Temp\`
- **Log Files**: `C:\Temp\*.log`

### Scheduled Tasks
- **Daily Update**: 2:00 AM
- **Health Monitor**: 6:00 AM  
- **Weekly Maintenance**: Sunday 3:00 AM
- **Frontend Auto-Start**: User login

## Error Handling

All scripts include comprehensive error handling:
- Try/catch blocks throughout
- Detailed logging with timestamps
- Exit codes for success/failure
- Retry logic for network operations
- Health checks and self-repair

## Logging

Logs are stored in `C:\Temp\` with timestamps:
- `Install_Log_YYYYMMDD_HHMMSS.txt`
- `CoffeeTable_DailyUpdate_Log_YYYYMMDD_HHMMSS.txt`
- `ScheduledTaskSetup_Log_YYYYMMDD_HHMMSS.txt`
- `NetworkDeploy_Log_YYYYMMDD_HHMMSS.txt`

## Security Considerations

- Requires domain administrator privileges
- Uses secure credential handling
- Validates target systems before deployment
- Includes input sanitization
- Follows principle of least privilege

## Performance

- Parallel deployment for multiple tables
- Progress tracking with estimated completion
- Resource monitoring during operations
- Efficient file transfer using Robocopy

## Troubleshooting

### Common Issues

1. **Connectivity Problems**
   - Verify network connectivity
   - Check firewall settings
   - Ensure administrative shares are accessible

2. **Permission Errors**
   - Run scripts as Administrator
   - Verify domain credentials
   - Check share permissions

3. **Installation Failures**
   - Check log files in `C:\Temp\`
   - Verify sufficient disk space
   - Ensure target systems are online

### Log Analysis

Check log files for:
- Error messages with timestamps
- Step-by-step execution details
- Exit codes and status information

## Support

For issues or questions:
1. Check log files in `C:\Temp\`
2. Verify network connectivity
3. Ensure proper credentials
4. Review script parameters

## Version History

- **v2.0** - Complete rewrite with enhanced error handling and domain support
- **v1.0** - Initial release with basic functionality

## License

This deployment system is provided as-is for gaming table management.
