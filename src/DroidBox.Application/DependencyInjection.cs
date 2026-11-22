using DroidBox.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace DroidBox.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IEmulatorService, EmulatorService>();
        services.AddScoped<IApkService, ApkService>();
        services.AddScoped<IDeviceService, DeviceService>();
        services.AddScoped<ISdkSetupService, SdkSetupService>();
        services.AddScoped<IScreenCaptureService, ScreenCaptureService>();
        return services;
    }
}
