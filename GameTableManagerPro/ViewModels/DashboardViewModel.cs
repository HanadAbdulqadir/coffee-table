using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GameTableManagerPro.Models;
using GameTableManagerPro.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace GameTableManagerPro.ViewModels;

public partial class DashboardViewModel : ObservableObject
{
    private readonly IDatabaseService _databaseService;
    private readonly IPowerShellService _powerShellService;

    [ObservableProperty]
    private string _statusMessage = "Loading dashboard...";

    [ObservableProperty]
    private int _totalTables;

    [ObservableProperty]
    private int _onlineTables;

    [ObservableProperty]
    private int _offlineTables;

    [ObservableProperty]
    private int _tablesNeedingAttention;

    [ObservableProperty]
    private int _totalDeployments;

    [ObservableProperty]
    private int _successfulDeployments;

    [ObservableProperty]
    private int _failedDeployments;

    [ObservableProperty]
    private ObservableCollection<GamingTable> _recentTables = new();

    [ObservableProperty]
    private ObservableCollection<DeploymentHistory> _recentDeployments = new();

    [ObservableProperty]
    private ObservableCollection<HealthMetric> _healthMetrics = new();

    [ObservableProperty]
    private bool _isLoading = true;

    public DashboardViewModel(IDatabaseService databaseService, IPowerShellService powerShellService)
    {
        _databaseService = databaseService;
        _powerShellService = powerShellService;
        InitializeAsync();
    }

    private async void InitializeAsync()
    {
        await LoadDashboardDataAsync();
    }

    [RelayCommand]
    private async Task LoadDashboardDataAsync()
    {
        try
        {
            IsLoading = true;
            StatusMessage = "Loading dashboard data...";

            // Load all data in parallel
            var tablesTask = _databaseService.GetAllTablesAsync();
            var onlineCountTask = _databaseService.GetOnlineTablesCountAsync();
            var offlineCountTask = _databaseService.GetOfflineTablesCountAsync();
            var attentionCountTask = _databaseService.GetTablesNeedingAttentionCountAsync();

            await Task.WhenAll(tablesTask, onlineCountTask, offlineCountTask, attentionCountTask);

            var tables = await tablesTask;
            
            TotalTables = tables.Count;
            OnlineTables = await onlineCountTask;
            OfflineTables = await offlineCountTask;
            TablesNeedingAttention = await attentionCountTask;

            // Calculate deployment statistics
            var allDeployments = tables.SelectMany(t => t.DeploymentHistory).ToList();
            TotalDeployments = allDeployments.Count;
            SuccessfulDeployments = allDeployments.Count(d => d.Status == "Success");
            FailedDeployments = TotalDeployments - SuccessfulDeployments;

            // Load recent data
            RecentTables = new ObservableCollection<GamingTable>(
                tables.OrderByDescending(t => t.LastSeen).Take(5));

            RecentDeployments = new ObservableCollection<DeploymentHistory>(
                allDeployments.OrderByDescending(d => d.StartTime).Take(10));

            // Generate health metrics
            GenerateHealthMetrics(tables);

            StatusMessage = "Dashboard loaded successfully";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error loading dashboard: {ex.Message}";
            MessageBox.Show($"Failed to load dashboard: {ex.Message}", "Error", 
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void GenerateHealthMetrics(List<GamingTable> tables)
    {
        HealthMetrics.Clear();

        // Table status distribution
        HealthMetrics.Add(new HealthMetric
        {
            Title = "Table Status Distribution",
            Value = $"{OnlineTables} Online, {OfflineTables} Offline, {TablesNeedingAttention} Needs Attention",
            Status = OnlineTables > 0 ? "Good" : "Warning",
            Icon = "📊"
        });

        // Deployment success rate
        var successRate = TotalDeployments > 0 ? (SuccessfulDeployments * 100.0 / TotalDeployments) : 100;
        HealthMetrics.Add(new HealthMetric
        {
            Title = "Deployment Success Rate",
            Value = $"{successRate:F1}% ({SuccessfulDeployments}/{TotalDeployments})",
            Status = successRate >= 90 ? "Good" : successRate >= 75 ? "Warning" : "Critical",
            Icon = "🚀"
        });

        // Recent activity
        var recentActivity = tables.Max(t => t.LastSeen);
        var hoursSinceLastActivity = (DateTime.Now - recentActivity).TotalHours;
        HealthMetrics.Add(new HealthMetric
        {
            Title = "Last Activity",
            Value = recentActivity.ToString("g"),
            Status = hoursSinceLastActivity < 24 ? "Good" : "Warning",
            Icon = "⏰"
        });

        // System health
        var healthyTables = tables.Count(t => t.Status == "Online");
        var healthPercentage = TotalTables > 0 ? (healthyTables * 100.0 / TotalTables) : 100;
        HealthMetrics.Add(new HealthMetric
        {
            Title = "System Health",
            Value = $"{healthPercentage:F1}% systems healthy",
            Status = healthPercentage >= 80 ? "Good" : healthPercentage >= 60 ? "Warning" : "Critical",
            Icon = "❤️"
        });
    }

    [RelayCommand]
    private async Task RefreshDashboard()
    {
        await LoadDashboardDataAsync();
    }

    [RelayCommand]
    private async Task DeployToAllTables()
    {
        try
        {
            StatusMessage = "Preparing to deploy to all tables...";
            
            // This would integrate with the NetworkDeploy.ps1 script
            var result = await _powerShellService.ExecuteScriptAsync(
                "../PowerShell/NetworkDeploy.ps1",
                new[] { "-TestMode" }); // Remove -TestMode for actual deployment

            if (result.Success)
            {
                StatusMessage = "Deployment initiated successfully";
                MessageBox.Show("Deployment to all tables started successfully!", 
                    "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                
                // Refresh dashboard after deployment
                await LoadDashboardDataAsync();
            }
            else
            {
                StatusMessage = "Deployment failed";
                MessageBox.Show($"Deployment failed: {result.Error}", 
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        catch (Exception ex)
        {
            StatusMessage = "Deployment error";
            MessageBox.Show($"Deployment error: {ex.Message}", 
                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private void ViewTableDetails(GamingTable table)
    {
        if (table != null)
        {
            StatusMessage = $"Viewing details for {table.Hostname}";
            // Navigation to table details would be implemented here
            MessageBox.Show($"Would navigate to details for {table.Hostname}", 
                "Navigation", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    [RelayCommand]
    private void ViewDeploymentDetails(DeploymentHistory deployment)
    {
        if (deployment != null)
        {
            StatusMessage = "Viewing deployment details";
            // Navigation to deployment details would be implemented here
            MessageBox.Show($"Would show details for deployment at {deployment.StartTime:g}", 
                "Navigation", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}

public class HealthMetric
{
    public string Title { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty; // Good, Warning, Critical
    public string Icon { get; set; } = string.Empty;
}
