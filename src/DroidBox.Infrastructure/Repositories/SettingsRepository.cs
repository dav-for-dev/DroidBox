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
