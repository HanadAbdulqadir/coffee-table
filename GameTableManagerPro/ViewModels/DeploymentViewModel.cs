using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GameTableManagerPro.Models;
using GameTableManagerPro.Services;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;

namespace GameTableManagerPro.ViewModels;

public partial class DeploymentViewModel : ObservableObject
{
    private readonly IDatabaseService _databaseService;
    private readonly IPowerShellService _powerShellService;
    private readonly INavigationService _navigationService;

    [ObservableProperty]
    private string _statusMessage = "Ready";

    [ObservableProperty]
    private bool _isLoading = false;

    [ObservableProperty]
    private ObservableCollection<GamingTable> _availableTables = new();

    [ObservableProperty]
    private ObservableCollection<GamingTable> _selectedTables = new();

    [ObservableProperty]
    private ObservableCollection<DeploymentTemplate> _deploymentTemplates = new();

    [ObservableProperty]
    private DeploymentTemplate? _selectedTemplate;

    [ObservableProperty]
    private DeploymentStep _currentStep = DeploymentStep.TableSelection;

    [ObservableProperty]
    private bool _isDeploymentInProgress = false;

    [ObservableProperty]
    private string _deploymentProgress = "0%";

    [ObservableProperty]
    private ObservableCollection<DeploymentLogEntry> _deploymentLogs = new();

    public DeploymentViewModel(IDatabaseService databaseService, IPowerShellService powerShellService, INavigationService navigationService)
    {
        _databaseService = databaseService;
        _powerShellService = powerShellService;
        _navigationService = navigationService;
        InitializeAsync();
    }

    private async void InitializeAsync()
    {
        await LoadDeploymentDataAsync();
    }

    [RelayCommand]
    private async Task LoadDeploymentDataAsync()
    {
        try
        {
            IsLoading = true;
            StatusMessage = "Loading deployment data...";

            // Load available tables
            var tables = await _databaseService.GetAllTablesAsync();
            AvailableTables = new ObservableCollection<GamingTable>(tables);

            // Load deployment templates
            DeploymentTemplates = new ObservableCollection<DeploymentTemplate>
            {
                new DeploymentTemplate { Id = 1, Name = "Full System Deployment", Description = "Complete system installation with all components", ScriptPath = "Silent-CoffeeTable-Installer-v2.ps1" },
                new DeploymentTemplate { Id = 2, Name = "Software Update Only", Description = "Update existing software components", ScriptPath = "DailyAutoUpdate.ps1" },
                new DeploymentTemplate { Id = 3, Name = "Configuration Update", Description = "Update system configuration only", ScriptPath = "NetworkDeploy.ps1" },
                new DeploymentTemplate { Id = 4, Name = "Custom Deployment", Description = "Custom deployment script", ScriptPath = "" }
            };

            SelectedTemplate = DeploymentTemplates[0];
            StatusMessage = "Deployment data loaded";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error loading deployment data: {ex.Message}";
            MessageBox.Show($"Failed to load deployment data: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void SelectAllTables()
    {
        SelectedTables = new ObservableCollection<GamingTable>(AvailableTables);
        StatusMessage = $"Selected all {AvailableTables.Count} tables";
    }

    [RelayCommand]
    private void ClearSelection()
    {
        SelectedTables.Clear();
        StatusMessage = "Selection cleared";
    }

    [RelayCommand]
    private void AddTableToSelection(GamingTable table)
    {
        if (table != null && !SelectedTables.Contains(table))
        {
            SelectedTables.Add(table);
            StatusMessage = $"Added {table.Hostname} to selection";
        }
    }

    [RelayCommand]
    private void RemoveTableFromSelection(GamingTable table)
    {
        if (table != null && SelectedTables.Contains(table))
        {
            SelectedTables.Remove(table);
            StatusMessage = $"Removed {table.Hostname} from selection";
        }
    }

    [RelayCommand]
    private void NextStep()
    {
        if (CurrentStep == DeploymentStep.TableSelection && SelectedTables.Count == 0)
        {
            MessageBox.Show("Please select at least one table to deploy to.", "Selection Required", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (CurrentStep == DeploymentStep.TemplateSelection && SelectedTemplate == null)
        {
            MessageBox.Show("Please select a deployment template.", "Template Required", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        CurrentStep = CurrentStep switch
        {
            DeploymentStep.TableSelection => DeploymentStep.TemplateSelection,
            DeploymentStep.TemplateSelection => DeploymentStep.Review,
            DeploymentStep.Review => DeploymentStep.Progress,
            _ => CurrentStep
        };

        StatusMessage = $"Moved to {CurrentStep} step";
    }

    [RelayCommand]
    private void PreviousStep()
    {
        CurrentStep = CurrentStep switch
        {
            DeploymentStep.TemplateSelection => DeploymentStep.TableSelection,
            DeploymentStep.Review => DeploymentStep.TemplateSelection,
            DeploymentStep.Progress => DeploymentStep.Review,
            _ => CurrentStep
        };

        StatusMessage = $"Moved to {CurrentStep} step";
    }

    [RelayCommand]
    private async Task StartDeploymentAsync()
    {
        if (SelectedTables.Count == 0 || SelectedTemplate == null)
        {
            MessageBox.Show("Please select tables and a template before starting deployment.", "Configuration Required", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        try
        {
            IsDeploymentInProgress = true;
            CurrentStep = DeploymentStep.Progress;
            StatusMessage = "Starting deployment...";

            DeploymentLogs.Clear();
            AddLogEntry("Deployment started", LogLevel.Info);

            // Simulate deployment progress (in real implementation, this would integrate with PowerShell service)
            for (int i = 0; i <= 100; i += 10)
            {
                if (!IsDeploymentInProgress) break;

                DeploymentProgress = $"{i}%";
                AddLogEntry($"Deployment progress: {i}%", LogLevel.Info);
                
                await Task.Delay(500); // Simulate work

                // Update progress for each table
                foreach (var table in SelectedTables)
                {
                    AddLogEntry($"Deploying to {table.Hostname}...", LogLevel.Info);
                    await Task.Delay(100);
                }
            }

            if (IsDeploymentInProgress)
            {
                AddLogEntry("Deployment completed successfully", LogLevel.Success);
                StatusMessage = "Deployment completed";
                MessageBox.Show("Deployment completed successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                AddLogEntry("Deployment cancelled", LogLevel.Warning);
                StatusMessage = "Deployment cancelled";
            }
        }
        catch (Exception ex)
        {
            AddLogEntry($"Deployment failed: {ex.Message}", LogLevel.Error);
            StatusMessage = "Deployment failed";
            MessageBox.Show($"Deployment failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsDeploymentInProgress = false;
        }
    }

    [RelayCommand]
    private void CancelDeployment()
    {
        IsDeploymentInProgress = false;
        AddLogEntry("Deployment cancelled by user", LogLevel.Warning);
        StatusMessage = "Deployment cancelled";
    }

    [RelayCommand]
    private void ViewDeploymentHistory()
    {
        _navigationService.NavigateTo("DeploymentHistory");
        StatusMessage = "Viewing deployment history";
    }

    [RelayCommand]
    private async Task TestDeploymentScriptAsync()
    {
        if (SelectedTemplate == null)
        {
            MessageBox.Show("Please select a deployment template first.", "Template Required", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        try
        {
            StatusMessage = "Testing deployment script...";
            var result = await _powerShellService.ExecuteScriptAsync(SelectedTemplate.ScriptPath, new[] { "-TestMode" });

            if (result.Success)
            {
                StatusMessage = "Deployment script test successful";
                MessageBox.Show("Deployment script test completed successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                StatusMessage = "Deployment script test failed";
                MessageBox.Show($"Deployment script test failed: {result.Error}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        catch (Exception ex)
        {
            StatusMessage = "Deployment script test error";
            MessageBox.Show($"Deployment script test error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void AddLogEntry(string message, LogLevel level)
    {
        DeploymentLogs.Add(new DeploymentLogEntry
        {
            Timestamp = DateTime.Now,
            Message = message,
            Level = level
        });
    }
}

public enum DeploymentStep
{
    TableSelection,
    TemplateSelection,
    Review,
    Progress
}

public class DeploymentTemplate
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string Description { get; set; }
    public required string ScriptPath { get; set; }
}

public class DeploymentLogEntry
{
    public DateTime Timestamp { get; set; }
    public required string Message { get; set; }
    public LogLevel Level { get; set; }
}

public enum LogLevel
{
    Info,
    Success,
    Warning,
    Error
}
