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
                "Please verify your Android SDK location or run the DroidBox Setup wizard.");
        }

        // Ensure default AVDs exist
        AvdManager.EnsureDefaultAvdConfigured(SdkPathResolver.GetSdkRoot());

        _state = EmulatorState.Starting;

        // Choose available AVD: Tablet or Mobile
        var userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        var avdDir = Path.Combine(userProfile, ".android", "avd");
        string avdName = SdkConstants.TabletDeviceName;

        if (!File.Exists(Path.Combine(avdDir, $"{avdName}.ini")))
        {
            if (File.Exists(Path.Combine(avdDir, "DroidBox_Tablet.ini"))) avdName = "DroidBox_Tablet";
            else if (File.Exists(Path.Combine(avdDir, $"{SdkConstants.MobileDeviceName}.ini"))) avdName = SdkConstants.MobileDeviceName;
            else if (File.Exists(Path.Combine(avdDir, "DroidBox_Mobile.ini"))) avdName = "DroidBox_Mobile";
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

        try
        {
            _emulatorProcess = new Process { StartInfo = startInfo };
            _emulatorProcess.Start();

            // Wait briefly to verify process did not immediately crash
            await Task.Delay(2500, cancellationToken);

            if (_emulatorProcess.HasExited)
            {
                _state = EmulatorState.Error;
                var code = _emulatorProcess.ExitCode;
                _logger.LogError("Emulator failed to start. Exit code: {Code}", code);
                throw new InvalidOperationException($"Emulator process exited prematurely with code {code}");
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
        return Task.FromResult(_state == EmulatorState.Running && _emulatorProcess != null && !_emulatorProcess.HasExited);
    }
}
