namespace DroidBox.Core.Entities;

public class AppSetting
{
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string Category { get; set; } = "General";
    public string Description { get; set; } = string.Empty;
    public DateTime ModifiedAt { get; set; } = DateTime.UtcNow;
}
