using testing.Models;

namespace testing.Repositories;

public interface IAksesLogRepository
{
    Task<AksesLog?> GetByIdAsync(int id);
    Task<IEnumerable<AksesLog>> GetAllAsync();
    Task<IEnumerable<AksesLog>> GetPagedAsync(int page, int pageSize);
    Task<IEnumerable<AksesLog>> GetByKartuIdAsync(int kartuId);
    Task<IEnumerable<AksesLog>> GetByRuanganIdAsync(int ruanganId);
    Task<IEnumerable<AksesLog>> GetLatestAsync(int count);
    Task<AksesLog?> GetActiveLogByKartuIdAsync(int kartuId);
    Task<bool> AnyByKartuIdAsync(int kartuId);
    Task<bool> AnyByRuanganIdAsync(int ruanganId);
    Task AddAsync(AksesLog aksesLog);
    void Update(AksesLog aksesLog);

    // Tambahkan method delete yang benar
    Task<bool> DeleteAsync(int id);
    void Remove(AksesLog aksesLog);

    Task<int> CountAsync();
    Task<int> CountByRuanganIdAsync(int ruanganId);
    Task<int> CountActiveByRuanganIdAsync(int ruanganId);
    Task<int> CountByDateRangeAsync(DateTime start, DateTime end);

    // Method tambahan
    Task<IEnumerable<AksesLog>> GetByDateRangeAsync(DateTime start, DateTime end);
    Task<bool> SaveAsync();

    // Method untuk stats
    Task<int> CountByKelasIdAsync(int kelasId);
    Task<int> CountActiveByKelasIdAsync(int kelasId);
    Task<int> CountByRuanganIdAndDateAsync(int ruanganId, DateTime date);

    Task<Dictionary<int, int>> GetMonthlyStatsAsync(int year);
    Task<Dictionary<DateTime, int>> GetDailyStatsAsync(DateTime start, DateTime end);
}