using Microsoft.AspNetCore.SignalR;
using testing.DTOs;
using testing.Hubs;
using testing.Repositories;

namespace testing.Services;

public class DashboardService : IDashboardService
{
    private readonly IAksesLogRepository _aksesLogRepository;
    private readonly IKartuRepository _kartuRepository;
    private readonly IKelasRepository _kelasRepository;
    private readonly IRuanganRepository _ruanganRepository;
    private readonly IUserRepository _userRepository;
    private readonly ILogger<DashboardService> _logger;
    private readonly IHubContext<LogHub> _hubContext; // Menggunakan LogHub yang sama

    // Timezone untuk WIB (UTC+7)
    private static readonly TimeZoneInfo WibTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");

    public DashboardService(
        IAksesLogRepository aksesLogRepository,
        IKartuRepository kartuRepository,
        IKelasRepository kelasRepository,
        IRuanganRepository ruanganRepository,
        IUserRepository userRepository,
        ILogger<DashboardService> logger,
        IHubContext<LogHub> hubContext) // Tambahkan parameter ini
    {
        _aksesLogRepository = aksesLogRepository;
        _kartuRepository = kartuRepository;
        _kelasRepository = kelasRepository;
        _ruanganRepository = ruanganRepository;
        _userRepository = userRepository;
        _logger = logger;
        _hubContext = hubContext; // Assign
    }

    public async Task<ApiResponse<DashboardStatsDto>> GetDashboardStats()
    {
        try
        {
            var totalAkses = await _aksesLogRepository.CountAsync();

            var semuaAkses = await _aksesLogRepository.GetAllAsync();
            var aktifSekarang = semuaAkses.Count(a => a.TimestampKeluar == null);

            var totalKartu = await _kartuRepository.CountAsync();
            var totalKelas = await _kelasRepository.CountAsync();
            var totalRuangan = await _ruanganRepository.CountAsync();
            var totalUsers = await _userRepository.CountAsync();

            var stats = new DashboardStatsDto
            {
                TotalAkses = totalAkses,
                AktifSekarang = aktifSekarang,
                TotalKartu = totalKartu,
                TotalKelas = totalKelas,
                TotalRuangan = totalRuangan,
                TotalUsers = totalUsers
            };

            // KIRIM NOTIFIKASI KE CLIENT (SAMA SEPERTI DI KELAS SERVICE)
            await SendDashboardNotification("DASHBOARD_STATS_FETCHED", stats);

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

            var hariIniWib = TimeZoneInfo.ConvertTimeFromUtc(hariIniUtc, WibTimeZone);

            var stats = new TodayStatsDto
            {
                Tanggal = hariIniWib.ToString("yyyy-MM-dd"),
                TotalCheckin = checkinHariIni,
                TotalCheckout = checkoutHariIni,
                MasihAktif = masihAktif,
                KartuPalingAktif = kartuAktif
            };

            // KIRIM NOTIFIKASI KE CLIENT
            await SendDashboardNotification("TODAY_STATS_FETCHED", stats);

            return ApiResponse<TodayStatsDto>.SuccessResult(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting today's stats");
            return ApiResponse<TodayStatsDto>.ErrorResult("Gagal mengambil statistik hari ini");
        }
    }

    public async Task<ApiResponse<object>> GetTapStats()
    {
        try
        {
            var totalAkses = await _aksesLogRepository.CountAsync();
            var aktifSekarang = await _aksesLogRepository.GetLatestAsync(1000)
                .ContinueWith(t => t.Result.Count(a => a.TimestampKeluar == null));
            var totalKartu = await _kartuRepository.CountAsync();
            var totalKelas = await _kelasRepository.CountAsync();

            var stats = new
            {
                TotalAkses = totalAkses,
                AktifSekarang = aktifSekarang,
                TotalKartu = totalKartu,
                TotalKelas = totalKelas
            };

            // KIRIM NOTIFIKASI KE CLIENT
            await SendDashboardNotification("TAP_STATS_FETCHED", stats);

            return ApiResponse<object>.SuccessResult(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting tap stats");
            return ApiResponse<object>.ErrorResult("Error retrieving tap stats");
        }
    }

    public async Task<ApiResponse<object>> GetTodayTapStats()
    {
        try
        {
            var hariIni = DateTime.Today;
            var besok = hariIni.AddDays(1);

            var semuaAkses = await _aksesLogRepository.GetAllAsync();
            var aksesHariIni = semuaAkses.Where(a => a.TimestampMasuk >= hariIni && a.TimestampMasuk < besok).ToList();

            var checkinHariIni = aksesHariIni.Count;
            var checkoutHariIni = aksesHariIni.Count(a => a.TimestampKeluar.HasValue);
            var masihAktif = aksesHariIni.Count(a => !a.TimestampKeluar.HasValue);

            var kartuAktif = aksesHariIni
                .GroupBy(a => a.Kartu?.Uid ?? "Unknown")
                .Select(g => new { KartuUID = g.Key, Jumlah = g.Count() })
                .OrderByDescending(x => x.Jumlah)
                .Take(5)
                .ToList();

            var stats = new
            {
                Tanggal = hariIni.ToString("yyyy-MM-dd"),
                TotalCheckin = checkinHariIni,
                TotalCheckout = checkoutHariIni,
                MasihAktif = masihAktif,
                KartuPalingAktif = kartuAktif
            };

            // KIRIM NOTIFIKASI KE CLIENT
            await SendDashboardNotification("TODAY_TAP_STATS_FETCHED", stats);

            return ApiResponse<object>.SuccessResult(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting today's tap stats");
            return ApiResponse<object>.ErrorResult("Error retrieving today's tap stats");
        }
    }

    // METODE UNTUK MENGIRIM NOTIFIKASI SIGNALR (MIRIP DENGAN KELAS SERVICE)
    private async Task SendDashboardNotification(string eventType, object data, string customMessage = null)
    {
        try
        {
            var notification = new
            {
                EventId = Guid.NewGuid(),
                EventType = eventType,
                Timestamp = DateTime.UtcNow,
                Data = data,
                Message = customMessage ?? $"Dashboard {GetEventAction(eventType)}"
            };

            // Kirim ke semua client yang terhubung (SAMA SEPERTI KELAS SERVICE)
            await _hubContext.Clients.All.SendAsync("DashboardNotification", notification);

            // Kirim ke grup admin untuk logging sistem (SAMA SEPERTI KELAS SERVICE)
            await _hubContext.Clients.Group("admin").SendAsync("SystemNotification", notification);

            _logger.LogDebug($"SignalR notification sent for {eventType}");
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
            "DASHBOARD_STATS_FETCHED" => "stats fetched",
            "TODAY_STATS_FETCHED" => "today's stats fetched",
            "TAP_STATS_FETCHED" => "tap stats fetched",
            "TODAY_TAP_STATS_FETCHED" => "today's tap stats fetched",
            _ => "data fetched"
        };
    }

    // METODE UNTUK REFRESH REAL-TIME DASHBOARD
    public async Task RefreshDashboardData()
    {
        try
        {
            // Dapatkan data terbaru
            var dashboardStats = await GetDashboardStats();
            var todayStats = await GetTodayStats();

            if (dashboardStats.Success)
            {
                await SendDashboardNotification("DASHBOARD_REFRESHED", dashboardStats.Data,
                    "Dashboard data refreshed in real-time");
            }

            _logger.LogInformation("Dashboard data refreshed via SignalR");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to refresh dashboard data");
        }
    }
}