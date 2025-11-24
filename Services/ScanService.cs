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
            _logger.LogInformation("Received card registration: UID={Uid}", request.Uid);

            if (string.IsNullOrWhiteSpace(request.Uid))
            {
                return ApiResponse<ScanResponse>.ErrorResult("UID kartu tidak boleh kosong");
            }

            // Normalisasi UID: trim spasi, pertahankan format asli
            var normalizedUid = request.Uid.Trim();

            var existingKartu = await _kartuRepository.GetByUidAsync(normalizedUid);
            if (existingKartu != null)
            {
                _logger.LogWarning("Kartu sudah terdaftar: UID={Uid}", normalizedUid);

                var response = new ScanResponse
                {
                    Success = false,
                    Status = "KARTU SUDAH TERDAFTAR",
                    Message = $"Kartu dengan UID {normalizedUid} sudah terdaftar dalam sistem",
                    Uid = normalizedUid
                };

                return ApiResponse<ScanResponse>.SuccessResult(response);
            }

            var kartu = new Kartu
            {
                Uid = normalizedUid, // Simpan dengan format asli
                Status = "AKTIF"
            };

            await _kartuRepository.AddAsync(kartu);
            var saved = await _kartuRepository.SaveAsync();

            if (!saved)
            {
                return ApiResponse<ScanResponse>.ErrorResult("Gagal mendaftarkan kartu");
            }

            _logger.LogInformation("Kartu berhasil didaftarkan: UID={Uid}", kartu.Uid);

            var logData = new
            {
                Type = "CARD_REGISTERED",
                Uid = kartu.Uid,
                Timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                Message = $"Kartu berhasil didaftarkan: {kartu.Uid}"
            };
            await _hubContext.Clients.All.SendAsync("ReceiveCardRegistration", logData);

            var scanResponse = new ScanResponse
            {
                Success = true,
                Status = "SUKSES",
                Message = "Kartu berhasil didaftarkan",
                Uid = kartu.Uid,
                Timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
            };

            return ApiResponse<ScanResponse>.SuccessResult(scanResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error registering card: UID={Uid}", request.Uid);
            return ApiResponse<ScanResponse>.ErrorResult("Terjadi kesalahan internal server");
        }
    }

    public async Task<ApiResponse<ScanCheckResponse>> CheckCard(ScanCheckRequest request)
    {
        try
        {
            _logger.LogInformation("Checking card: UID={Uid}", request.Uid);

            if (string.IsNullOrWhiteSpace(request.Uid))
            {
                return ApiResponse<ScanCheckResponse>.ErrorResult("UID kartu tidak boleh kosong");
            }

            var kartu = await _kartuRepository.GetByUidAsync(request.Uid);

            if (kartu != null)
            {
                _logger.LogInformation("Kartu ditemukan: UID={Uid}", request.Uid);

                var response = new ScanCheckResponse
                {
                    Success = true,
                    Status = "KARTU TERDAFTAR",
                    Message = "Kartu sudah terdaftar dalam sistem",
                    Uid = kartu.Uid,
                    Terdaftar = true,
                    StatusKartu = kartu.Status
                };

                return ApiResponse<ScanCheckResponse>.SuccessResult(response);
            }
            else
            {
                _logger.LogInformation("Kartu tidak terdaftar: UID={Uid}", request.Uid);

                var response = new ScanCheckResponse
                {
                    Success = true,
                    Status = "KARTU TIDAK TERDAFTAR",
                    Message = "Kartu belum terdaftar dalam sistem",
                    Uid = request.Uid,
                    Terdaftar = false
                };

                return ApiResponse<ScanCheckResponse>.SuccessResult(response);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking card: UID={Uid}", request.Uid);
            return ApiResponse<ScanCheckResponse>.ErrorResult("Terjadi kesalahan internal server");
        }
    }

    public async Task<ApiResponse<List<object>>> GetKartuTerdaftar()
    {
        try
        {
            var kartuTerdaftar = await _kartuRepository.GetAllAsync();
            var result = kartuTerdaftar.Select(k => new
            {
                Id = k.Id,
                Uid = k.Uid,
                Status = k.Status,
                Keterangan = k.Keterangan,
                TanggalDaftar = k.CreatedAt.HasValue ? k.CreatedAt.Value.ToString("yyyy-MM-dd HH:mm:ss") : "Unknown"
            }).OrderBy(k => k.Uid).Cast<object>().ToList();

            return ApiResponse<List<object>>.SuccessResult(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting kartu terdaftar");
            return ApiResponse<List<object>>.ErrorResult("Error retrieving kartu terdaftar");
        }
    }

    public async Task<ApiResponse<object>> DeleteKartu(int id)
    {
        try
        {
            var kartu = await _kartuRepository.GetByIdAsync(id);
            if (kartu == null)
            {
                return ApiResponse<object>.ErrorResult("Kartu tidak ditemukan");
            }

            var hasAccessLog = await _aksesLogRepository.AnyByKartuIdAsync(id);
            if (hasAccessLog)
            {
                return ApiResponse<object>.ErrorResult("Tidak dapat menghapus kartu karena memiliki riwayat akses");
            }

            var userWithCard = await _userRepository.GetByKartuUidAsync(kartu.Uid);
            if (userWithCard != null)
            {
                return ApiResponse<object>.ErrorResult("Tidak dapat menghapus kartu karena terdaftar pada user");
            }

            _kartuRepository.Remove(kartu);
            var saved = await _kartuRepository.SaveAsync();

            if (!saved)
            {
                return ApiResponse<object>.ErrorResult("Gagal menghapus kartu");
            }

            _logger.LogInformation("Kartu deleted: UID={Uid}", kartu.Uid);

            var logData = new
            {
                Type = "CARD_DELETED",
                Uid = kartu.Uid,
                Timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                Message = $"Kartu {kartu.Uid} berhasil dihapus"
            };
            await _hubContext.Clients.All.SendAsync("ReceiveCardRegistration", logData);

            return ApiResponse<object>.SuccessResult(null!, "Kartu berhasil dihapus");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting kartu: {Id}", id);
            return ApiResponse<object>.ErrorResult("Error deleting kartu");
        }
    }
}