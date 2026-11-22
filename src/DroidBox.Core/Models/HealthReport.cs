namespace DroidBox.Core.Models;

public record HealthComponent
{
    public string Name { get; init; } = string.Empty;
    public bool IsHealthy { get; init; }
    public string Status { get; init; } = string.Empty;
    public string? Details { get; init; }
}

public record HealthReport
{
    public bool IsHealthy => Components.All(c => c.IsHealthy);
    public IReadOnlyList<HealthComponent> Components { get; init; } = Array.Empty<HealthComponent>();
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
}
