using DroidBox.Core.Entities;
using DroidBox.Core.Interfaces;
using DroidBox.Core.Models;
using DroidBox.Shared.Constants;
using DroidBox.Shared.Helpers;
using Microsoft.Extensions.Logging;

namespace DroidBox.Runtime.AndroidSdk;

public class AvdManager : IAvdManager, IDeviceProfileManager
{
    private readonly ILogger<AvdManager> _logger;
    private readonly ISystemImageManager _systemImageManager;
    private DeviceProfile? _activeProfile;

    public AvdManager(ILogger<AvdManager> logger, ISystemImageManager systemImageManager)
    {
        _logger = logger;
        _systemImageManager = systemImageManager;
    }

    public static string GetAvdDirectory()
    {
        var userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        var dir = Path.Combine(userProfile, ".android", "avd");
        Directory.CreateDirectory(dir);
        return dir;
    }

    public Task<bool> AvdExistsAsync(string avdName, CancellationToken cancellationToken = default)
    {
        var avdDir = GetAvdDirectory();
        var iniFile = Path.Combine(avdDir, $"{avdName}.ini");
        return Task.FromResult(File.Exists(iniFile));
    }

    public Task<IReadOnlyList<string>> ListAvdsAsync(CancellationToken cancellationToken = default)
    {
        var avdDir = GetAvdDirectory();
        var inis = Directory.GetFiles(avdDir, "*.ini");
        var list = inis.Select(Path.GetFileNameWithoutExtension).Where(n => !string.IsNullOrWhiteSpace(n)).Cast<string>().ToList();
        return Task.FromResult<IReadOnlyList<string>>(list);
    }

    public async Task<OperationResult> EnsureAvdsCreatedAsync(string sdkRoot, CancellationToken cancellationToken = default)
    {
        try
        {
            var resMobile = await CreateMobileAvdAsync(sdkRoot, 2048, 4096, cancellationToken);
            var resTablet = await CreateTabletAvdAsync(sdkRoot, 2048, 4096, cancellationToken);

            if (resMobile.Success && resTablet.Success)
            {
                return OperationResult.Ok("Both DroidBox-Mobile and DroidBox-Tablet virtual devices are provisioned and ready.");
            }

            return OperationResult.Fail($"AVD provisioning issue: {resMobile.Message}; {resTablet.Message}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to ensure AVDs created");
            return OperationResult.Fail($"Failed to create AVDs: {ex.Message}");
        }
    }

    public async Task<OperationResult> CreateMobileAvdAsync(string sdkRoot, int ramMb = 2048, int storageMb = 4096, CancellationToken cancellationToken = default)
    {
        var sysImgRel = await _systemImageManager.GetBestCompatibleSystemImageAsync(sdkRoot, cancellationToken)
                        ?? @"system-images\android-34\google_apis\x86_64\";
        if (!sysImgRel.EndsWith('\\') && !sysImgRel.EndsWith('/')) sysImgRel += "\\";

        CreateAvdConfiguration("DroidBox-Mobile", sdkRoot, sysImgRel, 412, 915, 420, "portrait", ramMb, storageMb);
        CreateAvdConfiguration("DroidBox_Mobile", sdkRoot, sysImgRel, 412, 915, 420, "portrait", ramMb, storageMb);

        _logger.LogInformation("Provisioned DroidBox-Mobile AVD (412x915, portrait)");
        return OperationResult.Ok("DroidBox-Mobile provisioned successfully.");
    }

    public async Task<OperationResult> CreateTabletAvdAsync(string sdkRoot, int ramMb = 2048, int storageMb = 4096, CancellationToken cancellationToken = default)
    {
        var sysImgRel = await _systemImageManager.GetBestCompatibleSystemImageAsync(sdkRoot, cancellationToken)
                        ?? @"system-images\android-34\google_apis\x86_64\";
        if (!sysImgRel.EndsWith('\\') && !sysImgRel.EndsWith('/')) sysImgRel += "\\";

        CreateAvdConfiguration("DroidBox-Tablet", sdkRoot, sysImgRel, 1280, 800, 213, "landscape", ramMb, storageMb);
        CreateAvdConfiguration("DroidBox_Tablet", sdkRoot, sysImgRel, 1280, 800, 213, "landscape", ramMb, storageMb);

        _logger.LogInformation("Provisioned DroidBox-Tablet AVD (1280x800, landscape)");
        return OperationResult.Ok("DroidBox-Tablet provisioned successfully.");
    }

    private static void CreateAvdConfiguration(string avdName, string sdkRoot, string relImgPath, int width, int height, int density, string orientation, int ramMb, int storageMb)
    {
        var avdDir = GetAvdDirectory();
        var iniFile = Path.Combine(avdDir, $"{avdName}.ini");
        var boxAvdDir = Path.Combine(avdDir, $"{avdName}.avd");

        Directory.CreateDirectory(boxAvdDir);

        File.WriteAllText(iniFile, $"avd.ini.encoding=UTF-8\npath={boxAvdDir}\npath.rel=avd/{avdName}.avd\ntarget=android-34\n");

        var configIni = Path.Combine(boxAvdDir, "config.ini");
        var isPlayStore = relImgPath.Contains("playstore", StringComparison.OrdinalIgnoreCase);

        var content =
            $"PlayStore.enabled={(isPlayStore ? "true" : "false")}\n" +
            "abi.type=x86_64\n" +
            "avd.ini.encoding=UTF-8\n" +
            $"disk.dataPartition.size={storageMb}M\n" +
            "hw.cpu.arch=x86_64\n" +
            "hw.gpu.enabled=yes\n" +
            "hw.gpu.mode=auto\n" +
            $"hw.initialOrientation={orientation}\n" +
            "hw.keyboard=yes\n" +
            $"hw.lcd.density={density}\n" +
            $"hw.lcd.height={height}\n" +
            $"hw.lcd.width={width}\n" +
            $"hw.ramSize={ramMb}\n" +
            $"image.sysdir.1={relImgPath}\n" +
            $"tag.display={(isPlayStore ? "Google Play" : "Google APIs")}\n" +
            $"tag.id={(isPlayStore ? "google_apis_playstore" : "google_apis")}\n";

        File.WriteAllText(configIni, content);
    }

    public static void EnsureDefaultAvdConfigured(string sdkRoot)
    {
        try
        {
            var avdDir = GetAvdDirectory();
            var sysImgDir = Path.Combine(sdkRoot, "system-images");
            string relImgPath = @"system-images\android-34\google_apis\x86_64\";
            if (Directory.Exists(sysImgDir))
            {
                var sysImg = Directory.GetDirectories(sysImgDir, "x86_64", SearchOption.AllDirectories).FirstOrDefault();
                if (sysImg != null)
                {
                    relImgPath = Path.GetRelativePath(sdkRoot, sysImg) + "\\";
                }
            }

            CreateAvdConfiguration("DroidBox-Mobile", sdkRoot, relImgPath, 412, 915, 420, "portrait", 2048, 4096);
            CreateAvdConfiguration("DroidBox-Tablet", sdkRoot, relImgPath, 1280, 800, 213, "landscape", 2048, 4096);
            CreateAvdConfiguration("DroidBox_Tablet", sdkRoot, relImgPath, 1280, 800, 213, "landscape", 2048, 4096);
            CreateAvdConfiguration("DroidBox_Mobile", sdkRoot, relImgPath, 412, 915, 420, "portrait", 2048, 4096);
        }
        catch { }
    }

    // IDeviceProfileManager implementation
    public Task<IReadOnlyList<DeviceProfile>> GetProfilesAsync(CancellationToken cancellationToken = default)
    {
        IReadOnlyList<DeviceProfile> profiles = new List<DeviceProfile>
        {
            DeviceProfile.CreateMobile(),
            DeviceProfile.CreateTablet()
        };
        return Task.FromResult(profiles);
    }

    public Task<DeviceProfile> CreateProfileAsync(DeviceProfile profile, CancellationToken cancellationToken = default)
    {
        profile.IsCustom = true;
        return Task.FromResult(profile);
    }

    public Task DeleteProfileAsync(Guid profileId, CancellationToken cancellationToken = default) => Task.CompletedTask;

    public Task SwitchProfileAsync(Guid profileId, CancellationToken cancellationToken = default)
    {
        _activeProfile = profileId == DeviceProfile.CreateMobile().Id ? DeviceProfile.CreateMobile() : DeviceProfile.CreateTablet();
        return Task.CompletedTask;
    }

    public Task<DeviceProfile?> GetActiveProfileAsync(CancellationToken cancellationToken = default)
    {
        _activeProfile ??= DeviceProfile.CreateTablet();
        return Task.FromResult<DeviceProfile?>(_activeProfile);
    }

    public Task<OperationResult> CreateAvdAsync(string avdName, DeviceProfile profile, CancellationToken cancellationToken = default)
    {
        var sdk = SdkPathResolver.GetSdkRoot();
        CreateAvdConfiguration(avdName, sdk, @"system-images\android-34\google_apis\x86_64\", profile.Width, profile.Height, profile.Dpi, profile.Orientation.ToString().ToLower(), 2048, 4096);
        return Task.FromResult(OperationResult.Ok($"AVD {avdName} configured successfully"));
    }
}
