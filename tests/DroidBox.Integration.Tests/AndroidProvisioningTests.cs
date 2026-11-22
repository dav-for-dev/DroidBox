using DroidBox.Core.Interfaces;
using DroidBox.Core.Models;
using DroidBox.Runtime.AndroidSdk;
using DroidBox.Shared.Constants;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace DroidBox.Integration.Tests;

public class AndroidProvisioningTests
{
    [Fact]
    public async Task AndroidLicenseManager_AcceptAllLicensesAsync_ShouldCreateLicenseFiles()
    {
        var tempSdk = Path.Combine(Path.GetTempPath(), "droidbox_lic_test_" + Guid.NewGuid());
        Directory.CreateDirectory(tempSdk);
        try
        {
            var loggerMock = new Mock<ILogger<AndroidLicenseManager>>();
            var sut = new AndroidLicenseManager(loggerMock.Object);

            var result = await sut.AcceptAllLicensesAsync(tempSdk);

            result.Success.Should().BeTrue();
            File.Exists(Path.Combine(tempSdk, "licenses", "android-sdk-license")).Should().BeTrue();
            File.Exists(Path.Combine(tempSdk, "licenses", "android-sdk-preview-license")).Should().BeTrue();
            File.Exists(Path.Combine(tempSdk, "licenses", "android-googletv-license")).Should().BeTrue();

            var accepted = await sut.AreLicensesAcceptedAsync(tempSdk);
            accepted.Should().BeTrue();
        }
        finally
        {
            if (Directory.Exists(tempSdk)) Directory.Delete(tempSdk, true);
        }
    }

    [Fact]
    public async Task AvdManager_CreateMobileAndTabletAvds_ShouldCreateValidConfigurations()
    {
        var tempSdk = Path.Combine(Path.GetTempPath(), "droidbox_avd_test_" + Guid.NewGuid());
        Directory.CreateDirectory(tempSdk);
        try
        {
            var loggerMock = new Mock<ILogger<AvdManager>>();
            var sysImgMock = new Mock<ISystemImageManager>();
            sysImgMock.Setup(s => s.GetBestCompatibleSystemImageAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(@"system-images\android-34\google_apis\x86_64\");

            var sut = new AvdManager(loggerMock.Object, sysImgMock.Object);

            var mobileRes = await sut.CreateMobileAvdAsync(tempSdk);
            var tabletRes = await sut.CreateTabletAvdAsync(tempSdk);

            mobileRes.Success.Should().BeTrue();
            tabletRes.Success.Should().BeTrue();

            var existsMobile = await sut.AvdExistsAsync("DroidBox-Mobile");
            var existsTablet = await sut.AvdExistsAsync("DroidBox-Tablet");

            existsMobile.Should().BeTrue();
            existsTablet.Should().BeTrue();
        }
        finally
        {
            if (Directory.Exists(tempSdk)) Directory.Delete(tempSdk, true);
        }
    }

    [Fact]
    public async Task AndroidSdkManager_RunFullSetupAsync_ShouldFail_WhenLicensesNotAccepted()
    {
        var envMock = new Mock<IAndroidEnvironmentDetector>();
        var licMock = new Mock<IAndroidLicenseManager>();
        var instMock = new Mock<IAndroidSdkInstaller>();
        var avdMock = new Mock<IAvdManager>();
        var adbMock = new Mock<IAdbClient>();
        var loggerMock = new Mock<ILogger<AndroidSdkManager>>();

        var sut = new AndroidSdkManager(envMock.Object, licMock.Object, instMock.Object, avdMock.Object, adbMock.Object, loggerMock.Object);

        var result = await sut.RunFullSetupAsync("C:\\dummy", acceptLicenses: false);

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("License Agreement");
    }

    [Fact]
    public async Task AndroidSdkManager_RunFullSetupAsync_ShouldSucceed_WhenValid()
    {
        var envMock = new Mock<IAndroidEnvironmentDetector>();
        envMock.Setup(e => e.DetectEnvironmentAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new EnvironmentDetectionResult());

        var licMock = new Mock<IAndroidLicenseManager>();
        licMock.Setup(l => l.AcceptAllLicensesAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(OperationResult.Ok("Licenses accepted"));

        var instMock = new Mock<IAndroidSdkInstaller>();
        instMock.Setup(i => i.InstallMissingComponentsAsync(It.IsAny<string>(), It.IsAny<IReadOnlyList<string>>(), It.IsAny<IProgress<ProgressInfo>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(OperationResult.Ok("Installed"));

        var avdMock = new Mock<IAvdManager>();
        avdMock.Setup(a => a.EnsureAvdsCreatedAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(OperationResult.Ok("AVDs created"));

        var adbMock = new Mock<IAdbClient>();
        adbMock.Setup(a => a.ConnectAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var loggerMock = new Mock<ILogger<AndroidSdkManager>>();

        var sut = new AndroidSdkManager(envMock.Object, licMock.Object, instMock.Object, avdMock.Object, adbMock.Object, loggerMock.Object);

        var result = await sut.RunFullSetupAsync("C:\\dummy", acceptLicenses: true);

        result.Success.Should().BeTrue();
        avdMock.Verify(a => a.EnsureAvdsCreatedAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
