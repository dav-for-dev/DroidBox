using DroidBox.Core.Entities;
using DroidBox.Core.Enums;
using DroidBox.Infrastructure.Data;
using DroidBox.Infrastructure.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace DroidBox.Infrastructure.Tests;

public class RepositoryTests
{
    private DroidBoxDbContext CreateInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<DroidBoxDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var context = new DroidBoxDbContext(options);
        context.Database.EnsureCreated();
        return context;
    }

    [Fact]
    public async Task DeviceProfileRepository_AddAndGet_ShouldReturnProfiles()
    {
        using var context = CreateInMemoryDbContext();
        var repo = new DeviceProfileRepository(context);

        var tablet = DeviceProfile.CreateTablet();
        var mobile = DeviceProfile.CreateMobile();

        await repo.AddAsync(tablet);
        await repo.AddAsync(mobile);

        var profiles = await repo.GetAllAsync();
        profiles.Should().HaveCount(2);
        profiles.Should().Contain(p => p.Name == "tablet");
        profiles.Should().Contain(p => p.Name == "mobile");

        var retrieved = await repo.GetByNameAsync("tablet");
        retrieved.Should().NotBeNull();
        retrieved!.Width.Should().Be(1280);
    }

    [Fact]
    public async Task AppRepository_AddOrUpdateAppAsync_ShouldPersistApp()
    {
        using var context = CreateInMemoryDbContext();
        var repo = new AppRepository(context);

        var app = new InstalledApp
        {
            Id = Guid.NewGuid(),
            PackageName = "com.test.unitapp",
            AppName = "Unit App",
            VersionName = "1.0",
            VersionCode = 1,
            InstalledAt = DateTime.UtcNow
        };

        await repo.AddAsync(app);

        var retrieved = await repo.GetByPackageNameAsync("com.test.unitapp");
        retrieved.Should().NotBeNull();
        retrieved!.AppName.Should().Be("Unit App");

        var all = await repo.GetAllAsync();
        all.Should().ContainSingle(a => a.PackageName == "com.test.unitapp");

        await repo.DeleteAsync(app.Id);
        var afterRemove = await repo.GetByPackageNameAsync("com.test.unitapp");
        afterRemove.Should().BeNull();
    }
}
