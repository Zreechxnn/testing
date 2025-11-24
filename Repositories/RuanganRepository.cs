using Microsoft.EntityFrameworkCore;
using testing.Data;
using testing.Models;

namespace testing.Repositories;

public class RuanganRepository : IRuanganRepository
{
    private readonly LabDbContext _context;

    public RuanganRepository(LabDbContext context)
    {
        _context = context;
    }

    public async Task<Ruangan?> GetByIdAsync(int id)
    {
        return await _context.Ruangan
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task<Ruangan?> GetByIdWithAksesLogsAsync(int id)
    {
        return await _context.Ruangan
            .Include(r => r.AksesLogs)
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task<IEnumerable<Ruangan>> GetAllAsync()
    {
        return await _context.Ruangan
            .AsNoTracking()
            .OrderBy(r => r.Nama)
            .ToListAsync();
    }

    public async Task<bool> IsNamaExistAsync(string nama, int? excludeId = null)
    {
        return await _context.Ruangan
            .AnyAsync(r => r.Nama.ToLower() == nama.ToLower() && (excludeId == null || r.Id != excludeId));
    }

    public async Task AddAsync(Ruangan ruangan)
    {
        await _context.Ruangan.AddAsync(ruangan);
    }

    public void Update(Ruangan ruangan)
    {
        _context.Ruangan.Update(ruangan);
    }

    public void Remove(Ruangan ruangan)
    {
        _context.Ruangan.Remove(ruangan);
    }

    public async Task<int> CountAsync()
    {
        return await _context.Ruangan.CountAsync();
    }

    public async Task<bool> SaveAsync()
    {
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<int> GetTotalAksesCountAsync(int ruanganId)
    {
        return await _context.AksesLog
            .CountAsync(a => a.RuanganId == ruanganId);
    }

    public async Task<int> GetActiveAksesCountAsync(int ruanganId)
    {
        return await _context.AksesLog
            .CountAsync(a => a.RuanganId == ruanganId && a.TimestampKeluar == null);
    }
}