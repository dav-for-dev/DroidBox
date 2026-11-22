using DroidBox.Application.Services;
using DroidBox.Core.Entities;
using DroidBox.Core.Interfaces;
using DroidBox.Core.Models;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace DroidBox.Application.Tests;

public class ApplicationServiceTests
{
    private readonly Mock<IAdbClient> _adbClientMock;
    private readonly Mock<IApkAnalyzer> _apkAnalyzerMock;
    private readonly Mock<IAppRepository> _appRepositoryMock;
    private readonly Mock<ILogger<ApkService>> _apkLoggerMock;

    public ApplicationServiceTests()
    {
        _adbClientMock = new Mock<IAdbClient>();
        _apkAnalyzerMock = new Mock<IApkAnalyzer>();
        _appRepositoryMock = new Mock<IAppRepository>();
        _apkLoggerMock = new Mock<ILogger<ApkService>>();
    }

    [Fact]
    public async Task ApkService_InstallAsync_ShouldReturnFail_WhenFileDoesNotExist()
    {
        var sut = new ApkService(_adbClientMock.Object, _apkAnalyzerMock.Object, _appRepositoryMock.Object, _apkLoggerMock.Object);

        var result = await sut.InstallAsync("C:\\nonexistent\\app.apk");

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("not found");
    }

    [Fact]
    public async Task ApkService_UninstallAsync_ShouldCallAdbClient()
    {
        var pkg = "com.example.demo";
        _adbClientMock.Setup(a => a.UninstallAsync(pkg, It.IsAny<CancellationToken>()))
            .ReturnsAsync(OperationResult.Ok("Success"));

        _appRepositoryMock.Setup(r => r.GetByPackageNameAsync(pkg, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new InstalledApp { Id = Guid.NewGuid(), PackageName = pkg, AppName = "Demo" });

        var sut = new ApkService(_adbClientMock.Object, _apkAnalyzerMock.Object, _appRepositoryMock.Object, _apkLoggerMock.Object);

        var result = await sut.UninstallAsync(pkg);

        result.Success.Should().BeTrue();
        _adbClientMock.Verify(a => a.UninstallAsync(pkg, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task EmulatorService_StartAsync_ShouldCallRuntimeStart()
    {
        var runtimeMock = new Mock<IAndroidRuntime>();
        var loggerMock = new Mock<ILogger<EmulatorService>>();

        runtimeMock.Setup(r => r.StartAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var sut = new EmulatorService(runtimeMock.Object, loggerMock.Object);

        var result = await sut.StartAsync("TestAvd");

        result.Success.Should().BeTrue();
        result.Message.Should().Contain("started successfully");
        runtimeMock.Verify(r => r.StartAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
