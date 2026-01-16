using Microsoft.EntityFrameworkCore;
using testing.Data;
using testing.Models;

namespace testing.Repositories;

public class UserRepository : IUserRepository
{
    private readonly LabDbContext _context;

    public UserRepository(LabDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetByIdAsync(int id)
    {
        return await _context.Users
            .Include(u => u.Kartu)
            // REVISI: Ambil Kelas via AnggotaKelas
            .Include(u => u.AnggotaKelas!)
                .ThenInclude(ak => ak.Kelas)
                    .ThenInclude(k => k!.Periode) // Opsional: jika butuh nama periode
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == id);
    }

    public async Task<User?> GetByUsernameAsync(string username)
    {
        return await _context.Users
            .Include(u => u.Kartu)
            // REVISI
            .Include(u => u.AnggotaKelas!)
                .ThenInclude(ak => ak.Kelas)
                    .ThenInclude(k => k!.Periode)
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Username.ToLower() == username.ToLower());
    }

    public async Task<User?> GetByKartuUidAsync(string kartuUid)
    {
        return await _context.Users
            .Include(u => u.Kartu)
            // REVISI (Opsional jika butuh data kelas saat scan)
            .Include(u => u.AnggotaKelas!)
                .ThenInclude(ak => ak.Kelas)
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Kartu != null && u.Kartu.Any(k => k.Uid == kartuUid));
    }

    public async Task<IEnumerable<User>> GetAllAsync()
    {
        return await _context.Users
            .Include(u => u.Kartu)
            // REVISI
            .Include(u => u.AnggotaKelas!)
                .ThenInclude(ak => ak.Kelas)
            .AsNoTracking()
            .OrderBy(u => u.Username)
            .ToListAsync();
    }

    public async Task<IEnumerable<User>> GetPagedAsync(int page, int pageSize)
    {
        return await _context.Users
            .Include(u => u.Kartu)
            // REVISI
            .Include(u => u.AnggotaKelas!)
                .ThenInclude(ak => ak.Kelas)
            .AsNoTracking()
            .OrderBy(u => u.Username)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    // --- BAGIAN INI TIDAK BERUBAH ---
    public async Task<bool> IsUsernameExistAsync(string username, int? excludeId = null)
    {
        return await _context.Users
            .AnyAsync(u => u.Username.ToLower() == username.ToLower() && (excludeId == null || u.Id != excludeId));
    }

    public async Task AddAsync(User user) => await _context.Users.AddAsync(user);
    public void Update(User user) => _context.Users.Update(user);
    public void Remove(User user) => _context.Users.Remove(user);
    public async Task<int> CountAsync() => await _context.Users.CountAsync();

    public async Task<int> CountAdminsAsync() => await _context.Users.CountAsync(u => u.Role == "admin");
    public async Task<int> CountByRoleAsync(string role) => await _context.Users.CountAsync(u => u.Role == role);
    public async Task<bool> SaveAsync() => await _context.SaveChangesAsync() > 0;

    public async Task<User?> GetUserWithKartuByUidAsync(string kartuUid)
    {
        return await _context.Users
            .Include(u => u.Kartu)
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Kartu != null && u.Kartu.Any(k => k.Uid == kartuUid));
    }

    public async Task<IEnumerable<User>> GetUsersWithoutKartuAsync()
    {
        return await _context.Users
            .Where(u => u.Kartu == null || !u.Kartu.Any())
            .AsNoTracking()
            .OrderBy(u => u.Username)
            .ToListAsync();
    }
}