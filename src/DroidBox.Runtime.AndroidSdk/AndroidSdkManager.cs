using DroidBox.Core.Interfaces;
using DroidBox.Core.Models;
using DroidBox.Shared.Helpers;
using Microsoft.Extensions.Logging;

namespace DroidBox.Runtime.AndroidSdk;

public class AndroidSdkManager : IAndroidSdkManager
{
    private readonly IAndroidEnvironmentDetector _environmentDetector;
    private readonly IAndroidLicenseManager _licenseManager;
    private readonly IAndroidSdkInstaller _sdkInstaller;
    private readonly IAvdManager _avdManager;
    private readonly IAdbClient _adbClient;
    private readonly ILogger<AndroidSdkManager> _logger;

    public AndroidSdkManager(
        IAndroidEnvironmentDetector environmentDetector,
        IAndroidLicenseManager licenseManager,
        IAndroidSdkInstaller sdkInstaller,
        IAvdManager avdManager,
        IAdbClient adbClient,
        ILogger<AndroidSdkManager> logger)
    {
        _environmentDetector = environmentDetector;
        _licenseManager = licenseManager;
        _sdkInstaller = sdkInstaller;
        _avdManager = avdManager;
        _adbClient = adbClient;
        _logger = logger;
    }

    public Task<EnvironmentDetectionResult> CheckStatusAsync(string? customSdkPath = null, CancellationToken cancellationToken = default)
    {
        return _environmentDetector.DetectEnvironmentAsync(customSdkPath, cancellationToken);
    }

    public async Task<OperationResult> RunFullSetupAsync(
        string targetSdkRoot,
        bool acceptLicenses,
        IProgress<ProgressInfo>? progress = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Starting DroidBox Android Runtime Setup at {Root}", targetSdkRoot);

            if (!acceptLicenses)
            {
                return OperationResult.Fail("You must accept the official Google Android SDK License Agreement before downloading components.");
            }

            // Step 1: Detect existing environment
            progress?.Report(new ProgressInfo { Percentage = 5, CurrentStep = 1, TotalSteps = 6, Message = "Detecting existing Android SDK and virtualization..." });
            var detection = await _environmentDetector.DetectEnvironmentAsync(targetSdkRoot, cancellationToken);

            // Step 2: Accept licenses
            progress?.Report(new ProgressInfo { Percentage = 15, CurrentStep = 2, TotalSteps = 6, Message = "Configuring official Google SDK licenses..." });
            var licRes = await _licenseManager.AcceptAllLicensesAsync(targetSdkRoot, cancellationToken);
            if (!licRes.Success) return licRes;

            // Step 3: Download & Install missing components
            if (detection.MissingComponents.Any(c => c.Contains("Platform-Tools") || c.Contains("Emulator") || c.Contains("SDK Root")))
            {
                progress?.Report(new ProgressInfo { Percentage = 25, CurrentStep = 3, TotalSteps = 6, Message = "Downloading official Google Android components..." });
                var installRes = await _sdkInstaller.InstallMissingComponentsAsync(targetSdkRoot, detection.MissingComponents, progress, cancellationToken);
                if (!installRes.Success) return installRes;
            }

            // Step 4: Provision Mobile & Tablet Virtual Devices
            progress?.Report(new ProgressInfo { Percentage = 70, CurrentStep = 4, TotalSteps = 6, Message = "Provisioning DroidBox-Mobile and DroidBox-Tablet AVDs..." });
            var avdRes = await _avdManager.EnsureAvdsCreatedAsync(targetSdkRoot, cancellationToken);
            if (!avdRes.Success) return avdRes;

            // Step 5: Start ADB server and verify communication
            progress?.Report(new ProgressInfo { Percentage = 85, CurrentStep = 5, TotalSteps = 6, Message = "Initializing local ADB server..." });
            await _adbClient.ConnectAsync(cancellationToken);

            // Step 6: Final check
            progress?.Report(new ProgressInfo { Percentage = 100, CurrentStep = 6, TotalSteps = 6, Message = "Setup complete! DroidBox environment is ready." });
            _logger.LogInformation("DroidBox Android Runtime Setup finished successfully.");

            return OperationResult.Ok("Official Android Emulator and Virtual Devices configured successfully.");
        }
        catch (OperationCanceledException)
        {
            return OperationResult.Fail("Setup was cancelled.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Android runtime setup encountered an error");
            return OperationResult.Fail($"Setup error: {ex.Message}");
        }
    }

    public async Task<OperationResult> EnsureReadyAsync(IProgress<ProgressInfo>? progress = null, CancellationToken cancellationToken = default)
    {
        var sdkRoot = SdkPathResolver.GetSdkRoot();
        var status = await CheckStatusAsync(sdkRoot, cancellationToken);
        if (status.IsFullyConfigured)
        {
            return OperationResult.Ok("Environment is ready.");
        }

        return await RunFullSetupAsync(sdkRoot, true, progress, cancellationToken);
    }
}
