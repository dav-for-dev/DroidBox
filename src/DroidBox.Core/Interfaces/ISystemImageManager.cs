namespace DroidBox.Core.Interfaces;

public interface ISystemImageManager
{
    Task<IReadOnlyList<string>> GetInstalledSystemImagesAsync(string sdkRoot, CancellationToken cancellationToken = default);
    Task<string?> GetBestCompatibleSystemImageAsync(string sdkRoot, CancellationToken cancellationToken = default);
}
