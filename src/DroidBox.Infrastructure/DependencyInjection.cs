using DroidBox.Core.Interfaces;
using DroidBox.Infrastructure.Data;
using DroidBox.Infrastructure.Repositories;
using DroidBox.Shared.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DroidBox.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, string? connectionString = null)
    {
        var dbPath = connectionString ?? $"Data Source={PathHelper.GetDatabasePath()}";
        services.AddDbContext<DroidBoxDbContext>(options =>
            options.UseSqlite(dbPath));

        services.AddScoped<IAppRepository, AppRepository>();
        services.AddScoped<IDeviceProfileRepository, DeviceProfileRepository>();
        services.AddScoped<ISdkPackageRepository, SdkPackageRepository>();
        services.AddScoped<ISettingsRepository, SettingsRepository>();

        return services;
    }
}
