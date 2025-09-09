using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GameTableManagerPro.Models;
using GameTableManagerPro.Services;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace GameTableManagerPro.ViewModels;

public partial class HealthMonitorViewModel : ObservableObject
{
    private readonly IDatabaseService _databaseService;
    private readonly IPowerShellService _powerShellService;
    private readonly DispatcherTimer _healthCheckTimer;

    [ObservableProperty]
    private string _statusMessage = "Health monitoring initialized";

    [ObservableProperty]
    private bool _isLoading = false;

    [ObservableProperty]
    private bool _isMonitoringActive = false;

    [ObservableProperty]
    private ObservableCollection<GamingTable> _monitoredTables = new();

    [ObservableProperty]
    private ObservableCollection<HealthCheckResult> _healthCheckResults = new();

    [ObservableProperty]
    private ObservableCollection<HealthAlert> _activeAlerts = new();

    [ObservableProperty]
    private HealthMetrics _systemHealthMetrics = new();

    [ObservableProperty]
    private int _checkIntervalSeconds = 60;

    public HealthMonitorViewModel(IDatabaseService databaseService, IPowerShellService powerShellService)
    {
        _databaseService = databaseService;
        _powerShellService = powerShellService;

        _healthCheckTimer = new DispatcherTimer();
        _healthCheckTimer.Tick += OnHealthCheckTimerTick;
        _healthCheckTimer.Interval = TimeSpan.FromSeconds(CheckIntervalSeconds);

        InitializeAsync();
    }

    private async void InitializeAsync()
    {
        await LoadMonitoringDataAsync();
    }

    [RelayCommand]
    private async Task LoadMonitoringDataAsync()
    {
        try
        {
            IsLoading = true;
            StatusMessage = "Loading health monitoring data...";

            // Load all tables for monitoring
            var tables = await _databaseService.GetAllTablesAsync();
            MonitoredTables = new ObservableCollection<GamingTable>(tables);

            // Load recent health check results
            await LoadRecentHealthResultsAsync();

            StatusMessage = $"Monitoring {MonitoredTables.Count} tables";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error loading monitoring data: {ex.Message}";
            MessageBox.Show($"Failed to load monitoring data: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void ToggleMonitoring()
    {
        if (IsMonitoringActive)
        {
            StopMonitoring();
        }
        else
        {
            StartMonitoring();
        }
    }

    [RelayCommand]
    private void StartMonitoring()
    {
        _healthCheckTimer.Interval = TimeSpan.FromSeconds(CheckIntervalSeconds);
        _healthCheckTimer.Start();
        IsMonitoringActive = true;
        StatusMessage = $"Health monitoring started (interval: {CheckIntervalSeconds}s)";
        
        AddHealthResult(new HealthCheckResult
        {
            Timestamp = DateTime.Now,
            Message = "Health monitoring started",
            Status = HealthStatus.Info
        });
    }

    [RelayCommand]
    private void StopMonitoring()
    {
        _healthCheckTimer.Stop();
        IsMonitoringActive = false;
        StatusMessage = "Health monitoring stopped";
        
        AddHealthResult(new HealthCheckResult
        {
            Timestamp = DateTime.Now,
            Message = "Health monitoring stopped",
            Status = HealthStatus.Info
        });
    }

    [RelayCommand]
    private async Task RunManualHealthCheckAsync()
    {
        try
        {
            StatusMessage = "Running manual health check...";
            await PerformHealthChecksAsync();
            StatusMessage = "Manual health check completed";
        }
        catch (Exception ex)
        {
            StatusMessage = "Manual health check failed";
            MessageBox.Show($"Manual health check failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private async Task CheckSingleTableAsync(GamingTable table)
    {
        if (table == null) return;

        try
        {
            StatusMessage = $"Checking health of {table.Hostname}...";
            var result = await PerformTableHealthCheckAsync(table);
            AddHealthResult(result);
            StatusMessage = $"Health check completed for {table.Hostname}";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Health check failed for {table.Hostname}";
            AddHealthResult(new HealthCheckResult
            {
                Timestamp = DateTime.Now,
                Message = $"Health check failed: {ex.Message}",
                Status = HealthStatus.Error,
                TableName = table.Hostname
            });
        }
    }

    private async void OnHealthCheckTimerTick(object? sender, EventArgs e)
    {
        if (IsMonitoringActive)
        {
            await PerformHealthChecksAsync();
        }
    }

    private async Task PerformHealthChecksAsync()
    {
        try
        {
            AddHealthResult(new HealthCheckResult
            {
                Timestamp = DateTime.Now,
                Message = "Starting automated health checks",
                Status = HealthStatus.Info
            });

            int healthyCount = 0;
            int warningCount = 0;
            int errorCount = 0;

            foreach (var table in MonitoredTables)
            {
                var result = await PerformTableHealthCheckAsync(table);
                AddHealthResult(result);

                switch (result.Status)
                {
                    case HealthStatus.Healthy:
                        healthyCount++;
                        break;
                    case HealthStatus.Warning:
                        warningCount++;
                        break;
                    case HealthStatus.Error:
                        errorCount++;
                        break;
                }

                // Small delay between checks to avoid overwhelming the system
                await Task.Delay(100);
            }

            // Update system health metrics
            SystemHealthMetrics = new HealthMetrics
            {
                TotalTables = MonitoredTables.Count,
                HealthyTables = healthyCount,
                WarningTables = warningCount,
                ErrorTables = errorCount,
                HealthPercentage = MonitoredTables.Count > 0 ? (healthyCount * 100.0 / MonitoredTables.Count) : 100,
                LastUpdated = DateTime.Now
            };

            AddHealthResult(new HealthCheckResult
            {
                Timestamp = DateTime.Now,
                Message = $"Health checks completed: {healthyCount} healthy, {warningCount} warnings, {errorCount} errors",
                Status = healthyCount == MonitoredTables.Count ? HealthStatus.Healthy : 
                         errorCount > 0 ? HealthStatus.Error : HealthStatus.Warning
            });

            // Check for alerts
            CheckForAlerts(healthyCount, warningCount, errorCount);
        }
        catch (Exception ex)
        {
            AddHealthResult(new HealthCheckResult
            {
                Timestamp = DateTime.Now,
                Message = $"Health checks failed: {ex.Message}",
                Status = HealthStatus.Error
            });
        }
    }

    private async Task<HealthCheckResult> PerformTableHealthCheckAsync(GamingTable table)
    {
        try
        {
            // Simulate health check (in real implementation, this would ping the table, check services, etc.)
            var random = new Random();
            var status = random.Next(100) switch
            {
                < 80 => HealthStatus.Healthy,    // 80% chance healthy
                < 95 => HealthStatus.Warning,    // 15% chance warning
                _ => HealthStatus.Error          // 5% chance error
            };

            string message = status switch
            {
                HealthStatus.Healthy => "Table is healthy and responsive",
                HealthStatus.Warning => "Table showing warning signs (high latency)",
                HealthStatus.Error => "Table is unreachable or experiencing issues",
                _ => "Unknown health status"
            };

            // Update table status in database
            table.Status = status.ToString();
            table.LastSeen = DateTime.Now;
            await _databaseService.UpdateTableAsync(table);

            return new HealthCheckResult
            {
                Timestamp = DateTime.Now,
                Message = message,
                Status = status,
                TableName = table.Hostname,
                TableIp = table.IPAddress
            };
        }
        catch (Exception ex)
        {
            return new HealthCheckResult
            {
                Timestamp = DateTime.Now,
                Message = $"Health check error: {ex.Message}",
                Status = HealthStatus.Error,
                TableName = table.Hostname
            };
        }
    }

    private void CheckForAlerts(int healthyCount, int warningCount, int errorCount)
    {
        // Check for system-wide alerts
        if (errorCount > MonitoredTables.Count * 0.3) // More than 30% errors
        {
            AddAlert(new HealthAlert
            {
                Id = Guid.NewGuid(),
                Timestamp = DateTime.Now,
                Message = $"CRITICAL: {errorCount} tables experiencing errors (>30% of system)",
                Severity = AlertSeverity.Critical,
                IsAcknowledged = false
            });
        }
        else if (errorCount > 0)
        {
            AddAlert(new HealthAlert
            {
                Id = Guid.NewGuid(),
                Timestamp = DateTime.Now,
                Message = $"WARNING: {errorCount} tables experiencing errors",
                Severity = AlertSeverity.Warning,
                IsAcknowledged = false
            });
        }

        // Check for health percentage alerts
        if (SystemHealthMetrics.HealthPercentage < 70)
        {
            AddAlert(new HealthAlert
            {
                Id = Guid.NewGuid(),
                Timestamp = DateTime.Now,
                Message = $"CRITICAL: System health at {SystemHealthMetrics.HealthPercentage:F1}%",
                Severity = AlertSeverity.Critical,
                IsAcknowledged = false
            });
        }
        else if (SystemHealthMetrics.HealthPercentage < 85)
        {
            AddAlert(new HealthAlert
            {
                Id = Guid.NewGuid(),
                Timestamp = DateTime.Now,
                Message = $"WARNING: System health at {SystemHealthMetrics.HealthPercentage:F1}%",
                Severity = AlertSeverity.Warning,
                IsAcknowledged = false
            });
        }
    }

    [RelayCommand]
    private void AcknowledgeAlert(HealthAlert alert)
    {
        if (alert != null)
        {
            alert.IsAcknowledged = true;
            ActiveAlerts.Remove(alert);
            StatusMessage = $"Alert acknowledged: {alert.Message}";
        }
    }

    [RelayCommand]
    private void AcknowledgeAllAlerts()
    {
        foreach (var alert in ActiveAlerts)
        {
            alert.IsAcknowledged = true;
        }
        ActiveAlerts.Clear();
        StatusMessage = "All alerts acknowledged";
    }

    [RelayCommand]
    private void ClearHealthResults()
    {
        HealthCheckResults.Clear();
        StatusMessage = "Health results cleared";
    }

    private async Task LoadRecentHealthResultsAsync()
    {
        // Simulate loading recent results (in real implementation, this would query the database)
        await Task.Delay(100); // Simulate async operation
        
        HealthCheckResults.Clear();
        
        // Add some sample recent results
        AddHealthResult(new HealthCheckResult
        {
            Timestamp = DateTime.Now.AddMinutes(-5),
            Message = "System health check completed",
            Status = HealthStatus.Info
        });
    }

    private void AddHealthResult(HealthCheckResult result)
    {
        HealthCheckResults.Insert(0, result); // Add to beginning for reverse chronological order
        
        // Keep only the last 100 results
        while (HealthCheckResults.Count > 100)
        {
            HealthCheckResults.RemoveAt(HealthCheckResults.Count - 1);
        }
    }

    private void AddAlert(HealthAlert alert)
    {
        // Check if similar alert already exists
        var existingAlert = ActiveAlerts.Find(a => 
            a.Message == alert.Message && 
            a.Severity == alert.Severity && 
            !a.IsAcknowledged);

        if (existingAlert == null)
        {
            ActiveAlerts.Insert(0, alert);
            
            // Show notification for critical alerts
            if (alert.Severity == AlertSeverity.Critical)
            {
                MessageBox.Show(alert.Message, "Critical Alert", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
}

public class HealthCheckResult
{
    public DateTime Timestamp { get; set; }
    public required string Message { get; set; }
    public HealthStatus Status { get; set; }
    public string? TableName { get; set; }
    public string? TableIp { get; set; }
}

public class HealthAlert
{
    public Guid Id { get; set; }
    public DateTime Timestamp { get; set; }
    public required string Message { get; set; }
    public AlertSeverity Severity { get; set; }
    public bool IsAcknowledged { get; set; }
}

public class HealthMetrics
{
    public int TotalTables { get; set; }
    public int HealthyTables { get; set; }
    public int WarningTables { get; set; }
    public int ErrorTables { get; set; }
    public double HealthPercentage { get; set; }
    public DateTime LastUpdated { get; set; }
}

public enum HealthStatus
{
    Info,
    Healthy,
    Warning,
    Error
}

public enum AlertSeverity
{
    Info,
    Warning,
    Critical
}
