using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GameTableManagerPro.Models;
using GameTableManagerPro.Services;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace GameTableManagerPro.ViewModels;

public partial class AssetManagementViewModel : ObservableObject
{
    private readonly IDatabaseService _databaseService;
    private readonly IPowerShellService _powerShellService;

    [ObservableProperty]
    private string _statusMessage = "Loading asset data...";

    [ObservableProperty]
    private bool _isLoading = false;

    [ObservableProperty]
    private ObservableCollection<GameAsset> _assets = new();

    [ObservableProperty]
    private ObservableCollection<GameAsset> _filteredAssets = new();

    [ObservableProperty]
    private GameAsset? _selectedAsset;

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private string _assetTypeFilter = "All";

    [ObservableProperty]
    private bool _showUploadPanel = false;

    [ObservableProperty]
    private string _uploadFilePath = string.Empty;

    [ObservableProperty]
    private string _uploadAssetName = string.Empty;

    [ObservableProperty]
    private string _uploadAssetType = "Game";

    [ObservableProperty]
    private string _uploadVersion = "1.0.0";

    [ObservableProperty]
    private ObservableCollection<string> _assetTypes = new()
    {
        "All", "Game", "Emulator", "Driver", "Theme", "Configuration", "Script"
    };

    [ObservableProperty]
    private ObservableCollection<string> _availableVersions = new()
    {
        "1.0.0", "1.1.0", "1.2.0", "2.0.0", "2.1.0"
    };

    public AssetManagementViewModel(IDatabaseService databaseService, IPowerShellService powerShellService)
    {
        _databaseService = databaseService;
        _powerShellService = powerShellService;
        InitializeAsync();
    }

    private async void InitializeAsync()
    {
        await LoadAssetsAsync();
    }

    [RelayCommand]
    private async Task LoadAssetsAsync()
    {
        try
        {
            IsLoading = true;
            StatusMessage = "Loading assets...";

            // Simulate loading assets (in real implementation, this would query the database)
            await Task.Delay(500); // Simulate async operation
            
            // Sample assets for demonstration
            Assets = new ObservableCollection<GameAsset>
            {
                new GameAsset
                {
                    Id = 1,
                    Name = "RetroArch Core Pack",
                    Type = "Emulator",
                    Version = "1.10.3",
                    FileSize = "45.2 MB",
                    UploadDate = DateTime.Now.AddDays(-7),
                    Description = "Complete RetroArch core collection",
                    FilePath = @"C:\Assets\Emulators\retroarch_cores.zip"
                },
                new GameAsset
                {
                    Id = 2,
                    Name = "Classic Arcade Theme",
                    Type = "Theme",
                    Version = "2.1.0",
                    FileSize = "12.8 MB",
                    UploadDate = DateTime.Now.AddDays(-3),
                    Description = "Vintage arcade cabinet theme",
                    FilePath = @"C:\Assets\Themes\arcade_classic.zip"
                },
                new GameAsset
                {
                    Id = 3,
                    Name = "Touchscreen Drivers",
                    Type = "Driver",
                    Version = "1.5.2",
                    FileSize = "8.3 MB",
                    UploadDate = DateTime.Now.AddDays(-1),
                    Description = "Multi-touch display drivers",
                    FilePath = @"C:\Assets\Drivers\touch_drivers.exe"
                }
            };

            ApplyFilters();
            StatusMessage = $"Loaded {Assets.Count} assets";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error loading assets: {ex.Message}";
            MessageBox.Show($"Failed to load assets: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void ApplyFilters()
    {
        var query = Assets.AsEnumerable();

        // Apply search filter
        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            query = query.Where(a =>
                a.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                (a.Description ?? "").Contains(SearchText, StringComparison.OrdinalIgnoreCase));
        }

        // Apply type filter
        if (AssetTypeFilter != "All")
        {
            query = query.Where(a => a.Type == AssetTypeFilter);
        }

        FilteredAssets = new ObservableCollection<GameAsset>(query);
        StatusMessage = $"Showing {FilteredAssets.Count} of {Assets.Count} assets";
    }

    [RelayCommand]
    private void ClearFilters()
    {
        SearchText = string.Empty;
        AssetTypeFilter = "All";
        ApplyFilters();
        StatusMessage = "Filters cleared";
    }

    [RelayCommand]
    private void ShowUploadDialog()
    {
        UploadFilePath = string.Empty;
        UploadAssetName = string.Empty;
        UploadAssetType = "Game";
        UploadVersion = "1.0.0";
        ShowUploadPanel = true;
        StatusMessage = "Ready to upload new asset";
    }

    [RelayCommand]
    private void CancelUpload()
    {
        ShowUploadPanel = false;
        StatusMessage = "Upload cancelled";
    }

    [RelayCommand]
    private async Task UploadAssetAsync()
    {
        if (string.IsNullOrWhiteSpace(UploadFilePath) || !File.Exists(UploadFilePath))
        {
            MessageBox.Show("Please select a valid file to upload.", "Invalid File", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (string.IsNullOrWhiteSpace(UploadAssetName))
        {
            MessageBox.Show("Please enter a name for the asset.", "Name Required", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        try
        {
            IsLoading = true;
            StatusMessage = "Uploading asset...";

            var fileInfo = new FileInfo(UploadFilePath);
            var destinationPath = Path.Combine(@"C:\Assets", UploadAssetType, $"{UploadAssetName}_{UploadVersion}{fileInfo.Extension}");

            // Ensure directory exists
            Directory.CreateDirectory(Path.GetDirectoryName(destinationPath)!);

            // Copy file to assets directory
            File.Copy(UploadFilePath, destinationPath, true);

            // Create asset record
            var newAsset = new GameAsset
            {
                Name = UploadAssetName,
                Type = UploadAssetType,
                Version = UploadVersion,
                FileSize = GetFileSizeDisplay(fileInfo.Length),
                UploadDate = DateTime.Now,
                Description = $"Uploaded asset: {UploadAssetName}",
                FilePath = destinationPath
            };

            Assets.Add(newAsset);
            ApplyFilters();
            ShowUploadPanel = false;

            StatusMessage = $"Asset '{UploadAssetName}' uploaded successfully";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error uploading asset: {ex.Message}";
            MessageBox.Show($"Failed to upload asset: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task DeployAssetAsync(GameAsset asset)
    {
        if (asset == null) return;

        try
        {
            StatusMessage = $"Deploying {asset.Name}...";

            // Simulate deployment (in real implementation, this would use PowerShell service)
            await Task.Delay(2000); // Simulate deployment time

            StatusMessage = $"{asset.Name} deployed successfully";
            MessageBox.Show($"Asset '{asset.Name}' has been deployed to all tables.", "Deployment Complete", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            StatusMessage = $"Deployment failed";
            MessageBox.Show($"Failed to deploy asset: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private void ViewAssetDetails(GameAsset asset)
    {
        if (asset == null) return;

        MessageBox.Show(
            $"Asset Details:\n\n" +
            $"Name: {asset.Name}\n" +
            $"Type: {asset.Type}\n" +
            $"Version: {asset.Version}\n" +
            $"Size: {asset.FileSize}\n" +
            $"Uploaded: {asset.UploadDate:g}\n" +
            $"Description: {asset.Description}\n" +
            $"Path: {asset.FilePath}",
            "Asset Details",
            MessageBoxButton.OK,
            MessageBoxImage.Information);

        StatusMessage = $"Viewing details for {asset.Name}";
    }

    [RelayCommand]
    private async Task DeleteAssetAsync(GameAsset asset)
    {
        if (asset == null) return;

        var result = MessageBox.Show(
            $"Are you sure you want to delete asset '{asset.Name}'? This will remove the file and asset record.",
            "Confirm Delete",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (result == MessageBoxResult.Yes)
        {
            try
            {
                IsLoading = true;
                StatusMessage = $"Deleting {asset.Name}...";

                // Delete file if it exists
                if (File.Exists(asset.FilePath))
                {
                    File.Delete(asset.FilePath);
                }

                // Remove from collection
                Assets.Remove(asset);
                ApplyFilters();

                StatusMessage = $"Asset '{asset.Name}' deleted successfully";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error deleting asset";
                MessageBox.Show($"Failed to delete asset: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }
    }

    [RelayCommand]
    private void OpenAssetLocation(GameAsset asset)
    {
        if (asset == null || !File.Exists(asset.FilePath)) return;

        try
        {
            // Open file location in Explorer
            System.Diagnostics.Process.Start("explorer.exe", $"/select,\"{asset.FilePath}\"");
            StatusMessage = $"Opened location for {asset.Name}";
        }
        catch (Exception ex)
        {
            StatusMessage = "Error opening file location";
            MessageBox.Show($"Failed to open file location: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private async Task ValidateAssetsAsync()
    {
        try
        {
            IsLoading = true;
            StatusMessage = "Validating assets...";

            int validCount = 0;
            int missingCount = 0;

            foreach (var asset in Assets)
            {
                if (File.Exists(asset.FilePath))
                {
                    validCount++;
                }
                else
                {
                    missingCount++;
                }
                await Task.Delay(10); // Small delay for UI responsiveness
            }

            StatusMessage = $"Validation complete: {validCount} valid, {missingCount} missing";
            
            if (missingCount > 0)
            {
                MessageBox.Show(
                    $"Asset validation found {missingCount} missing files.\n\n" +
                    "Please check your asset storage location or re-upload missing assets.",
                    "Validation Results",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }
            else
            {
                MessageBox.Show(
                    "All assets are valid and accessible!",
                    "Validation Complete",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
        }
        catch (Exception ex)
        {
            StatusMessage = "Validation failed";
            MessageBox.Show($"Asset validation failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsLoading = false;
        }
    }

    private string GetFileSizeDisplay(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB" };
        int order = 0;
        double len = bytes;

        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len /= 1024;
        }

        return $"{len:0.##} {sizes[order]}";
    }

    partial void OnSearchTextChanged(string value)
    {
        ApplyFilters();
    }

    partial void OnAssetTypeFilterChanged(string value)
    {
        ApplyFilters();
    }

    partial void OnSelectedAssetChanged(GameAsset? value)
    {
        if (value != null)
        {
            StatusMessage = $"Selected: {value.Name}";
        }
    }
}

public class GameAsset
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string Type { get; set; }
    public required string Version { get; set; }
    public string? FileSize { get; set; }
    public DateTime UploadDate { get; set; }
    public string? Description { get; set; }
    public required string FilePath { get; set; }
}
