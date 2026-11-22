using DroidBox.Core.Entities;
using DroidBox.Core.Enums;
using DroidBox.Core.Interfaces;
using DroidBox.Core.Models;
using Microsoft.Extensions.Logging;

namespace DroidBox.Application.Services;

public class ApkService : IApkService
{
    private readonly IAdbClient _adbClient;
    private readonly IApkAnalyzer _apkAnalyzer;
    private readonly IAppRepository _appRepository;
    private readonly ILogger<ApkService> _logger;

    public ApkService(
        IAdbClient adbClient,
        IApkAnalyzer apkAnalyzer,
        IAppRepository appRepository,
        ILogger<ApkService> logger)
    {
        _adbClient = adbClient;
        _apkAnalyzer = apkAnalyzer;
        _appRepository = appRepository;
        _logger = logger;
    }

    public async Task<OperationResult> InstallAsync(string apkPath, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!File.Exists(apkPath))
            {
                return OperationResult.Fail($"APK file not found: {apkPath}");
            }

            _logger.LogInformation("Analyzing APK before installation: {ApkPath}", apkPath);
            var apkInfo = await _apkAnalyzer.AnalyzeAsync(apkPath, cancellationToken);

            _logger.LogInformation("Installing APK {PackageName} via ADB", apkInfo.PackageName);
            var installResult = await _adbClient.InstallApkAsync(apkPath, cancellationToken);

            if (!installResult.Success)
            {
                return installResult;
            }

            var app = new InstalledApp
            {
                PackageName = apkInfo.PackageName,
                AppName = string.IsNullOrWhiteSpace(apkInfo.AppName) ? apkInfo.PackageName : apkInfo.AppName,
                VersionName = apkInfo.VersionName,
                VersionCode = apkInfo.VersionCode,
                MinSdk = apkInfo.MinSdkVersion,
                TargetSdk = apkInfo.TargetSdkVersion,
                CpuArchitecture = string.Join(",", apkInfo.SupportedAbis),
                InstallPath = apkPath,
                InstalledAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _appRepository.AddAsync(app, cancellationToken);
            _logger.LogInformation("APK {PackageName} installed and recorded successfully", apkInfo.PackageName);

            return OperationResult.Ok($"Successfully installed {app.AppName} ({app.PackageName})");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to install APK {ApkPath}", apkPath);
            return OperationResult.Fail($"Installation error: {ex.Message}");
        }
    }

    public async Task<OperationResult> UninstallAsync(string packageName, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Uninstalling package: {PackageName}", packageName);
            var result = await _adbClient.UninstallAsync(packageName, cancellationToken);

            if (result.Success)
            {
                var existing = await _appRepository.GetByPackageNameAsync(packageName, cancellationToken);
                if (existing != null)
                {
                    await _appRepository.DeleteAsync(existing.Id, cancellationToken);
                }
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to uninstall package {PackageName}", packageName);
            return OperationResult.Fail($"Uninstall error: {ex.Message}");
        }
    }

    public async Task<OperationResult> LaunchAsync(string packageName, string? activity = null, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Launching package: {PackageName}", packageName);
            return await _adbClient.LaunchActivityAsync(packageName, activity, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to launch package {PackageName}", packageName);
            return OperationResult.Fail($"Launch error: {ex.Message}");
        }
    }

    public async Task<IReadOnlyList<InstalledApp>> GetInstalledAppsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await _appRepository.GetAllAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get installed apps");
            return Array.Empty<InstalledApp>();
        }
    }

    public async Task<ApkInfo> AnalyzeApkAsync(string apkPath, CancellationToken cancellationToken = default)
    {
        return await _apkAnalyzer.AnalyzeAsync(apkPath, cancellationToken);
    }
}
