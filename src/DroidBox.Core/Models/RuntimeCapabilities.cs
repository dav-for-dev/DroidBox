namespace DroidBox.Core.Models;

public record RuntimeCapabilities
{
    public bool SupportsGpu { get; init; }
    public bool SupportsAudio { get; init; }
    public bool SupportsCamera { get; init; }
    public bool SupportsSensors { get; init; }
    public bool SupportsMultiTouch { get; init; }
    public int MaxRamMb { get; init; }
    public IReadOnlyList<string> SupportedAbis { get; init; } = Array.Empty<string>();
}
