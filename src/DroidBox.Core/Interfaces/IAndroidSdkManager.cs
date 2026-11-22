using DroidBox.Core.Models;

namespace DroidBox.Core.Interfaces;

public interface IAndroidSdkManager
{
    Task<EnvironmentDetectionResult> CheckStatusAsync(string? customSdkPath = null, CancellationToken cancellationToken = default);
    Task<OperationResult> RunFullSetupAsync(string targetSdkRoot, bool acceptLicenses, IProgress<ProgressInfo>? progress = null, CancellationToken cancellationToken = default);
    Task<OperationResult> EnsureReadyAsync(IProgress<ProgressInfo>? progress = null, CancellationToken cancellationToken = default);
}
