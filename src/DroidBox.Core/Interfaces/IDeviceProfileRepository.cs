using DroidBox.Core.Entities;

namespace DroidBox.Core.Interfaces;

public interface IDeviceProfileRepository
{
    Task<DeviceProfile?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<DeviceProfile?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<DeviceProfile>> GetAllAsync(CancellationToken cancellationToken = default);
    Task AddAsync(DeviceProfile profile, CancellationToken cancellationToken = default);
    Task UpdateAsync(DeviceProfile profile, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
