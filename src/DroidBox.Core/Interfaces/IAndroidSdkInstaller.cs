using DroidBox.Core.Models;

namespace DroidBox.Core.Interfaces;

public interface IAndroidSdkInstaller
{
    Task<OperationResult> InstallMissingComponentsAsync(
        string targetSdkRoot,
        IReadOnlyList<string> components,
        IProgress<ProgressInfo>? progress = null,
        CancellationToken cancellationToken = default);
}
