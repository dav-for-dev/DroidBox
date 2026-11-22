using DroidBox.Core.Enums;

namespace DroidBox.Display;

public class DisplayConfiguration
{
    public int Width { get; set; } = 1280;
    public int Height { get; set; } = 800;
    public int Dpi { get; set; } = 213;
    public DeviceOrientation Orientation { get; set; } = DeviceOrientation.Landscape;
    public int TargetFps { get; set; } = 30;
    public bool UseGrpcStreaming { get; set; } = false;
    public bool ShowControlBar { get; set; } = true;
}
