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

    public async Task<AksesLog?> GetByIdAsync(int id)
    {
        return await _context.AksesLog
            .Include(a => a.Kartu)
            .ThenInclude(k => k!.User)
            .Include(a => a.Kartu)
            .ThenInclude(k => k!.Kelas)
            .Include(a => a.Ruangan)
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<IEnumerable<AksesLog>> GetAllAsync()
    {
        return await _context.AksesLog
            .Include(a => a.Kartu)
            .ThenInclude(k => k!.User)
            .Include(a => a.Kartu)
            .ThenInclude(k => k!.Kelas)
            .Include(a => a.Ruangan)
            .AsNoTracking()
            .OrderByDescending(a => a.TimestampMasuk)
            .ToListAsync();
    }

    public async Task<IEnumerable<AksesLog>> GetPagedAsync(int page, int pageSize)
    {
        return await _context.AksesLog
            .Include(a => a.Kartu)
            .ThenInclude(k => k!.User)
            .Include(a => a.Kartu)
            .ThenInclude(k => k!.Kelas)
            .Include(a => a.Ruangan)
            .AsNoTracking()
            .OrderByDescending(a => a.TimestampMasuk)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<IEnumerable<AksesLog>> GetByKartuIdAsync(int kartuId)
    {
        return await _context.AksesLog
            .Include(a => a.Kartu)
            .ThenInclude(k => k!.User)
            .Include(a => a.Kartu)
            .ThenInclude(k => k!.Kelas)
            .Include(a => a.Ruangan)
            .Where(a => a.KartuId == kartuId)
            .AsNoTracking()
            .OrderByDescending(a => a.TimestampMasuk)
            .ToListAsync();
    }

    public async Task<IEnumerable<AksesLog>> GetByRuanganIdAsync(int ruanganId)
    {
        return await _context.AksesLog
            .Include(a => a.Kartu)
            .ThenInclude(k => k!.User)
            .Include(a => a.Kartu)
            .ThenInclude(k => k!.Kelas)
            .Include(a => a.Ruangan)
            .Where(a => a.RuanganId == ruanganId)
            .AsNoTracking()
            .OrderByDescending(a => a.TimestampMasuk)
            .ToListAsync();
    }

    public async Task<IEnumerable<AksesLog>> GetLatestAsync(int count)
    {
        return await _context.AksesLog
            .Include(a => a.Kartu)
            .ThenInclude(k => k!.User)
            .Include(a => a.Kartu)
            .ThenInclude(k => k!.Kelas)
            .Include(a => a.Ruangan)
            .AsNoTracking()
            .OrderByDescending(a => a.TimestampMasuk)
            .Take(count)
            .ToListAsync();
    }

    public async Task<AksesLog?> GetActiveLogByKartuIdAsync(int kartuId)
    {
        return await _context.AksesLog
            .Include(a => a.Kartu)
            .ThenInclude(k => k!.User)
            .Include(a => a.Kartu)
            .ThenInclude(k => k!.Kelas)
            .Include(a => a.Ruangan)
            .Where(a => a.KartuId == kartuId && a.TimestampKeluar == null)
            .FirstOrDefaultAsync();
    }

    public async Task<bool> AnyByKartuIdAsync(int kartuId)
    {
        return await _context.AksesLog
            .AnyAsync(a => a.KartuId == kartuId);
    }

    public async Task<bool> AnyByRuanganIdAsync(int ruanganId)
    {
        return await _context.AksesLog
            .AnyAsync(a => a.RuanganId == ruanganId);
    }

    public async Task AddAsync(AksesLog aksesLog)
    {
        await _context.AksesLog.AddAsync(aksesLog);
    }

    public void Update(AksesLog aksesLog)
    {
        _context.AksesLog.Update(aksesLog);
    }

    // Method delete yang benar
    public async Task<bool> DeleteAsync(int id)
    {
        var aksesLog = await _context.AksesLog.FindAsync(id);
        if (aksesLog == null)
        {
            return false;
        }

        _context.AksesLog.Remove(aksesLog);
        return await _context.SaveChangesAsync() > 0;
    }

    public void Remove(AksesLog aksesLog)
    {
        _context.AksesLog.Remove(aksesLog);
    }

    public async Task<int> CountAsync()
    {
        return await _context.AksesLog.CountAsync();
    }

    public async Task<int> CountByRuanganIdAsync(int ruanganId)
    {
        return await _context.AksesLog
            .CountAsync(a => a.RuanganId == ruanganId);
    }

    public async Task<int> CountActiveByRuanganIdAsync(int ruanganId)
    {
        return await _context.AksesLog
            .CountAsync(a => a.RuanganId == ruanganId && a.TimestampKeluar == null);
    }

    public async Task<int> CountByDateRangeAsync(DateTime start, DateTime end)
    {
        return await _context.AksesLog
            .CountAsync(a => a.TimestampMasuk >= start && a.TimestampMasuk < end);
    }

    public async Task<IEnumerable<AksesLog>> GetByDateRangeAsync(DateTime start, DateTime end)
    {
        return await _context.AksesLog
            .Include(a => a.Kartu)
            .ThenInclude(k => k!.User)
            .Include(a => a.Kartu)
            .ThenInclude(k => k!.Kelas)
            .Include(a => a.Ruangan)
            .Where(a => a.TimestampMasuk >= start && a.TimestampMasuk < end)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<bool> SaveAsync()
    {
        try
        {
            var result = await _context.SaveChangesAsync();
            return result > 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving AksesLog: {ex.Message}");
            throw;
        }
    }

    public async Task<int> CountByKelasIdAsync(int kelasId)
    {
        return await _context.AksesLog
            .Include(a => a.Kartu)
            .CountAsync(a => a.Kartu != null && a.Kartu.KelasId == kelasId);
    }

    public async Task<int> CountActiveByKelasIdAsync(int kelasId)
    {
        return await _context.AksesLog
            .Include(a => a.Kartu)
            .CountAsync(a => a.Kartu != null && a.Kartu.KelasId == kelasId && a.TimestampKeluar == null);
    }

    public async Task<int> CountByRuanganIdAndDateAsync(int ruanganId, DateTime date)
    {
        var start = date.Date;
        var end = start.AddDays(1);
        return await _context.AksesLog
            .CountAsync(a => a.RuanganId == ruanganId && a.TimestampMasuk >= start && a.TimestampMasuk < end);
    }

    public async Task<Dictionary<int, int>> GetMonthlyStatsAsync(int year)
    {
        // Grouping berdasarkan Bulan dan Hitung Jumlahnya
        return await _context.AksesLog
            .Where(a => a.TimestampMasuk.Year == year)
            .GroupBy(a => a.TimestampMasuk.Month)
            .Select(g => new { Month = g.Key, Count = g.Count() })
            .ToDictionaryAsync(g => g.Month, g => g.Count);
    }

    public async Task<Dictionary<DateTime, int>> GetDailyStatsAsync(DateTime start, DateTime end)
    {
        // Grouping berdasarkan Tanggal (Date Only)
        return await _context.AksesLog
            .Where(a => a.TimestampMasuk >= start && a.TimestampMasuk <= end)
            .GroupBy(a => a.TimestampMasuk.Date)
            .Select(g => new { Date = g.Key, Count = g.Count() })
            .ToDictionaryAsync(g => g.Date, g => g.Count);
    }

    public async Task<Dictionary<DateTime, int>> GetMonthlyStatsRangeAsync(DateTime start, DateTime end)
    {
        try
        {
            // Grouping berdasarkan Tahun dan Bulan (tanggal 1 setiap bulan sebagai key)
            return await _context.AksesLog
            .Where(a => a.TimestampMasuk >= start && a.TimestampMasuk <= end)
            .GroupBy(a => new { a.TimestampMasuk.Year, a.TimestampMasuk.Month })
            .Select(g => new
            {
                YearMonth = new DateTime(g.Key.Year, g.Key.Month, 1),
                Count = g.Count()
            })
            .ToDictionaryAsync(g => g.YearMonth, g => g.Count);
    }
        catch (Exception ex)
        {
            // Log error dan return dictionary kosong
            Console.WriteLine($"Error in GetMonthlyStatsRangeAsync: {ex.Message}");
            return new Dictionary<DateTime, int>();
        }
    }

    public async Task<bool> DeleteAllAsync()
    {
        var allLogs = await _context.AksesLog.ToListAsync();

        if (!allLogs.Any())
        {
            return false;
        }

        _context.AksesLog.RemoveRange(allLogs);
        return await _context.SaveChangesAsync() > 0;
    }
}