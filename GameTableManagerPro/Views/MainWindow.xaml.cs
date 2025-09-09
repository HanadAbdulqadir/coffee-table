using GameTableManagerPro.ViewModels;
using System.Windows;

namespace GameTableManagerPro.Views;

public partial class MainWindow : Window
{
    public MainWindow(MainWindowViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
