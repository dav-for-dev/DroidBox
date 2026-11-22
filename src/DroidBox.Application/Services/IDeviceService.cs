using DroidBox.Core.Entities;
using DroidBox.Core.Models;

namespace DroidBox.Application.Services;

public interface IDeviceService
{
    Task<IReadOnlyList<DeviceProfile>> GetProfilesAsync(CancellationToken cancellationToken = default);
    Task<DeviceProfile?> GetActiveProfileAsync(CancellationToken cancellationToken = default);
    Task<OperationResult> SwitchProfileAsync(string profileName, CancellationToken cancellationToken = default);
    Task<OperationResult> CreateProfileAsync(DeviceProfile profile, CancellationToken cancellationToken = default);
}
