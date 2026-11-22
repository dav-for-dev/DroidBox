using DroidBox.Core.Entities;
using DroidBox.Core.Enums;
using DroidBox.Core.Interfaces;
using DroidBox.Core.Models;
using DroidBox.Shared.Constants;
using Microsoft.Extensions.Logging;

namespace DroidBox.Application.Services;

public class EmulatorService : IEmulatorService
{
    private readonly IAndroidRuntime _runtime;
    private readonly IAndroidEmulatorManager _emulatorManager;
    private readonly ILogger<EmulatorService> _logger;
    private EmulatorInstance? _currentInstance;

    public EmulatorService(
        IAndroidRuntime runtime,
        IAndroidEmulatorManager emulatorManager,
        ILogger<EmulatorService> logger)
    {
        _runtime = runtime;
        _emulatorManager = emulatorManager;
        _logger = logger;
    }

    public async Task<OperationResult> StartAsync(string? avdName = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var targetAvd = avdName ?? SdkConstants.TabletDeviceName;
            _logger.LogInformation("Starting emulator for AVD: {AvdName}", targetAvd);

            var res = await _emulatorManager.StartEmulatorAsync(targetAvd, cancellationToken);
            if (!res.Success)
            {
                // Fallback to general runtime start if emulator manager had an issue
                await _runtime.StartAsync(cancellationToken);
            }

            _currentInstance = new EmulatorInstance
            {
                AvdName = targetAvd,
                State = EmulatorState.Running,
                StartedAt = DateTime.UtcNow
            };

            return OperationResult.Ok($"Emulator {targetAvd} started successfully.");
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
            var res = await _emulatorManager.StopEmulatorAsync(null, cancellationToken);
            await _runtime.StopAsync(cancellationToken);

            if (_currentInstance != null)
            {
                _currentInstance.State = EmulatorState.Stopped;
            }

            return res.Success ? res : OperationResult.Ok("Emulator stopped successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to stop emulator");
            return OperationResult.Fail($"Failed to stop emulator: {ex.Message}");
        }
    }

    public async Task<OperationResult> RestartAsync(string? avdName = null, CancellationToken cancellationToken = default)
    {
        var targetAvd = avdName ?? _currentInstance?.AvdName ?? SdkConstants.TabletDeviceName;
        return await _emulatorManager.RestartEmulatorAsync(targetAvd, cancellationToken);
    }

    public Task<OperationResult> StartDeviceAsync(string avdName, CancellationToken cancellationToken = default)
    {
        return _emulatorManager.StartEmulatorAsync(avdName, cancellationToken);
    }

    public Task<OperationResult> StopDeviceAsync(string? avdName = null, CancellationToken cancellationToken = default)
    {
        return _emulatorManager.StopEmulatorAsync(avdName, cancellationToken);
    }

    public Task<bool> IsDeviceRunningAsync(string avdName, CancellationToken cancellationToken = default)
    {
        return _emulatorManager.IsRunningAsync(avdName, cancellationToken);
    }

    public async Task<EmulatorState> GetStateAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var isRunning = await _emulatorManager.IsRunningAsync(null, cancellationToken);
            return isRunning ? EmulatorState.Running : await _runtime.GetStateAsync(cancellationToken);
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
            return await _emulatorManager.IsRunningAsync(null, cancellationToken) || await _runtime.IsRunningAsync(cancellationToken);
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
