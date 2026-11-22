using DroidBox.Core.Models;

namespace DroidBox.Core.Interfaces;

public interface IAndroidEnvironmentDetector
{
    Task<EnvironmentDetectionResult> DetectEnvironmentAsync(string? customSdkPath = null, CancellationToken cancellationToken = default);
}
