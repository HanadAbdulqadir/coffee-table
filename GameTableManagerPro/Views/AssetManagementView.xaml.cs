using Microsoft.Win32;
using System.Windows;
using System.Windows.Controls;

namespace GameTableManagerPro.Views;

public partial class AssetManagementView : UserControl
{
    public AssetManagementView()
    {
        InitializeComponent();
    }

    private void BrowseFileButton_Click(object sender, RoutedEventArgs e)
    {
        var openFileDialog = new OpenFileDialog
        {
            Title = "Select Asset File",
            Filter = "All Files (*.*)|*.*|" +
                    "Zip Archives (*.zip)|*.zip|" +
                    "Executables (*.exe)|*.exe|" +
                    "Configuration Files (*.ini,*.cfg)|*.ini;*.cfg|" +
                    "Script Files (*.ps1,*.bat)|*.ps1;*.bat",
            Multiselect = false
        };

        if (openFileDialog.ShowDialog() == true)
        {
            if (DataContext is ViewModels.AssetManagementViewModel viewModel)
            {
                viewModel.UploadFilePath = openFileDialog.FileName;
                
                // Auto-fill name if empty
                if (string.IsNullOrEmpty(viewModel.UploadAssetName))
                {
                    viewModel.UploadAssetName = System.IO.Path.GetFileNameWithoutExtension(openFileDialog.FileName);
                }
            }
        }
    }
}
