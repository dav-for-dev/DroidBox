using DroidBox.Core.Entities;
using DroidBox.Core.Interfaces;
using DroidBox.Core.Models;
using Microsoft.Extensions.Logging;

namespace DroidBox.Application.Services;

public class DeviceService : IDeviceService
{
    private readonly IDeviceProfileManager _profileManager;
    private readonly IDeviceProfileRepository _profileRepository;
    private readonly ILogger<DeviceService> _logger;

    public DeviceService(
        IDeviceProfileManager profileManager,
        IDeviceProfileRepository profileRepository,
        ILogger<DeviceService> logger)
    {
        _profileManager = profileManager;
        _profileRepository = profileRepository;
        _logger = logger;
    }

    public async Task<IReadOnlyList<DeviceProfile>> GetProfilesAsync(CancellationToken cancellationToken = default)
    {
        var dbProfiles = await _profileRepository.GetAllAsync(cancellationToken);
        if (dbProfiles.Count == 0)
        {
            // Seed defaults
            var tablet = DeviceProfile.CreateTablet();
            var mobile = DeviceProfile.CreateMobile();
            await _profileRepository.AddAsync(tablet, cancellationToken);
            await _profileRepository.AddAsync(mobile, cancellationToken);
            return new[] { tablet, mobile };
        }
        return dbProfiles;
    }

    public async Task<DeviceProfile?> GetActiveProfileAsync(CancellationToken cancellationToken = default)
    {
        return await _profileManager.GetActiveProfileAsync(cancellationToken);
    }

    public async Task<OperationResult> SwitchProfileAsync(string profileName, CancellationToken cancellationToken = default)
    {
        try
        {
            var profile = await _profileRepository.GetByNameAsync(profileName, cancellationToken);
            if (profile == null)
            {
                return OperationResult.Fail($"Device profile not found: {profileName}");
            }

            await _profileManager.SwitchProfileAsync(profile.Id, cancellationToken);
            _logger.LogInformation("Switched to device profile: {ProfileName}", profileName);
            return OperationResult.Ok($"Switched to profile: {profile.DisplayName}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to switch device profile to {ProfileName}", profileName);
            return OperationResult.Fail($"Profile switch error: {ex.Message}");
        }
    }

    public async Task<OperationResult> CreateProfileAsync(DeviceProfile profile, CancellationToken cancellationToken = default)
    {
        try
        {
            profile.IsCustom = true;
            await _profileRepository.AddAsync(profile, cancellationToken);
            return OperationResult.Ok($"Profile {profile.Name} created");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create profile {ProfileName}", profile.Name);
            return OperationResult.Fail($"Create profile error: {ex.Message}");
        }
    }
}
