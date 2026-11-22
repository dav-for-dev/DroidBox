$ErrorActionPreference = "Stop"

Set-Location "E:\code-from-haris\DroidBox\src\DroidBox.Application"

New-Item -ItemType Directory -Path "Services" -Force | Out-Null

# --- IEmulatorService & EmulatorService ---
@'
using DroidBox.Core.Entities;
using DroidBox.Core.Enums;
using DroidBox.Core.Models;

namespace DroidBox.Application.Services;

public interface IEmulatorService
{
    Task<OperationResult> StartAsync(string? avdName = null, CancellationToken cancellationToken = default);
    Task<OperationResult> StopAsync(CancellationToken cancellationToken = default);
    Task<EmulatorState> GetStateAsync(CancellationToken cancellationToken = default);
    Task<bool> IsRunningAsync(CancellationToken cancellationToken = default);
    Task<EmulatorInstance?> GetCurrentInstanceAsync(CancellationToken cancellationToken = default);
}
'@ | Set-Content -Path "Services\IEmulatorService.cs"

@'
using DroidBox.Core.Entities;
using DroidBox.Core.Enums;
using DroidBox.Core.Interfaces;
using DroidBox.Core.Models;
using Microsoft.Extensions.Logging;

namespace DroidBox.Application.Services;

public class EmulatorService : IEmulatorService
{
    private readonly IAndroidRuntime _runtime;
    private readonly ILogger<EmulatorService> _logger;
    private EmulatorInstance? _currentInstance;

    public EmulatorService(IAndroidRuntime runtime, ILogger<EmulatorService> logger)
    {
        _runtime = runtime;
        _logger = logger;
    }

    public async Task<OperationResult> StartAsync(string? avdName = null, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Starting emulator: {AvdName}", avdName ?? "default");
            await _runtime.StartAsync(cancellationToken);

            _currentInstance = new EmulatorInstance
            {
                AvdName = avdName ?? "DroidBox_Default",
                State = EmulatorState.Running,
                StartedAt = DateTime.UtcNow
            };

            return OperationResult.Ok("Emulator started successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start emulator");
            return OperationResult.Fail($"Failed to start emulator: {ex.Message}");
        }
    }

    public async Task<OperationResult> StopAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Stopping emulator");
            await _runtime.StopAsync(cancellationToken);

            if (_currentInstance != null)
            {
                _currentInstance.State = EmulatorState.Stopped;
            }

            return OperationResult.Ok("Emulator stopped successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to stop emulator");
            return OperationResult.Fail($"Failed to stop emulator: {ex.Message}");
        }
    }

    public async Task<EmulatorState> GetStateAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await _runtime.GetStateAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to query emulator state");
            return EmulatorState.Error;
        }
    }

    public async Task<bool> IsRunningAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await _runtime.IsRunningAsync(cancellationToken);
        }
        catch
        {
            return false;
        }
    }

    public Task<EmulatorInstance?> GetCurrentInstanceAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_currentInstance);
    }
}
'@ | Set-Content -Path "Services\EmulatorService.cs"

# --- IApkService & ApkService ---
@'
using DroidBox.Core.Entities;
using DroidBox.Core.Models;

namespace DroidBox.Application.Services;

public interface IApkService
{
    Task<OperationResult> InstallAsync(string apkPath, CancellationToken cancellationToken = default);
    Task<OperationResult> UninstallAsync(string packageName, CancellationToken cancellationToken = default);
    Task<OperationResult> LaunchAsync(string packageName, string? activity = null, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<InstalledApp>> GetInstalledAppsAsync(CancellationToken cancellationToken = default);
    Task<ApkInfo> AnalyzeApkAsync(string apkPath, CancellationToken cancellationToken = default);
}
'@ | Set-Content -Path "Services\IApkService.cs"

@'
using DroidBox.Core.Entities;
using DroidBox.Core.Enums;
using DroidBox.Core.Interfaces;
using DroidBox.Core.Models;
using Microsoft.Extensions.Logging;

namespace DroidBox.Application.Services;

public class ApkService : IApkService
{
    private readonly IAdbClient _adbClient;
    private readonly IApkAnalyzer _apkAnalyzer;
    private readonly IAppRepository _appRepository;
    private readonly ILogger<ApkService> _logger;

    public ApkService(
        IAdbClient adbClient,
        IApkAnalyzer apkAnalyzer,
        IAppRepository appRepository,
        ILogger<ApkService> logger)
    {
        _adbClient = adbClient;
        _apkAnalyzer = apkAnalyzer;
        _appRepository = appRepository;
        _logger = logger;
    }

    public async Task<OperationResult> InstallAsync(string apkPath, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!File.Exists(apkPath))
            {
                return OperationResult.Fail($"APK file not found: {apkPath}");
            }

            _logger.LogInformation("Analyzing APK before installation: {ApkPath}", apkPath);
            var apkInfo = await _apkAnalyzer.AnalyzeAsync(apkPath, cancellationToken);

            _logger.LogInformation("Installing APK {PackageName} via ADB", apkInfo.PackageName);
            var installResult = await _adbClient.InstallApkAsync(apkPath, cancellationToken);

            if (!installResult.Success)
            {
                return installResult;
            }

            var app = new InstalledApp
            {
                PackageName = apkInfo.PackageName,
                AppName = string.IsNullOrWhiteSpace(apkInfo.AppName) ? apkInfo.PackageName : apkInfo.AppName,
                VersionName = apkInfo.VersionName,
                VersionCode = apkInfo.VersionCode,
                MinSdk = apkInfo.MinSdkVersion,
                TargetSdk = apkInfo.TargetSdkVersion,
                CpuArchitecture = string.Join(",", apkInfo.SupportedAbis),
                InstallPath = apkPath,
                InstalledAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _appRepository.AddAsync(app, cancellationToken);
            _logger.LogInformation("APK {PackageName} installed and recorded successfully", apkInfo.PackageName);

            return OperationResult.Ok($"Successfully installed {app.AppName} ({app.PackageName})");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to install APK {ApkPath}", apkPath);
            return OperationResult.Fail($"Installation error: {ex.Message}");
        }
    }

    public async Task<OperationResult> UninstallAsync(string packageName, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Uninstalling package: {PackageName}", packageName);
            var result = await _adbClient.UninstallAsync(packageName, cancellationToken);

            if (result.Success)
            {
                var existing = await _appRepository.GetByPackageNameAsync(packageName, cancellationToken);
                if (existing != null)
                {
                    await _appRepository.DeleteAsync(existing.Id, cancellationToken);
                }
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to uninstall package {PackageName}", packageName);
            return OperationResult.Fail($"Uninstall error: {ex.Message}");
        }
    }

    public async Task<OperationResult> LaunchAsync(string packageName, string? activity = null, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Launching package: {PackageName}", packageName);
            return await _adbClient.LaunchActivityAsync(packageName, activity, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to launch package {PackageName}", packageName);
            return OperationResult.Fail($"Launch error: {ex.Message}");
        }
    }

    public async Task<IReadOnlyList<InstalledApp>> GetInstalledAppsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await _appRepository.GetAllAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get installed apps");
            return Array.Empty<InstalledApp>();
        }
    }

    public async Task<ApkInfo> AnalyzeApkAsync(string apkPath, CancellationToken cancellationToken = default)
    {
        return await _apkAnalyzer.AnalyzeAsync(apkPath, cancellationToken);
    }
}
'@ | Set-Content -Path "Services\ApkService.cs"

# --- IDeviceService & DeviceService ---
@'
using DroidBox.Core.Entities;
using DroidBox.Core.Models;

namespace DroidBox.Application.Services;

public interface IDeviceService
{
    Task<IReadOnlyList<DeviceProfile>> GetProfilesAsync(CancellationToken cancellationToken = default);
    Task<DeviceProfile?> GetActiveProfileAsync(CancellationToken cancellationToken = default);
    Task<OperationResult> SwitchProfileAsync(string profileName, CancellationToken cancellationToken = default);
    Task<OperationResult> CreateProfileAsync(DeviceProfile profile, CancellationToken cancellationToken = default);
}
'@ | Set-Content -Path "Services\IDeviceService.cs"

@'
using DroidBox.Core.Entities;
using DroidBox.Core.Interfaces;
using DroidBox.Core.Models;
using Microsoft.Extensions.Logging;

namespace DroidBox.Application.Services;

public class DeviceService : IDeviceService
{
    private readonly IDeviceProfileManager _profileManager;
    private readonly IDeviceProfileRepository _profileRepository;
    private readonly ILogger<DeviceService> _logger;

    public DeviceService(
        IDeviceProfileManager profileManager,
        IDeviceProfileRepository profileRepository,
        ILogger<DeviceService> logger)
    {
        _profileManager = profileManager;
        _profileRepository = profileRepository;
        _logger = logger;
    }

    public async Task<IReadOnlyList<DeviceProfile>> GetProfilesAsync(CancellationToken cancellationToken = default)
    {
        var dbProfiles = await _profileRepository.GetAllAsync(cancellationToken);
        if (dbProfiles.Count == 0)
        {
            // Seed defaults
            var tablet = DeviceProfile.CreateTablet();
            var mobile = DeviceProfile.CreateMobile();
            await _profileRepository.AddAsync(tablet, cancellationToken);
            await _profileRepository.AddAsync(mobile, cancellationToken);
            return new[] { tablet, mobile };
        }
        return dbProfiles;
    }

    public async Task<DeviceProfile?> GetActiveProfileAsync(CancellationToken cancellationToken = default)
    {
        return await _profileManager.GetActiveProfileAsync(cancellationToken);
    }

    public async Task<OperationResult> SwitchProfileAsync(string profileName, CancellationToken cancellationToken = default)
    {
        try
        {
            var profile = await _profileRepository.GetByNameAsync(profileName, cancellationToken);
            if (profile == null)
            {
                return OperationResult.Fail($"Device profile not found: {profileName}");
            }

            await _profileManager.SwitchProfileAsync(profile.Id, cancellationToken);
            _logger.LogInformation("Switched to device profile: {ProfileName}", profileName);
            return OperationResult.Ok($"Switched to profile: {profile.DisplayName}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to switch device profile to {ProfileName}", profileName);
            return OperationResult.Fail($"Profile switch error: {ex.Message}");
        }
    }

    public async Task<OperationResult> CreateProfileAsync(DeviceProfile profile, CancellationToken cancellationToken = default)
    {
        try
        {
            profile.IsCustom = true;
            await _profileRepository.AddAsync(profile, cancellationToken);
            return OperationResult.Ok($"Profile {profile.Name} created");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create profile {ProfileName}", profile.Name);
            return OperationResult.Fail($"Create profile error: {ex.Message}");
        }
    }
}
'@ | Set-Content -Path "Services\DeviceService.cs"

# --- ISdkSetupService & SdkSetupService ---
@'
using DroidBox.Core.Models;

namespace DroidBox.Application.Services;

public interface ISdkSetupService
{
    Task<HealthReport> CheckHealthAsync(CancellationToken cancellationToken = default);
    Task<OperationResult> SetupAsync(IProgress<ProgressInfo>? progress = null, CancellationToken cancellationToken = default);
}
'@ | Set-Content -Path "Services\ISdkSetupService.cs"

@'
using DroidBox.Core.Interfaces;
using DroidBox.Core.Models;
using Microsoft.Extensions.Logging;

namespace DroidBox.Application.Services;

public class SdkSetupService : ISdkSetupService
{
    private readonly IRuntimeInstaller _installer;
    private readonly IRuntimeHealthChecker _healthChecker;
    private readonly ILogger<SdkSetupService> _logger;

    public SdkSetupService(
        IRuntimeInstaller installer,
        IRuntimeHealthChecker healthChecker,
        ILogger<SdkSetupService> logger)
    {
        _installer = installer;
        _healthChecker = healthChecker;
        _logger = logger;
    }

    public async Task<HealthReport> CheckHealthAsync(CancellationToken cancellationToken = default)
    {
        return await _healthChecker.CheckHealthAsync(cancellationToken);
    }

    public async Task<OperationResult> SetupAsync(IProgress<ProgressInfo>? progress = null, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Beginning Android environment setup");
            progress?.Report(new ProgressInfo { Percentage = 10, Message = "Checking system compatibility...", CurrentStep = 1, TotalSteps = 5 });

            progress?.Report(new ProgressInfo { Percentage = 30, Message = "Checking SDK components...", CurrentStep = 2, TotalSteps = 5 });
            var result = await _installer.InstallAsync("platform-tools", progress, cancellationToken);
            if (!result.Success) return result;

            progress?.Report(new ProgressInfo { Percentage = 70, Message = "Setting up emulator...", CurrentStep = 3, TotalSteps = 5 });
            result = await _installer.InstallAsync("emulator", progress, cancellationToken);
            if (!result.Success) return result;

            progress?.Report(new ProgressInfo { Percentage = 90, Message = "Verifying setup...", CurrentStep = 4, TotalSteps = 5 });
            var health = await _healthChecker.CheckHealthAsync(cancellationToken);

            progress?.Report(new ProgressInfo { Percentage = 100, Message = "Setup complete!", CurrentStep = 5, TotalSteps = 5 });
            return OperationResult.Ok(health.IsHealthy ? "Environment is ready" : "Setup completed with warnings");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Setup failed");
            return OperationResult.Fail($"Setup error: {ex.Message}");
        }
    }
}
'@ | Set-Content -Path "Services\SdkSetupService.cs"

# --- IScreenCaptureService & ScreenCaptureService ---
@'
namespace DroidBox.Application.Services;

public interface IScreenCaptureService
{
    Task<byte[]> CaptureAsync(CancellationToken cancellationToken = default);
    Task<string> SaveScreenshotAsync(string outputPath, CancellationToken cancellationToken = default);
}
'@ | Set-Content -Path "Services\IScreenCaptureService.cs"

@'
using DroidBox.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace DroidBox.Application.Services;

public class ScreenCaptureService : IScreenCaptureService
{
    private readonly IScreenshotService _screenshotService;
    private readonly ILogger<ScreenCaptureService> _logger;

    public ScreenCaptureService(IScreenshotService screenshotService, ILogger<ScreenCaptureService> logger)
    {
        _screenshotService = screenshotService;
        _logger = logger;
    }

    public async Task<byte[]> CaptureAsync(CancellationToken cancellationToken = default)
    {
        return await _screenshotService.CaptureAsync(cancellationToken);
    }

    public async Task<string> SaveScreenshotAsync(string outputPath, CancellationToken cancellationToken = default)
    {
        var bytes = await _screenshotService.CaptureAsync(cancellationToken);
        var dir = Path.GetDirectoryName(Path.GetFullPath(outputPath));
        if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }

        await File.WriteAllBytesAsync(outputPath, bytes, cancellationToken);
        _logger.LogInformation("Saved screenshot to {Path} ({Bytes} bytes)", outputPath, bytes.Length);
        return outputPath;
    }
}
'@ | Set-Content -Path "Services\ScreenCaptureService.cs"

# --- DependencyInjection.cs ---
@'
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
'@ | Set-Content -Path "DependencyInjection.cs"

Write-Host "DroidBox.Application files created successfully!"
