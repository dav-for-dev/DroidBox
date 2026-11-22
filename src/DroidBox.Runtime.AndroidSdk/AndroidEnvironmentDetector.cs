using DroidBox.Core.Interfaces;
using DroidBox.Core.Models;
using DroidBox.Shared.Helpers;
using Microsoft.Extensions.Logging;

namespace DroidBox.Runtime.AndroidSdk;

public class AndroidEnvironmentDetector : IAndroidEnvironmentDetector
{
    private readonly ILogger<AndroidEnvironmentDetector> _logger;

    public AndroidEnvironmentDetector(ILogger<AndroidEnvironmentDetector> logger)
    {
        _logger = logger;
    }

    public Task<EnvironmentDetectionResult> DetectEnvironmentAsync(string? customSdkPath = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var sdkRoot = !string.IsNullOrWhiteSpace(customSdkPath) && Directory.Exists(customSdkPath)
                ? customSdkPath
                : SdkPathResolver.GetSdkRoot();

            var adbPath = SdkPathResolver.GetAdbPath();
            var isAdbInstalled = File.Exists(adbPath);

            var emulatorPath = SdkPathResolver.GetEmulatorPath();
            var isEmulatorInstalled = !string.IsNullOrWhiteSpace(emulatorPath) && File.Exists(emulatorPath);

            var isSdkInstalled = Directory.Exists(sdkRoot) && (isAdbInstalled || isEmulatorInstalled);

            // Check system images
            var installedImages = new List<string>();
            var sysImgDir = Path.Combine(sdkRoot, "system-images");
            if (Directory.Exists(sysImgDir))
            {
                var x86Dirs = Directory.GetDirectories(sysImgDir, "x86_64", SearchOption.AllDirectories);
                foreach (var dir in x86Dirs)
                {
                    installedImages.Add(Path.GetRelativePath(sysImgDir, dir));
                }
            }
            var areSystemImagesInstalled = installedImages.Count > 0;

            // Check Virtualization
            var isWhpxAvailable = WhpxDetector.IsWhpxAvailable();
            var virtMessage = isWhpxAvailable
                ? "Windows Hypervisor Platform (WHPX) is available."
                : "WHPX is not detected. Emulator may run slower or require Windows Hypervisor feature enabled.";

            // Check Disk Space & Memory
            long freeDiskSpace = 0;
            try
            {
                var targetDrive = Path.GetPathRoot(sdkRoot) ?? "C:\\";
                var drive = new DriveInfo(targetDrive);
                freeDiskSpace = drive.AvailableFreeSpace;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not determine free disk space");
            }

            long totalRam = 0;
            try
            {
                totalRam = GC.GetGCMemoryInfo().TotalAvailableMemoryBytes;
            }
            catch { }

            // Check AVDs in %USERPROFILE%\.android\avd
            var userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            var avdDir = Path.Combine(userProfile, ".android", "avd");
            var isMobileAvd = File.Exists(Path.Combine(avdDir, "DroidBox-Mobile.ini")) || File.Exists(Path.Combine(avdDir, "DroidBox_Mobile.ini"));
            var isTabletAvd = File.Exists(Path.Combine(avdDir, "DroidBox-Tablet.ini")) || File.Exists(Path.Combine(avdDir, "DroidBox_Tablet.ini"));

            // Check Licenses
            var licenseDir = Path.Combine(sdkRoot, "licenses");
            var areLicensesAccepted = File.Exists(Path.Combine(licenseDir, "android-sdk-license"));

            // Missing components
            var missing = new List<string>();
            if (!isSdkInstalled) missing.Add("Android SDK Root");
            if (!isAdbInstalled) missing.Add("Android Platform-Tools (ADB)");
            if (!isEmulatorInstalled) missing.Add("Android Emulator");
            if (!areSystemImagesInstalled) missing.Add("Android System Image (x86_64)");
            if (!isMobileAvd) missing.Add("DroidBox-Mobile Device");
            if (!isTabletAvd) missing.Add("DroidBox-Tablet Device");
            if (!areLicensesAccepted) missing.Add("Google SDK License Agreement");

            var result = new EnvironmentDetectionResult
            {
                IsSdkInstalled = isSdkInstalled,
                SdkPath = isSdkInstalled ? sdkRoot : null,
                IsAdbInstalled = isAdbInstalled,
                AdbPath = isAdbInstalled ? adbPath : null,
                IsEmulatorInstalled = isEmulatorInstalled,
                EmulatorPath = isEmulatorInstalled ? emulatorPath : null,
                AreSystemImagesInstalled = areSystemImagesInstalled,
                InstalledSystemImages = installedImages,
                IsVirtualizationAvailable = isWhpxAvailable,
                VirtualizationStatusMessage = virtMessage,
                AvailableDiskSpaceBytes = freeDiskSpace,
                TotalPhysicalMemoryBytes = totalRam,
                IsMobileAvdCreated = isMobileAvd,
                IsTabletAvdCreated = isTabletAvd,
                AreLicensesAccepted = areLicensesAccepted,
                MissingComponents = missing
            };

            return Task.FromResult(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to detect Android environment");
            return Task.FromResult(new EnvironmentDetectionResult());
        }
    }
}
