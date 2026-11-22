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

        var emulatorPath = SdkPathResolver.GetEmulatorPath();
        if (string.IsNullOrEmpty(emulatorPath) || !File.Exists(emulatorPath))
        {
            _state = EmulatorState.Error;
            throw new FileNotFoundException(
                "Android Emulator (emulator.exe) was not found in your Android SDK installation. " +
                "Please verify your Android SDK location or install the Emulator component in Android Studio.");
        }

        // Ensure default AVD exists
        AvdManager.EnsureDefaultAvdConfigured(SdkPathResolver.GetSdkRoot());

        _state = EmulatorState.Starting;
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
                _logger.LogError("Emulator failed to start. Exit code: {Code}, Error: {Error}", _emulatorProcess.ExitCode, err);
                throw new InvalidOperationException($"Emulator process exited prematurely: {err}");
            }

            _state = EmulatorState.Running;
            _logger.LogInformation("Emulator process started successfully (PID: {Pid})", _emulatorProcess.Id);
        }
        catch (Exception ex)
        {
            _state = EmulatorState.Error;
            _logger.LogError(ex, "Exception starting emulator");
            throw;
        }
    }

    public Task StopAsync(CancellationToken cancellationToken = default)
    {
        if (_emulatorProcess != null && !_emulatorProcess.HasExited)
        {
            _logger.LogInformation("Stopping emulator process (PID: {Pid})", _emulatorProcess.Id);
            _state = EmulatorState.Stopping;
            _emulatorProcess.Kill(entireProcessTree: true);
            _emulatorProcess.Dispose();
            _emulatorProcess = null;
        }

        _state = EmulatorState.Stopped;
        return Task.CompletedTask;
    }

    public Task<EmulatorState> GetStateAsync(CancellationToken cancellationToken = default)
    {
        if (_emulatorProcess == null || _emulatorProcess.HasExited)
        {
            _state = EmulatorState.Stopped;
        }
        return Task.FromResult(_state);
    }

    public Task<bool> IsRunningAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_state == EmulatorState.Running);
    }
}
