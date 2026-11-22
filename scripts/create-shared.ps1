$ErrorActionPreference = "Stop"

Set-Location "E:\code-from-haris\DroidBox\src\DroidBox.Shared"

New-Item -ItemType Directory -Path "Constants" -Force | Out-Null
New-Item -ItemType Directory -Path "Helpers" -Force | Out-Null

# --- CONSTANTS ---
@'
namespace DroidBox.Shared.Constants;

public static class AppConstants
{
    public const string AppName = "DroidBox";
    public const string AppVersion = "0.1.0";
    public const string Tagline = "Install. Run. Test. Without a Physical Device.";
    public const string PipeName = "DroidBox.Ipc.Pipe";
    public const int DefaultAdbPort = 5555;
    public const int DefaultEmulatorPort = 5554;
    public const string DefaultAvdName = "DroidBox_Default";
}
'@ | Set-Content -Path "Constants\AppConstants.cs"

@'
namespace DroidBox.Shared.Constants;

public static class SdkConstants
{
    public const string CmdlineToolsUrl = "https://dl.google.com/android/repository/commandlinetools-win-11076708_latest.zip";
    public const string DefaultSystemImage = "system-images;android-34;google_apis;x86_64";
    public const string DefaultPlatform = "platforms;android-34";
    public const string DefaultEmulatorPackage = "emulator";
    public const string PlatformToolsPackage = "platform-tools";
    public const string CmdlineToolsPackage = "cmdline-tools;latest";
    public const int DefaultApiLevel = 34;
}
'@ | Set-Content -Path "Constants\SdkConstants.cs"

@'
namespace DroidBox.Shared.Constants;

public static class FileExtensions
{
    public const string Apk = ".apk";
    public const string Xapk = ".xapk";
    public const string Apks = ".apks";
    public const string Aab = ".aab";
}
'@ | Set-Content -Path "Constants\FileExtensions.cs"

# --- HELPERS ---
@'
namespace DroidBox.Shared.Helpers;

public static class PathHelper
{
    private static readonly string LocalAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

    public static string GetDataDirectory()
    {
        var path = Path.Combine(LocalAppData, "DroidBox");
        EnsureDirectoryExists(path);
        return path;
    }

    public static string GetSdkDirectory()
    {
        var envHome = Environment.GetEnvironmentVariable("ANDROID_HOME") 
                   ?? Environment.GetEnvironmentVariable("ANDROID_SDK_ROOT");

        if (!string.IsNullOrWhiteSpace(envHome) && Directory.Exists(envHome))
        {
            return envHome;
        }

        var path = Path.Combine(GetDataDirectory(), "android-sdk");
        EnsureDirectoryExists(path);
        return path;
    }

    public static string GetAvdDirectory()
    {
        var path = Path.Combine(GetDataDirectory(), "avd");
        EnsureDirectoryExists(path);
        return path;
    }

    public static string GetLogsDirectory()
    {
        var path = Path.Combine(GetDataDirectory(), "logs");
        EnsureDirectoryExists(path);
        return path;
    }

    public static string GetDatabasePath()
    {
        return Path.Combine(GetDataDirectory(), "droidbox.db");
    }

    public static string GetDownloadsDirectory()
    {
        var path = Path.Combine(GetDataDirectory(), "downloads");
        EnsureDirectoryExists(path);
        return path;
    }

    public static string GetDocsDirectory()
    {
        var appDir = AppDomain.CurrentDomain.BaseDirectory;
        var path = Path.Combine(appDir, "docs");
        if (!Directory.Exists(path))
        {
            path = Path.Combine(GetDataDirectory(), "docs");
            EnsureDirectoryExists(path);
        }
        return path;
    }

    public static void EnsureDirectoryExists(string path)
    {
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
    }
}
'@ | Set-Content -Path "Helpers\PathHelper.cs"

@'
using System.Diagnostics;
using System.Text;

namespace DroidBox.Shared.Helpers;

public record ProcessResult(int ExitCode, string StandardOutput, string StandardError)
{
    public bool Success => ExitCode == 0;
}

public static class ProcessHelper
{
    public static async Task<ProcessResult> RunProcessAsync(
        string executable,
        string arguments,
        string? workingDirectory = null,
        IDictionary<string, string>? environmentVariables = null,
        int timeoutMs = 60000,
        CancellationToken cancellationToken = default)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = executable,
            Arguments = arguments,
            WorkingDirectory = workingDirectory ?? Directory.GetCurrentDirectory(),
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            StandardOutputEncoding = Encoding.UTF8,
            StandardErrorEncoding = Encoding.UTF8
        };

        if (environmentVariables != null)
        {
            foreach (var kvp in environmentVariables)
            {
                startInfo.EnvironmentVariables[kvp.Key] = kvp.Value;
            }
        }

        using var process = new Process { StartInfo = startInfo };
        var outputBuilder = new StringBuilder();
        var errorBuilder = new StringBuilder();

        process.OutputDataReceived += (_, e) =>
        {
            if (e.Data != null) outputBuilder.AppendLine(e.Data);
        };
        process.ErrorDataReceived += (_, e) =>
        {
            if (e.Data != null) errorBuilder.AppendLine(e.Data);
        };

        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        using var timeoutCts = new CancellationTokenSource(timeoutMs);
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);

        try
        {
            await process.WaitForExitAsync(linkedCts.Token);
            return new ProcessResult(process.ExitCode, outputBuilder.ToString(), errorBuilder.ToString());
        }
        catch (OperationCanceledException)
        {
            try
            {
                if (!process.HasExited)
                {
                    process.Kill(true);
                }
            }
            catch
            {
                // Ignore process kill failure
            }

            if (timeoutCts.IsCancellationRequested)
            {
                return new ProcessResult(-1, outputBuilder.ToString(), $"Process timed out after {timeoutMs}ms");
            }
            throw;
        }
    }
}
'@ | Set-Content -Path "Helpers\ProcessHelper.cs"

@'
using System.Security.Cryptography;

namespace DroidBox.Shared.Helpers;

public static class HashHelper
{
    public static async Task<string> ComputeSha256Async(string filePath, CancellationToken cancellationToken = default)
    {
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException("File not found for hash calculation", filePath);
        }

        using var sha256 = SHA256.Create();
        using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, true);
        var hashBytes = await sha256.ComputeHashAsync(stream);
        return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
    }

    public static bool VerifyHash(string filePath, string expectedHash)
    {
        var computed = ComputeSha256Async(filePath).GetAwaiter().GetResult();
        return string.Equals(computed, expectedHash.Trim(), StringComparison.OrdinalIgnoreCase);
    }
}
'@ | Set-Content -Path "Helpers\HashHelper.cs"

@'
using System.Runtime.InteropServices;

namespace DroidBox.Shared.Helpers;

public static class PlatformHelper
{
    public static bool IsWindows10OrLater()
    {
        return RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && Environment.OSVersion.Version.Major >= 10;
    }

    public static bool IsVirtualizationEnabled()
    {
        try
        {
            // Simple heuristic check: on modern Windows 10/11 x64, hypervisor is detected if WHPX/Hyper-V is active
            return Environment.Is64BitOperatingSystem;
        }
        catch
        {
            return false;
        }
    }

    public static long GetAvailableRamMb()
    {
        try
        {
            var gcMemoryInfo = GC.GetGCMemoryInfo();
            return gcMemoryInfo.TotalAvailableMemoryBytes / (1024 * 1024);
        }
        catch
        {
            return 4096; // Fallback 4GB
        }
    }

    public static long GetFreeDiskSpaceMb(string path)
    {
        try
        {
            var root = Path.GetPathRoot(Path.GetFullPath(path));
            if (string.IsNullOrEmpty(root)) return 0;
            var drive = new DriveInfo(root);
            return drive.AvailableFreeSpace / (1024 * 1024);
        }
        catch
        {
            return 10240; // Fallback 10GB
        }
    }
}
'@ | Set-Content -Path "Helpers\PlatformHelper.cs"

Write-Host "DroidBox.Shared files created successfully!"
