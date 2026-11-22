using DroidBox.Core.Entities;

namespace DroidBox.Core.Interfaces;

public interface IAppRepository
{
    Task<InstalledApp?> GetByPackageNameAsync(string packageName, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<InstalledApp>> GetAllAsync(CancellationToken cancellationToken = default);
    Task AddAsync(InstalledApp app, CancellationToken cancellationToken = default);
    Task UpdateAsync(InstalledApp app, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
