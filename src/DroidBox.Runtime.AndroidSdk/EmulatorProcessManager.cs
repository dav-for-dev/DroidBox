using System.Diagnostics;
using DroidBox.Core.Enums;
using DroidBox.Core.Interfaces;
using DroidBox.Shared.Constants;
using DroidBox.Shared.Helpers;
using Microsoft.Extensions.Logging;

namespace DroidBox.Runtime.AndroidSdk;

public class EmulatorProcessManager : IAndroidRuntime
{
    private readonly ILogger<EmulatorProcessManager> _logger;
    private Process? _emulatorProcess;
    private EmulatorState _state = EmulatorState.Stopped;

    public EmulatorProcessManager(ILogger<EmulatorProcessManager> logger)
    {
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        if (_state == EmulatorState.Running || _state == EmulatorState.Starting)
        {
            _logger.LogInformation("Emulator is already running or starting");
            return;
        }

        _state = EmulatorState.Starting;
        var emulatorPath = SdkPathResolver.GetEmulatorPath();
        var avdName = AppConstants.DefaultAvdName;

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
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };

        try
        {
            _emulatorProcess = new Process { StartInfo = startInfo };
            _emulatorProcess.Start();

            // Wait briefly for process to initialize
            await Task.Delay(2000, cancellationToken);

            if (_emulatorProcess.HasExited)
            {
                _state = EmulatorState.Error;
                var err = await _emulatorProcess.StandardError.ReadToEndAsync(cancellationToken);
                _logger.LogError("Emulator process exited immediately with code {Code}: {Error}", _emulatorProcess.ExitCode, err);
                throw new InvalidOperationException($"Emulator failed to start: {err}");
            }

            _state = EmulatorState.Running;
            _logger.LogInformation("Emulator process started with PID {Pid}", _emulatorProcess.Id);
        }
        catch (Exception ex)
        {
            _state = EmulatorState.Error;
            _logger.LogError(ex, "Failed to start emulator process");
            throw;
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken = default)
    {
        if (_emulatorProcess == null || _emulatorProcess.HasExited)
        {
            _state = EmulatorState.Stopped;
            return;
        }

        _state = EmulatorState.Stopping;
        _logger.LogInformation("Stopping emulator process (PID {Pid})", _emulatorProcess.Id);

        try
        {
            // Graceful stop via adb emu kill
            var adb = SdkPathResolver.GetAdbPath();
            await ProcessHelper.RunProcessAsync(adb, "emu kill", timeoutMs: 5000, cancellationToken: cancellationToken);

            using var timeoutCts = new CancellationTokenSource(5000);
            using var linked = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);
            await _emulatorProcess.WaitForExitAsync(linked.Token);
        }
        catch
        {
            try
            {
                if (!_emulatorProcess.HasExited)
                {
                    _emulatorProcess.Kill(true);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error while killing emulator process");
            }
        }
        finally
        {
            _emulatorProcess?.Dispose();
            _emulatorProcess = null;
            _state = EmulatorState.Stopped;
            _logger.LogInformation("Emulator stopped");
        }
    }

    public Task<EmulatorState> GetStateAsync(CancellationToken cancellationToken = default)
    {
        if (_emulatorProcess != null && _emulatorProcess.HasExited)
        {
            _state = EmulatorState.Stopped;
        }
        return Task.FromResult(_state);
    }

    public Task<bool> IsRunningAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_state == EmulatorState.Running && _emulatorProcess != null && !_emulatorProcess.HasExited);
    }
}
