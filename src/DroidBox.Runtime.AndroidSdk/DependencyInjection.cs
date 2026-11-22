using DroidBox.Core.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace DroidBox.Runtime.AndroidSdk;

public static class DependencyInjection
{
    public static IServiceCollection AddAndroidSdkRuntime(this IServiceCollection services)
    {
        services.AddSingleton<AndroidSdkProvider>();
        services.AddSingleton<IRuntimeProvider>(sp => sp.GetRequiredService<AndroidSdkProvider>());
        services.AddSingleton<IRuntimeHealthChecker>(sp => sp.GetRequiredService<AndroidSdkProvider>());
        services.AddSingleton<IScreenshotService>(sp => sp.GetRequiredService<AndroidSdkProvider>());

        services.AddSingleton<IAdbClient, AdbClientImpl>();
        services.AddSingleton<IApkAnalyzer, ApkAnalyzerImpl>();
        services.AddSingleton<IDeviceProfileManager, AvdManager>();
        services.AddSingleton<IAndroidRuntime, EmulatorProcessManager>();
        services.AddSingleton<IRuntimeInstaller, SdkInstaller>();

        return services;
    }
}
