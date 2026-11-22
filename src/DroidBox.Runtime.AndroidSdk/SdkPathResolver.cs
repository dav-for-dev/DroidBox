using DroidBox.Shared.Helpers;

namespace DroidBox.Runtime.AndroidSdk;

public static class SdkPathResolver
{
    public static string GetSdkRoot() => PathHelper.GetSdkDirectory();

    public static string GetAdbPath()
    {
        var sdk = GetSdkRoot();
        var adb = Path.Combine(sdk, "platform-tools", "adb.exe");
        if (File.Exists(adb)) return adb;

        var standardAdb = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Android", "Sdk", "platform-tools", "adb.exe");
        if (File.Exists(standardAdb)) return standardAdb;

        return "adb";
    }

    public static string? GetEmulatorPath()
    {
        var sdk = GetSdkRoot();
        var emu = Path.Combine(sdk, "emulator", "emulator.exe");
        if (File.Exists(emu)) return emu;

        var standardEmu = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Android", "Sdk", "emulator", "emulator.exe");
        if (File.Exists(standardEmu)) return standardEmu;

        return null;
    }

    public static string GetSdkManagerPath()
    {
        var sdk = GetSdkRoot();
        var mgr = Path.Combine(sdk, "cmdline-tools", "latest", "bin", "sdkmanager.bat");
        if (File.Exists(mgr)) return mgr;

        var cmdlineDir = Path.Combine(sdk, "cmdline-tools");
        if (Directory.Exists(cmdlineDir))
        {
            var bat = Directory.GetFiles(cmdlineDir, "sdkmanager.bat", SearchOption.AllDirectories).FirstOrDefault();
            if (bat != null) return bat;
        }

        return "sdkmanager";
    }

    public static string GetAvdManagerPath()
    {
        var sdk = GetSdkRoot();
        var avd = Path.Combine(sdk, "cmdline-tools", "latest", "bin", "avdmanager.bat");
        if (File.Exists(avd)) return avd;

        var cmdlineDir = Path.Combine(sdk, "cmdline-tools");
        if (Directory.Exists(cmdlineDir))
        {
            var bat = Directory.GetFiles(cmdlineDir, "avdmanager.bat", SearchOption.AllDirectories).FirstOrDefault();
            if (bat != null) return bat;
        }

        return "avdmanager";
    }

    public static string? GetAapt2Path()
    {
        var sdk = GetSdkRoot();
        var buildToolsDir = Path.Combine(sdk, "build-tools");
        if (Directory.Exists(buildToolsDir))
        {
            var aapt = Directory.GetFiles(buildToolsDir, "aapt2.exe", SearchOption.AllDirectories).FirstOrDefault();
            if (aapt != null) return aapt;
        }

        return null;
    }
}
