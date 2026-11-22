using DroidBox.Core.Models;

namespace DroidBox.Core.Interfaces;

public interface IRuntimeProvider
{
    string Name { get; }
    Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default);
    Task InitializeAsync(CancellationToken cancellationToken = default);
    RuntimeCapabilities GetCapabilities();
}
