using DroidBox.Core.Models;

namespace DroidBox.Application.Services;

public interface ISdkSetupService
{
    Task<HealthReport> CheckHealthAsync(CancellationToken cancellationToken = default);
    Task<EnvironmentDetectionResult> DetectEnvironmentAsync(CancellationToken cancellationToken = default);
    Task<OperationResult> SetupAsync(IProgress<ProgressInfo>? progress = null, CancellationToken cancellationToken = default);
    Task<OperationResult> RunFullSetupAsync(bool acceptLicenses, IProgress<ProgressInfo>? progress = null, CancellationToken cancellationToken = default);
}
