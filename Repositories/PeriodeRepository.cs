using Microsoft.EntityFrameworkCore;
using testing.Data;
using testing.Models;

namespace testing.Repositories;

public interface IPeriodeRepository
{
    Task<IEnumerable<Periode>> GetAllAsync();
    Task AddAsync(Periode periode);
    Task<bool> SaveAsync();
}

public class PeriodeRepository : IPeriodeRepository
{
    private readonly LabDbContext _context;

    public PeriodeRepository(LabDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Periode>> GetAllAsync()
    {
        return await _context.Periode.OrderByDescending(p => p.Id).ToListAsync();
    }

    public async Task AddAsync(Periode periode)
    {
        await _context.Periode.AddAsync(periode);
    }

    public async Task<bool> SaveAsync()
    {
        return await _context.SaveChangesAsync() > 0;
    }
}