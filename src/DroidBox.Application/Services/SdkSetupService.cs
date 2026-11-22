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
