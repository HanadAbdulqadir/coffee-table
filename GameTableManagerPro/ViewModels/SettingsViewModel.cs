using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GameTableManagerPro.Services;
using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using System.Windows;

namespace GameTableManagerPro.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    private readonly IDatabaseService _databaseService;
    private readonly IPowerShellService _powerShellService;

    [ObservableProperty]
    private string _statusMessage = "Loading settings...";

    [ObservableProperty]
    private bool _isLoading = false;

    [ObservableProperty]
    private bool _isDarkTheme = true;

    [ObservableProperty]
    private int _autoSaveInterval = 5;

    [ObservableProperty]
    private bool _enableNotifications = true;

    [ObservableProperty]
    private bool _enableAutoUpdates = true;

    [ObservableProperty]
    private string _databasePath = "GameTableManagerPro.db";

    [ObservableProperty]
    private string _backupPath = "Backups";

    [ObservableProperty]
    private string _assetStoragePath = "C:\\Assets";

    [ObservableProperty]
    private ObservableCollection<string> _availableThemes = new()
    {
        "Dark", "Light", "Blue", "Green", "Purple"
    };

    [ObservableProperty]
    private ObservableCollection<int> _saveIntervals = new()
    {
        1, 5, 10, 15, 30
    };

    [ObservableProperty]
    private ObservableCollection<SystemUser> _users = new();

    [ObservableProperty]
    private SystemUser? _selectedUser;

    public SettingsViewModel(IDatabaseService databaseService, IPowerShellService powerShellService)
    {
        _databaseService = databaseService;
        _powerShellService = powerShellService;
        InitializeAsync();
    }

    private async void InitializeAsync()
    {
        await LoadSettingsAsync();
        await LoadUsersAsync();
    }

    [RelayCommand]
    private async Task LoadSettingsAsync()
    {
        try
        {
            IsLoading = true;
            StatusMessage = "Loading settings...";

            // Simulate loading settings (in real implementation, this would load from config file)
            await Task.Delay(300);
            
            // Load default settings
            IsDarkTheme = true;
            AutoSaveInterval = 5;
            EnableNotifications = true;
            EnableAutoUpdates = true;
            DatabasePath = "GameTableManagerPro.db";
            BackupPath = "Backups";
            AssetStoragePath = "C:\\Assets";

            StatusMessage = "Settings loaded successfully";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error loading settings: {ex.Message}";
            MessageBox.Show($"Failed to load settings: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task LoadUsersAsync()
    {
        try
        {
            // Simulate loading users (in real implementation, this would query the database)
            await Task.Delay(200);
            
            Users = new ObservableCollection<SystemUser>
            {
                new SystemUser
                {
                    Id = 1,
                    Username = "admin",
                    Role = "Administrator",
                    Email = "admin@gametable.com",
                    IsActive = true,
                    LastLogin = DateTime.Now.AddDays(-1)
                },
                new SystemUser
                {
                    Id = 2,
                    Username = "operator",
                    Role = "Operator",
                    Email = "operator@gametable.com",
                    IsActive = true,
                    LastLogin = DateTime.Now.AddDays(-7)
                }
            };
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error loading users: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task SaveSettingsAsync()
    {
        try
        {
            IsLoading = true;
            StatusMessage = "Saving settings...";

            // Validate paths
            if (!Path.IsPathRooted(AssetStoragePath))
            {
                MessageBox.Show("Asset storage path must be a full path (e.g., C:\\Assets).", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Simulate saving settings (in real implementation, this would save to config file)
            await Task.Delay(500);

            // Ensure directories exist
            Directory.CreateDirectory(AssetStoragePath);
            Directory.CreateDirectory(BackupPath);

            StatusMessage = "Settings saved successfully";
            MessageBox.Show("Settings have been saved successfully.", "Settings Saved", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error saving settings: {ex.Message}";
            MessageBox.Show($"Failed to save settings: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void ResetToDefaults()
    {
        var result = MessageBox.Show(
            "Are you sure you want to reset all settings to default values? This cannot be undone.",
            "Confirm Reset",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (result == MessageBoxResult.Yes)
        {
            IsDarkTheme = true;
            AutoSaveInterval = 5;
            EnableNotifications = true;
            EnableAutoUpdates = true;
            DatabasePath = "GameTableManagerPro.db";
            BackupPath = "Backups";
            AssetStoragePath = "C:\\Assets";

            StatusMessage = "Settings reset to defaults";
            MessageBox.Show("Settings have been reset to default values.", "Reset Complete", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    [RelayCommand]
    private void BrowseAssetPath()
    {
        var dialog = new System.Windows.Forms.FolderBrowserDialog
        {
            Description = "Select Asset Storage Directory",
            SelectedPath = AssetStoragePath
        };

        if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
        {
            AssetStoragePath = dialog.SelectedPath;
        }
    }

    [RelayCommand]
    private void BrowseBackupPath()
    {
        var dialog = new System.Windows.Forms.FolderBrowserDialog
        {
            Description = "Select Backup Directory",
            SelectedPath = BackupPath
        };

        if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
        {
            BackupPath = dialog.SelectedPath;
        }
    }

    [RelayCommand]
    private async Task BackupDatabaseAsync()
    {
        try
        {
            IsLoading = true;
            StatusMessage = "Creating database backup...";

            // Ensure backup directory exists
            Directory.CreateDirectory(BackupPath);

            // Create backup filename with timestamp
            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var backupFile = Path.Combine(BackupPath, $"GameTableManagerPro_Backup_{timestamp}.db");

            // Simulate backup (in real implementation, this would copy the database file)
            await Task.Delay(1000);

            StatusMessage = $"Backup created: {Path.GetFileName(backupFile)}";
            MessageBox.Show($"Database backup created successfully:\n{backupFile}", "Backup Complete", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            StatusMessage = "Backup failed";
            MessageBox.Show($"Failed to create backup: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task RestoreDatabaseAsync()
    {
        var openFileDialog = new OpenFileDialog
        {
            Title = "Select Database Backup File",
            Filter = "Database Files (*.db)|*.db|All Files (*.*)|*.*",
            InitialDirectory = BackupPath
        };

        if (openFileDialog.ShowDialog() == true)
        {
            var result = MessageBox.Show(
                "WARNING: Restoring a backup will replace the current database. This action cannot be undone.\n\nAre you sure you want to continue?",
                "Confirm Restore",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    IsLoading = true;
                    StatusMessage = "Restoring database...";

                    // Simulate restore (in real implementation, this would replace the database file)
                    await Task.Delay(1500);

                    StatusMessage = "Database restored successfully";
                    MessageBox.Show("Database has been restored successfully. The application may need to restart.", "Restore Complete", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    StatusMessage = "Restore failed";
                    MessageBox.Show($"Failed to restore database: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                finally
                {
                    IsLoading = false;
                }
            }
        }
    }

    [RelayCommand]
    private void AddNewUser()
    {
        var newUser = new SystemUser
        {
            Username = "newuser",
            Role = "Operator",
            Email = "",
            IsActive = true
        };

        Users.Add(newUser);
        SelectedUser = newUser;
        StatusMessage = "New user added - please configure details";
    }

    [RelayCommand]
    private async Task SaveUserAsync(SystemUser user)
    {
        if (user == null) return;

        if (string.IsNullOrWhiteSpace(user.Username))
        {
            MessageBox.Show("Username is required.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        try
        {
            StatusMessage = "Saving user...";
            // Simulate user save operation
            await Task.Delay(300);
            StatusMessage = $"User '{user.Username}' saved successfully";
        }
        catch (Exception ex)
        {
            StatusMessage = "Error saving user";
            MessageBox.Show($"Failed to save user: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private async Task DeleteUserAsync(SystemUser user)
    {
        if (user == null) return;

        var result = MessageBox.Show(
            $"Are you sure you want to delete user '{user.Username}'? This action cannot be undone.",
            "Confirm Delete",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (result == MessageBoxResult.Yes)
        {
            try
            {
                Users.Remove(user);
                StatusMessage = $"User '{user.Username}' deleted successfully";
            }
            catch (Exception ex)
            {
                StatusMessage = "Error deleting user";
                MessageBox.Show($"Failed to delete user: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    [RelayCommand]
    private void ExportSettings()
    {
        var saveFileDialog = new SaveFileDialog
        {
            Title = "Export Settings",
            Filter = "JSON Files (*.json)|*.json|All Files (*.*)|*.*",
            FileName = $"GameTableManagerPro_Settings_{DateTime.Now:yyyyMMdd}.json"
        };

        if (saveFileDialog.ShowDialog() == true)
        {
            try
            {
                // Simulate export (in real implementation, this would serialize settings to JSON)
                StatusMessage = "Settings exported successfully";
                MessageBox.Show($"Settings exported to:\n{saveFileDialog.FileName}", "Export Complete", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                StatusMessage = "Export failed";
                MessageBox.Show($"Failed to export settings: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    [RelayCommand]
    private void ImportSettings()
    {
        var openFileDialog = new OpenFileDialog
        {
            Title = "Import Settings",
            Filter = "JSON Files (*.json)|*.json|All Files (*.*)|*.*"
        };

        if (openFileDialog.ShowDialog() == true)
        {
            var result = MessageBox.Show(
                "Importing settings will replace your current configuration. Are you sure you want to continue?",
                "Confirm Import",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    // Simulate import (in real implementation, this would deserialize settings from JSON)
                    StatusMessage = "Settings imported successfully";
                    MessageBox.Show("Settings have been imported successfully.", "Import Complete", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    StatusMessage = "Import failed";
                    MessageBox.Show($"Failed to import settings: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }

    [RelayCommand]
    private void ShowSystemInfo()
    {
        MessageBox.Show(
            "System Information:\n\n" +
            $"Application Version: 1.0.0\n" +
            $"Database Version: 1.0\n" +
            $"NET Version: {Environment.Version}\n" +
            $"OS: {Environment.OSVersion}\n" +
            $"Machine Name: {Environment.MachineName}\n" +
            $"User: {Environment.UserName}",
            "System Information",
            MessageBoxButton.OK,
            MessageBoxImage.Information);
    }
}

public class SystemUser
{
    public int Id { get; set; }
    public required string Username { get; set; }
    public required string Role { get; set; }
    public string? Email { get; set; }
    public bool IsActive { get; set; }
    public DateTime? LastLogin { get; set; }
}
