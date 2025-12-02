using testing.Models;

namespace testing.Repositories;

public interface IKelasRepository
{
    Task<Kelas?> GetByIdAsync(int id);
    Task<IEnumerable<Kelas>> GetAllAsync();
    Task<bool> IsNamaExistAsync(string nama, int? excludeId = null);
    Task AddAsync(Kelas kelas);
    void Update(Kelas kelas);
    void Remove(Kelas kelas);
    Task<int> CountAsync();
    Task<bool> SaveAsync();
}