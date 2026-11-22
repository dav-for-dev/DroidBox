using System.IO.Compression;
using System.Text.RegularExpressions;
using DroidBox.Core.Entities;
using DroidBox.Core.Interfaces;
using DroidBox.Shared.Helpers;
using Microsoft.Extensions.Logging;

namespace DroidBox.Runtime.AndroidSdk;

public class ApkAnalyzerImpl : IApkAnalyzer
{
    private readonly ILogger<ApkAnalyzerImpl> _logger;

    public ApkAnalyzerImpl(ILogger<ApkAnalyzerImpl> logger)
    {
        _logger = logger;
    }

    public async Task<ApkInfo> AnalyzeAsync(string apkPath, CancellationToken cancellationToken = default)
    {
        if (!File.Exists(apkPath))
        {
            throw new FileNotFoundException("APK file not found", apkPath);
        }

        var fileInfo = new FileInfo(apkPath);
        var apkInfo = new ApkInfo
        {
            FilePath = apkPath,
            FileSize = fileInfo.Length,
            AppName = Path.GetFileNameWithoutExtension(apkPath),
            PackageName = "com.example." + Path.GetFileNameWithoutExtension(apkPath).ToLowerInvariant().Replace("-", "_").Replace(".", "_"),
            VersionName = "1.0",
            VersionCode = 1,
            MinSdkVersion = 24,
            TargetSdkVersion = 34
        };

        // Try aapt2 if available
        var aapt2 = SdkPathResolver.GetAapt2Path();
        if (aapt2 != null && File.Exists(aapt2))
        {
            try
            {
                var dumpResult = await ProcessHelper.RunProcessAsync(aapt2, $"dump badging \"{apkPath}\"", cancellationToken: cancellationToken);
                if (dumpResult.Success && !string.IsNullOrWhiteSpace(dumpResult.StandardOutput))
                {
                    ParseBadgingOutput(dumpResult.StandardOutput, apkInfo);
                    return apkInfo;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "aapt2 dump failed, falling back to zip parsing");
            }
        }

        // Fallback: inspect zip archive structure
        try
        {
            using var archive = ZipFile.OpenRead(apkPath);
            var abis = new HashSet<string>();
            foreach (var entry in archive.Entries)
            {
                if (entry.FullName.StartsWith("lib/"))
                {
                    var parts = entry.FullName.Split('/');
                    if (parts.Length > 1) abis.Add(parts[1]);
                }
            }
            if (abis.Count > 0)
            {
                apkInfo.SupportedAbis = abis.ToList();
            }
            else
            {
                apkInfo.SupportedAbis = new List<string> { "arm64-v8a", "x86_64" };
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to inspect APK zip entries");
        }

        return apkInfo;
    }

    private static void ParseBadgingOutput(string output, ApkInfo info)
    {
        var packageMatch = Regex.Match(output, @"package:\s+name='([^']+)'\s+versionCode='([^']+)'\s+versionName='([^']+)'");
        if (packageMatch.Success)
        {
            info.PackageName = packageMatch.Groups[1].Value;
            if (int.TryParse(packageMatch.Groups[2].Value, out var code)) info.VersionCode = code;
            info.VersionName = packageMatch.Groups[3].Value;
        }

        var labelMatch = Regex.Match(output, @"application-label:'([^']+)'");
        if (labelMatch.Success)
        {
            info.AppName = labelMatch.Groups[1].Value;
        }

        var minSdkMatch = Regex.Match(output, @"sdkVersion:'([^']+)'");
        if (minSdkMatch.Success && int.TryParse(minSdkMatch.Groups[1].Value, out var minSdk))
        {
            info.MinSdkVersion = minSdk;
        }

        var targetSdkMatch = Regex.Match(output, @"targetSdkVersion:'([^']+)'");
        if (targetSdkMatch.Success && int.TryParse(targetSdkMatch.Groups[1].Value, out var targetSdk))
        {
            info.TargetSdkVersion = targetSdk;
        }

        var permMatches = Regex.Matches(output, @"uses-permission:\s+name='([^']+)'");
        foreach (Match match in permMatches)
        {
            info.Permissions.Add(match.Groups[1].Value);
        }
    }

    public async Task<byte[]?> ExtractIconAsync(string apkPath, CancellationToken cancellationToken = default)
    {
        try
        {
            using var archive = ZipFile.OpenRead(apkPath);
            var iconEntry = archive.Entries.FirstOrDefault(e =>
                e.FullName.EndsWith(".png", StringComparison.OrdinalIgnoreCase) &&
                (e.FullName.Contains("icon", StringComparison.OrdinalIgnoreCase) || e.FullName.Contains("logo", StringComparison.OrdinalIgnoreCase)));

            if (iconEntry != null)
            {
                using var stream = iconEntry.Open();
                using var ms = new MemoryStream();
                await stream.CopyToAsync(ms, cancellationToken);
                return ms.ToArray();
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to extract icon from APK");
        }

        return null;
    }

    public async Task<IReadOnlyList<string>> GetPermissionsAsync(string apkPath, CancellationToken cancellationToken = default)
    {
        var info = await AnalyzeAsync(apkPath, cancellationToken);
        return info.Permissions;
    }

    public Task<bool> ValidateAsync(string apkPath, CancellationToken cancellationToken = default)
    {
        if (!File.Exists(apkPath)) return Task.FromResult(false);

        try
        {
            using var archive = ZipFile.OpenRead(apkPath);
            var hasManifest = archive.Entries.Any(e => e.FullName == "AndroidManifest.xml");
            return Task.FromResult(hasManifest);
        }
        catch
        {
            return Task.FromResult(false);
        }
    }
}
