using DroidBox.Core.Enums;

namespace DroidBox.Core.Entities;

public class DeviceProfile
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public int Width { get; set; }
    public int Height { get; set; }
    public int Dpi { get; set; }
    public DeviceOrientation Orientation { get; set; }
    public DeviceType DeviceType { get; set; }
    public bool IsDefault { get; set; }
    public bool IsCustom { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public static DeviceProfile CreateTablet() => new()
    {
        Name = "tablet",
        DisplayName = "Standard Tablet (1280x800)",
        Width = 1280,
        Height = 800,
        Dpi = 213,
        Orientation = DeviceOrientation.Landscape,
        DeviceType = DeviceType.Tablet,
        IsDefault = true
    };

    public static DeviceProfile CreateMobile() => new()
    {
        Name = "mobile",
        DisplayName = "Standard Mobile (412x915)",
        Width = 412,
        Height = 915,
        Dpi = 420,
        Orientation = DeviceOrientation.Portrait,
        DeviceType = DeviceType.Mobile,
        IsDefault = false
    };
}
