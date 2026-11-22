$ErrorActionPreference = "Stop"

Set-Location "E:\code-from-haris\DroidBox\src\DroidBox.Runtime.Qemu"

# --- QemuProvider.cs ---
@'
using DroidBox.Core.Interfaces;
using DroidBox.Core.Models;
using Microsoft.Extensions.Logging;

namespace DroidBox.Runtime.Qemu;

/// <summary>
/// Experimental secondary runtime provider using QEMU and Android-x86.
/// Note: This is an experimental preview provider and not yet intended for primary production workflows.
/// </summary>
public class QemuProvider : IRuntimeProvider
{
    private readonly ILogger<QemuProvider> _logger;

    public string Name => "QEMU (Experimental)";

    public QemuProvider(ILogger<QemuProvider> logger)
    {
        _logger = logger;
    }

    public Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default)
    {
        // Check if qemu-system-x86_64 exists on PATH or standard install dirs
        _logger.LogInformation("Checking QEMU availability (Experimental)");
        return Task.FromResult(false); // Currently experimental / opt-in
    }

    public Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("QEMU provider initialized in experimental mode");
        return Task.CompletedTask;
    }

    public RuntimeCapabilities GetCapabilities() => new()
    {
        SupportsGpu = false,
        SupportsAudio = false,
        SupportsCamera = false,
        SupportsSensors = false,
        SupportsMultiTouch = false,
        MaxRamMb = 4096,
        SupportedAbis = new[] { "x86_64", "x86" }
    };
}
'@ | Set-Content -Path "QemuProvider.cs"

# --- QemuInstaller.cs ---
@'
using DroidBox.Core.Entities;
using DroidBox.Core.Interfaces;
using DroidBox.Core.Models;
using Microsoft.Extensions.Logging;

namespace DroidBox.Runtime.Qemu;

public class QemuInstaller : IRuntimeInstaller
{
    private readonly ILogger<QemuInstaller> _logger;

    public QemuInstaller(ILogger<QemuInstaller> logger)
    {
        _logger = logger;
    }

    public Task<OperationResult> InstallAsync(string component, IProgress<ProgressInfo>? progress = null, CancellationToken cancellationToken = default)
    {
        _logger.LogWarning("QEMU automated installer is an experimental feature and not yet implemented");
        return Task.FromResult(OperationResult.Fail("QEMU automated runtime installer is experimental and not yet available."));
    }

    public Task<bool> IsInstalledAsync(string component, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(false);
    }

    public Task<IReadOnlyList<SdkPackage>> GetInstalledPackagesAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IReadOnlyList<SdkPackage>>(Array.Empty<SdkPackage>());
    }
}
'@ | Set-Content -Path "QemuInstaller.cs"

# --- QemuProcessManager.cs ---
@'
using DroidBox.Core.Enums;
using DroidBox.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace DroidBox.Runtime.Qemu;

public class QemuProcessManager : IAndroidRuntime
{
    private readonly ILogger<QemuProcessManager> _logger;

    public QemuProcessManager(ILogger<QemuProcessManager> logger)
    {
        _logger = logger;
    }

    public Task StartAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogWarning("QEMU runtime start requested on experimental provider");
        throw new NotSupportedException("QEMU + Android-x86 runtime provider is currently experimental.");
    }

    public Task StopAsync(CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public Task<EmulatorState> GetStateAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(EmulatorState.Stopped);
    }

    public Task<bool> IsRunningAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(false);
    }
}
'@ | Set-Content -Path "QemuProcessManager.cs"

# --- DependencyInjection.cs ---
@'
using DroidBox.Core.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace DroidBox.Runtime.Qemu;

public static class DependencyInjection
{
    public static IServiceCollection AddQemuRuntime(this IServiceCollection services)
    {
        services.AddSingleton<QemuProvider>();
        return services;
    }
}
'@ | Set-Content -Path "DependencyInjection.cs"

Write-Host "DroidBox.Runtime.Qemu files created successfully!"
