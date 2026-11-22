using DroidBox.Core.Entities;
using DroidBox.Core.Enums;
using DroidBox.Core.Models;

namespace DroidBox.Application.Services;

public interface IEmulatorService
{
    Task<OperationResult> StartAsync(string? avdName = null, CancellationToken cancellationToken = default);
    Task<OperationResult> StopAsync(CancellationToken cancellationToken = default);
    Task<OperationResult> RestartAsync(string? avdName = null, CancellationToken cancellationToken = default);
    Task<OperationResult> StartDeviceAsync(string avdName, CancellationToken cancellationToken = default);
    Task<OperationResult> StopDeviceAsync(string? avdName = null, CancellationToken cancellationToken = default);
    Task<bool> IsDeviceRunningAsync(string avdName, CancellationToken cancellationToken = default);
    Task<EmulatorState> GetStateAsync(CancellationToken cancellationToken = default);
    Task<bool> IsRunningAsync(CancellationToken cancellationToken = default);
    Task<EmulatorInstance?> GetCurrentInstanceAsync(CancellationToken cancellationToken = default);
}
