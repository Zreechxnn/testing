using testing.DTOs;

namespace testing.Services;

public interface IAksesLogService
{
    Task<ApiResponse<List<AksesLogDto>>> GetAllAksesLog();
    Task<ApiResponse<PagedResponse<AksesLogDto>>> GetAksesLogPaged(PagedRequest request);
    Task<ApiResponse<AksesLogDto>> GetAksesLogById(int id);
    Task<ApiResponse<List<AksesLogDto>>> GetAksesLogByKartuId(int kartuId);
    Task<ApiResponse<List<AksesLogDto>>> GetAksesLogByRuanganId(int ruanganId);
    Task<ApiResponse<List<AksesLogDto>>> GetLatestAksesLog(int count);
    Task<ApiResponse<AksesLogDto?>> GetActiveAksesLogByKartuId(int kartuId);

    // Tambahkan method delete
    Task<ApiResponse<object>> DeleteAksesLog(int id);

    Task<ApiResponse<DashboardStatsDto>> GetDashboardStats();
    Task<ApiResponse<TodayStatsDto>> GetTodayStats();
    Task<ApiResponse<List<MonthlyStatsDto>>> GetMonthlyStats(int year);
    Task<ApiResponse<List<DailyStatsDto>>> GetLast30DaysStats();

    Task<ApiResponse<object>> DeleteAllAksesLog();
}