using DroidBox.Core.Entities;
using DroidBox.Core.Models;

namespace DroidBox.Core.Interfaces;

public interface IRuntimeInstaller
{
    Task<OperationResult> InstallAsync(string component, IProgress<ProgressInfo>? progress = null, CancellationToken cancellationToken = default);
    Task<bool> IsInstalledAsync(string component, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<SdkPackage>> GetInstalledPackagesAsync(CancellationToken cancellationToken = default);
}
