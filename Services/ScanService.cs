using AutoMapper;
using Microsoft.AspNetCore.SignalR;
using testing.Hubs;
using testing.DTOs;
using testing.Models;
using testing.Repositories;

namespace testing.Services;

public class ScanService : IScanService
{
    private readonly IKartuRepository _kartuRepository;
    private readonly IAksesLogRepository _aksesLogRepository;
    private readonly IUserRepository _userRepository;
    private readonly IHubContext<LogHub> _hubContext;
    private readonly IMapper _mapper;
    private readonly ILogger<ScanService> _logger;

    public ScanService(
        IKartuRepository kartuRepository,
        IAksesLogRepository aksesLogRepository,
        IUserRepository userRepository,
        IHubContext<LogHub> hubContext,
        IMapper mapper,
        ILogger<ScanService> logger)
    {
        _kartuRepository = kartuRepository;
        _aksesLogRepository = aksesLogRepository;
        _userRepository = userRepository;
        _hubContext = hubContext;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<ApiResponse<ScanResponse>> RegisterCard(ScanRequest request)
    {
        try
        {
            // 1. Validasi Input Dasar
            if (string.IsNullOrWhiteSpace(request.Uid))
            {
                return ApiResponse<ScanResponse>.ErrorResult("UID kartu tidak boleh kosong");
            }

            // Normalisasi: Hapus spasi di awal/akhir
            var normalizedUid = request.Uid.Trim();

            _logger.LogInformation("Processing Register: UID={Uid}", normalizedUid);

            // 2. CEK DUPLIKAT (PENTING AGAR TIDAK ERROR 500)
            // Kita cek apakah kartu ini sudah ada di database?
            var existingKartu = await _kartuRepository.GetByUidAsync(normalizedUid);

            if (existingKartu != null)
            {
                // Jika sudah ada, jangan throw error.
                // Kembalikan sukses dengan status "EXISTING". 
                // Ini mencegah alat/frontend mengira sistem crash.
                return ApiResponse<ScanResponse>.SuccessResult(new ScanResponse
                {
                    Success = true,
                    Status = "EXISTING",
                    Message = "Kartu sudah terdaftar sebelumnya",
                    Uid = existingKartu.Uid,
                    Timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                });
            }

            // 3. Buat Object Baru
            // Kita biarkan UserId dan KelasId NULL sesuai constraint database
            var kartu = new Kartu
            {
                Uid = normalizedUid,
                Status = "AKTIF",
                Keterangan = "Registered via Scan", // Keterangan default
                CreatedAt = DateTime.UtcNow,        // Wajib isi CreatedAt

                // Pastikan ini NULL agar lolos constraint "CK_Kartu_SingleOwner"
                UserId = null,
                KelasId = null
            };

            // 4. Simpan ke Database
            await _kartuRepository.AddAsync(kartu);
            var saved = await _kartuRepository.SaveAsync();

            if (!saved)
            {
                return ApiResponse<ScanResponse>.ErrorResult("Database menolak penyimpanan data (No rows affected).");
            }

            // 5. Kirim Notifikasi Realtime ke Dashboard (SignalR)
            try
            {
                var logData = new
                {
                    Type = "CARD_REGISTERED",
                    Uid = kartu.Uid,
                    Timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    Message = $"Kartu baru berhasil didaftarkan: {kartu.Uid}"
                };
                await _hubContext.Clients.All.SendAsync("ReceiveCardRegistration", logData);
            }
            catch (Exception hubEx)
            {
                // Jangan biarkan error SignalR membatalkan response API
                _logger.LogWarning("Gagal mengirim notifikasi SignalR: {Message}", hubEx.Message);
            }

            // 6. Return Sukses
            var scanResponse = new ScanResponse
            {
                Success = true,
                Status = "SUCCESS",
                Message = "Kartu berhasil didaftarkan",
                Uid = kartu.Uid,
                Timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
            };

            return ApiResponse<ScanResponse>.SuccessResult(scanResponse);
        }
        catch (Exception ex)
        {
            // Tangkap Error Database Sebenarnya (InnerException)
            var realError = ex.InnerException != null ? ex.InnerException.Message : ex.Message;

            _logger.LogError(ex, "CRITICAL ERROR RegisterCard: {Message}", realError);

            // Tampilkan error asli di response API untuk debugging
            return ApiResponse<ScanResponse>.ErrorResult($"Gagal Register: {realError}");
        }
    }

    public async Task<ApiResponse<ScanCheckResponse>> CheckCard(ScanCheckRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Uid))
                return ApiResponse<ScanCheckResponse>.ErrorResult("UID kosong");

            var kartu = await _kartuRepository.GetByUidAsync(request.Uid);

            if (kartu != null)
            {
                return ApiResponse<ScanCheckResponse>.SuccessResult(new ScanCheckResponse
                {
                    Success = true,
                    Status = "TERDAFTAR",
                    Message = "Kartu terdaftar",
                    Uid = kartu.Uid,
                    Terdaftar = true,
                    StatusKartu = kartu.Status
                });
            }
            else
            {
                return ApiResponse<ScanCheckResponse>.SuccessResult(new ScanCheckResponse
                {
                    Success = true,
                    Status = "BELUM TERDAFTAR",
                    Message = "Kartu belum terdaftar",
                    Uid = request.Uid,
                    Terdaftar = false
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking card");
            return ApiResponse<ScanCheckResponse>.ErrorResult("Server Error");
        }
    }

    public async Task<ApiResponse<List<object>>> GetKartuTerdaftar()
    {
        try
        {
            var data = await _kartuRepository.GetAllAsync();
            var result = data.Select(k => new
            {
                k.Id,
                k.Uid,
                k.Status,
                k.Keterangan,
                Tanggal = k.CreatedAt.HasValue ? k.CreatedAt.Value.ToString("yyyy-MM-dd HH:mm") : "-"
            }).Cast<object>().ToList();

            return ApiResponse<List<object>>.SuccessResult(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting list");
            return ApiResponse<List<object>>.ErrorResult("Server Error");
        }
    }

    public async Task<ApiResponse<object>> DeleteKartu(int id)
    {
        try
        {
            var kartu = await _kartuRepository.GetByIdAsync(id);
            if (kartu == null) return ApiResponse<object>.ErrorResult("Tidak ditemukan");

            // Cek Relasi sebelum hapus
            if (await _aksesLogRepository.AnyByKartuIdAsync(id))
                return ApiResponse<object>.ErrorResult("Gagal: Kartu memiliki riwayat log akses");

            if (await _userRepository.GetByKartuUidAsync(kartu.Uid) != null)
                return ApiResponse<object>.ErrorResult("Gagal: Kartu masih dipakai User");

            _kartuRepository.Remove(kartu);
            if (await _kartuRepository.SaveAsync())
            {
                // Notif SignalR Delete
                try
                {
                    await _hubContext.Clients.All.SendAsync("ReceiveCardRegistration", new
                    {
                        Type = "CARD_DELETED",
                        Uid = kartu.Uid
                    });
                }
                catch { }

                return ApiResponse<object>.SuccessResult(null!, "Terhapus");
            }

            return ApiResponse<object>.ErrorResult("Gagal hapus");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error delete");
            return ApiResponse<object>.ErrorResult($"Server Error: {ex.Message}");
        }
    }
}