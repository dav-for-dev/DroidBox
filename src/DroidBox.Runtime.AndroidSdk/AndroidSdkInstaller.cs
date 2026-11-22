using System.IO.Compression;
using DroidBox.Core.Entities;
using DroidBox.Core.Interfaces;
using DroidBox.Core.Models;
using DroidBox.Shared.Constants;
using DroidBox.Shared.Helpers;
using Microsoft.Extensions.Logging;

namespace DroidBox.Runtime.AndroidSdk;

public class AndroidSdkInstaller : IAndroidSdkInstaller, IRuntimeInstaller
{
    private readonly ILogger<AndroidSdkInstaller> _logger;
    private readonly HttpClient _httpClient;

    public AndroidSdkInstaller(ILogger<AndroidSdkInstaller> logger)
    {
        _logger = logger;
        _httpClient = new HttpClient { Timeout = TimeSpan.FromMinutes(10) };
    }

    public async Task<OperationResult> InstallMissingComponentsAsync(
        string targetSdkRoot,
        IReadOnlyList<string> components,
        IProgress<ProgressInfo>? progress = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            Directory.CreateDirectory(targetSdkRoot);

            // 1. Check disk space (minimum 3 GB required for setup)
            var targetDrive = Path.GetPathRoot(targetSdkRoot) ?? "C:\\";
            var drive = new DriveInfo(targetDrive);
            if (drive.AvailableFreeSpace < 3L * 1024 * 1024 * 1024)
            {
                var freeGb = Math.Round(drive.AvailableFreeSpace / (1024.0 * 1024 * 1024), 1);
                return OperationResult.Fail($"Insufficient disk space on {targetDrive}. Only {freeGb} GB available, at least 3.0 GB required.");
            }

            int step = 1;
            int totalSteps = 3;

            // 2. Download Platform Tools if missing
            var adbPath = Path.Combine(targetSdkRoot, "platform-tools", "adb.exe");
            if (!File.Exists(adbPath))
            {
                progress?.Report(new ProgressInfo
                {
                    Percentage = 15,
                    CurrentStep = step++,
                    TotalSteps = totalSteps,
                    Message = "Downloading official Google Android Platform Tools (ADB)..."
                });

                var dlResult = await DownloadAndExtractZipAsync(
                    SdkConstants.PlatformToolsUrl,
                    targetSdkRoot,
                    "platform-tools.zip",
                    progress,
                    cancellationToken);

                if (!dlResult.Success) return dlResult;
            }

            // 3. Download Commandline Tools if missing
            var cmdlineDir = Path.Combine(targetSdkRoot, "cmdline-tools", "latest");
            var sdkManagerBat = Path.Combine(cmdlineDir, "bin", "sdkmanager.bat");
            if (!File.Exists(sdkManagerBat))
            {
                progress?.Report(new ProgressInfo
                {
                    Percentage = 40,
                    CurrentStep = step++,
                    TotalSteps = totalSteps,
                    Message = "Downloading official Google Android Command-line Tools..."
                });

                var tempExtractDir = Path.Combine(Path.GetTempPath(), "droidbox_cmdline");
                var dlResult = await DownloadAndExtractZipAsync(
                    SdkConstants.CmdlineToolsUrl,
                    tempExtractDir,
                    "commandlinetools.zip",
                    progress,
                    cancellationToken);

                if (dlResult.Success)
                {
                    Directory.CreateDirectory(Path.Combine(targetSdkRoot, "cmdline-tools"));
                    var extractedCmdline = Path.Combine(tempExtractDir, "cmdline-tools");
                    if (Directory.Exists(extractedCmdline))
                    {
                        if (Directory.Exists(cmdlineDir)) Directory.Delete(cmdlineDir, true);
                        Directory.Move(extractedCmdline, cmdlineDir);
                    }
                    try { Directory.Delete(tempExtractDir, true); } catch { }
                }
            }

            // 4. Install Emulator & System Image via sdkmanager if cmdline-tools is available
            if (File.Exists(sdkManagerBat))
            {
                progress?.Report(new ProgressInfo
                {
                    Percentage = 70,
                    CurrentStep = step++,
                    TotalSteps = totalSteps,
                    Message = "Configuring official Android Emulator & system packages..."
                });

                await ProcessHelper.RunProcessAsync(
                    sdkManagerBat,
                    $"\"emulator\" \"{SdkConstants.DefaultPlatform}\" --sdk_root=\"{targetSdkRoot}\"",
                    timeoutMs: 300000,
                    cancellationToken: cancellationToken);
            }

            progress?.Report(new ProgressInfo
            {
                Percentage = 100,
                CurrentStep = totalSteps,
                TotalSteps = totalSteps,
                Message = "Android SDK components installed successfully."
            });

            return OperationResult.Ok("Official Android SDK components installed successfully.");
        }
        catch (OperationCanceledException)
        {
            return OperationResult.Fail("Installation cancelled by user.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to install missing Android SDK components");
            return OperationResult.Fail($"Installation error: {ex.Message}");
        }
    }

    private async Task<OperationResult> DownloadAndExtractZipAsync(
        string url,
        string destinationDir,
        string tempFileName,
        IProgress<ProgressInfo>? progress,
        CancellationToken cancellationToken)
    {
        var tempZip = Path.Combine(Path.GetTempPath(), tempFileName);
        const int maxRetries = 3;

        for (int attempt = 1; attempt <= maxRetries; attempt++)
        {
            try
            {
                if (File.Exists(tempZip)) File.Delete(tempZip);

                using (var response = await _httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cancellationToken))
                {
                    response.EnsureSuccessStatusCode();

                    var totalBytes = response.Content.Headers.ContentLength ?? -1L;
                    using var contentStream = await response.Content.ReadAsStreamAsync(cancellationToken);
                    using var fileStream = new FileStream(tempZip, FileMode.Create, FileAccess.Write, FileShare.None, 81920, true);

                    var buffer = new byte[81920];
                    long totalRead = 0;
                    int bytesRead;

                    while ((bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length, cancellationToken)) > 0)
                    {
                        await fileStream.WriteAsync(buffer, 0, bytesRead, cancellationToken);
                        totalRead += bytesRead;

                        if (totalBytes > 0)
                        {
                            var pct = (double)totalRead / totalBytes * 100.0;
                            var mb = Math.Round(totalRead / (1024.0 * 1024.0), 1);
                            var totalMb = Math.Round(totalBytes / (1024.0 * 1024.0), 1);
                            progress?.Report(new ProgressInfo
                            {
                                Percentage = pct,
                                Message = $"Downloading {tempFileName}: {mb} MB / {totalMb} MB..."
                            });
                        }
                    }
                }

                // Extract Zip
                Directory.CreateDirectory(destinationDir);
                ZipFile.ExtractToDirectory(tempZip, destinationDir, overwriteFiles: true);

                try { File.Delete(tempZip); } catch { }
                return OperationResult.Ok("Download and extraction complete.");
            }
            catch (OperationCanceledException)
            {
                try { if (File.Exists(tempZip)) File.Delete(tempZip); } catch { }
                throw;
            }
            catch (Exception ex) when (attempt < maxRetries)
            {
                _logger.LogWarning(ex, "Download attempt {Attempt} failed for {Url}. Retrying in 2 seconds...", attempt, url);
                await Task.Delay(2000, cancellationToken);
            }
            catch (Exception ex)
            {
                try { if (File.Exists(tempZip)) File.Delete(tempZip); } catch { }
                return OperationResult.Fail($"Failed to download {tempFileName} after {maxRetries} attempts: {ex.Message}");
            }
        }

        return OperationResult.Fail($"Failed to download {tempFileName}.");
    }

    // IRuntimeInstaller implementation for backwards compatibility
    public async Task<OperationResult> InstallAsync(string component, IProgress<ProgressInfo>? progress = null, CancellationToken cancellationToken = default)
    {
        return await InstallMissingComponentsAsync(SdkPathResolver.GetSdkRoot(), new[] { component }, progress, cancellationToken);
    }

    public Task<bool> IsInstalledAsync(string component, CancellationToken cancellationToken = default)
    {
        var sdk = SdkPathResolver.GetSdkRoot();
        if (component == "platform-tools") return Task.FromResult(File.Exists(Path.Combine(sdk, "platform-tools", "adb.exe")));
        if (component == "emulator") return Task.FromResult(File.Exists(Path.Combine(sdk, "emulator", "emulator.exe")));
        return Task.FromResult(Directory.Exists(Path.Combine(sdk, component)));
    }

    public Task<IReadOnlyList<SdkPackage>> GetInstalledPackagesAsync(CancellationToken cancellationToken = default)
    {
        var packages = new List<SdkPackage>();
        var sdk = SdkPathResolver.GetSdkRoot();

        if (File.Exists(Path.Combine(sdk, "platform-tools", "adb.exe")))
        {
            packages.Add(new SdkPackage
            {
                PackageName = SdkConstants.PlatformToolsPackage,
                DisplayName = "Android SDK Platform-Tools",
                InstallPath = Path.Combine(sdk, "platform-tools"),
                IsRequired = true
            });
        }

        if (File.Exists(Path.Combine(sdk, "emulator", "emulator.exe")))
        {
            packages.Add(new SdkPackage
            {
                PackageName = SdkConstants.DefaultEmulatorPackage,
                DisplayName = "Android Emulator",
                InstallPath = Path.Combine(sdk, "emulator"),
                IsRequired = true
            });
        }

        return Task.FromResult<IReadOnlyList<SdkPackage>>(packages);
    }
}
