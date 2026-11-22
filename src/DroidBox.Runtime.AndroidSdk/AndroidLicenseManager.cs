using DroidBox.Core.Interfaces;
using DroidBox.Core.Models;
using Microsoft.Extensions.Logging;

namespace DroidBox.Runtime.AndroidSdk;

public class AndroidLicenseManager : IAndroidLicenseManager
{
    private readonly ILogger<AndroidLicenseManager> _logger;

    // Official Google Android SDK License Hashes
    private static readonly string AndroidSdkLicenseContent =
        "89388d22e3fd5dd85865f73494298699f14b218b\n" +
        "24333f8a63b6825ea9c5514f83c2829b004d1fee\n" +
        "84831b9409646a3e15e5a538c26bf0bc8224da50\n" +
        "d975f751698a77b662f1254ddbeed3901e976f5a";

    private static readonly string AndroidSdkPreviewLicenseContent =
        "84831b9409646a3e15e5a538c26bf0bc8224da50";

    private static readonly string AndroidGoogletvLicenseContent =
        "601085b94cd77f6b54ee86482f80c039f0f163b3";

    public AndroidLicenseManager(ILogger<AndroidLicenseManager> logger)
    {
        _logger = logger;
    }

    public Task<bool> AreLicensesAcceptedAsync(string sdkRoot, CancellationToken cancellationToken = default)
    {
        var licensePath = Path.Combine(sdkRoot, "licenses", "android-sdk-license");
        return Task.FromResult(File.Exists(licensePath));
    }

    public async Task<OperationResult> AcceptAllLicensesAsync(string sdkRoot, CancellationToken cancellationToken = default)
    {
        try
        {
            var licenseDir = Path.Combine(sdkRoot, "licenses");
            Directory.CreateDirectory(licenseDir);

            await File.WriteAllTextAsync(Path.Combine(licenseDir, "android-sdk-license"), AndroidSdkLicenseContent, cancellationToken);
            await File.WriteAllTextAsync(Path.Combine(licenseDir, "android-sdk-preview-license"), AndroidSdkPreviewLicenseContent, cancellationToken);
            await File.WriteAllTextAsync(Path.Combine(licenseDir, "android-googletv-license"), AndroidGoogletvLicenseContent, cancellationToken);

            _logger.LogInformation("Official Android SDK licenses written successfully to {Dir}", licenseDir);
            return OperationResult.Ok("Official Google Android SDK licenses accepted successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to write official Android SDK licenses");
            return OperationResult.Fail($"Failed to accept licenses: {ex.Message}");
        }
    }

    public string GetLicenseAgreementText()
    {
        return "By installing the Google Android SDK and Emulator components, you agree to the terms and conditions of the official Android Software Development Kit License Agreement (https://developer.android.com/studio/terms). DroidBox downloads official Google components directly from official Google repositories without modification.";
    }
}
