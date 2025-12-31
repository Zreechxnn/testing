using AutoMapper;
using Microsoft.AspNetCore.SignalR;
using testing.DTOs;
using testing.Hubs;
using testing.Models;
using testing.Repositories;

namespace testing.Services;

public class KelasService : IKelasService
{
    private readonly IKelasRepository _kelasRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<KelasService> _logger;
    private readonly IHubContext<LogHub> _hubContext;

    public KelasService(
        IKelasRepository kelasRepository,
        IMapper mapper,
        ILogger<KelasService> logger,
        IHubContext<LogHub> hubContext)
    {
        _kelasRepository = kelasRepository;
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
            if (string.IsNullOrWhiteSpace(request.Nama))
            {
                return ApiResponse<KelasDto>.ErrorResult("Nama kelas harus diisi");
            }

            var existingKelas = await _kelasRepository.IsNamaExistAsync(request.Nama);
            if (existingKelas)
            {
                return ApiResponse<KelasDto>.ErrorResult("Kelas dengan nama tersebut sudah terdaftar");
            }

            var kelas = _mapper.Map<Kelas>(request);
            await _kelasRepository.AddAsync(kelas);
            var saved = await _kelasRepository.SaveAsync();

            if (!saved)
            {
                return ApiResponse<KelasDto>.ErrorResult("Gagal menyimpan kelas");
            }

            _logger.LogInformation("Kelas created: {Nama}", kelas.Nama);

            var kelasDto = _mapper.Map<KelasDto>(kelas);

            // TAMBAHKAN NOTIFIKASI SIGNALR
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
            // 1. Ambil data lama
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

            // 3. Validasi Duplikat
            var existingKelas = await _kelasRepository.IsNamaExistAsync(request.Nama, id);
            if (existingKelas)
            {
                return ApiResponse<KelasDto>.ErrorResult("Kelas dengan nama tersebut sudah terdaftar");
            }

            var namaLama = kelas.Nama;

            // 4. MAPPING DATA BARU
            _mapper.Map(request, kelas);

            // ============================================================
            // [FIX PENTING 1]: Paksa ID agar tetap sama dengan ID URL
            // Karena request body tidak punya ID, Mapper mungkin mengubahnya jadi 0
            // ============================================================
            kelas.Id = id;

            // ============================================================
            // [FIX PENTING 2]: Null-kan objek relasi agar EF Core
            // hanya melihat perubahan pada kolom PeriodeId
            // ============================================================
            kelas.Periode = null;

            // 5. Update & Save
            // _kelasRepository.Update(kelas); <--- HAPUS BARIS INI (Tidak perlu jika data diload dr DB)

            var saved = await _kelasRepository.SaveAsync();

            if (!saved)
            {
                // Jika data yang dikirim SAMA PERSIS dengan di DB, saved akan false.
                // Kita bisa anggap sukses atau error tergantung kebutuhan.
                // Di sini kita return error untuk debugging.
                return ApiResponse<KelasDto>.ErrorResult("Gagal mengupdate kelas (Data tidak berubah atau ID salah)");
            }

            _logger.LogInformation("Kelas updated: {Id} - {Nama}", kelas.Id, kelas.Nama);

            var kelasDto = _mapper.Map<KelasDto>(kelas);

            // Kirim Notif SignalR (Code Anda sebelumnya)
            // ...

            return ApiResponse<KelasDto>.SuccessResult(kelasDto, "Kelas berhasil diupdate");
        }
        catch (Exception ex)
        {
            // Log error lengkap biar ketahuan di terminal
            _logger.LogError(ex, "Error updating kelas: {Id}. Message: {Message}", id, ex.Message);
            // Tampilkan pesan error asli di response sementara untuk debugging
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
            return ApiResponse<object>.ErrorResult("Gagal menghapus kelas");
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
            await _hubContext.Clients.Group("admin").SendAsync("SystemNotification", notification);
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

    public async Task<ApiResponse<List<KelasDto>>> GetKelasByPeriode(int periodeId)
    {
        var data = await _kelasRepository.GetByPeriodeAsync(periodeId);
        return ApiResponse<List<KelasDto>>.SuccessResult(_mapper.Map<List<KelasDto>>(data));
    }
}