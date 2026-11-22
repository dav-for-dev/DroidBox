using DroidBox.Core.Entities;
using DroidBox.Core.Enums;
using DroidBox.Core.Models;
using FluentAssertions;
using Xunit;

namespace DroidBox.Core.Tests;

public class CoreEntityTests
{
    [Fact]
    public void DeviceProfile_Defaults_ShouldBeValid()
    {
        var profile = new DeviceProfile
        {
            Name = "tablet",
            DisplayName = "Standard Tablet",
            DeviceType = DeviceType.Tablet,
            Width = 1280,
            Height = 800,
            Dpi = 213,
            Orientation = DeviceOrientation.Landscape
        };

        profile.Name.Should().Be("tablet");
        profile.DeviceType.Should().Be(DeviceType.Tablet);
        profile.Width.Should().Be(1280);
        profile.Height.Should().Be(800);
        profile.Dpi.Should().Be(213);
        profile.Orientation.Should().Be(DeviceOrientation.Landscape);
    }

    [Fact]
    public void OperationResult_Success_ShouldHaveExpectedProperties()
    {
        var result = OperationResult.Ok("Operation succeeded");

        result.Success.Should().BeTrue();
        result.Message.Should().Be("Operation succeeded");
        result.ErrorCode.Should().BeNull();
    }

    [Fact]
    public void OperationResult_Fail_ShouldHaveExpectedProperties()
    {
        var result = OperationResult.Fail("Operation failed", "ERR_01");

        result.Success.Should().BeFalse();
        result.Message.Should().Be("Operation failed");
        result.ErrorCode.Should().Be("ERR_01");
    }

    [Fact]
    public void ApkInfo_ShouldStoreMetadataCorrectly()
    {
        var info = new ApkInfo
        {
            PackageName = "com.example.testapp",
            VersionName = "1.0.0",
            VersionCode = 1,
            MinSdkVersion = 24,
            TargetSdkVersion = 34,
            AppName = "Test App",
            SupportedAbis = new List<string> { "x86_64", "arm64-v8a" }
        };

        info.PackageName.Should().Be("com.example.testapp");
        info.VersionName.Should().Be("1.0.0");
        info.VersionCode.Should().Be(1);
        info.MinSdkVersion.Should().Be(24);
        info.TargetSdkVersion.Should().Be(34);
        info.AppName.Should().Be("Test App");
        info.SupportedAbis.Should().Contain("x86_64");
    }
}
