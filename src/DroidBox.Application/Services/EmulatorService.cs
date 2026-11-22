using DroidBox.Core.Entities;
using DroidBox.Core.Enums;
using DroidBox.Core.Interfaces;
using DroidBox.Core.Models;
using Microsoft.Extensions.Logging;

namespace DroidBox.Application.Services;

public class EmulatorService : IEmulatorService
{
    private readonly IAndroidRuntime _runtime;
    private readonly ILogger<EmulatorService> _logger;
    private EmulatorInstance? _currentInstance;

    public EmulatorService(IAndroidRuntime runtime, ILogger<EmulatorService> logger)
    {
        _runtime = runtime;
        _logger = logger;
    }

    public async Task<OperationResult> StartAsync(string? avdName = null, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Starting emulator: {AvdName}", avdName ?? "default");
            await _runtime.StartAsync(cancellationToken);

            _currentInstance = new EmulatorInstance
            {
                AvdName = avdName ?? "DroidBox_Default",
                State = EmulatorState.Running,
                StartedAt = DateTime.UtcNow
            };

            return OperationResult.Ok("Emulator started successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start emulator");
            return OperationResult.Fail($"Failed to start emulator: {ex.Message}");
        }
    }

    public async Task<OperationResult> StopAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Stopping emulator");
            await _runtime.StopAsync(cancellationToken);

            if (_currentInstance != null)
            {
                _currentInstance.State = EmulatorState.Stopped;
            }

            return OperationResult.Ok("Emulator stopped successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to stop emulator");
            return OperationResult.Fail($"Failed to stop emulator: {ex.Message}");
        }
    }

    public async Task<EmulatorState> GetStateAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await _runtime.GetStateAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to query emulator state");
            return EmulatorState.Error;
        }
    }

    public async Task<bool> IsRunningAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await _runtime.IsRunningAsync(cancellationToken);
        }
        catch
        {
            return false;
        }
    }

    public Task<EmulatorInstance?> GetCurrentInstanceAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_currentInstance);
    }
}
