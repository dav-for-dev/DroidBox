namespace DroidBox.Core.Entities;

public class ApkInfo
{
    public string FilePath { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string PackageName { get; set; } = string.Empty;
    public string AppName { get; set; } = string.Empty;
    public string VersionName { get; set; } = string.Empty;
    public int VersionCode { get; set; }
    public int MinSdkVersion { get; set; }
    public int TargetSdkVersion { get; set; }
    public List<string> Permissions { get; set; } = new();
    public List<string> SupportedAbis { get; set; } = new();
    public bool IsSplitApk { get; set; }
    public string? SplitName { get; set; }
}
