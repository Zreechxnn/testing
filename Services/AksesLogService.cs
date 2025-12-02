using AutoMapper;
using testing.DTOs;
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

    // Timezone untuk WIB (UTC+7)
    private static readonly TimeZoneInfo WibTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");

    public AksesLogService(
        IAksesLogRepository aksesLogRepository,
        IKartuRepository kartuRepository,
        IKelasRepository kelasRepository,
        IRuanganRepository ruanganRepository,
        IUserRepository userRepository,
        IMapper mapper,
        ILogger<AksesLogService> logger)
    {
        _aksesLogRepository = aksesLogRepository;
        _kartuRepository = kartuRepository;
        _kelasRepository = kelasRepository;
        _ruanganRepository = ruanganRepository;
        _userRepository = userRepository;
        _mapper = mapper;
        _logger = logger;
    }

    // Method helper untuk konversi UTC ke WIB
    private AksesLogDto ConvertToWibDto(AksesLog aksesLog)
    {
        var dto = _mapper.Map<AksesLogDto>(aksesLog);

        // Konversi UTC ke WIB untuk display
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
}