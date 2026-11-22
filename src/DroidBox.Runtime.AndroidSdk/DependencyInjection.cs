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

        // New Official Runtime Services
        services.AddSingleton<IAndroidEnvironmentDetector, AndroidEnvironmentDetector>();
        services.AddSingleton<IAndroidLicenseManager, AndroidLicenseManager>();
        services.AddSingleton<ISystemImageManager, SystemImageManager>();

        services.AddSingleton<AvdManager>();
        services.AddSingleton<IAvdManager>(sp => sp.GetRequiredService<AvdManager>());
        services.AddSingleton<IDeviceProfileManager>(sp => sp.GetRequiredService<AvdManager>());

        services.AddSingleton<IAndroidEmulatorManager, AndroidEmulatorManager>();

        services.AddSingleton<AndroidSdkInstaller>();
        services.AddSingleton<IAndroidSdkInstaller>(sp => sp.GetRequiredService<AndroidSdkInstaller>());
        services.AddSingleton<IRuntimeInstaller>(sp => sp.GetRequiredService<AndroidSdkInstaller>());

        services.AddSingleton<IAndroidSdkManager, AndroidSdkManager>();
        services.AddSingleton<IAndroidRuntime, EmulatorProcessManager>();

        return services;
    }
}
