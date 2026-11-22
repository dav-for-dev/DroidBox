using DroidBox.Core.Enums;
using DroidBox.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace DroidBox.Runtime.Qemu;

public class QemuProcessManager : IAndroidRuntime
{
    private readonly ILogger<QemuProcessManager> _logger;

    public QemuProcessManager(ILogger<QemuProcessManager> logger)
    {
        _logger = logger;
    }

    public Task StartAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogWarning("QEMU runtime start requested on experimental provider");
        throw new NotSupportedException("QEMU + Android-x86 runtime provider is currently experimental.");
    }

    public Task StopAsync(CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public Task<EmulatorState> GetStateAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(EmulatorState.Stopped);
    }

    public Task<bool> IsRunningAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(false);
    }
}
