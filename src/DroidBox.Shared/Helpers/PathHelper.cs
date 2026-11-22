namespace DroidBox.Shared.Helpers;

public static class PathHelper
{
    private static readonly string LocalAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
    private static string? _customSdkDir;

    public static void SetCustomSdkDirectory(string? path)
    {
        _customSdkDir = path;
    }

    public static string GetDefaultRecommendedSdkDirectory()
    {
        return Path.Combine(LocalAppData, "DroidBox", "AndroidSdk");
    }

    public static string GetDataDirectory()
    {
        var path = Path.Combine(LocalAppData, "DroidBox");
        EnsureDirectoryExists(path);
        return path;
    }

    public static string GetSdkDirectory()
    {
        if (!string.IsNullOrWhiteSpace(_customSdkDir) && Directory.Exists(_customSdkDir))
        {
            return _customSdkDir;
        }

        var envHome = Environment.GetEnvironmentVariable("ANDROID_HOME") 
                   ?? Environment.GetEnvironmentVariable("ANDROID_SDK_ROOT");

        if (!string.IsNullOrWhiteSpace(envHome) && Directory.Exists(envHome))
        {
            return envHome;
        }

        // 1. DroidBox recommended directory if it exists
        var droidBoxSdk = GetDefaultRecommendedSdkDirectory();
        if (Directory.Exists(droidBoxSdk))
        {
            return droidBoxSdk;
        }

        // 2. Standard Android Studio default location on Windows
        var standardLocalSdk = Path.Combine(LocalAppData, "Android", "Sdk");
        if (Directory.Exists(standardLocalSdk))
        {
            return standardLocalSdk;
        }

        // 3. Program Files location
        var progFilesSdk = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Android", "android-sdk");
        if (Directory.Exists(progFilesSdk))
        {
            return progFilesSdk;
        }

        // 4. Return default DroidBox recommended directory
        return droidBoxSdk;
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

    public static string GetScreenshotsDirectory()
    {
        var path = Path.Combine(GetDataDirectory(), "screenshots");
        EnsureDirectoryExists(path);
        return path;
    }

    public static string GetDocsDirectory()
    {
        var path = Path.Combine(GetDataDirectory(), "docs");
        EnsureDirectoryExists(path);
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
