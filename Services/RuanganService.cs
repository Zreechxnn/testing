using AutoMapper;
using Microsoft.AspNetCore.SignalR;
using testing.DTOs;
using testing.Hubs;
using testing.Models;
using testing.Repositories;

namespace testing.Services;

public class RuanganService : IRuanganService
{
    private readonly IRuanganRepository _ruanganRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<RuanganService> _logger;
    private readonly IHubContext<LogHub> _hubContext; // TAMBAHKAN INI

    public RuanganService(
        IRuanganRepository ruanganRepository,
        IMapper mapper,
        ILogger<RuanganService> logger,
        IHubContext<LogHub> hubContext) // TAMBAHKAN PARAMETER
    {
        _ruanganRepository = ruanganRepository;
        _mapper = mapper;
        _logger = logger;
        _hubContext = hubContext; // ASSIGN
    }

    public async Task<ApiResponse<List<RuanganDto>>> GetAllRuangan()
    {
        try
        {
            var ruanganList = await _ruanganRepository.GetAllAsync();
            var ruanganDtos = _mapper.Map<List<RuanganDto>>(ruanganList);
            return ApiResponse<List<RuanganDto>>.SuccessResult(ruanganDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all ruangan");
            return ApiResponse<List<RuanganDto>>.ErrorResult("Gagal mengambil data ruangan");
        }
    }

    public async Task<ApiResponse<RuanganDto>> GetRuanganById(int id)
    {
        try
        {
            var ruangan = await _ruanganRepository.GetByIdAsync(id);
            if (ruangan == null)
            {
                return ApiResponse<RuanganDto>.ErrorResult("Ruangan tidak ditemukan");
            }

            var ruanganDto = _mapper.Map<RuanganDto>(ruangan);
            return ApiResponse<RuanganDto>.SuccessResult(ruanganDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving ruangan by id: {Id}", id);
            return ApiResponse<RuanganDto>.ErrorResult("Gagal mengambil data ruangan");
        }
    }

    public async Task<ApiResponse<RuanganDto>> CreateRuangan(RuanganCreateRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Nama))
            {
                return ApiResponse<RuanganDto>.ErrorResult("Nama ruangan harus diisi");
            }

            var existingRuangan = await _ruanganRepository.IsNamaExistAsync(request.Nama);
            if (existingRuangan)
            {
                return ApiResponse<RuanganDto>.ErrorResult("Ruangan dengan nama tersebut sudah terdaftar");
            }

            var ruangan = _mapper.Map<Ruangan>(request);
            await _ruanganRepository.AddAsync(ruangan);
            var saved = await _ruanganRepository.SaveAsync();

            if (!saved)
            {
                return ApiResponse<RuanganDto>.ErrorResult("Gagal menyimpan ruangan");
            }

            _logger.LogInformation("Ruangan created: {Nama}", ruangan.Nama);

            var ruanganDto = _mapper.Map<RuanganDto>(ruangan);

            // TAMBAHKAN NOTIFIKASI SIGNALR
            await SendRuanganNotification("RUANGAN_CREATED", ruanganDto);

            return ApiResponse<RuanganDto>.SuccessResult(ruanganDto, "Ruangan berhasil ditambahkan");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating ruangan: {Nama}", request.Nama);
            return ApiResponse<RuanganDto>.ErrorResult("Gagal membuat ruangan");
        }
    }

    public async Task<ApiResponse<RuanganDto>> UpdateRuangan(int id, RuanganUpdateRequest request)
    {
        try
        {
            var ruangan = await _ruanganRepository.GetByIdAsync(id);
            if (ruangan == null)
            {
                return ApiResponse<RuanganDto>.ErrorResult("Ruangan tidak ditemukan");
            }

            if (string.IsNullOrWhiteSpace(request.Nama))
            {
                return ApiResponse<RuanganDto>.ErrorResult("Nama ruangan harus diisi");
            }

            var existingRuangan = await _ruanganRepository.IsNamaExistAsync(request.Nama, id);
            if (existingRuangan)
            {
                return ApiResponse<RuanganDto>.ErrorResult("Ruangan dengan nama tersebut sudah terdaftar");
            }

            _mapper.Map(request, ruangan);
            _ruanganRepository.Update(ruangan);
            var saved = await _ruanganRepository.SaveAsync();

            if (!saved)
            {
                return ApiResponse<RuanganDto>.ErrorResult("Gagal mengupdate ruangan");
            }

            _logger.LogInformation("Ruangan updated: {Id} - {Nama}", ruangan.Id, ruangan.Nama);

            var ruanganDto = _mapper.Map<RuanganDto>(ruangan);

            // TAMBAHKAN NOTIFIKASI SIGNALR
            await SendRuanganNotification("RUANGAN_UPDATED", ruanganDto);

            return ApiResponse<RuanganDto>.SuccessResult(ruanganDto, "Ruangan berhasil diupdate");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating ruangan: {Id}", id);
            return ApiResponse<RuanganDto>.ErrorResult("Gagal mengupdate ruangan");
        }
    }

    public async Task<ApiResponse<object>> DeleteRuangan(int id)
    {
        try
        {
            var ruangan = await _ruanganRepository.GetByIdWithAksesLogsAsync(id);
            if (ruangan == null)
            {
                return ApiResponse<object>.ErrorResult("Ruangan tidak ditemukan");
            }

            if (ruangan.AksesLogs != null && ruangan.AksesLogs.Any())
            {
                return ApiResponse<object>.ErrorResult("Tidak dapat menghapus ruangan karena memiliki riwayat akses");
            }

            // Simpan data untuk notifikasi
            var ruanganDto = new RuanganDto
            {
                Id = ruangan.Id,
                Nama = ruangan.Nama
            };

            _ruanganRepository.Remove(ruangan);
            var saved = await _ruanganRepository.SaveAsync();

            if (!saved)
            {
                return ApiResponse<object>.ErrorResult("Gagal menghapus ruangan");
            }

            _logger.LogInformation("Ruangan deleted: {Id} - {Nama}", ruangan.Id, ruangan.Nama);

            // TAMBAHKAN NOTIFIKASI SIGNALR
            await SendRuanganNotification("RUANGAN_DELETED", ruanganDto);

            return ApiResponse<object>.SuccessResult(null!, "Ruangan berhasil dihapus");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting ruangan: {Id}", id);
            return ApiResponse<object>.ErrorResult("Gagal menghapus ruangan");
        }
    }

    public async Task<ApiResponse<RuanganStatsDto>> GetRuanganStats(int id)
    {
        try
        {
            var ruangan = await _ruanganRepository.GetByIdAsync(id);
            if (ruangan == null)
            {
                return ApiResponse<RuanganStatsDto>.ErrorResult("Ruangan tidak ditemukan");
            }

            var totalAkses = await _ruanganRepository.GetTotalAksesCountAsync(id);
            var aktifSekarang = await _ruanganRepository.GetActiveAksesCountAsync(id);

            var stats = new RuanganStatsDto
            {
                Ruangan = ruangan.Nama,
                TotalAkses = totalAkses,
                AktifSekarang = aktifSekarang
            };

            return ApiResponse<RuanganStatsDto>.SuccessResult(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting ruangan stats: {Id}", id);
            return ApiResponse<RuanganStatsDto>.ErrorResult("Gagal mengambil statistik ruangan");
        }
    }

    // METHOD BARU: Helper untuk mengirim notifikasi SignalR
    private async Task SendRuanganNotification(string eventType, RuanganDto ruanganDto, string customMessage = null)
    {
        try
        {
            var notification = new
            {
                EventId = Guid.NewGuid(),
                EventType = eventType,
                Timestamp = DateTime.UtcNow,
                Data = ruanganDto,
                Message = customMessage ?? $"Ruangan {ruanganDto.Nama} telah {GetEventAction(eventType)}"
            };

            // Kirim ke semua client yang terhubung
            await _hubContext.Clients.All.SendAsync("RuanganNotification", notification);

            // Kirim ke grup admin untuk logging sistem
            await _hubContext.Clients.Group("admin").SendAsync("SystemNotification", notification);

            _logger.LogDebug($"SignalR notification sent for {eventType}: {ruanganDto.Nama}");
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
            "RUANGAN_CREATED" => "dibuat",
            "RUANGAN_UPDATED" => "diperbarui",
            "RUANGAN_DELETED" => "dihapus",
            _ => "diubah"
        };
    }
}