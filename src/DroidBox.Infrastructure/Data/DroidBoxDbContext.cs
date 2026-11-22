using DroidBox.Core.Entities;
using DroidBox.Shared.Helpers;
using Microsoft.EntityFrameworkCore;

namespace DroidBox.Infrastructure.Data;

public class DroidBoxDbContext : DbContext
{
    public DbSet<InstalledApp> InstalledApps => Set<InstalledApp>();
    public DbSet<DeviceProfile> DeviceProfiles => Set<DeviceProfile>();
    public DbSet<SdkPackage> SdkPackages => Set<SdkPackage>();
    public DbSet<AppSetting> AppSettings => Set<AppSetting>();
    public DbSet<ApkHistory> ApkHistories => Set<ApkHistory>();

    public DroidBoxDbContext(DbContextOptions<DroidBoxDbContext> options) : base(options)
    {
    }

    public DroidBoxDbContext()
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            var dbPath = PathHelper.GetDatabasePath();
            optionsBuilder.UseSqlite($"Data Source={dbPath}");
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<InstalledApp>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.PackageName).IsUnique();
            entity.Property(e => e.PackageName).IsRequired().HasMaxLength(256);
            entity.Property(e => e.AppName).IsRequired().HasMaxLength(256);
        });

        modelBuilder.Entity<DeviceProfile>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Name).IsUnique();
            entity.Property(e => e.Name).IsRequired().HasMaxLength(64);
        });

        modelBuilder.Entity<SdkPackage>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.PackageName).IsUnique();
        });

        modelBuilder.Entity<AppSetting>(entity =>
        {
            entity.HasKey(e => e.Key);
            entity.Property(e => e.Key).HasMaxLength(128);
        });

        modelBuilder.Entity<ApkHistory>(entity =>
        {
            entity.HasKey(e => e.Id);
        });
    }
}
