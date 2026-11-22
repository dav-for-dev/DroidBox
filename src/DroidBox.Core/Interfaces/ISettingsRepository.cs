namespace DroidBox.Core.Interfaces;

public interface ISettingsRepository
{
    Task<string?> GetAsync(string key, CancellationToken cancellationToken = default);
    Task SetAsync(string key, string value, CancellationToken cancellationToken = default);
    Task<IDictionary<string, string>> GetAllAsync(CancellationToken cancellationToken = default);
}
