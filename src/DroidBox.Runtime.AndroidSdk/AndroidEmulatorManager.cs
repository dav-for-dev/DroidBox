using System.Diagnostics;
using DroidBox.Core.Enums;
using DroidBox.Core.Interfaces;
using DroidBox.Core.Models;
using DroidBox.Shared.Constants;
using DroidBox.Shared.Helpers;
using Microsoft.Extensions.Logging;

namespace DroidBox.Runtime.AndroidSdk;

public class AndroidEmulatorManager : IAndroidEmulatorManager
{
    private readonly ILogger<AndroidEmulatorManager> _logger;
    private readonly IAvdManager _avdManager;
    private Process? _currentProcess;
    private string? _runningAvdName;

    public AndroidEmulatorManager(ILogger<AndroidEmulatorManager> logger, IAvdManager avdManager)
    {
        _logger = logger;
        _avdManager = avdManager;
    }

    public async Task<OperationResult> StartEmulatorAsync(string avdName, CancellationToken cancellationToken = default)
    {
        try
        {
            // If already running this AVD, return ok
            if (_currentProcess != null && !_currentProcess.HasExited && string.Equals(_runningAvdName, avdName, StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogInformation("AVD {Name} is already running", avdName);
                return OperationResult.Ok($"Emulator {avdName} is already running");
            }

            // If another AVD is running, stop it to conserve RAM/CPU
            if (_currentProcess != null && !_currentProcess.HasExited)
            {
                _logger.LogInformation("Stopping existing AVD {Old} before starting {New}", _runningAvdName, avdName);
                await StopEmulatorAsync(_runningAvdName, cancellationToken);
            }

            var emulatorPath = SdkPathResolver.GetEmulatorPath();
            if (string.IsNullOrEmpty(emulatorPath) || !File.Exists(emulatorPath))
            {
                return OperationResult.Fail("Android Emulator executable (emulator.exe) was not found in the Android SDK.");
            }

            // Ensure AVD exists
            var exists = await _avdManager.AvdExistsAsync(avdName, cancellationToken);
            if (!exists)
            {
                _logger.LogInformation("AVD {Name} does not exist yet. Provisioning now...", avdName);
                await _avdManager.EnsureAvdsCreatedAsync(SdkPathResolver.GetSdkRoot(), cancellationToken);
            }

            var args = $"-avd {avdName} -no-boot-anim -gpu auto";
            if (WhpxDetector.IsWhpxAvailable())
            {
                args += " -accel whpx";
            }

            _logger.LogInformation("Launching emulator: {Path} {Args}", emulatorPath, args);

            var startInfo = new ProcessStartInfo
            {
                FileName = emulatorPath,
                Arguments = args,
                UseShellExecute = false,
                CreateNoWindow = false,
                WorkingDirectory = Path.GetDirectoryName(emulatorPath) ?? string.Empty
            };

            _currentProcess = new Process { StartInfo = startInfo };
            _currentProcess.Start();
            _runningAvdName = avdName;

            // Wait briefly to verify it didn't immediately crash
            await Task.Delay(2500, cancellationToken);

            if (_currentProcess.HasExited)
            {
                var exitCode = _currentProcess.ExitCode;
                _currentProcess = null;
                _runningAvdName = null;
                return OperationResult.Fail($"Emulator process exited prematurely with code {exitCode}.");
            }

            return OperationResult.Ok($"Emulator {avdName} started successfully (PID: {_currentProcess.Id}).");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start emulator for AVD {Name}", avdName);
            _currentProcess = null;
            _runningAvdName = null;
            return OperationResult.Fail($"Failed to start emulator: {ex.Message}");
        }
    }

    public Task<OperationResult> StopEmulatorAsync(string? avdName = null, CancellationToken cancellationToken = default)
    {
        try
        {
            if (_currentProcess != null && !_currentProcess.HasExited)
            {
                _logger.LogInformation("Stopping emulator process (PID: {Pid})", _currentProcess.Id);
                _currentProcess.Kill(entireProcessTree: true);
                _currentProcess.Dispose();
            }

            _currentProcess = null;
            _runningAvdName = null;
            return Task.FromResult(OperationResult.Ok("Emulator stopped successfully."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to stop emulator");
            return Task.FromResult(OperationResult.Fail($"Failed to stop emulator: {ex.Message}"));
        }
    }

    public async Task<OperationResult> RestartEmulatorAsync(string avdName, CancellationToken cancellationToken = default)
    {
        await StopEmulatorAsync(avdName, cancellationToken);
        await Task.Delay(1500, cancellationToken);
        return await StartEmulatorAsync(avdName, cancellationToken);
    }

    public Task<bool> IsRunningAsync(string? avdName = null, CancellationToken cancellationToken = default)
    {
        if (_currentProcess == null || _currentProcess.HasExited)
        {
            _currentProcess = null;
            _runningAvdName = null;
            return Task.FromResult(false);
        }

        if (string.IsNullOrWhiteSpace(avdName))
        {
            return Task.FromResult(true);
        }

        return Task.FromResult(string.Equals(_runningAvdName, avdName, StringComparison.OrdinalIgnoreCase));
    }

    public Task<EmulatorState> GetStateAsync(string? avdName = null, CancellationToken cancellationToken = default)
    {
        if (_currentProcess == null || _currentProcess.HasExited)
        {
            return Task.FromResult(EmulatorState.Stopped);
        }

        if (string.IsNullOrWhiteSpace(avdName) || string.Equals(_runningAvdName, avdName, StringComparison.OrdinalIgnoreCase))
        {
            return Task.FromResult(EmulatorState.Running);
        }

        return Task.FromResult(EmulatorState.Stopped);
    }

    public Task<string?> GetActiveAvdNameAsync(CancellationToken cancellationToken = default)
    {
        if (_currentProcess != null && !_currentProcess.HasExited)
        {
            return Task.FromResult(_runningAvdName);
        }
        return Task.FromResult<string?>(null);
    }
}
