using GameTableManagerPro.Data;
using GameTableManagerPro.ViewModels;
using GameTableManagerPro.Views;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Windows;

namespace GameTableManagerPro;

public partial class App : Application
{
    private readonly IHost _host;

    public App()
    {
        _host = Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                ConfigureServices(services);
            })
            .Build();
    }

    private void ConfigureServices(IServiceCollection services)
    {
        // Register DbContext with factory
        services.AddDbContextFactory<AppDbContext>(options =>
            options.UseSqlite("Data Source=GameTableManagerPro.db"));

        // Register ViewModels
        services.AddSingleton<MainWindowViewModel>();
        services.AddTransient<DashboardViewModel>();
        services.AddTransient<DeploymentViewModel>();
        services.AddTransient<TableManagementViewModel>();
        services.AddTransient<HealthMonitorViewModel>();

        // Register Views
        services.AddSingleton<MainWindow>();
        services.AddTransient<DashboardView>();
        services.AddTransient<DeploymentView>();
        services.AddTransient<TableView>();
        services.AddTransient<HealthMonitorView>();

        // Register Services
        services.AddSingleton<IPowerShellService, PowerShellService>();
        services.AddSingleton<IDatabaseService, DatabaseService>();
    }

    protected override async void OnStartup(StartupEventArgs e)
    {
        await _host.StartAsync();

        // Initialize Database
        using (var scope = _host.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            // Ensure the database is created. 
            // For production, you'd use migrations.
            await dbContext.Database.EnsureCreatedAsync(); 
            DbInitializer.Initialize(dbContext);
        }

        var mainWindow = _host.Services.GetRequiredService<MainWindow>();
        mainWindow.Show();

        base.OnStartup(e);
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        using (_host)
        {
            await _host.StopAsync(TimeSpan.FromSeconds(5));
        }
        base.OnExit(e);
    }
}
