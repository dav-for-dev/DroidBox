namespace DroidBox.Core.Models;

public record ProgressInfo
{
    public double Percentage { get; init; }
    public string Message { get; init; } = string.Empty;
    public int CurrentStep { get; init; }
    public int TotalSteps { get; init; }
}
