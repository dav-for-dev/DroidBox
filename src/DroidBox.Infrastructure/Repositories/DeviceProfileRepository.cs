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
