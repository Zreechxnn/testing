using Microsoft.EntityFrameworkCore;
using testing.Data;
using testing.Models;

namespace testing.Repositories;

public class AksesLogRepository : IAksesLogRepository
{
    private readonly LabDbContext _context;

    public AksesLogRepository(LabDbContext context)
    {
        _context = context;
    }

    private IQueryable<AksesLog> GetBaseQuery()
    {
        return _context.AksesLog
            .Include(a => a.Ruangan)
            .Include(a => a.Kartu)
                .ThenInclude(k => k!.Kelas)
            .Include(a => a.Kartu)
                .ThenInclude(k => k!.User)
                    .ThenInclude(u => u!.AnggotaKelas!)
                        .ThenInclude(ak => ak.Kelas)
            .AsNoTracking();
    }

    public async Task<AksesLog?> GetByIdAsync(int id)
    {
        return await GetBaseQuery()
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<IEnumerable<AksesLog>> GetAllAsync()
    {
        return await GetBaseQuery()
            .OrderByDescending(a => a.TimestampMasuk)
            .ToListAsync();
    }

    public async Task<IEnumerable<AksesLog>> GetPagedAsync(int page, int pageSize)
    {
        return await GetBaseQuery()
            .OrderByDescending(a => a.TimestampMasuk)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<IEnumerable<AksesLog>> GetByKartuIdAsync(int kartuId)
    {
        return await GetBaseQuery()
            .Where(a => a.KartuId == kartuId)
            .OrderByDescending(a => a.TimestampMasuk)
            .ToListAsync();
    }

    public async Task<IEnumerable<AksesLog>> GetByRuanganIdAsync(int ruanganId)
    {
        return await GetBaseQuery()
            .Where(a => a.RuanganId == ruanganId)
            .OrderByDescending(a => a.TimestampMasuk)
            .ToListAsync();
    }

    public async Task<IEnumerable<AksesLog>> GetLatestAsync(int count)
    {
        return await GetBaseQuery()
            .OrderByDescending(a => a.TimestampMasuk)
            .Take(count)
            .ToListAsync();
    }

    public async Task<AksesLog?> GetActiveLogByKartuIdAsync(int kartuId)
    {
        return await GetBaseQuery()
            .Where(a => a.KartuId == kartuId && a.TimestampKeluar == null)
            .FirstOrDefaultAsync();
    }

    // ... Sisa method (AddAsync, Update, DeleteAsync, CountAsync, dll) biarkan tetap sama ...
    // Pastikan copy paste sisa method-nya dari kode lamamu agar tidak hilang.

    public async Task<bool> AnyByKartuIdAsync(int kartuId) => await _context.AksesLog.AnyAsync(a => a.KartuId == kartuId);
    public async Task<bool> AnyByRuanganIdAsync(int ruanganId) => await _context.AksesLog.AnyAsync(a => a.RuanganId == ruanganId);
    public async Task AddAsync(AksesLog aksesLog) => await _context.AksesLog.AddAsync(aksesLog);
    public void Update(AksesLog aksesLog) => _context.AksesLog.Update(aksesLog);
    public async Task<bool> DeleteAsync(int id)
    {
        var log = await _context.AksesLog.FindAsync(id);
        if (log == null) return false;
        _context.AksesLog.Remove(log);
        return await _context.SaveChangesAsync() > 0;
    }
    public void Remove(AksesLog aksesLog) => _context.AksesLog.Remove(aksesLog);
    public async Task<int> CountAsync() => await _context.AksesLog.CountAsync();
    public async Task<int> CountByRuanganIdAsync(int ruanganId) => await _context.AksesLog.CountAsync(a => a.RuanganId == ruanganId);
    public async Task<int> CountActiveByRuanganIdAsync(int ruanganId) => await _context.AksesLog.CountAsync(a => a.RuanganId == ruanganId && a.TimestampKeluar == null);
    public async Task<int> CountByDateRangeAsync(DateTime start, DateTime end) => await _context.AksesLog.CountAsync(a => a.TimestampMasuk >= start && a.TimestampMasuk < end);

    public async Task<IEnumerable<AksesLog>> GetByDateRangeAsync(DateTime start, DateTime end)
    {
        return await GetBaseQuery()
            .Where(a => a.TimestampMasuk >= start && a.TimestampMasuk < end)
            .ToListAsync();
    }

    public async Task<bool> SaveAsync() => await _context.SaveChangesAsync() > 0;

    public async Task<int> CountByKelasIdAsync(int kelasId)
    {
        return await _context.AksesLog
            .Include(a => a.Kartu)
            .CountAsync(a => a.Kartu != null && a.Kartu.KelasId == kelasId);
    }
    public async Task<int> CountActiveByKelasIdAsync(int kelasId) => await _context.AksesLog.Include(a => a.Kartu).CountAsync(a => a.Kartu != null && a.Kartu.KelasId == kelasId && a.TimestampKeluar == null);
    public async Task<int> CountByRuanganIdAndDateAsync(int ruanganId, DateTime date)
    {
        var s = date.Date; var e = s.AddDays(1);
        return await _context.AksesLog.CountAsync(a => a.RuanganId == ruanganId && a.TimestampMasuk >= s && a.TimestampMasuk < e);
    }
    public async Task<Dictionary<int, int>> GetMonthlyStatsAsync(int year) => await _context.AksesLog.Where(a => a.TimestampMasuk.Year == year).GroupBy(a => a.TimestampMasuk.Month).Select(g => new { M = g.Key, C = g.Count() }).ToDictionaryAsync(g => g.M, g => g.C);
    public async Task<Dictionary<DateTime, int>> GetDailyStatsAsync(DateTime start, DateTime end) => await _context.AksesLog.Where(a => a.TimestampMasuk >= start && a.TimestampMasuk <= end).GroupBy(a => a.TimestampMasuk.Date).Select(g => new { D = g.Key, C = g.Count() }).ToDictionaryAsync(g => g.D, g => g.C);
    public async Task<bool> DeleteAllAsync() { var all = await _context.AksesLog.ToListAsync(); if (!all.Any()) return false; _context.AksesLog.RemoveRange(all); return await _context.SaveChangesAsync() > 0; }
}