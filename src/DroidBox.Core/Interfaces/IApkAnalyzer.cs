using DroidBox.Core.Entities;

namespace DroidBox.Core.Interfaces;

public interface IApkAnalyzer
{
    Task<ApkInfo> AnalyzeAsync(string apkPath, CancellationToken cancellationToken = default);
    Task<byte[]?> ExtractIconAsync(string apkPath, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<string>> GetPermissionsAsync(string apkPath, CancellationToken cancellationToken = default);
    Task<bool> ValidateAsync(string apkPath, CancellationToken cancellationToken = default);
}
