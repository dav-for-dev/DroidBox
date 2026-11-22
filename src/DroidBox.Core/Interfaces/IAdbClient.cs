using DroidBox.Core.Models;

namespace DroidBox.Core.Interfaces;

public interface IAdbClient
{
    Task<bool> ConnectAsync(CancellationToken cancellationToken = default);
    Task<OperationResult> InstallApkAsync(string apkPath, CancellationToken cancellationToken = default);
    Task<OperationResult> UninstallAsync(string packageName, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<string>> ListPackagesAsync(CancellationToken cancellationToken = default);
    Task<OperationResult> LaunchActivityAsync(string packageName, string? activity = null, CancellationToken cancellationToken = default);
    Task<string> ExecuteCommandAsync(string command, CancellationToken cancellationToken = default);
    Task<bool> IsConnectedAsync(CancellationToken cancellationToken = default);
}
