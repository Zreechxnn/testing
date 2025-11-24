using testing.Models;

namespace testing.Repositories;

public interface IKartuRepository
{
    Task<Kartu?> GetByIdAsync(int id);
    Task<Kartu?> GetByUidAsync(string uid);
    Task<Kartu?> GetByUserIdAsync(int userId);
    Task<Kartu?> GetByKelasIdAsync(int kelasId);
    Task<IEnumerable<Kartu>> GetAllAsync();
    Task<IEnumerable<Kartu>> GetPagedAsync(int page, int pageSize);
    Task<bool> IsUidExistAsync(string uid, int? excludeId = null);
    Task<bool> IsUserHasCardAsync(int userId);
    Task<bool> IsKelasHasCardAsync(int kelasId);
    Task AddAsync(Kartu kartu);
    void Update(Kartu kartu);
    void Remove(Kartu kartu);
    Task<int> CountAsync();
    Task<bool> SaveAsync();
}