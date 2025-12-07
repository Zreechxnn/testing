using AutoMapper;
using Microsoft.AspNetCore.SignalR;
using testing.Hubs;
using testing.DTOs;
using testing.Models;
using testing.Repositories;

namespace testing.Services;

public class TapService : ITapService
{
    private readonly IKartuRepository _kartuRepository;
    private readonly IRuanganRepository _ruanganRepository;
    private readonly IAksesLogRepository _aksesLogRepository;
    private readonly IKelasRepository _kelasRepository;
    private readonly IUserRepository _userRepository;
    private readonly IHubContext<LogHub> _hubContext;
    private readonly IBroadcastService _broadcastService; // INJECT INI
    private readonly IMapper _mapper;
    private readonly ILogger<TapService> _logger;
    private static readonly TimeZoneInfo WibTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");

    public TapService(
        IKartuRepository kartuRepository,
        IRuanganRepository ruanganRepository,
        IAksesLogRepository aksesLogRepository,
        IKelasRepository kelasRepository,
        IUserRepository userRepository,
        IHubContext<LogHub> hubContext,
        IBroadcastService broadcastService, // TAMBAHKAN PARAMETER
        IMapper mapper,
        ILogger<TapService> logger)
    {
        _kartuRepository = kartuRepository;
        _ruanganRepository = ruanganRepository;
        _aksesLogRepository = aksesLogRepository;
        _kelasRepository = kelasRepository;
        _userRepository = userRepository;
        _hubContext = hubContext;
        _broadcastService = broadcastService; // ASSIGN
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<ApiResponse<TapResponse>> ProcessTap(TapRequest request)
    {
        try
        {
            _logger.LogInformation("=== START PROCESS TAP ===");
            _logger.LogInformation("Received tap: UID={Uid}, Ruangan={Ruangan}, Time={Time}",
                request.Uid, request.IdRuangan, request.Timestamp);

            if (string.IsNullOrWhiteSpace(request.Uid))
            {
                _logger.LogWarning("UID is empty or null");
                return ApiResponse<TapResponse>.ErrorResult("UID tidak boleh kosong");
            }

            var normalizedUid = request.Uid.Trim();

            if (request.IdRuangan <= 0)
            {
                _logger.LogWarning("Invalid Ruangan ID: {RuanganId}", request.IdRuangan);
                return ApiResponse<TapResponse>.ErrorResult("Ruangan ID tidak valid");
            }

            _logger.LogInformation("Searching for card with UID: {Uid}", normalizedUid);
            var kartu = await _kartuRepository.GetByUidAsync(normalizedUid);
            if (kartu == null)
            {
                _logger.LogWarning("Kartu tidak terdaftar: {Uid}", normalizedUid);
                return ApiResponse<TapResponse>.SuccessResult(new TapResponse
                {
                    Status = "KARTU TIDAK TERDAFTAR",
                    Message = "Kartu tidak terdaftar dalam sistem",
                    Ruangan = "Unknown",
                    Waktu = GetCurrentWibTime()
                });
            }

            _logger.LogInformation("Card details - ID: {KartuId}, UID: {Uid}, Status: {Status}, UserId: {UserId}, KelasId: {KelasId}",
                kartu.Id, kartu.Uid, kartu.Status, kartu.UserId, kartu.KelasId);

            if (kartu.Status != "AKTIF")
            {
                _logger.LogWarning("Kartu tidak aktif: {Uid} - Status: {Status}", normalizedUid, kartu.Status);
                return ApiResponse<TapResponse>.SuccessResult(new TapResponse
                {
                    Status = "KARTU TIDAK AKTIF",
                    Message = $"Kartu tidak aktif. Status: {kartu.Status}",
                    Ruangan = "Unknown",
                    Waktu = GetCurrentWibTime()
                });
            }

            _logger.LogInformation("Searching for room with ID: {RuanganId}", request.IdRuangan);
            var ruangan = await _ruanganRepository.GetByIdAsync(request.IdRuangan);
            if (ruangan == null)
            {
                _logger.LogWarning("Ruangan tidak ditemukan: {RuanganId}", request.IdRuangan);
                return ApiResponse<TapResponse>.ErrorResult("Ruangan tidak valid");
            }

            _logger.LogInformation("Room found: ID={RuanganId}, Name={RuanganNama}", ruangan.Id, ruangan.Nama);

            string identitas = GetIdentitasFromKartu(kartu);

            DateTime tapTime;
            DateTime tapTimeUtc;

            if (DateTime.TryParse(request.Timestamp, out tapTime))
            {
                if (tapTime.Kind == DateTimeKind.Unspecified)
                    tapTimeUtc = TimeZoneInfo.ConvertTimeToUtc(tapTime, WibTimeZone);
                else if (tapTime.Kind == DateTimeKind.Local)
                    tapTimeUtc = tapTime.ToUniversalTime();
                else
                    tapTimeUtc = tapTime;
            }
            else
            {
                tapTimeUtc = DateTime.UtcNow;
                tapTime = TimeZoneInfo.ConvertTimeFromUtc(tapTimeUtc, WibTimeZone);
            }

            tapTimeUtc = DateTime.SpecifyKind(tapTimeUtc, DateTimeKind.Utc);

            var result = await ProcessTapLogic(kartu, ruangan, tapTime, tapTimeUtc, identitas);

            _logger.LogInformation("=== END PROCESS TAP - SUCCESS ===");
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "!!! ERROR PROCESSING TAP: UID={Uid}, Ruangan={Ruangan}", request.Uid, request.IdRuangan);
            return ApiResponse<TapResponse>.ErrorResult("Terjadi kesalahan internal server");
        }
    }

    private async Task<ApiResponse<TapResponse>> ProcessTapLogic(Kartu kartu, Ruangan ruangan, DateTime tapTime, DateTime tapTimeUtc, string identitas)
    {
        var lastAccess = await _aksesLogRepository.GetActiveLogByKartuIdAsync(kartu.Id);

        if (lastAccess == null)
        {
            return await ProcessCheckIn(kartu, ruangan, tapTime, tapTimeUtc, identitas);
        }
        else
        {
            return await ProcessCheckOut(kartu, ruangan, lastAccess, tapTime, tapTimeUtc, identitas);
        }
    }

    private async Task<ApiResponse<TapResponse>> ProcessCheckIn(Kartu kartu, Ruangan ruangan, DateTime tapTime, DateTime tapTimeUtc, string identitas)
    {
        var aksesLog = new AksesLog
        {
            KartuId = kartu.Id,
            RuanganId = ruangan.Id,
            TimestampMasuk = tapTimeUtc,
            Status = "CHECKIN"
        };

        await _aksesLogRepository.AddAsync(aksesLog);
        var saved = await _aksesLogRepository.SaveAsync();

        if (!saved)
        {
            return ApiResponse<TapResponse>.ErrorResult("Gagal melakukan check-in");
        }

        var response = new TapResponse
        {
            Status = "SUKSES CHECK-IN",
            Message = "Check-in berhasil",
            Ruangan = ruangan.Nama,
            Waktu = tapTime.ToString("yyyy-MM-dd HH:mm:ss"),
            NamaKelas = identitas
        };

        // 1. Kirim Log spesifik ke Tabel (Realtime List)
        await SendSignalRNotification(aksesLog, kartu, ruangan, tapTime, null, "CHECKIN", identitas);

        // 2. TRIGGER UPDATE DASHBOARD STATS (Counter Cards)
        // Fire and forget agar tidak memperlambat response ke alat
        _ = _broadcastService.PushDashboardStatsAsync();

        return ApiResponse<TapResponse>.SuccessResult(response);
    }

    private async Task<ApiResponse<TapResponse>> ProcessCheckOut(Kartu kartu, Ruangan ruangan, AksesLog lastAccess, DateTime tapTime, DateTime tapTimeUtc, string identitas)
    {
        if (lastAccess.RuanganId != ruangan.Id)
        {
            var ruanganCheckIn = await _ruanganRepository.GetByIdAsync(lastAccess.RuanganId);
            var ruanganNama = ruanganCheckIn?.Nama ?? "Unknown";

            var response = new TapResponse
            {
                Status = "ERROR: BEDA LAB",
                Message = $"Check-out harus dilakukan di ruangan yang sama dengan check-in (Ruangan: {ruanganNama})",
                Ruangan = ruangan.Nama,
                Waktu = tapTime.ToString("yyyy-MM-dd HH:mm:ss"),
                NamaKelas = identitas
            };
            return ApiResponse<TapResponse>.SuccessResult(response);
        }
        else
        {
            lastAccess.TimestampKeluar = tapTimeUtc;
            lastAccess.Status = "CHECKOUT";

            _aksesLogRepository.Update(lastAccess);
            var saved = await _aksesLogRepository.SaveAsync();

            if (!saved)
            {
                return ApiResponse<TapResponse>.ErrorResult("Gagal melakukan check-out");
            }

            var durasi = lastAccess.TimestampKeluar.HasValue ?
                        (lastAccess.TimestampKeluar.Value - lastAccess.TimestampMasuk).TotalMinutes.ToString("F1") + " menit" :
                        "0 menit";

            var response = new TapResponse
            {
                Status = "SUKSES CHECK-OUT",
                Message = "Check-out berhasil",
                Ruangan = ruangan.Nama,
                Waktu = tapTime.ToString("yyyy-MM-dd HH:mm:ss"),
                NamaKelas = identitas
            };

            // 1. Kirim Log spesifik ke Tabel (Realtime List)
            await SendSignalRNotification(lastAccess, kartu, ruangan, tapTime, durasi, "CHECKOUT", identitas);

            // 2. TRIGGER UPDATE DASHBOARD STATS (Counter Cards)
            // Fire and forget
            _ = _broadcastService.PushDashboardStatsAsync();

            return ApiResponse<TapResponse>.SuccessResult(response);
        }
    }

    private string GetIdentitasFromKartu(Kartu kartu)
    {
        if (kartu.KelasId.HasValue && kartu.Kelas != null) return kartu.Kelas.Nama;
        if (kartu.UserId.HasValue && kartu.User != null) return kartu.User.Username;
        return "Tidak Teridentifikasi";
    }

    private async Task SendSignalRNotification(AksesLog aksesLog, Kartu kartu, Ruangan ruangan, DateTime tapTime, string? durasi, string eventType, string identitas)
    {
        try
        {
            var notification = new
            {
                EventId = Guid.NewGuid(),
                EventType = eventType,
                Timestamp = DateTime.UtcNow,
                Data = new
                {
                    LogId = aksesLog.Id,
                    KartuUid = kartu.Uid,
                    Ruangan = ruangan.Nama,
                    RuanganId = ruangan.Id,
                    Identitas = identitas,
                    WaktuMasuk = aksesLog.TimestampMasuk,
                    WaktuKeluar = aksesLog.TimestampKeluar,
                    Durasi = durasi,
                    Status = aksesLog.Status
                }
            };

            if (eventType == "CHECKIN")
            {
                await _hubContext.Clients.All.SendAsync("ReceiveCheckIn", notification);
                // UpdateDashboard di sini untuk list log realtime, bukan stats counter
                await _hubContext.Clients.Group("dashboard").SendAsync("UpdateDashboard", notification);
            }
            else
            {
                await _hubContext.Clients.All.SendAsync("ReceiveCheckOut", notification);
                await _hubContext.Clients.Group("dashboard").SendAsync("UpdateDashboard", notification);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send SignalR notification");
        }
    }

    private string GetCurrentWibTime()
    {
        return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, WibTimeZone).ToString("yyyy-MM-dd HH:mm:ss");
    }

    // --- METHOD GET LAINNYA TIDAK BERUBAH (Hanya dicantumkan agar file lengkap) ---
    public async Task<ApiResponse<List<object>>> GetLogs(int? ruanganId = null)
    {
        try
        {
            IEnumerable<AksesLog> logs;
            if (ruanganId.HasValue)
                logs = await _aksesLogRepository.GetByRuanganIdAsync(ruanganId.Value);
            else
                logs = await _aksesLogRepository.GetLatestAsync(50);

            var result = logs.Select(a =>
            {
                var masukWib = TimeZoneInfo.ConvertTimeFromUtc(a.TimestampMasuk, WibTimeZone);
                var keluarWib = a.TimestampKeluar.HasValue ?
                               TimeOptions(a.TimestampKeluar.Value) : (DateTime?)null;

                string identitas = "Tidak Teridentifikasi";
                if (a.Kartu != null) identitas = GetIdentitasFromKartu(a.Kartu);

                return new
                {
                    Id = a.Id,
                    KartuUid = a.Kartu?.Uid ?? "Unknown",
                    Ruangan = a.Ruangan?.Nama ?? "Unknown",
                    Masuk = masukWib.ToString("yyyy-MM-dd HH:mm:ss"),
                    Keluar = keluarWib?.ToString("yyyy-MM-dd HH:mm:ss"),
                    Status = a.Status,
                    Durasi = a.TimestampKeluar.HasValue ?
                             (a.TimestampKeluar.Value - a.TimestampMasuk).TotalMinutes.ToString("F1") + " menit" : "Masih aktif",
                    Identitas = identitas
                };
            }).Cast<object>().ToList();

            return ApiResponse<List<object>>.SuccessResult(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting logs");
            return ApiResponse<List<object>>.ErrorResult("Error retrieving logs");
        }
    }

    private DateTime TimeOptions(DateTime utc)
    {
        return TimeZoneInfo.ConvertTimeFromUtc(utc, WibTimeZone);
    }

    // ... Sisa method GetKartu, GetRuangan, GetKelas, GetStats, GetStatsHariIni sama persis seperti file asli ...
    // (Disingkat agar tidak terlalu panjang, karena logika tidak berubah)

    public async Task<ApiResponse<List<object>>> GetKartu()
    {
        var kartuList = await _kartuRepository.GetAllAsync();
        var result = kartuList.Select(k => new
        {
            Id = k.Id,
            UID = k.Uid,
            Status = k.Status,
            Keterangan = k.Keterangan,
            UserId = k.UserId,
            KelasId = k.KelasId,
            UserUsername = k.User?.Username,
            KelasNama = k.Kelas?.Nama,
            Identitas = GetIdentitasFromKartu(k)
        }).Cast<object>().ToList();
        return ApiResponse<List<object>>.SuccessResult(result);
    }

    public async Task<ApiResponse<List<object>>> GetRuangan()
    {
        var list = await _ruanganRepository.GetAllAsync();
        return ApiResponse<List<object>>.SuccessResult(list.Select(r => new { r.Id, r.Nama }).Cast<object>().ToList());
    }

    public async Task<ApiResponse<List<object>>> GetKelas()
    {
        var list = await _kelasRepository.GetAllAsync();
        return ApiResponse<List<object>>.SuccessResult(list.Select(k => new { k.Id, k.Nama }).Cast<object>().ToList());
    }

    public async Task<ApiResponse<object>> GetStats()
    {
        var totalAkses = await _aksesLogRepository.CountAsync();
        var logs = await _aksesLogRepository.GetLatestAsync(1000);
        var aktifSekarang = logs.Count(a => a.TimestampKeluar == null);
        var totalKartu = await _kartuRepository.CountAsync();
        var totalKelas = await _kelasRepository.CountAsync();

        return ApiResponse<object>.SuccessResult(new { TotalAkses = totalAkses, AktifSekarang = aktifSekarang, TotalKartu = totalKartu, TotalKelas = totalKelas });
    }

    public async Task<ApiResponse<object>> GetStatsHariIni()
    {
        var hariIni = DateTime.Today;
        var besok = hariIni.AddDays(1);
        var semuaAkses = await _aksesLogRepository.GetAllAsync();
        var aksesHariIni = semuaAkses.Where(a => a.TimestampMasuk >= hariIni && a.TimestampMasuk < besok).ToList();

        var checkin = aksesHariIni.Count;
        var checkout = aksesHariIni.Count(a => a.TimestampKeluar.HasValue);
        var aktif = aksesHariIni.Count(a => !a.TimestampKeluar.HasValue);

        var kartuAktif = aksesHariIni.Where(a => a.Kartu != null)
            .GroupBy(a => a.Kartu!.Uid).Select(g => new { KartuUID = g.Key, Jumlah = g.Count() })
            .OrderByDescending(x => x.Jumlah).Take(5).ToList();

        return ApiResponse<object>.SuccessResult(new { Tanggal = hariIni.ToString("yyyy-MM-dd"), TotalCheckin = checkin, TotalCheckout = checkout, MasihAktif = aktif, KartuPalingAktif = kartuAktif });
    }
}