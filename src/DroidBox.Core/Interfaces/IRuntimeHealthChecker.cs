using DroidBox.Core.Models;

namespace DroidBox.Core.Interfaces;

public interface IRuntimeHealthChecker
{
    Task<HealthReport> CheckHealthAsync(CancellationToken cancellationToken = default);
    Task<IDictionary<string, string>> GetDiagnosticsAsync(CancellationToken cancellationToken = default);
}
