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
        _logger.LogInformation("Switched to profile: {Name}", _activeProfile.Name);
        return Task.CompletedTask;
    }

    public Task<DeviceProfile?> GetActiveProfileAsync(CancellationToken cancellationToken = default)
    {
        _activeProfile ??= DeviceProfile.CreateTablet();
        return Task.FromResult<DeviceProfile?>(_activeProfile);
    }

    public async Task<OperationResult> CreateAvdAsync(string avdName, DeviceProfile profile, CancellationToken cancellationToken = default)
    {
        var avdManager = SdkPathResolver.GetAvdManagerPath();
        var systemImage = SdkConstants.DefaultSystemImage;

        var args = $"create avd -n {avdName} -k \"{systemImage}\" --force";
        _logger.LogInformation("Creating AVD: {Name} with image {Image}", avdName, systemImage);

        try
        {
            var result = await ProcessHelper.RunProcessAsync(avdManager, args, cancellationToken: cancellationToken);
            if (result.Success)
            {
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

    public static void EnsureDefaultAvdConfigured(string sdkRoot)
    {
        try
        {
            var userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            var avdDir = Path.Combine(userProfile, ".android", "avd");
            Directory.CreateDirectory(avdDir);

            var iniFile = Path.Combine(avdDir, "DroidBox_Tablet.ini");
            var boxAvdDir = Path.Combine(avdDir, "DroidBox_Tablet.avd");

            if (!File.Exists(iniFile) || !Directory.Exists(boxAvdDir))
            {
                Directory.CreateDirectory(boxAvdDir);

                var sysImgDir = Path.Combine(sdkRoot, "system-images");
                string relImgPath = @"system-images\android-35\google_apis_playstore\x86_64\";
                if (Directory.Exists(sysImgDir))
                {
                    var sysImg = Directory.GetDirectories(sysImgDir, "x86_64", SearchOption.AllDirectories).FirstOrDefault();
                    if (sysImg != null)
                    {
                        relImgPath = Path.GetRelativePath(sdkRoot, sysImg) + "\\";
                    }
                }

                File.WriteAllText(iniFile, $"avd.ini.encoding=UTF-8\npath={boxAvdDir}\npath.rel=avd/DroidBox_Tablet.avd\ntarget=android-35\n");
                File.WriteAllText(Path.Combine(boxAvdDir, "config.ini"),
                    "PlayStore.enabled=true\n" +
                    "abi.type=x86_64\n" +
                    "avd.ini.encoding=UTF-8\n" +
                    "disk.dataPartition.size=4096M\n" +
                    "hw.cpu.arch=x86_64\n" +
                    "hw.gpu.enabled=yes\n" +
                    "hw.gpu.mode=auto\n" +
                    "hw.initialOrientation=landscape\n" +
                    "hw.keyboard=yes\n" +
                    "hw.lcd.density=213\n" +
                    "hw.lcd.height=800\n" +
                    "hw.lcd.width=1280\n" +
                    "hw.ramSize=2048\n" +
                    $"image.sysdir.1={relImgPath}\n" +
                    "tag.display=Google Play\n" +
                    "tag.id=google_apis_playstore\n");
            }
        }
        catch
        {
            // Best effort configuration
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
