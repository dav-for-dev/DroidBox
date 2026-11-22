using DroidBox.Core.Entities;

namespace DroidBox.Core.Interfaces;

public interface IDeviceProfileManager
{
    Task<IReadOnlyList<DeviceProfile>> GetProfilesAsync(CancellationToken cancellationToken = default);
    Task<DeviceProfile> CreateProfileAsync(DeviceProfile profile, CancellationToken cancellationToken = default);
    Task DeleteProfileAsync(Guid profileId, CancellationToken cancellationToken = default);
    Task SwitchProfileAsync(Guid profileId, CancellationToken cancellationToken = default);
    Task<DeviceProfile?> GetActiveProfileAsync(CancellationToken cancellationToken = default);
}
