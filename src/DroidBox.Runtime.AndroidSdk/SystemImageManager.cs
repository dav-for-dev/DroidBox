using DroidBox.Core.Interfaces;
using DroidBox.Shared.Constants;
using Microsoft.Extensions.Logging;

namespace DroidBox.Runtime.AndroidSdk;

public class SystemImageManager : ISystemImageManager
{
    private readonly ILogger<SystemImageManager> _logger;

    public SystemImageManager(ILogger<SystemImageManager> logger)
    {
        _logger = logger;
    }

    public Task<IReadOnlyList<string>> GetInstalledSystemImagesAsync(string sdkRoot, CancellationToken cancellationToken = default)
    {
        var list = new List<string>();
        var sysImgDir = Path.Combine(sdkRoot, "system-images");
        if (Directory.Exists(sysImgDir))
        {
            var x86Dirs = Directory.GetDirectories(sysImgDir, "x86_64", SearchOption.AllDirectories);
            foreach (var dir in x86Dirs)
            {
                list.Add(Path.GetRelativePath(sdkRoot, dir));
            }
        }
        return Task.FromResult<IReadOnlyList<string>>(list);
    }

    public async Task<string?> GetBestCompatibleSystemImageAsync(string sdkRoot, CancellationToken cancellationToken = default)
    {
        var images = await GetInstalledSystemImagesAsync(sdkRoot, cancellationToken);
        if (images.Count > 0)
        {
            // Prefer Google Play Store image, then Google APIs
            var playStore = images.FirstOrDefault(i => i.Contains("google_apis_playstore", StringComparison.OrdinalIgnoreCase));
            if (playStore != null) return playStore;

            var googleApis = images.FirstOrDefault(i => i.Contains("google_apis", StringComparison.OrdinalIgnoreCase));
            if (googleApis != null) return googleApis;

            return images[0];
        }

        return SdkConstants.DefaultSystemImage;
    }
}
