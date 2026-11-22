using DroidBox.Core.Entities;
using DroidBox.Core.Interfaces;
using DroidBox.Core.Models;
using DroidBox.Shared.Constants;
using DroidBox.Shared.Helpers;
using Microsoft.Extensions.Logging;

namespace DroidBox.Runtime.AndroidSdk;

public class AvdManager : IDeviceProfileManager
{
    private readonly ILogger<AvdManager> _logger;
    private DeviceProfile? _activeProfile;

    public AvdManager(ILogger<AvdManager> logger)
    {
        _logger = logger;
    }

    public Task<IReadOnlyList<DeviceProfile>> GetProfilesAsync(CancellationToken cancellationToken = default)
    {
        IReadOnlyList<DeviceProfile> profiles = new List<DeviceProfile>
        {
            DeviceProfile.CreateTablet(),
            DeviceProfile.CreateMobile()
        };
        return Task.FromResult(profiles);
    }

    public Task<DeviceProfile> CreateProfileAsync(DeviceProfile profile, CancellationToken cancellationToken = default)
    {
        profile.IsCustom = true;
        _logger.LogInformation("Created custom profile: {Name}", profile.Name);
        return Task.FromResult(profile);
    }

    public Task DeleteProfileAsync(Guid profileId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Deleted profile {Id}", profileId);
        return Task.CompletedTask;
    }

    public Task SwitchProfileAsync(Guid profileId, CancellationToken cancellationToken = default)
    {
        if (profileId == DeviceProfile.CreateTablet().Id || _activeProfile?.Name == "tablet")
        {
            _activeProfile = DeviceProfile.CreateTablet();
        }
        else
        {
            _activeProfile = DeviceProfile.CreateMobile();
        }

        _logger.LogInformation("Switched active profile to: {Name}", _activeProfile.Name);
        return Task.CompletedTask;
    }

    public Task<DeviceProfile?> GetActiveProfileAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult<DeviceProfile?>(_activeProfile ?? DeviceProfile.CreateTablet());
    }

    public async Task<OperationResult> CreateAvdAsync(string avdName, string systemImage, DeviceProfile profile, CancellationToken cancellationToken = default)
    {
        try
        {
            var avdManagerPath = SdkPathResolver.GetAvdManagerPath();
            var args = $"create avd -n \"{avdName}\" -k \"{systemImage}\" --force";

            _logger.LogInformation("Creating AVD: {AvdName}", avdName);
            var result = await ProcessHelper.RunProcessAsync(avdManagerPath, args, cancellationToken: cancellationToken);

            if (result.Success)
            {
                // Write hardware configuration for the profile
                ConfigureAvdHardware(avdName, profile);
                return OperationResult.Ok($"AVD {avdName} created successfully");
            }

            return OperationResult.Fail($"Failed to create AVD: {result.StandardError}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create AVD {AvdName}", avdName);
            return OperationResult.Fail($"Create AVD exception: {ex.Message}");
        }
    }

    private void ConfigureAvdHardware(string avdName, DeviceProfile profile)
    {
        try
        {
            var avdDir = Path.Combine(PathHelper.GetAvdDirectory(), $"{avdName}.avd");
            if (!Directory.Exists(avdDir)) return;

            var configIni = Path.Combine(avdDir, "config.ini");
            var lines = new List<string>
            {
                $"hw.lcd.width={profile.Width}",
                $"hw.lcd.height={profile.Height}",
                $"hw.lcd.density={profile.Dpi}",
                "hw.ramSize=2048",
                "vm.heapSize=256",
                "hw.gpu.enabled=yes",
                "hw.gpu.mode=auto"
            };

            File.AppendAllLines(configIni, lines);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not update config.ini for AVD {AvdName}", avdName);
        }
    }
}
