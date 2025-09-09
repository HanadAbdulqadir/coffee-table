using GameTableManagerPro.Services;
using GameTableManagerPro.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;

namespace GameTableManagerPro.Views;

public partial class MainWindow : Window
{
    private readonly INavigationService _navigationService;

    public MainWindow(MainWindowViewModel viewModel, INavigationService navigationService)
    {
        InitializeComponent();
        DataContext = viewModel;
        _navigationService = navigationService;
        
        // Subscribe to navigation changes
        _navigationService.CurrentViewChanged += OnCurrentViewChanged;
        
        // Load initial view
        LoadCurrentView();
    }

    private void OnCurrentViewChanged(object? sender, string viewName)
    {
        Dispatcher.Invoke(() =>
        {
            LoadCurrentView();
        });
    }

    private void LoadCurrentView()
    {
        var viewName = _navigationService.CurrentView;
        
        switch (viewName)
        {
            case "Dashboard":
                var dashboardView = App.Current.Services.GetService<DashboardView>();
                MainContent.Content = dashboardView;
                break;
                
            case "Deployment":
                var deploymentView = App.Current.Services.GetService<DeploymentView>();
                MainContent.Content = deploymentView;
                break;
                
            case "TableManagement":
                var tableView = App.Current.Services.GetService<TableView>();
                MainContent.Content = tableView;
                break;
                
            case "HealthMonitoring":
                var healthMonitorView = App.Current.Services.GetService<HealthMonitorView>();
                MainContent.Content = healthMonitorView;
                break;
                
            case "Settings":
                // Placeholder for settings view
                MainContent.Content = new TextBlock 
                { 
                    Text = "Settings View (Coming Soon)", 
                    Foreground = System.Windows.Media.Brushes.White,
                    FontSize = 18,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                };
                break;
                
            default:
                MainContent.Content = new TextBlock 
                { 
                    Text = "Welcome to GameTable Manager Pro", 
                    Foreground = System.Windows.Media.Brushes.White,
                    FontSize = 18,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                };
                break;
        }
    }

    protected override void OnClosed(EventArgs e)
    {
        _navigationService.CurrentViewChanged -= OnCurrentViewChanged;
        base.OnClosed(e);
    }
}
