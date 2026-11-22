namespace DroidBox.Core.Entities;

public class InstalledApp
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string PackageName { get; set; } = string.Empty;
    public string AppName { get; set; } = string.Empty;
    public string VersionName { get; set; } = string.Empty;
    public int VersionCode { get; set; }
    public int MinSdk { get; set; }
    public int TargetSdk { get; set; }
    public string CpuArchitecture { get; set; } = string.Empty;
    public string InstallPath { get; set; } = string.Empty;
    public DateTime InstalledAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public string? IconPath { get; set; }
    public bool IsSystemApp { get; set; }
}
