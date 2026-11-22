$ErrorActionPreference = "Stop"

Set-Location "E:\code-from-haris\DroidBox\src\DroidBox.Infrastructure"

New-Item -ItemType Directory -Path "Data" -Force | Out-Null
New-Item -ItemType Directory -Path "Repositories" -Force | Out-Null
New-Item -ItemType Directory -Path "Logging" -Force | Out-Null

# --- DroidBoxDbContext.cs ---
@'
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
'@ | Set-Content -Path "Data\DroidBoxDbContext.cs"

# --- REPOSITORIES ---
@'
using DroidBox.Core.Entities;
using DroidBox.Core.Interfaces;
using DroidBox.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DroidBox.Infrastructure.Repositories;

public class AppRepository : IAppRepository
{
    private readonly DroidBoxDbContext _context;

    public AppRepository(DroidBoxDbContext context)
    {
        _context = context;
    }

    public async Task<InstalledApp?> GetByPackageNameAsync(string packageName, CancellationToken cancellationToken = default)
    {
        return await _context.InstalledApps
            .FirstOrDefaultAsync(a => a.PackageName == packageName, cancellationToken);
    }

    public async Task<IReadOnlyList<InstalledApp>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.InstalledApps
            .OrderByDescending(a => a.InstalledAt)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(InstalledApp app, CancellationToken cancellationToken = default)
    {
        var existing = await GetByPackageNameAsync(app.PackageName, cancellationToken);
        if (existing != null)
        {
            existing.AppName = app.AppName;
            existing.VersionName = app.VersionName;
            existing.VersionCode = app.VersionCode;
            existing.UpdatedAt = DateTime.UtcNow;
            existing.InstallPath = app.InstallPath;
            _context.InstalledApps.Update(existing);
        }
        else
        {
            await _context.InstalledApps.AddAsync(app, cancellationToken);
        }
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(InstalledApp app, CancellationToken cancellationToken = default)
    {
        _context.InstalledApps.Update(app);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var app = await _context.InstalledApps.FindAsync(new object[] { id }, cancellationToken);
        if (app != null)
        {
            _context.InstalledApps.Remove(app);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
'@ | Set-Content -Path "Repositories\AppRepository.cs"

@'
using DroidBox.Core.Entities;
using DroidBox.Core.Interfaces;
using DroidBox.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DroidBox.Infrastructure.Repositories;

public class DeviceProfileRepository : IDeviceProfileRepository
{
    private readonly DroidBoxDbContext _context;

    public DeviceProfileRepository(DroidBoxDbContext context)
    {
        _context = context;
    }

    public async Task<DeviceProfile?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.DeviceProfiles.FindAsync(new object[] { id }, cancellationToken);
    }

    public async Task<DeviceProfile?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        return await _context.DeviceProfiles
            .FirstOrDefaultAsync(p => p.Name.ToLower() == name.ToLower(), cancellationToken);
    }

    public async Task<IReadOnlyList<DeviceProfile>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.DeviceProfiles.ToListAsync(cancellationToken);
    }

    public async Task AddAsync(DeviceProfile profile, CancellationToken cancellationToken = default)
    {
        await _context.DeviceProfiles.AddAsync(profile, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(DeviceProfile profile, CancellationToken cancellationToken = default)
    {
        _context.DeviceProfiles.Update(profile);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var profile = await _context.DeviceProfiles.FindAsync(new object[] { id }, cancellationToken);
        if (profile != null)
        {
            _context.DeviceProfiles.Remove(profile);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
'@ | Set-Content -Path "Repositories\DeviceProfileRepository.cs"

@'
using DroidBox.Core.Entities;
using DroidBox.Core.Interfaces;
using DroidBox.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DroidBox.Infrastructure.Repositories;

public class SdkPackageRepository : ISdkPackageRepository
{
    private readonly DroidBoxDbContext _context;

    public SdkPackageRepository(DroidBoxDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<SdkPackage>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SdkPackages.ToListAsync(cancellationToken);
    }

    public async Task AddAsync(SdkPackage package, CancellationToken cancellationToken = default)
    {
        await _context.SdkPackages.AddAsync(package, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var pkg = await _context.SdkPackages.FindAsync(new object[] { id }, cancellationToken);
        if (pkg != null)
        {
            _context.SdkPackages.Remove(pkg);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
'@ | Set-Content -Path "Repositories\SdkPackageRepository.cs"

@'
using DroidBox.Core.Entities;
using DroidBox.Core.Interfaces;
using DroidBox.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DroidBox.Infrastructure.Repositories;

public class SettingsRepository : ISettingsRepository
{
    private readonly DroidBoxDbContext _context;

    public SettingsRepository(DroidBoxDbContext context)
    {
        _context = context;
    }

    public async Task<string?> GetAsync(string key, CancellationToken cancellationToken = default)
    {
        var setting = await _context.AppSettings.FindAsync(new object[] { key }, cancellationToken);
        return setting?.Value;
    }

    public async Task SetAsync(string key, string value, CancellationToken cancellationToken = default)
    {
        var setting = await _context.AppSettings.FindAsync(new object[] { key }, cancellationToken);
        if (setting != null)
        {
            setting.Value = value;
            setting.ModifiedAt = DateTime.UtcNow;
            _context.AppSettings.Update(setting);
        }
        else
        {
            await _context.AppSettings.AddAsync(new AppSetting
            {
                Key = key,
                Value = value,
                ModifiedAt = DateTime.UtcNow
            }, cancellationToken);
        }
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<IDictionary<string, string>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.AppSettings
            .ToDictionaryAsync(s => s.Key, s => s.Value, cancellationToken);
    }
}
'@ | Set-Content -Path "Repositories\SettingsRepository.cs"

# --- LOGGING ---
@'
using DroidBox.Shared.Helpers;
using Serilog;
using Serilog.Events;

namespace DroidBox.Infrastructure.Logging;

public static class LoggingConfiguration
{
    public static void ConfigureLogger()
    {
        var logDir = PathHelper.GetLogsDirectory();
        var logPath = Path.Combine(logDir, "droidbox-.log");

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
            .Enrich.FromLogContext()
            .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
            .WriteTo.File(
                path: logPath,
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 14,
                fileSizeLimitBytes: 10 * 1024 * 1024,
                rollOnFileSizeLimit: true,
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
            .CreateLogger();
    }
}
'@ | Set-Content -Path "Logging\LoggingConfiguration.cs"

# --- DependencyInjection.cs ---
@'
using DroidBox.Core.Interfaces;
using DroidBox.Infrastructure.Data;
using DroidBox.Infrastructure.Repositories;
using DroidBox.Shared.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DroidBox.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, string? connectionString = null)
    {
        var dbPath = connectionString ?? $"Data Source={PathHelper.GetDatabasePath()}";
        services.AddDbContext<DroidBoxDbContext>(options =>
            options.UseSqlite(dbPath));

        services.AddScoped<IAppRepository, AppRepository>();
        services.AddScoped<IDeviceProfileRepository, DeviceProfileRepository>();
        services.AddScoped<ISdkPackageRepository, SdkPackageRepository>();
        services.AddScoped<ISettingsRepository, SettingsRepository>();

        return services;
    }
}
'@ | Set-Content -Path "DependencyInjection.cs"

Write-Host "DroidBox.Infrastructure files created successfully!"
