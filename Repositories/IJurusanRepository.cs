using testing.Models;

namespace testing.Repositories;

public interface IJurusanRepository
{
    Task<IEnumerable<Jurusan>> GetAllAsync();
    Task<Jurusan?> GetByIdAsync(int id);
    Task<bool> IsKodeExistAsync(string kode, int? excludeId = null);
    Task AddAsync(Jurusan jurusan);
    void Update(Jurusan jurusan);
    void Remove(Jurusan jurusan);
    Task<bool> SaveAsync();
}