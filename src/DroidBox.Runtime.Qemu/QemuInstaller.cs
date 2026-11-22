using DroidBox.Core.Entities;
using DroidBox.Core.Interfaces;
using DroidBox.Core.Models;
using Microsoft.Extensions.Logging;

namespace DroidBox.Runtime.Qemu;

public class QemuInstaller : IRuntimeInstaller
{
    private readonly ILogger<QemuInstaller> _logger;

    public QemuInstaller(ILogger<QemuInstaller> logger)
    {
        _logger = logger;
    }

    public Task<OperationResult> InstallAsync(string component, IProgress<ProgressInfo>? progress = null, CancellationToken cancellationToken = default)
    {
        _logger.LogWarning("QEMU automated installer is an experimental feature and not yet implemented");
        return Task.FromResult(OperationResult.Fail("QEMU automated runtime installer is experimental and not yet available."));
    }

    public Task<bool> IsInstalledAsync(string component, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(false);
    }

    public Task<IReadOnlyList<SdkPackage>> GetInstalledPackagesAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IReadOnlyList<SdkPackage>>(Array.Empty<SdkPackage>());
    }
}
