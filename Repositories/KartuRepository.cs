using Microsoft.EntityFrameworkCore;
using testing.Data;
using testing.Models;

namespace testing.Repositories;

public class KartuRepository : IKartuRepository
{
    private readonly LabDbContext _context;

    public KartuRepository(LabDbContext context)
    {
        _context = context;
    }

    public async Task<Kartu?> GetByIdAsync(int id)
    {
        return await _context.Kartu
            .Include(k => k.User)
            .Include(k => k.Kelas)
            .FirstOrDefaultAsync(k => k.Id == id);
    }

    public async Task<Kartu?> GetByIdReadOnlyAsync(int id)
    {
        return await _context.Kartu
            .Include(k => k.User)
            .Include(k => k.Kelas)
            .AsNoTracking()  // Hanya untuk read-only
            .FirstOrDefaultAsync(k => k.Id == id);
    }

    public async Task<Kartu?> GetByUidAsync(string uid)
    {
        return await _context.Kartu
            .Include(k => k.User)
            .Include(k => k.Kelas)
            .AsNoTracking()
            .FirstOrDefaultAsync(k => k.Uid == uid);
    }

    public async Task<Kartu?> GetByUserIdAsync(int userId)
    {
        return await _context.Kartu
            .Include(k => k.User)
            .Include(k => k.Kelas)
            .AsNoTracking()
            .FirstOrDefaultAsync(k => k.UserId == userId);
    }

    public async Task<Kartu?> GetByKelasIdAsync(int kelasId)
    {
        return await _context.Kartu
            .Include(k => k.User)
            .Include(k => k.Kelas)
            .AsNoTracking()
            .FirstOrDefaultAsync(k => k.KelasId == kelasId);
    }

    public async Task<IEnumerable<Kartu>> GetAllAsync()
    {
        return await _context.Kartu
            .Include(k => k.User)
            .Include(k => k.Kelas)
            .AsNoTracking()
            .OrderByDescending(k => k.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Kartu>> GetPagedAsync(int page, int pageSize)
    {
        return await _context.Kartu
            .Include(k => k.User)
            .Include(k => k.Kelas)
            .AsNoTracking()
            .OrderByDescending(k => k.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<bool> IsUidExistAsync(string uid, int? excludeId = null)
    {
        return await _context.Kartu
            .AnyAsync(k => k.Uid == uid && (excludeId == null || k.Id != excludeId));
    }

    public async Task<bool> IsUserHasCardAsync(int userId)
    {
        return await _context.Kartu
            .AnyAsync(k => k.UserId == userId);
    }

    public async Task<bool> IsKelasHasCardAsync(int kelasId)
    {
        return await _context.Kartu
            .AnyAsync(k => k.KelasId == kelasId);
    }

    public async Task AddAsync(Kartu kartu)
    {
        await _context.Kartu.AddAsync(kartu);
    }

    public void Update(Kartu kartu)
    {
        _context.Kartu.Update(kartu);
    }

    public void Remove(Kartu kartu)
    {
        _context.Kartu.Remove(kartu);
    }

    public async Task<int> CountAsync()
    {
        return await _context.Kartu.CountAsync();
    }

    public async Task<bool> SaveAsync()
    {
        return await _context.SaveChangesAsync() > 0;
    }
}