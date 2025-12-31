using AutoMapper;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using testing.DTOs;
using testing.Hubs;
using testing.Models;
using testing.Repositories;

namespace testing.Services;

public class KelasService : IKelasService
{
    private readonly IKelasRepository _kelasRepository;
    private readonly IPeriodeRepository _periodeRepository; // Inject Repo Periode
    private readonly IMapper _mapper;
    private readonly ILogger<KelasService> _logger;
    private readonly IHubContext<LogHub> _hubContext;

    public KelasService(
        IKelasRepository kelasRepository,
        IPeriodeRepository periodeRepository, // Inject di Constructor
        IMapper mapper,
        ILogger<KelasService> logger,
        IHubContext<LogHub> hubContext)
    {
        _kelasRepository = kelasRepository;
        _periodeRepository = periodeRepository; // Assign
        _mapper = mapper;
        _logger = logger;
        _hubContext = hubContext;
    }

    public async Task<ApiResponse<List<KelasDto>>> GetAllKelas()
    {
        try
        {
            var kelasList = await _kelasRepository.GetAllAsync();
            var kelasDtos = _mapper.Map<List<KelasDto>>(kelasList);
            return ApiResponse<List<KelasDto>>.SuccessResult(kelasDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all kelas");
            return ApiResponse<List<KelasDto>>.ErrorResult("Gagal mengambil data kelas");
        }
    }

    public async Task<ApiResponse<KelasDto>> GetKelasById(int id)
    {
        try
        {
            var kelas = await _kelasRepository.GetByIdAsync(id);
            if (kelas == null)
            {
                return ApiResponse<KelasDto>.ErrorResult("Kelas tidak ditemukan");
            }

            var kelasDto = _mapper.Map<KelasDto>(kelas);
            return ApiResponse<KelasDto>.SuccessResult(kelasDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving kelas by id: {Id}", id);
            return ApiResponse<KelasDto>.ErrorResult("Gagal mengambil data kelas");
        }
    }

    public async Task<ApiResponse<KelasDto>> CreateKelas(KelasCreateRequest request)
    {
        try
        {
            // 1. Validasi Nama Kosong
            if (string.IsNullOrWhiteSpace(request.Nama))
            {
                return ApiResponse<KelasDto>.ErrorResult("Nama kelas harus diisi");
            }

            // 2. Validasi Nama Duplikat
            var existingKelas = await _kelasRepository.IsNamaExistAsync(request.Nama);
            if (existingKelas)
            {
                return ApiResponse<KelasDto>.ErrorResult("Kelas dengan nama tersebut sudah terdaftar");
            }

            // 3. Validasi Periode (Wajib Ada)
            var periodeExist = await _periodeRepository.GetByIdAsync(request.PeriodeId);
            if (periodeExist == null)
            {
                return ApiResponse<KelasDto>.ErrorResult($"Periode dengan ID {request.PeriodeId} tidak ditemukan");
            }

            // 4. Mapping & Save
            var kelas = _mapper.Map<Kelas>(request);

            // Pastikan PeriodeId terisi (Backup jika mapper gagal)
            kelas.PeriodeId = request.PeriodeId;

            await _kelasRepository.AddAsync(kelas);
            var saved = await _kelasRepository.SaveAsync();

            if (!saved)
            {
                return ApiResponse<KelasDto>.ErrorResult("Gagal menyimpan kelas");
            }

            _logger.LogInformation("Kelas created: {Nama}", kelas.Nama);

            // 5. Return Data & SignalR
            var kelasDto = _mapper.Map<KelasDto>(kelas);
            kelasDto.PeriodeNama = periodeExist.Nama; // Isi manual nama periode

            await SendKelasNotification("KELAS_CREATED", kelasDto, $"Kelas baru '{kelas.Nama}' berhasil ditambahkan");

            return ApiResponse<KelasDto>.SuccessResult(kelasDto, "Kelas berhasil ditambahkan");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating kelas: {Nama}", request.Nama);
            return ApiResponse<KelasDto>.ErrorResult("Gagal membuat kelas");
        }
    }

    public async Task<ApiResponse<KelasDto>> UpdateKelas(int id, KelasUpdateRequest request)
    {
        try
        {
            // 1. Ambil Data Lama
            var kelas = await _kelasRepository.GetByIdAsync(id);
            if (kelas == null)
            {
                return ApiResponse<KelasDto>.ErrorResult("Kelas tidak ditemukan");
            }

            // 2. Validasi Nama
            if (string.IsNullOrWhiteSpace(request.Nama))
            {
                return ApiResponse<KelasDto>.ErrorResult("Nama kelas harus diisi");
            }

            // 3. Validasi Duplikat Nama
            var existingKelas = await _kelasRepository.IsNamaExistAsync(request.Nama, id);
            if (existingKelas)
            {
                return ApiResponse<KelasDto>.ErrorResult("Kelas dengan nama tersebut sudah terdaftar");
            }

            // 4. Validasi Periode Baru
            var periodeBaru = await _periodeRepository.GetByIdAsync(request.PeriodeId);
            if (periodeBaru == null)
            {
                return ApiResponse<KelasDto>.ErrorResult($"Periode dengan ID {request.PeriodeId} tidak ditemukan");
            }

            var namaLama = kelas.Nama;

            // 5. Update data secara eksplisit
            kelas.Nama = request.Nama.Trim();
            kelas.PeriodeId = request.PeriodeId;

            // 6. Simpan perubahan
            await _kelasRepository.SaveAsync();

            _logger.LogInformation("Kelas updated: {Id} - {Nama}", kelas.Id, kelas.Nama);

            // 7. Siapkan Response
            var kelasDto = new KelasDto
            {
                Id = kelas.Id,
                Nama = kelas.Nama,
                PeriodeId = kelas.PeriodeId,
                PeriodeNama = periodeBaru.Nama
            };

            await SendKelasNotification("KELAS_UPDATED", kelasDto,
                $"Kelas '{namaLama}' berhasil diupdate menjadi '{kelas.Nama}'");

            return ApiResponse<KelasDto>.SuccessResult(kelasDto, "Kelas berhasil diupdate");
        }
        catch (DbUpdateException dbEx)
        {
            _logger.LogError(dbEx, "Database error updating kelas: {Id}", id);
            return ApiResponse<KelasDto>.ErrorResult($"Database error: {dbEx.InnerException?.Message ?? dbEx.Message}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating kelas: {Id}", id);
            return ApiResponse<KelasDto>.ErrorResult($"Gagal mengupdate kelas: {ex.Message}");
        }
    }

    public async Task<ApiResponse<object>> DeleteKelas(int id)
    {
        try
        {
            var kelas = await _kelasRepository.GetByIdAsync(id);
            if (kelas == null)
            {
                return ApiResponse<object>.ErrorResult("Kelas tidak ditemukan");
            }

            _kelasRepository.Remove(kelas);
            var saved = await _kelasRepository.SaveAsync();

            if (!saved)
            {
                return ApiResponse<object>.ErrorResult("Gagal menghapus kelas");
            }

            _logger.LogInformation("Kelas deleted: {Id} - {Nama}", kelas.Id, kelas.Nama);

            await SendKelasNotification("KELAS_DELETED", new KelasDto
            {
                Id = id,
                Nama = kelas.Nama
            }, $"Kelas '{kelas.Nama}' berhasil dihapus");

            return ApiResponse<object>.SuccessResult(null!, "Kelas berhasil dihapus");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting kelas: {Id}", id);
            return ApiResponse<object>.ErrorResult("Gagal menghapus kelas (Mungkin sedang digunakan data lain)");
        }
    }

    public async Task<ApiResponse<KelasStatsDto>> GetKelasStats(int id)
    {
        try
        {
            var kelas = await _kelasRepository.GetByIdAsync(id);
            if (kelas == null)
            {
                return ApiResponse<KelasStatsDto>.ErrorResult("Kelas tidak ditemukan");
            }

            // Statistik dummy/placeholder (implementasi real butuh query ke log akses)
            var stats = new KelasStatsDto
            {
                Kelas = kelas.Nama,
                TotalAkses = 0,
                AktifSekarang = 0
            };

            return ApiResponse<KelasStatsDto>.SuccessResult(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting kelas stats: {Id}", id);
            return ApiResponse<KelasStatsDto>.ErrorResult("Gagal mengambil statistik kelas");
        }
    }

    public async Task<ApiResponse<List<KelasDto>>> GetKelasByPeriode(int periodeId)
    {
        var data = await _kelasRepository.GetByPeriodeAsync(periodeId);
        return ApiResponse<List<KelasDto>>.SuccessResult(_mapper.Map<List<KelasDto>>(data));
    }

    private async Task SendKelasNotification(string eventType, KelasDto kelasDto, string customMessage = null)
    {
        try
        {
            var notification = new
            {
                EventId = Guid.NewGuid(),
                EventType = eventType,
                Timestamp = DateTime.UtcNow,
                Data = kelasDto,
                Message = customMessage ?? $"Kelas {kelasDto.Nama} telah {GetEventAction(eventType)}"
            };

            await _hubContext.Clients.All.SendAsync("KelasNotification", notification);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, $"Failed to send SignalR notification for {eventType}");
        }
    }

    private string GetEventAction(string eventType)
    {
        return eventType switch
        {
            "KELAS_CREATED" => "dibuat",
            "KELAS_UPDATED" => "diperbarui",
            "KELAS_DELETED" => "dihapus",
            _ => "diubah"
        };
    }
}