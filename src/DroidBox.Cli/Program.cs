using System.CommandLine;
using DroidBox.Application;
using DroidBox.Application.Services;
using DroidBox.Cli.Commands;
using DroidBox.Infrastructure;
using DroidBox.Infrastructure.Logging;
using DroidBox.Runtime.AndroidSdk;
using DroidBox.Runtime.Qemu;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace DroidBox.Cli;

public class Program
{
    public static async Task<int> Main(string[] args)
    {
        LoggingConfiguration.ConfigureLogger();

        var host = Host.CreateDefaultBuilder()
            .ConfigureServices((_, services) =>
            {
                services.AddInfrastructureServices();
                services.AddApplicationServices();
                services.AddAndroidSdkRuntime();
                services.AddQemuRuntime();
            })
            .Build();

        // Ensure database and tables are created
        using (var scope = host.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<DroidBox.Infrastructure.Data.DroidBoxDbContext>();
            dbContext.Database.EnsureCreated();
        }

        var emulatorService = host.Services.GetRequiredService<IEmulatorService>();
        var apkService = host.Services.GetRequiredService<IApkService>();
        var deviceService = host.Services.GetRequiredService<IDeviceService>();
        var setupService = host.Services.GetRequiredService<ISdkSetupService>();
        var screenService = host.Services.GetRequiredService<IScreenCaptureService>();

        var root = new RootCommand("DroidBox - Lightweight Android Testing Environment for Windows");

        root.AddCommand(SetupCommand.Build(setupService));
        root.AddCommand(DoctorCommand.Build(setupService));
        root.AddCommand(StatusCommand.Build(emulatorService, deviceService));
        root.AddCommand(InstallCommand.Build(apkService, deviceService));
        root.AddCommand(LaunchCommand.Build(apkService));
        root.AddCommand(UninstallCommand.Build(apkService));
        root.AddCommand(ListCommand.Build(apkService));
        root.AddCommand(DevicesCommand.Build(deviceService));
        root.AddCommand(DeviceCommand.Build(deviceService));
        root.AddCommand(ScreenshotCommand.Build(screenService));
        root.AddCommand(LogsCommand.Build());
        root.AddCommand(DocsCommand.Build());
        root.AddCommand(UpdateCommand.Build());
        root.AddCommand(StopCommand.Build(emulatorService));

        return await root.InvokeAsync(args);
    }
}
