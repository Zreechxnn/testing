using AutoMapper;
using testing.DTOs;
using testing.Models;
using testing.Repositories;

namespace testing.Services;

public class PeriodeService : IPeriodeService
{
    private readonly IPeriodeRepository _repo;
    private readonly IMapper _mapper;
    private readonly ILogger<PeriodeService> _logger;

    public PeriodeService(IPeriodeRepository repo, IMapper mapper)
    {
        _repo = repo;
        _mapper = mapper;
        _logger = LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger<PeriodeService>();
    }

    public async Task<ApiResponse<List<PeriodeDto>>> GetAll()
    {
        var data = await _repo.GetAllAsync();
        return ApiResponse<List<PeriodeDto>>.SuccessResult(_mapper.Map<List<PeriodeDto>>(data));
    }

    public async Task<ApiResponse<PeriodeDto>> Create(PeriodeCreateRequest request)
    {
        var model = _mapper.Map<Periode>(request);
        await _repo.AddAsync(model);
        await _repo.SaveAsync();
        return ApiResponse<PeriodeDto>.SuccessResult(_mapper.Map<PeriodeDto>(model));
    }

    public async Task<ApiResponse<object>> SetActive(int id)
    {
        try
        {
            var periode = await _repo.GetByIdAsync(id);
            if (periode == null) return ApiResponse<object>.ErrorResult("Periode tidak ditemukan");

            var allPeriode = await _repo.GetAllAsync();
            foreach (var p in allPeriode)
            {
                p.IsAktif = (p.Id == id);
                _repo.Update(p);
            }

            await _repo.SaveAsync();
            return ApiResponse<object>.SuccessResult(null, $"Periode {periode.Nama} sekarang aktif");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Gagal mengaktifkan periode");
            return ApiResponse<object>.ErrorResult("Gagal mengaktifkan periode");
        }
    }

    public async Task<ApiResponse<object>> Delete(int id)
    {
        try
        {
            var periode = await _repo.GetByIdAsync(id);
            if (periode == null)
            {
                return ApiResponse<object>.ErrorResult("Periode tidak ditemukan");
            }

            // Opsional: Cek jika periode aktif, cegah penghapusan
            if (periode.IsAktif)
            {
                return ApiResponse<object>.ErrorResult("Tidak dapat menghapus periode yang sedang aktif.");
            }

            _repo.Remove(periode);
            await _repo.SaveAsync();

            return ApiResponse<object>.SuccessResult(null, "Periode berhasil dihapus");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting periode {Id}", id);
            // Error ini biasanya muncul jika masih ada Kelas yang terhubung ke Periode ini
            return ApiResponse<object>.ErrorResult("Gagal menghapus periode. Pastikan tidak ada Kelas yang terhubung dengan periode ini.");
        }
    }
}