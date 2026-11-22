namespace DroidBox.Core.Entities;

public class SdkPackage
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string PackageName { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public string InstallPath { get; set; } = string.Empty;
    public DateTime InstalledAt { get; set; } = DateTime.UtcNow;
    public bool IsRequired { get; set; }
}
