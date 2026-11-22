using DroidBox.Core.Interfaces;
using DroidBox.Core.Models;
using DroidBox.Shared.Helpers;
using Microsoft.Extensions.Logging;

namespace DroidBox.Application.Services;

public class SdkSetupService : ISdkSetupService
{
    private readonly IAndroidSdkManager _sdkManager;
    private readonly IRuntimeHealthChecker _healthChecker;
    private readonly ILogger<SdkSetupService> _logger;

    public SdkSetupService(
        IAndroidSdkManager sdkManager,
        IRuntimeHealthChecker healthChecker,
        ILogger<SdkSetupService> logger)
    {
        _sdkManager = sdkManager;
        _healthChecker = healthChecker;
        _logger = logger;
    }

    public Task<HealthReport> CheckHealthAsync(CancellationToken cancellationToken = default)
    {
        return _healthChecker.CheckHealthAsync(cancellationToken);
    }

    public Task<EnvironmentDetectionResult> DetectEnvironmentAsync(CancellationToken cancellationToken = default)
    {
        return _sdkManager.CheckStatusAsync(null, cancellationToken);
    }

    public Task<OperationResult> SetupAsync(IProgress<ProgressInfo>? progress = null, CancellationToken cancellationToken = default)
    {
        return _sdkManager.EnsureReadyAsync(progress, cancellationToken);
    }

    public Task<OperationResult> RunFullSetupAsync(bool acceptLicenses, IProgress<ProgressInfo>? progress = null, CancellationToken cancellationToken = default)
    {
        var sdkRoot = PathHelper.GetSdkDirectory();
        return _sdkManager.RunFullSetupAsync(sdkRoot, acceptLicenses, progress, cancellationToken);
    }
}
