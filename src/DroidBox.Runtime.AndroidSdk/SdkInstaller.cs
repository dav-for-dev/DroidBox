using System.IO.Compression;
using DroidBox.Core.Entities;
using DroidBox.Core.Interfaces;
using DroidBox.Core.Models;
using DroidBox.Shared.Constants;
using DroidBox.Shared.Helpers;
using Microsoft.Extensions.Logging;

namespace DroidBox.Runtime.AndroidSdk;

public class SdkInstaller : IRuntimeInstaller
{
    private readonly ILogger<SdkInstaller> _logger;

    public SdkInstaller(ILogger<SdkInstaller> logger)
    {
        _logger = logger;
    }

    public async Task<OperationResult> InstallAsync(string component, IProgress<ProgressInfo>? progress = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var sdkManager = SdkPathResolver.GetSdkManagerPath();
            _logger.LogInformation("Installing SDK component: {Component}", component);

            progress?.Report(new ProgressInfo { Percentage = 20, Message = $"Installing {component}..." });

            // Accept licenses automatically
            var result = await ProcessHelper.RunProcessAsync(
                sdkManager,
                $"\"{component}\" --sdk_root=\"{SdkPathResolver.GetSdkRoot()}\"",
                timeoutMs: 300000,
                cancellationToken: cancellationToken);

            if (result.Success)
            {
                progress?.Report(new ProgressInfo { Percentage = 100, Message = $"{component} installed successfully" });
                return OperationResult.Ok($"Component {component} installed successfully");
            }

            return OperationResult.Fail($"Failed to install {component}: {result.StandardError}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to install component {Component}", component);
            return OperationResult.Fail($"Install error: {ex.Message}");
        }
    }

    public Task<bool> IsInstalledAsync(string component, CancellationToken cancellationToken = default)
    {
        var sdk = SdkPathResolver.GetSdkRoot();
        if (component == "platform-tools")
        {
            return Task.FromResult(File.Exists(Path.Combine(sdk, "platform-tools", "adb.exe")));
        }
        if (component == "emulator")
        {
            return Task.FromResult(File.Exists(Path.Combine(sdk, "emulator", "emulator.exe")));
        }
        return Task.FromResult(Directory.Exists(Path.Combine(sdk, component)));
    }

    public Task<IReadOnlyList<SdkPackage>> GetInstalledPackagesAsync(CancellationToken cancellationToken = default)
    {
        var packages = new List<SdkPackage>();
        var sdk = SdkPathResolver.GetSdkRoot();

        if (File.Exists(Path.Combine(sdk, "platform-tools", "adb.exe")))
        {
            packages.Add(new SdkPackage
            {
                PackageName = SdkConstants.PlatformToolsPackage,
                DisplayName = "Android SDK Platform-Tools",
                InstallPath = Path.Combine(sdk, "platform-tools"),
                IsRequired = true
            });
        }

        if (File.Exists(Path.Combine(sdk, "emulator", "emulator.exe")))
        {
            packages.Add(new SdkPackage
            {
                PackageName = SdkConstants.DefaultEmulatorPackage,
                DisplayName = "Android Emulator",
                InstallPath = Path.Combine(sdk, "emulator"),
                IsRequired = true
            });
        }

        return Task.FromResult<IReadOnlyList<SdkPackage>>(packages);
    }
}
