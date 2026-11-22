using DroidBox.Core.Enums;

namespace DroidBox.Core.Entities;

public class ApkHistory
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string PackageName { get; set; } = string.Empty;
    public string AppName { get; set; } = string.Empty;
    public string VersionName { get; set; } = string.Empty;
    public string ApkPath { get; set; } = string.Empty;
    public ApkAction Action { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
