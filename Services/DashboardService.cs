using testing.DTOs;
using testing.Repositories;

namespace testing.Services;

public class DashboardService : IDashboardService
{
    private readonly IAksesLogRepository _aksesLogRepository;
    private readonly IKartuRepository _kartuRepository;
    private readonly IKelasRepository _kelasRepository;
    private readonly IRuanganRepository _ruanganRepository;
    private readonly IUserRepository _userRepository;
    private readonly IJurusanRepository _jurusanRepository; // INJECT INI
    private readonly ILogger<DashboardService> _logger;

    private static readonly TimeZoneInfo WibTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");

    public DashboardService(
        IAksesLogRepository aksesLogRepository,
        IKartuRepository kartuRepository,
        IKelasRepository kelasRepository,
        IRuanganRepository ruanganRepository,
        IUserRepository userRepository,
        IJurusanRepository jurusanRepository, // Tambahkan parameter
        ILogger<DashboardService> logger)
    {
        _aksesLogRepository = aksesLogRepository;
        _kartuRepository = kartuRepository;
        _kelasRepository = kelasRepository;
        _ruanganRepository = ruanganRepository;
        _userRepository = userRepository;
        _jurusanRepository = jurusanRepository; // Assign
        _logger = logger;
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

            // Hitung Jurusan (Manual count karena repository blm punya CountAsync, atau tambah di repo)
            var allJurusan = await _jurusanRepository.GetAllAsync();
            var totalJurusan = allJurusan.Count();

            // Hitung 30 Hari & Tahun Ini
            var now = DateTime.UtcNow;
            var thirtyDaysAgo = now.AddDays(-30);
            var startOfYear = new DateTime(now.Year, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            var akses30Hari = semuaAkses.Count(a => a.TimestampMasuk >= thirtyDaysAgo);
            var aksesTahunIni = semuaAkses.Count(a => a.TimestampMasuk >= startOfYear);

            var stats = new DashboardStatsDto
            {
                TotalAkses = totalAkses,
                AktifSekarang = aktifSekarang,
                Akses30Hari = akses30Hari,
                AksesTahunIni = aksesTahunIni,
                TotalKartu = totalKartu,
                TotalKelas = totalKelas,
                TotalJurusan = totalJurusan, // Field baru
                TotalRuangan = totalRuangan,
                TotalUsers = totalUsers
            };

            return ApiResponse<DashboardStatsDto>.SuccessResult(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting dashboard stats");
            return ApiResponse<DashboardStatsDto>.ErrorResult("Gagal mengambil statistik dashboard");
        }
    }

    // ... (Sisa method GetTodayStats, GetTapStats, dll tetap sama)
    public async Task<ApiResponse<TodayStatsDto>> GetTodayStats()
    {
        // Copy paste method GetTodayStats dari file lamamu...
        // Tidak ada perubahan logika di sini
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
        // Copy paste method lama...
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
        // Copy paste method lama...
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

            return ApiResponse<object>.SuccessResult(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting today's tap stats");
            return ApiResponse<object>.ErrorResult("Error retrieving today's tap stats");
        }
    }
}