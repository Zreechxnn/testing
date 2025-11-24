using testing.Models;

namespace testing.Repositories;

public interface IRuanganRepository
{
    Task<Ruangan?> GetByIdAsync(int id);
    Task<Ruangan?> GetByIdWithAksesLogsAsync(int id);
    Task<IEnumerable<Ruangan>> GetAllAsync();
    Task<bool> IsNamaExistAsync(string nama, int? excludeId = null);
    Task AddAsync(Ruangan ruangan);
    void Update(Ruangan ruangan);
    void Remove(Ruangan ruangan);
    Task<int> CountAsync();
    Task<bool> SaveAsync();
    Task<int> GetTotalAksesCountAsync(int ruanganId);
    Task<int> GetActiveAksesCountAsync(int ruanganId);
}