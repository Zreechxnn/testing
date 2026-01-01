using AutoMapper;
using Microsoft.AspNetCore.SignalR;
using testing.DTOs;
using testing.Hubs;
using testing.Models;
using testing.Repositories;
namespace testing.Services;

public class AksesLogService : IAksesLogService
{
    private readonly IAksesLogRepository _aksesLogRepository;
    private readonly IKartuRepository _kartuRepository;
    private readonly IKelasRepository _kelasRepository;
    private readonly IRuanganRepository _ruanganRepository;
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<AksesLogService> _logger;
    private readonly IHubContext<LogHub> _hubContext;
    private readonly IBroadcastService _broadcastService;

    // Timezone untuk WIB (UTC+7)
    private static readonly TimeZoneInfo WibTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");

    public AksesLogService(
        IAksesLogRepository aksesLogRepository,
        IKartuRepository kartuRepository,
        IKelasRepository kelasRepository,
        IRuanganRepository ruanganRepository,
        IUserRepository userRepository,
        IMapper mapper,
        ILogger<AksesLogService> logger,
        IHubContext<LogHub> hubContext,
        IBroadcastService broadcastService = null)
    {
        _aksesLogRepository = aksesLogRepository;
        _kartuRepository = kartuRepository;
        _kelasRepository = kelasRepository;
        _ruanganRepository = ruanganRepository;
        _userRepository = userRepository;
        _mapper = mapper;
        _logger = logger;
        _hubContext = hubContext;
        _broadcastService = broadcastService;
    }

    //  konversi UTC ke WIB
    private AksesLogDto ConvertToWibDto(AksesLog aksesLog)
    {
        var dto = _mapper.Map<AksesLogDto>(aksesLog);

        // Konversi UTC ke WIB  ()display)
        dto.TimestampMasuk = TimeZoneInfo.ConvertTimeFromUtc(aksesLog.TimestampMasuk, WibTimeZone);
        if (aksesLog.TimestampKeluar.HasValue)
        {
            dto.TimestampKeluar = TimeZoneInfo.ConvertTimeFromUtc(aksesLog.TimestampKeluar.Value, WibTimeZone);
        }

        return dto;
    }

    public async Task<ApiResponse<List<AksesLogDto>>> GetAllAksesLog()
    {
        try
        {
            var aksesLogs = await _aksesLogRepository.GetAllAsync();
            var aksesLogDtos = aksesLogs.Select(ConvertToWibDto).ToList();
            return ApiResponse<List<AksesLogDto>>.SuccessResult(aksesLogDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all akses log");
            return ApiResponse<List<AksesLogDto>>.ErrorResult("Gagal mengambil data akses log");
        }
    }

    public async Task<ApiResponse<PagedResponse<AksesLogDto>>> GetAksesLogPaged(PagedRequest request)
    {
        try
        {
            if (!request.IsValid())
            {
                return ApiResponse<PagedResponse<AksesLogDto>>.ErrorResult("Parameter pagination tidak valid");
            }

            var aksesLogs = await _aksesLogRepository.GetPagedAsync(request.Page, request.PageSize);
            var totalCount = await _aksesLogRepository.CountAsync();
            var aksesLogDtos = aksesLogs.Select(ConvertToWibDto).ToList();

            var pagedResponse = new PagedResponse<AksesLogDto>(
                aksesLogDtos,
                request.Page,
                request.PageSize,
                totalCount
            );

            return ApiResponse<PagedResponse<AksesLogDto>>.SuccessResult(pagedResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving paged akses log");
            return ApiResponse<PagedResponse<AksesLogDto>>.ErrorResult("Gagal mengambil data akses log");
        }
    }

    public async Task<ApiResponse<AksesLogDto>> GetAksesLogById(int id)
    {
        try
        {
            var aksesLog = await _aksesLogRepository.GetByIdAsync(id);
            if (aksesLog == null)
            {
                return ApiResponse<AksesLogDto>.ErrorResult("Akses log tidak ditemukan");
            }

            var aksesLogDto = ConvertToWibDto(aksesLog);
            return ApiResponse<AksesLogDto>.SuccessResult(aksesLogDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving akses log by id: {Id}", id);
            return ApiResponse<AksesLogDto>.ErrorResult("Gagal mengambil data akses log");
        }
    }

    public async Task<ApiResponse<List<AksesLogDto>>> GetAksesLogByKartuId(int kartuId)
    {
        try
        {
            var aksesLogs = await _aksesLogRepository.GetByKartuIdAsync(kartuId);
            var aksesLogDtos = aksesLogs.Select(ConvertToWibDto).ToList();
            return ApiResponse<List<AksesLogDto>>.SuccessResult(aksesLogDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving akses log by kartu id: {KartuId}", kartuId);
            return ApiResponse<List<AksesLogDto>>.ErrorResult("Gagal mengambil data akses log");
        }
    }

    public async Task<ApiResponse<List<AksesLogDto>>> GetAksesLogByRuanganId(int ruanganId)
    {
        try
        {
            var aksesLogs = await _aksesLogRepository.GetByRuanganIdAsync(ruanganId);
            var aksesLogDtos = aksesLogs.Select(ConvertToWibDto).ToList();
            return ApiResponse<List<AksesLogDto>>.SuccessResult(aksesLogDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving akses log by ruangan id: {RuanganId}", ruanganId);
            return ApiResponse<List<AksesLogDto>>.ErrorResult("Gagal mengambil data akses log");
        }
    }

    public async Task<ApiResponse<List<AksesLogDto>>> GetLatestAksesLog(int count)
    {
        try
        {
            if (count <= 0 || count > 1000)
            {
                return ApiResponse<List<AksesLogDto>>.ErrorResult("Count harus antara 1 dan 1000");
            }

            var aksesLogs = await _aksesLogRepository.GetLatestAsync(count);
            var aksesLogDtos = aksesLogs.Select(ConvertToWibDto).ToList();
            return ApiResponse<List<AksesLogDto>>.SuccessResult(aksesLogDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving latest akses log: {Count}", count);
            return ApiResponse<List<AksesLogDto>>.ErrorResult("Gagal mengambil data akses log");
        }
    }

    public async Task<ApiResponse<AksesLogDto?>> GetActiveAksesLogByKartuId(int kartuId)
    {
        try
        {
            var aksesLog = await _aksesLogRepository.GetActiveLogByKartuIdAsync(kartuId);
            if (aksesLog == null)
            {
                return ApiResponse<AksesLogDto?>.SuccessResult(null, "Tidak ada akses log aktif");
            }

            var aksesLogDto = ConvertToWibDto(aksesLog);
            return ApiResponse<AksesLogDto?>.SuccessResult(aksesLogDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving active akses log by kartu id: {KartuId}", kartuId);
            return ApiResponse<AksesLogDto?>.ErrorResult("Gagal mengambil data akses log");
        }
    }

    public async Task<ApiResponse<object>> DeleteAksesLog(int id)
    {
        try
        {
            var aksesLog = await _aksesLogRepository.GetByIdAsync(id);
            if (aksesLog == null)
            {
                return ApiResponse<object>.ErrorResult("Akses log tidak ditemukan");
            }

            var deleted = await _aksesLogRepository.DeleteAsync(id);
            if (!deleted)
            {
                return ApiResponse<object>.ErrorResult("Gagal menghapus akses log");
            }

            _logger.LogInformation("Akses log deleted: {Id}", id);

            // TAMBAHKAN NOTIFIKASI SIGNALR
            try
            {
                await _hubContext.Clients.All.SendAsync("AksesLogDeleted", new
                {
                    Id = id,
                    Timestamp = DateTime.UtcNow,
                    Message = $"Akses log dengan ID {id} telah dihapus",
                    KartuUid = aksesLog.Kartu?.Uid ?? "Unknown",
                    Ruangan = aksesLog.Ruangan?.Nama ?? "Unknown"
                });

                await _hubContext.Clients.Group("admin").SendAsync("SystemNotification", new
                {
                    Type = "AKSES_LOG_DELETED",
                    Data = new { Id = id, DeletedAt = DateTime.UtcNow },
                    Message = $"Akses log ID {id} dihapus dari sistem"
                });
            }
            catch (Exception hubEx)
            {
                _logger.LogWarning(hubEx, "Gagal mengirim notifikasi SignalR untuk penghapusan akses log");
            }

            return ApiResponse<object>.SuccessResult(null, "Akses log berhasil dihapus");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting akses log: {Id}", id);
            return ApiResponse<object>.ErrorResult("Gagal menghapus akses log");
        }
    }

    public async Task<ApiResponse<DashboardStatsDto>> GetDashboardStats()
    {
        try
        {
            var totalKartu = await _kartuRepository.CountAsync();
            var totalKelas = await _kelasRepository.CountAsync();
            var totalRuangan = await _ruanganRepository.CountAsync();
            var totalUsers = await _userRepository.CountAsync();

            // Ambil semua log (jika data sangat besar nanti perlu optimasi CountAsync dengan filter)
            var semuaAkses = await _aksesLogRepository.GetAllAsync();

            var totalAkses = semuaAkses.Count();
            var aktifSekarang = semuaAkses.Count(a => a.TimestampKeluar == null);

            // --- LOGIKA BARU: HITUNG 30 HARI & 1 TAHUN ---
            var now = DateTime.UtcNow;
            var thirtyDaysAgo = now.AddDays(-30);
            var startOfYear = new DateTime(now.Year, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            var akses30Hari = semuaAkses.Count(a => a.TimestampMasuk >= thirtyDaysAgo);
            var aksesTahunIni = semuaAkses.Count(a => a.TimestampMasuk >= startOfYear);
            // ---------------------------------------------

            var stats = new DashboardStatsDto
            {
                TotalAkses = totalAkses,
                AktifSekarang = aktifSekarang,

                // Isi field baru
                Akses30Hari = akses30Hari,
                AksesTahunIni = aksesTahunIni,

                TotalKartu = totalKartu,
                TotalKelas = totalKelas,
                TotalRuangan = totalRuangan,
                TotalUsers = totalUsers
            };

            // Broadcast update via SignalR
            try
            {
                await _hubContext.Clients.Group("dashboard").SendAsync("DashboardStatsUpdated", new
                {
                    Timestamp = DateTime.UtcNow,
                    Stats = stats
                });
            }
            catch (Exception hubEx)
            {
                _logger.LogDebug(hubEx, "Gagal broadcast dashboard stats");
            }

            return ApiResponse<DashboardStatsDto>.SuccessResult(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting dashboard stats");
            return ApiResponse<DashboardStatsDto>.ErrorResult("Gagal mengambil statistik dashboard");
        }
    }

    public async Task<ApiResponse<TodayStatsDto>> GetTodayStats()
    {
        try
        {
            // Gunakan UTC date untuk konsistensi dengan data di database
            var hariIniUtc = DateTime.UtcNow.Date;
            var besokUtc = hariIniUtc.AddDays(1);

            var aksesHariIni = await _aksesLogRepository.GetByDateRangeAsync(hariIniUtc, besokUtc);

            var checkinHariIni = aksesHariIni.Count();
            var checkoutHariIni = aksesHariIni.Count(a => a.TimestampKeluar.HasValue);
            var masihAktif = aksesHariIni.Count(a => !a.TimestampKeluar.HasValue);

            var kartuAktif = aksesHariIni
                .Where(a => a.Kartu != null)
                .GroupBy(a => a.Kartu!.Uid)
                .Select(g => new { KartuUID = g.Key, Jumlah = g.Count() })
                .OrderByDescending(x => x.Jumlah)
                .Take(5)
                .ToList();

            // Tampilkan tanggal dalam WIB
            var hariIniWib = TimeZoneInfo.ConvertTimeFromUtc(hariIniUtc, WibTimeZone);

            var stats = new TodayStatsDto
            {
                Tanggal = hariIniWib.ToString("yyyy-MM-dd"),
                TotalCheckin = checkinHariIni,
                TotalCheckout = checkoutHariIni,
                MasihAktif = masihAktif,
                KartuPalingAktif = kartuAktif
            };

            return ApiResponse<TodayStatsDto>.SuccessResult(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting today's stats");
            return ApiResponse<TodayStatsDto>.ErrorResult("Gagal mengambil statistik hari ini");
        }
    }
    public async Task<ApiResponse<List<MonthlyStatsDto>>> GetMonthlyStats(int year)
    {
        try
        {
            // 1. Ambil data mentah dari DB (hanya bulan yang ada akses)
            var rawData = await _aksesLogRepository.GetMonthlyStatsAsync(year);

            // 2. Siapkan array nama bulan (Indonesia)
            string[] namaBulan = { "Jan", "Feb", "Mar", "Apr", "Mei", "Jun", "Jul", "Agu", "Sep", "Okt", "Nov", "Des" };
            var result = new List<MonthlyStatsDto>();

            // 3. Loop 1-12 untuk memastikan format data lengkap
            for (int i = 1; i <= 12; i++)
            {
                result.Add(new MonthlyStatsDto
                {
                    Bulan = namaBulan[i - 1],
                    // Jika ada data di DB ambil nilainya, jika tidak set 0
                    Total = rawData.ContainsKey(i) ? rawData[i] : 0
                });
            }

            return ApiResponse<List<MonthlyStatsDto>>.SuccessResult(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting monthly stats");
            return ApiResponse<List<MonthlyStatsDto>>.ErrorResult("Gagal mengambil statistik bulanan");
        }
    }

    public async Task<ApiResponse<List<DailyStatsDto>>> GetLast30DaysStats()
    {
        try
        {
            // 1. Tentukan tanggal patokan (Hari ini jam 00:00)
            var todayDate = DateTime.UtcNow.Date;
            var startDate = todayDate.AddDays(-29); // 30 hari ke belakang

            // 2. Tentukan batas akhir QUERY DATABASE
            // FIX: Tambahkan 1 hari dikurang 1 tick agar mencakup sampai jam 23:59:59 hari ini
            // Kalau cuma pakai todayDate, data jam 08:00 pagi hari ini tidak akan terambil.
            var queryEndDate = todayDate.AddDays(1).AddTicks(-1);

            // 3. Ambil data mentah dari DB dengan range yang SUDAH DIPERBAIKI
            var rawData = await _aksesLogRepository.GetDailyStatsAsync(startDate, queryEndDate);

            var result = new List<DailyStatsDto>();

            // 4. Loop untuk mengisi grafik (termasuk tanggal yang datanya 0)
            for (var date = startDate; date <= todayDate; date = date.AddDays(1))
            {
                // Konversi tanggal ke WIB agar label di grafik sesuai user Indonesia
                var dateWib = TimeZoneInfo.ConvertTimeFromUtc(date, WibTimeZone);

                result.Add(new DailyStatsDto
                {
                    // Format Label: "21 Dec"
                    Tanggal = dateWib.ToString("dd MMM"),

                    // Cek apakah ada data di tanggal tersebut
                    Total = rawData.ContainsKey(date) ? rawData[date] : 0
                });
            }

            return ApiResponse<List<DailyStatsDto>>.SuccessResult(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting last 30 days stats");
            return ApiResponse<List<DailyStatsDto>>.ErrorResult("Gagal mengambil statistik 30 hari terakhir");
        }
    }

    public async Task<ApiResponse<object>> DeleteAllAksesLog()
    {
        try
        {
            var deleted = await _aksesLogRepository.DeleteAllAsync();

            if (!deleted)
            {
                return ApiResponse<object>.SuccessResult(null, "Data aktivitas sudah kosong");
            }

            _logger.LogInformation("All AksesLog deleted by Admin");

            try
            {
                await _hubContext.Clients.All.SendAsync("AllAksesLogsDeleted", new
                {
                    Timestamp = DateTime.UtcNow,
                    Message = "Semua riwayat aktivitas telah dihapus"
                });

                await _hubContext.Clients.Group("admin").SendAsync("SystemNotification", new
                {
                    Type = "ALL_LOGS_DELETED",
                    Data = new { DeletedAt = DateTime.UtcNow },
                    Message = "Seorang admin telah menghapus seluruh riwayat aktivitas"
                });
            }
            catch (Exception hubEx)
            {
                _logger.LogWarning(hubEx, "Gagal mengirim notifikasi SignalR delete all");
            }

            return ApiResponse<object>.SuccessResult(null, "Semua aktivitas berhasil dihapus");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting all akses logs");
            return ApiResponse<object>.ErrorResult("Gagal menghapus semua aktivitas");
        }
    }

    public async Task<ApiResponse<AksesLogDto>> UpdateKeterangan(int id, AksesLogUpdateRequest request)
    {
        var log = await _aksesLogRepository.GetByIdAsync(id);
        if (log == null) return ApiResponse<AksesLogDto>.ErrorResult("Log tidak ditemukan");

        log.Keterangan = request.Keterangan;
        _aksesLogRepository.Update(log);
        await _aksesLogRepository.SaveAsync();

        return ApiResponse<AksesLogDto>.SuccessResult(_mapper.Map<AksesLogDto>(log), "Catatan berhasil ditambahkan");
    }

    public async Task<ApiResponse<List<MonthlyStatsDto>>> GetLast12MonthsStats()
    {
        try
        {
            var nowUtc = DateTime.UtcNow;
            var todayUtc = nowUtc.Date;

            var startDate = new DateTime(todayUtc.Year, todayUtc.Month, 1).AddMonths(-11);

            var endDate = todayUtc.AddDays(1).AddTicks(-1);

            var rawData = await _aksesLogRepository.GetMonthlyStatsRangeAsync(startDate, endDate);

            string[] namaBulan = { "Jan", "Feb", "Mar", "Apr", "Mei", "Jun", "Jul", "Agu", "Sep", "Okt", "Nov", "Des" };
            var result = new List<MonthlyStatsDto>();

            for (int i = 0; i < 12; i++)
            {
                var currentMonth = startDate.AddMonths(i);
                var monthKey = new DateTime(currentMonth.Year, currentMonth.Month, 1);

                var label = $"{namaBulan[monthKey.Month - 1]} {monthKey.Year}";

                result.Add(new MonthlyStatsDto
                {
                    Bulan = label,
                    Total = rawData.ContainsKey(monthKey) ? rawData[monthKey] : 0
                });
            }

            return ApiResponse<List<MonthlyStatsDto>>.SuccessResult(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting last 12 months stats");
            return ApiResponse<List<MonthlyStatsDto>>.ErrorResult("Gagal mengambil statistik 12 bulan terakhir");
        }
    }

    public async Task<ApiResponse<List<DailyStatsDto>>> GetLast6MonthsStats()
    {
        try
        {
            var nowUtc = DateTime.UtcNow;
            var todayUtc = nowUtc.Date;

            var startDate = new DateTime(todayUtc.Year, todayUtc.Month, 1).AddMonths(-5);

            var endDate = todayUtc.AddDays(1).AddTicks(-1);

            var rawData = await _aksesLogRepository.GetMonthlyStatsRangeAsync(startDate, endDate);

            var result = new List<DailyStatsDto>();

            for (int i = 0; i < 6; i++)
            {
                var currentMonth = startDate.AddMonths(i);
                var monthKey = new DateTime(currentMonth.Year, currentMonth.Month, 1);

                var monthKeyWib = TimeZoneInfo.ConvertTimeFromUtc(monthKey, WibTimeZone);

                var label = monthKeyWib.ToString("MMM yyyy", new System.Globalization.CultureInfo("id-ID"));

                result.Add(new DailyStatsDto
                {
                    Tanggal = label,
                    Total = rawData.ContainsKey(monthKey) ? rawData[monthKey] : 0
                });
            }

            return ApiResponse<List<DailyStatsDto>>.SuccessResult(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting last 6 months stats");
            return ApiResponse<List<DailyStatsDto>>.ErrorResult("Gagal mengambil statistik 6 bulan terakhir");
        }
    }

    public async Task<ApiResponse<List<DailyStatsDto>>> GetLast7DaysStats()
    {
        try
        {

            var todayDate = DateTime.UtcNow.Date;
            var startDate = todayDate.AddDays(-6);

            var queryEndDate = todayDate.AddDays(1).AddTicks(-1);

            var rawData = await _aksesLogRepository.GetDailyStatsAsync(startDate, queryEndDate);

            var result = new List<DailyStatsDto>();

            for (var date = startDate; date <= todayDate; date = date.AddDays(1))
            {
                var dateWib = TimeZoneInfo.ConvertTimeFromUtc(date, WibTimeZone);

                var hari = dateWib.ToString("ddd", new System.Globalization.CultureInfo("id-ID"));

                result.Add(new DailyStatsDto
                {

                    Tanggal = $"{hari}, {dateWib:dd MMM}",

                    Total = rawData.ContainsKey(date) ? rawData[date] : 0
                });
            }

            return ApiResponse<List<DailyStatsDto>>.SuccessResult(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting last 7 days stats");
            return ApiResponse<List<DailyStatsDto>>.ErrorResult("Gagal mengambil statistik 7 hari terakhir");
        }
    }

}