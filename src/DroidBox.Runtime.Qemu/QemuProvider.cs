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
