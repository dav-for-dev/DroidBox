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
    private readonly IAdbClient _adbClient;
    private Process? _currentProcess;
    private string? _runningAvdName;

    public AndroidEmulatorManager(ILogger<AndroidEmulatorManager> logger, IAvdManager avdManager, IAdbClient adbClient)
    {
        _logger = logger;
        _avdManager = avdManager;
        _adbClient = adbClient;
    }

    public async Task<OperationResult> StartEmulatorAsync(string avdName, CancellationToken cancellationToken = default)
    {
        try
        {
            // If already running this AVD or any emulator device is connected via ADB, return ok!
            var isRunningAlready = await IsRunningAsync(avdName, cancellationToken);
            if (isRunningAlready)
            {
                _runningAvdName = avdName;
                _logger.LogInformation("AVD {Name} is already running", avdName);
                return OperationResult.Ok($"Emulator {avdName} is already running.");
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

            // Wait briefly to check if QEMU or emulator process is alive or ADB connects
            await Task.Delay(3000, cancellationToken);

            var isQemuRunning = Process.GetProcessesByName("qemu-system-x86_64").Any() ||
                                Process.GetProcessesByName("emulator").Any(p => !p.HasExited);
            var isAdb = await _adbClient.IsConnectedAsync(cancellationToken);

            if (!isQemuRunning && !isAdb && _currentProcess.HasExited && _currentProcess.ExitCode != 0)
            {
                var exitCode = _currentProcess.ExitCode;
                _currentProcess = null;
                _runningAvdName = null;
                return OperationResult.Fail($"Emulator process exited prematurely with code {exitCode}.");
            }

            return OperationResult.Ok($"Emulator {avdName} started successfully.");
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
            foreach (var p in Process.GetProcessesByName("emulator"))
            {
                try { p.Kill(entireProcessTree: true); } catch { }
            }
            foreach (var p in Process.GetProcessesByName("qemu-system-x86_64"))
            {
                try { p.Kill(entireProcessTree: true); } catch { }
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
        await Task.Delay(2000, cancellationToken);
        return await StartEmulatorAsync(avdName, cancellationToken);
    }

    public async Task<bool> IsRunningAsync(string? avdName = null, CancellationToken cancellationToken = default)
    {
        var isQemu = Process.GetProcessesByName("qemu-system-x86_64").Any() ||
                     Process.GetProcessesByName("emulator").Any(p => !p.HasExited);

        if (!isQemu)
        {
            return false;
        }

        var isAdb = await _adbClient.IsConnectedAsync(cancellationToken);
        return isQemu || isAdb;
    }

    public async Task<EmulatorState> GetStateAsync(string? avdName = null, CancellationToken cancellationToken = default)
    {
        var isRunning = await IsRunningAsync(avdName, cancellationToken);
        return isRunning ? EmulatorState.Running : EmulatorState.Stopped;
    }

    public Task<string?> GetActiveAvdNameAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_runningAvdName);
    }
}
