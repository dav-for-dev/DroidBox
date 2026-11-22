using DroidBox.Core.Enums;
using DroidBox.Core.Models;

namespace DroidBox.Core.Interfaces;

public interface IAndroidEmulatorManager
{
    Task<OperationResult> StartEmulatorAsync(string avdName, CancellationToken cancellationToken = default);
    Task<OperationResult> StopEmulatorAsync(string? avdName = null, CancellationToken cancellationToken = default);
    Task<OperationResult> RestartEmulatorAsync(string avdName, CancellationToken cancellationToken = default);
    Task<bool> IsRunningAsync(string? avdName = null, CancellationToken cancellationToken = default);
    Task<EmulatorState> GetStateAsync(string? avdName = null, CancellationToken cancellationToken = default);
    Task<string?> GetActiveAvdNameAsync(CancellationToken cancellationToken = default);
}
