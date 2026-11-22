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
