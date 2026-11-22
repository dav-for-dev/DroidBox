using System.Runtime.InteropServices;
using Microsoft.Win32;

namespace DroidBox.Runtime.AndroidSdk;

public static class WhpxDetector
{
    public static bool IsWhpxAvailable()
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return false;
        }

        try
        {
            // Check Hyper-V / WHPX registry entries
            using var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Virtualization");
            if (key != null) return true;

            // In 64-bit Windows with hardware virtualization enabled, WHPX is available
            return Environment.Is64BitOperatingSystem;
        }
        catch
        {
            return false;
        }
    }
}
