using DroidBox.Application;
using DroidBox.Application.Services;
using DroidBox.Core.Interfaces;
using DroidBox.Infrastructure;
using DroidBox.Infrastructure.Data;
using DroidBox.Runtime.AndroidSdk;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;

namespace DroidBox.Integration.Tests;

public class DependencyInjectionTests
{
    [Fact]
    public void ServiceContainer_ShouldResolveAllRequiredServices()
    {
        var services = new ServiceCollection();

        services.AddLogging(builder => builder.AddConsole());
        services.AddInfrastructureServices();
        services.AddApplicationServices();
        services.AddAndroidSdkRuntime();

        var provider = services.BuildServiceProvider();

        // Verify Infrastructure
        var db = provider.GetRequiredService<DroidBoxDbContext>();
        db.Should().NotBeNull();

        var appRepo = provider.GetRequiredService<IAppRepository>();
        appRepo.Should().NotBeNull();

        var profileRepo = provider.GetRequiredService<IDeviceProfileRepository>();
        profileRepo.Should().NotBeNull();

        // Verify Core Runtimes
        var runtime = provider.GetRequiredService<IAndroidRuntime>();
        runtime.Should().NotBeNull();

        // Verify Application Services
        var emulatorService = provider.GetRequiredService<IEmulatorService>();
        emulatorService.Should().NotBeNull();

        var apkService = provider.GetRequiredService<IApkService>();
        apkService.Should().NotBeNull();

        var deviceService = provider.GetRequiredService<IDeviceService>();
        deviceService.Should().NotBeNull();
    }
}
