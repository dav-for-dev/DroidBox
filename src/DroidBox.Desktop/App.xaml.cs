using System.Windows;
using DroidBox.Application;
using DroidBox.Desktop.Services;
using DroidBox.Desktop.ViewModels;
using DroidBox.Desktop.Views;
using DroidBox.Display;
using DroidBox.Infrastructure;
using DroidBox.Infrastructure.Data;
using DroidBox.Infrastructure.Logging;
using DroidBox.Runtime.AndroidSdk;
using DroidBox.Runtime.Qemu;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace DroidBox.Desktop;

public partial class App : System.Windows.Application
{
    private IHost? _host;

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        LoggingConfiguration.ConfigureLogger();

        _host = Host.CreateDefaultBuilder()
            .ConfigureServices((_, services) =>
            {
                services.AddInfrastructureServices();
                services.AddApplicationServices();
                services.AddAndroidSdkRuntime();
                services.AddQemuRuntime();
                services.AddDisplayServices();

                // UI Services
                services.AddSingleton<INavigationService, NavigationService>();
                services.AddSingleton<IDialogService, DialogService>();
                services.AddSingleton<IThemeService, ThemeService>();

                // ViewModels
                services.AddSingleton<MainViewModel>();
                services.AddTransient<DashboardViewModel>();
                services.AddTransient<ApplicationsViewModel>();
                services.AddTransient<EmulatorViewModel>();
                services.AddTransient<DocumentationViewModel>();
                services.AddTransient<SettingsViewModel>();

                // Views
                services.AddSingleton<MainWindow>();
                services.AddTransient<DashboardView>();
                services.AddTransient<ApplicationsView>();
                services.AddTransient<EmulatorView>();
                services.AddTransient<DocumentationView>();
                services.AddTransient<SettingsView>();
            })
            .Build();

        await _host.StartAsync();

        using (var scope = _host.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<DroidBoxDbContext>();
            db.Database.EnsureCreated();
        }

        var mainWindow = _host.Services.GetRequiredService<MainWindow>();
        mainWindow.Show();
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        if (_host != null)
        {
            await _host.StopAsync();
            _host.Dispose();
        }
        base.OnExit(e);
    }
}
