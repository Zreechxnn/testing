using Microsoft.EntityFrameworkCore;
using testing.Data;
using testing.Models;

namespace testing.Repositories;

public class KelasRepository : IKelasRepository
{
    private readonly LabDbContext _context;

    public KelasRepository(LabDbContext context)
    {
        _context = context;
    }

    public async Task<Kelas?> GetByIdAsync(int id)
    {
        return await _context.Kelas
            .Include(k => k.Periode)
            .Include(k => k.Jurusan) // Tambahkan Include Jurusan
            .FirstOrDefaultAsync(k => k.Id == id);
    }

    public async Task<IEnumerable<Kelas>> GetAllAsync()
    {
        return await _context.Kelas
            .Include(k => k.Periode)
            .Include(k => k.Jurusan) // Tambahkan Include Jurusan
            .AsNoTracking()
            .OrderBy(k => k.Nama)
            .ToListAsync();
    }

    public async Task<bool> IsNamaExistAsync(string nama, int periodeId, int? excludeId = null)
    {
        return await _context.Kelas
            .AnyAsync(k => k.Nama.ToLower() == nama.ToLower()
                        && k.PeriodeId == periodeId
                        && (excludeId == null || k.Id != excludeId));
    }

    public async Task AddAsync(Kelas kelas)
    {
        await _context.Kelas.AddAsync(kelas);
    }

    public void Update(Kelas kelas)
    {
        _context.Kelas.Update(kelas);
    }

    public void Remove(Kelas kelas)
    {
        _context.Kelas.Remove(kelas);
    }

    public async Task<int> CountAsync()
    {
        return await _context.Kelas.CountAsync();
    }

    public async Task<bool> SaveAsync()
    {
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<IEnumerable<Kelas>> GetByPeriodeAsync(int periodeId)
    {
        return await _context.Kelas
            .Include(k => k.Periode)
            .Include(k => k.Jurusan)
            .Where(k => k.PeriodeId == periodeId)
            .OrderBy(k => k.Nama)
            .ToListAsync();
    }

    // --- IMPLEMENTASI BARU ---

    public async Task<IEnumerable<Kelas>> GetByJurusanAsync(int jurusanId)
    {
        return await _context.Kelas
            .Include(k => k.Periode)
            .Include(k => k.Jurusan)
            .Where(k => k.JurusanId == jurusanId)
            .OrderBy(k => k.Tingkat).ThenBy(k => k.Nama)
            .ToListAsync();
    }

    public async Task<IEnumerable<Kelas>> GetByJurusanAndTingkatAsync(int jurusanId, int tingkat)
    {
        return await _context.Kelas
            .Include(k => k.Periode)
            .Include(k => k.Jurusan)
            .Where(k => k.JurusanId == jurusanId && k.Tingkat == tingkat)
            .OrderBy(k => k.Nama)
            .ToListAsync();
    }
}