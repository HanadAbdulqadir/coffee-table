using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GameTableManagerPro.Services;
using System.Threading.Tasks;
using System.Windows;

namespace GameTableManagerPro.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    private readonly IDatabaseService _databaseService;
    private readonly IPowerShellService _powerShellService;

    [ObservableProperty]
    private string _title = "GameTable Manager Pro";

    [ObservableProperty]
    private string _statusMessage = "Ready";

    [ObservableProperty]
    private int _onlineTablesCount;

    [ObservableProperty]
    private int _offlineTablesCount;

    [ObservableProperty]
    private int _tablesNeedingAttentionCount;

    [ObservableProperty]
    private bool _isDatabaseConnected;

    public MainWindowViewModel(IDatabaseService databaseService, IPowerShellService powerShellService)
    {
        _databaseService = databaseService;
        _powerShellService = powerShellService;
        InitializeAsync();
    }

    private async void InitializeAsync()
    {
        await LoadDashboardStatsAsync();
    }

    private async Task LoadDashboardStatsAsync()
    {
        try
        {
            StatusMessage = "Loading dashboard statistics...";
            
            IsDatabaseConnected = await _databaseService.TestConnectionAsync();
            
            if (IsDatabaseConnected)
            {
                OnlineTablesCount = await _databaseService.GetOnlineTablesCountAsync();
                OfflineTablesCount = await _databaseService.GetOfflineTablesCountAsync();
                TablesNeedingAttentionCount = await _databaseService.GetTablesNeedingAttentionCountAsync();
                
                StatusMessage = $"Dashboard loaded - {OnlineTablesCount} online, {OfflineTablesCount} offline";
            }
            else
            {
                StatusMessage = "Database connection failed";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error loading dashboard: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task RefreshDashboard()
    {
        await LoadDashboardStatsAsync();
    }

    [RelayCommand]
    private void ShowDashboard()
    {
        StatusMessage = "Showing Dashboard";
        // Navigation logic will be implemented here
    }

    [RelayCommand]
    private void ShowTableManagement()
    {
        StatusMessage = "Showing Table Management";
        // Navigation logic will be implemented here
    }

    [RelayCommand]
    private void ShowDeployment()
    {
        StatusMessage = "Showing Deployment";
        // Navigation logic will be implemented here
    }

    [RelayCommand]
    private void ShowHealthMonitoring()
    {
        StatusMessage = "Showing Health Monitoring";
        // Navigation logic will be implemented here
    }

    [RelayCommand]
    private void ShowSettings()
    {
        StatusMessage = "Showing Settings";
        // Navigation logic will be implemented here
    }

    [RelayCommand]
    private async Task TestPowerShellConnection()
    {
        try
        {
            StatusMessage = "Testing PowerShell connection...";
            var result = await _powerShellService.ExecuteCommandAsync("Get-Process -Name powershell | Select-Object -First 1");
            
            if (result.Success)
            {
                StatusMessage = "PowerShell connection successful";
                MessageBox.Show("PowerShell is working correctly!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                StatusMessage = "PowerShell test failed";
                MessageBox.Show($"PowerShell test failed: {result.Error}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        catch (Exception ex)
        {
            StatusMessage = "PowerShell test error";
            MessageBox.Show($"PowerShell test error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private async Task BackupDatabase()
    {
        try
        {
            StatusMessage = "Creating database backup...";
            var backupPath = $"GameTableManagerPro_Backup_{DateTime.Now:yyyyMMdd_HHmmss}.db";
            
            if (await _databaseService.BackupDatabaseAsync(backupPath))
            {
                StatusMessage = "Database backup created successfully";
                MessageBox.Show($"Database backed up to: {backupPath}", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                StatusMessage = "Database backup failed";
                MessageBox.Show("Database backup failed", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        catch (Exception ex)
        {
            StatusMessage = "Backup error";
            MessageBox.Show($"Backup error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
