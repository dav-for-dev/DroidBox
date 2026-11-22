using DroidBox.Application;
using DroidBox.Infrastructure;
using DroidBox.Infrastructure.Data;
using DroidBox.Infrastructure.Logging;
using DroidBox.Runtime.AndroidSdk;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace DroidBox.Bootstrapper;

public class Program
{
    public static async Task Main(string[] args)
    {
        LoggingConfiguration.ConfigureLogger();

        var host = Host.CreateDefaultBuilder(args)
            .ConfigureServices((_, services) =>
            {
                services.AddInfrastructureServices();
                services.AddApplicationServices();
                services.AddAndroidSdkRuntime();
                services.AddHostedService<IpcServer>();
            })
            .Build();

        using (var scope = host.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<DroidBoxDbContext>();
            db.Database.EnsureCreated();
        }

        await host.RunAsync();
    }
}
