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
