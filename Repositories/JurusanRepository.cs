using Microsoft.EntityFrameworkCore;
using testing.Data;
using testing.Models;

namespace testing.Repositories;

public class JurusanRepository : IJurusanRepository
{
    private readonly LabDbContext _context;

    public JurusanRepository(LabDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Jurusan>> GetAllAsync()
    {
        return await _context.Jurusan
            .AsNoTracking()
            .OrderBy(j => j.Nama)
            .ToListAsync();
    }

    public async Task<Jurusan?> GetByIdAsync(int id)
    {
        return await _context.Jurusan.FindAsync(id);
    }

    public async Task<bool> IsKodeExistAsync(string kode, int? excludeId = null)
    {
        return await _context.Jurusan
            .AnyAsync(j => j.Kode.ToLower() == kode.ToLower() && (excludeId == null || j.Id != excludeId));
    }

    public async Task AddAsync(Jurusan jurusan)
    {
        await _context.Jurusan.AddAsync(jurusan);
    }

    public void Update(Jurusan jurusan)
    {
        _context.Jurusan.Update(jurusan);
    }

    public void Remove(Jurusan jurusan)
    {
        _context.Jurusan.Remove(jurusan);
    }

    public async Task<bool> SaveAsync()
    {
        return await _context.SaveChangesAsync() > 0;
    }
}