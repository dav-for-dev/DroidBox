using DroidBox.Core.Entities;
using DroidBox.Core.Models;

namespace DroidBox.Application.Services;

public interface IApkService
{
    Task<OperationResult> InstallAsync(string apkPath, CancellationToken cancellationToken = default);
    Task<OperationResult> UninstallAsync(string packageName, CancellationToken cancellationToken = default);
    Task<OperationResult> LaunchAsync(string packageName, string? activity = null, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<InstalledApp>> GetInstalledAppsAsync(CancellationToken cancellationToken = default);
    Task<ApkInfo> AnalyzeApkAsync(string apkPath, CancellationToken cancellationToken = default);
}
