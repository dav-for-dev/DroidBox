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
