using DroidBox.Core.Interfaces;
using DroidBox.Core.Models;
using DroidBox.Shared.Helpers;
using Microsoft.Extensions.Logging;

namespace DroidBox.Runtime.AndroidSdk;

public class AndroidSdkProvider : IRuntimeProvider, IRuntimeHealthChecker, IScreenshotService
{
    private readonly ILogger<AndroidSdkProvider> _logger;

    public string Name => "AndroidSdk";

    public AndroidSdkProvider(ILogger<AndroidSdkProvider> logger)
    {
        _logger = logger;
    }

    public Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default)
    {
        var sdk = SdkPathResolver.GetSdkRoot();
        var adb = SdkPathResolver.GetAdbPath();
        var available = Directory.Exists(sdk) || File.Exists(adb);
        return Task.FromResult(available);
    }

    public Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Initializing Android SDK Runtime Provider");
        PathHelper.EnsureDirectoryExists(SdkPathResolver.GetSdkRoot());
        return Task.CompletedTask;
    }

    public RuntimeCapabilities GetCapabilities() => new()
    {
        SupportsGpu = true,
        SupportsAudio = true,
        SupportsCamera = true,
        SupportsSensors = true,
        SupportsMultiTouch = true,
        MaxRamMb = 8192,
        SupportedAbis = new[] { "x86_64", "arm64-v8a", "x86", "armeabi-v7a" }
    };

    public Task<HealthReport> CheckHealthAsync(CancellationToken cancellationToken = default)
    {
        var components = new List<HealthComponent>
        {
            new()
            {
                Name = "Windows OS",
                IsHealthy = PlatformHelper.IsWindows10OrLater(),
                Status = PlatformHelper.IsWindows10OrLater() ? "OK" : "Warning: Windows 10+ recommended",
                Details = Environment.OSVersion.VersionString
            },
            new()
            {
                Name = "Hardware Virtualization (WHPX)",
                IsHealthy = WhpxDetector.IsWhpxAvailable(),
                Status = WhpxDetector.IsWhpxAvailable() ? "Enabled" : "Disabled / Not Detected",
                Details = "Hardware acceleration provides near-native performance"
            },
            new()
            {
                Name = "Available RAM",
                IsHealthy = PlatformHelper.GetAvailableRamMb() >= 2048,
                Status = $"{PlatformHelper.GetAvailableRamMb()} MB available",
                Details = "At least 2048 MB recommended"
            },
            new()
            {
                Name = "Free Disk Space",
                IsHealthy = PlatformHelper.GetFreeDiskSpaceMb(PathHelper.GetDataDirectory()) >= 4096,
                Status = $"{PlatformHelper.GetFreeDiskSpaceMb(PathHelper.GetDataDirectory())} MB free",
                Details = "At least 4096 MB recommended for SDK and AVDs"
            },
            new()
            {
                Name = "Android SDK",
                IsHealthy = Directory.Exists(SdkPathResolver.GetSdkRoot()),
                Status = Directory.Exists(SdkPathResolver.GetSdkRoot()) ? "Detected" : "Not Installed",
                Details = SdkPathResolver.GetSdkRoot()
            }
        };

        return Task.FromResult(new HealthReport { Components = components });
    }

    public Task<IDictionary<string, string>> GetDiagnosticsAsync(CancellationToken cancellationToken = default)
    {
        IDictionary<string, string> diag = new Dictionary<string, string>
        {
            ["SdkRoot"] = SdkPathResolver.GetSdkRoot(),
            ["AdbPath"] = SdkPathResolver.GetAdbPath(),
            ["EmulatorPath"] = SdkPathResolver.GetEmulatorPath(),
            ["AvdManagerPath"] = SdkPathResolver.GetAvdManagerPath(),
            ["WhpxAvailable"] = WhpxDetector.IsWhpxAvailable().ToString(),
            ["OSVersion"] = Environment.OSVersion.ToString(),
            ["RamMb"] = PlatformHelper.GetAvailableRamMb().ToString()
        };
        return Task.FromResult(diag);
    }

    public async Task<byte[]> CaptureAsync(CancellationToken cancellationToken = default)
    {
        var adb = SdkPathResolver.GetAdbPath();
        var tempPng = Path.Combine(Path.GetTempPath(), $"droidbox_screen_{Guid.NewGuid():N}.png");

        try
        {
            await ProcessHelper.RunProcessAsync(adb, "shell screencap -p /sdcard/screenshot.png", cancellationToken: cancellationToken);
            await ProcessHelper.RunProcessAsync(adb, $"pull /sdcard/screenshot.png \"{tempPng}\"", cancellationToken: cancellationToken);

            if (File.Exists(tempPng))
            {
                var bytes = await File.ReadAllBytesAsync(tempPng, cancellationToken);
                return bytes;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "screencap failed, generating placeholder frame");
        }
        finally
        {
            if (File.Exists(tempPng))
            {
                try { File.Delete(tempPng); } catch { }
            }
        }

        // Return empty byte array if screencap not active
        return Array.Empty<byte>();
    }

    public Task StartStreamingAsync(Action<byte[]> onFrame, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public Task StopStreamingAsync(CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}
