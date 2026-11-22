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
