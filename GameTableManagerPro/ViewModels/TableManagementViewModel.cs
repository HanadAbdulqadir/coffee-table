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

public partial class TableManagementViewModel : ObservableObject
{
    private readonly IDatabaseService _databaseService;
    private readonly INavigationService _navigationService;

    [ObservableProperty]
    private string _statusMessage = "Loading table data...";

    [ObservableProperty]
    private bool _isLoading = false;

    [ObservableProperty]
    private ObservableCollection<GamingTable> _allTables = new();

    [ObservableProperty]
    private ObservableCollection<GamingTable> _filteredTables = new();

    [ObservableProperty]
    private GamingTable? _selectedTable;

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private string _statusFilter = "All";

    [ObservableProperty]
    private bool _showEditPanel = false;

    [ObservableProperty]
    private bool _isEditing = false;

    [ObservableProperty]
    private GamingTable _editTable = new();

    [ObservableProperty]
    private ObservableCollection<string> _availableStatuses = new()
    {
        "All", "Online", "Offline", "Needs Attention", "Deploying"
    };

    public TableManagementViewModel(IDatabaseService databaseService, INavigationService navigationService)
    {
        _databaseService = databaseService;
        _navigationService = navigationService;
        InitializeAsync();
    }

    private async void InitializeAsync()
    {
        await LoadTablesAsync();
    }

    [RelayCommand]
    private async Task LoadTablesAsync()
    {
        try
        {
            IsLoading = true;
            StatusMessage = "Loading tables...";

            var tables = await _databaseService.GetAllTablesAsync();
            AllTables = new ObservableCollection<GamingTable>(tables);
            ApplyFilters();

            StatusMessage = $"Loaded {AllTables.Count} tables";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error loading tables: {ex.Message}";
            MessageBox.Show($"Failed to load tables: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void ApplyFilters()
    {
        var query = AllTables.AsEnumerable();

        // Apply search filter
        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            query = query.Where(t =>
                t.Hostname.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                t.IPAddress.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                (t.HardwareInfo ?? "").Contains(SearchText, StringComparison.OrdinalIgnoreCase));
        }

        // Apply status filter
        if (StatusFilter != "All")
        {
            query = query.Where(t => t.Status == StatusFilter);
        }

        FilteredTables = new ObservableCollection<GamingTable>(query);
        StatusMessage = $"Showing {FilteredTables.Count} of {AllTables.Count} tables";
    }

    [RelayCommand]
    private void ClearFilters()
    {
        SearchText = string.Empty;
        StatusFilter = "All";
        ApplyFilters();
        StatusMessage = "Filters cleared";
    }

    [RelayCommand]
    private void AddNewTable()
    {
        EditTable = new GamingTable
        {
            Hostname = "NewTable",
            IPAddress = "192.168.1.100",
            Status = "Online",
            Version = "1.0.0",
            LastSeen = DateTime.Now,
            HardwareInfo = "CPU: i5, RAM: 16GB, GPU: RTX 3060"
        };
        IsEditing = false;
        ShowEditPanel = true;
        StatusMessage = "Adding new table";
    }

    [RelayCommand]
    private void EditSelectedTable()
    {
        if (SelectedTable == null)
        {
            MessageBox.Show("Please select a table to edit.", "Selection Required", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        // Create a copy for editing
        EditTable = new GamingTable
        {
            Id = SelectedTable.Id,
            Hostname = SelectedTable.Hostname,
            IPAddress = SelectedTable.IPAddress,
            Status = SelectedTable.Status,
            Version = SelectedTable.Version,
            LastSeen = SelectedTable.LastSeen,
            HardwareInfo = SelectedTable.HardwareInfo
        };
        IsEditing = true;
        ShowEditPanel = true;
        StatusMessage = $"Editing {SelectedTable.Hostname}";
    }

    [RelayCommand]
    private async Task SaveTableAsync()
    {
        if (string.IsNullOrWhiteSpace(EditTable.Hostname) || string.IsNullOrWhiteSpace(EditTable.IPAddress))
        {
            MessageBox.Show("Hostname and IP Address are required.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        try
        {
            IsLoading = true;
            StatusMessage = "Saving table...";

            if (IsEditing)
            {
                await _databaseService.UpdateTableAsync(EditTable);
                StatusMessage = $"Table {EditTable.Hostname} updated successfully";
            }
            else
            {
                await _databaseService.AddTableAsync(EditTable);
                StatusMessage = $"Table {EditTable.Hostname} added successfully";
            }

            // Refresh the table list
            await LoadTablesAsync();
            ShowEditPanel = false;
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error saving table: {ex.Message}";
            MessageBox.Show($"Failed to save table: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void CancelEdit()
    {
        ShowEditPanel = false;
        StatusMessage = "Edit cancelled";
    }

    [RelayCommand]
    private async Task DeleteSelectedTableAsync()
    {
        if (SelectedTable == null)
        {
            MessageBox.Show("Please select a table to delete.", "Selection Required", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var result = MessageBox.Show(
            $"Are you sure you want to delete table '{SelectedTable.Hostname}'? This action cannot be undone.",
            "Confirm Delete",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (result == MessageBoxResult.Yes)
        {
            try
            {
                IsLoading = true;
                StatusMessage = $"Deleting {SelectedTable.Hostname}...";

                await _databaseService.DeleteTableAsync(SelectedTable.Id);
                StatusMessage = $"Table {SelectedTable.Hostname} deleted successfully";

                // Refresh the table list
                await LoadTablesAsync();
                SelectedTable = null;
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error deleting table: {ex.Message}";
                MessageBox.Show($"Failed to delete table: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }
    }

    [RelayCommand]
    private async Task RefreshSelectedTableAsync()
    {
        if (SelectedTable == null)
        {
            MessageBox.Show("Please select a table to refresh.", "Selection Required", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        try
        {
            StatusMessage = $"Refreshing {SelectedTable.Hostname}...";
            
            // Simulate table refresh (in real implementation, this would ping the table)
            SelectedTable.LastSeen = DateTime.Now;
            await _databaseService.UpdateTableAsync(SelectedTable);
            
            StatusMessage = $"Table {SelectedTable.Hostname} refreshed";
            await LoadTablesAsync(); // Refresh the list to show updated timestamp
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error refreshing table: {ex.Message}";
            MessageBox.Show($"Failed to refresh table: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private void ViewTableDetails()
    {
        if (SelectedTable == null)
        {
            MessageBox.Show("Please select a table to view details.", "Selection Required", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        StatusMessage = $"Viewing details for {SelectedTable.Hostname}";
        // Navigation to detailed view would be implemented here
        MessageBox.Show(
            $"Table Details:\n\n" +
            $"Hostname: {SelectedTable.Hostname}\n" +
            $"IP Address: {SelectedTable.IPAddress}\n" +
            $"Status: {SelectedTable.Status}\n" +
            $"Version: {SelectedTable.Version}\n" +
            $"Last Seen: {SelectedTable.LastSeen:g}\n" +
            $"Hardware: {SelectedTable.HardwareInfo}",
            "Table Details",
            MessageBoxButton.OK,
            MessageBoxImage.Information);
    }

    [RelayCommand]
    private async Task ExportTablesAsync()
    {
        try
        {
            StatusMessage = "Exporting table data...";
            
            // Simulate export (in real implementation, this would export to CSV/JSON)
            var exportData = FilteredTables.Select(t => new
            {
                t.Hostname,
                t.IPAddress,
                t.Status,
                t.Version,
                LastSeen = t.LastSeen.ToString("g"),
                t.HardwareInfo
            }).ToList();

            // Show export summary
            MessageBox.Show(
                $"Exported {exportData.Count} tables successfully!\n\n" +
                "The data has been prepared for export and would be saved to a file in a real implementation.",
                "Export Complete",
                MessageBoxButton.OK,
                MessageBoxImage.Information);

            StatusMessage = $"Exported {exportData.Count} tables";
        }
        catch (Exception ex)
        {
            StatusMessage = "Export failed";
            MessageBox.Show($"Export failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private void ShowDeploymentHistory()
    {
        if (SelectedTable == null)
        {
            MessageBox.Show("Please select a table to view deployment history.", "Selection Required", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        StatusMessage = $"Showing deployment history for {SelectedTable.Hostname}";
        // Navigation to deployment history would be implemented here
        MessageBox.Show(
            $"Deployment history for {SelectedTable.Hostname} would be shown here.\n\n" +
            "This would include all deployment attempts, success/failure status, and timestamps.",
            "Deployment History",
            MessageBoxButton.OK,
            MessageBoxImage.Information);
    }

    [RelayCommand]
    private async Task BulkStatusUpdateAsync(string newStatus)
    {
        if (FilteredTables.Count == 0)
        {
            MessageBox.Show("No tables to update based on current filters.", "No Tables", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var result = MessageBox.Show(
            $"Are you sure you want to update status of {FilteredTables.Count} tables to '{newStatus}'?",
            "Confirm Bulk Update",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (result == MessageBoxResult.Yes)
        {
            try
            {
                IsLoading = true;
                StatusMessage = $"Updating {FilteredTables.Count} tables to {newStatus}...";

                foreach (var table in FilteredTables)
                {
                    table.Status = newStatus;
                    await _databaseService.UpdateTableAsync(table);
                }

                StatusMessage = $"Updated {FilteredTables.Count} tables to {newStatus}";
                await LoadTablesAsync(); // Refresh the list
            }
            catch (Exception ex)
            {
                StatusMessage = "Bulk update failed";
                MessageBox.Show($"Bulk update failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }
    }

    partial void OnSearchTextChanged(string value)
    {
        ApplyFilters();
    }

    partial void OnStatusFilterChanged(string value)
    {
        ApplyFilters();
    }

    partial void OnSelectedTableChanged(GamingTable? value)
    {
        if (value != null)
        {
            StatusMessage = $"Selected: {value.Hostname}";
        }
    }
}
