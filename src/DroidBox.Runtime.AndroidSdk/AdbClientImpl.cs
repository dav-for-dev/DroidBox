using System.Text.RegularExpressions;
using DroidBox.Core.Interfaces;
using DroidBox.Core.Models;
using DroidBox.Shared.Helpers;
using Microsoft.Extensions.Logging;

namespace DroidBox.Runtime.AndroidSdk;

public class AdbClientImpl : IAdbClient
{
    private readonly ILogger<AdbClientImpl> _logger;
    private bool _isConnected;

    public AdbClientImpl(ILogger<AdbClientImpl> logger)
    {
        _logger = logger;
    }

    private string AdbPath => SdkPathResolver.GetAdbPath();

    public async Task<bool> ConnectAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await ProcessHelper.RunProcessAsync(AdbPath, "start-server", cancellationToken: cancellationToken);
            _isConnected = result.Success;
            return _isConnected;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start ADB server");
            _isConnected = false;
            return false;
        }
    }

    public async Task<OperationResult> InstallApkAsync(string apkPath, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Executing adb install: {Path}", apkPath);
            var result = await ProcessHelper.RunProcessAsync(AdbPath, $"install -r \"{apkPath}\"", timeoutMs: 120000, cancellationToken: cancellationToken);

            if (result.Success && (result.StandardOutput.Contains("Success") || string.IsNullOrWhiteSpace(result.StandardError)))
            {
                return OperationResult.Ok("APK installed successfully");
            }

            var err = !string.IsNullOrWhiteSpace(result.StandardError) ? result.StandardError : result.StandardOutput;
            return OperationResult.Fail($"ADB install failed: {err.Trim()}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception during adb install");
            return OperationResult.Fail($"ADB install exception: {ex.Message}");
        }
    }

    public async Task<OperationResult> UninstallAsync(string packageName, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Executing adb uninstall: {Package}", packageName);
            var result = await ProcessHelper.RunProcessAsync(AdbPath, $"uninstall {packageName}", cancellationToken: cancellationToken);

            if (result.Success && result.StandardOutput.Contains("Success"))
            {
                return OperationResult.Ok("Package uninstalled successfully");
            }

            return OperationResult.Fail($"ADB uninstall failed: {result.StandardError.Trim()}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception during adb uninstall");
            return OperationResult.Fail($"ADB uninstall exception: {ex.Message}");
        }
    }

    public async Task<IReadOnlyList<string>> ListPackagesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await ProcessHelper.RunProcessAsync(AdbPath, "shell pm list packages -3", cancellationToken: cancellationToken);
            if (!result.Success) return Array.Empty<string>();

            return result.StandardOutput
                .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                .Where(line => line.StartsWith("package:"))
                .Select(line => line.Substring("package:".Length).Trim())
                .ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to list packages");
            return Array.Empty<string>();
        }
    }

    public async Task<OperationResult> LaunchActivityAsync(string packageName, string? activity = null, CancellationToken cancellationToken = default)
    {
        try
        {
            string args;
            if (!string.IsNullOrWhiteSpace(activity))
            {
                args = $"shell am start -n {packageName}/{activity}";
            }
            else
            {
                args = $"shell monkey -p {packageName} -c android.intent.category.LAUNCHER 1";
            }

            var result = await ProcessHelper.RunProcessAsync(AdbPath, args, cancellationToken: cancellationToken);
            if (result.Success)
            {
                return OperationResult.Ok($"Launched {packageName}");
            }

            return OperationResult.Fail($"Launch failed: {result.StandardError}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to launch activity");
            return OperationResult.Fail($"Launch error: {ex.Message}");
        }
    }

    public async Task<string> ExecuteCommandAsync(string command, CancellationToken cancellationToken = default)
    {
        var result = await ProcessHelper.RunProcessAsync(AdbPath, $"shell {command}", cancellationToken: cancellationToken);
        return result.StandardOutput;
    }

    public async Task<bool> IsConnectedAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await ProcessHelper.RunProcessAsync(AdbPath, "devices", cancellationToken: cancellationToken);
            if (!result.Success) return false;

            return result.StandardOutput.Contains("\tdevice");
        }
        catch
        {
            return false;
        }
    }
}
