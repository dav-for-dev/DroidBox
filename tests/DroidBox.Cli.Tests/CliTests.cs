using System.CommandLine;
using DroidBox.Application.Services;
using DroidBox.Cli.Commands;
using FluentAssertions;
using Moq;
using Xunit;

namespace DroidBox.Cli.Tests;

public class CliTests
{
    [Fact]
    public void InstallCommand_Build_ShouldHaveApkArgument()
    {
        var apkServiceMock = new Mock<IApkService>();
        var deviceServiceMock = new Mock<IDeviceService>();

        var cmd = InstallCommand.Build(apkServiceMock.Object, deviceServiceMock.Object);

        cmd.Name.Should().Be("install");
        cmd.Arguments.Should().Contain(a => a.Name == "apk");
    }

    [Fact]
    public void LaunchCommand_Build_ShouldHavePackageArgument()
    {
        var apkServiceMock = new Mock<IApkService>();
        var cmd = LaunchCommand.Build(apkServiceMock.Object);

        cmd.Name.Should().Be("launch");
        cmd.Arguments.Should().Contain(a => a.Name == "package");
    }

    [Fact]
    public void StopCommand_Build_ShouldHaveCorrectName()
    {
        var emulatorServiceMock = new Mock<IEmulatorService>();
        var cmd = StopCommand.Build(emulatorServiceMock.Object);

        cmd.Name.Should().Be("stop");
    }

    [Fact]
    public void DoctorCommand_Build_ShouldHaveCorrectName()
    {
        var setupServiceMock = new Mock<ISdkSetupService>();
        var cmd = DoctorCommand.Build(setupServiceMock.Object);

        cmd.Name.Should().Be("doctor");
    }
}
