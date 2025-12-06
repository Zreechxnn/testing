using testing.DTOs;

namespace testing.Services;

public interface IDashboardService
{
    Task<ApiResponse<DashboardStatsDto>> GetDashboardStats();
    Task<ApiResponse<TodayStatsDto>> GetTodayStats();
    Task<ApiResponse<object>> GetTapStats();
    Task<ApiResponse<object>> GetTodayTapStats();

    Task RefreshDashboardData();
}