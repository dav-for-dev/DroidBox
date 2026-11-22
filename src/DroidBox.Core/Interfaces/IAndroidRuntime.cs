using DroidBox.Core.Enums;

namespace DroidBox.Core.Interfaces;

public interface IAndroidRuntime
{
    Task StartAsync(CancellationToken cancellationToken = default);
    Task StopAsync(CancellationToken cancellationToken = default);
    Task<EmulatorState> GetStateAsync(CancellationToken cancellationToken = default);
    Task<bool> IsRunningAsync(CancellationToken cancellationToken = default);
}
