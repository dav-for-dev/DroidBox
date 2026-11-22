using DroidBox.Core.Entities;

namespace DroidBox.Core.Interfaces;

public interface ISdkPackageRepository
{
    Task<IReadOnlyList<SdkPackage>> GetAllAsync(CancellationToken cancellationToken = default);
    Task AddAsync(SdkPackage package, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
