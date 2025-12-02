using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using testing.DTOs;
using testing.Services;

namespace testing.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DashboardController : ControllerBase
{
    private readonly IAksesLogService _aksesLogService;
    private readonly ITapService _tapService;
    private readonly ILogger<DashboardController> _logger;

    public DashboardController(IAksesLogService aksesLogService, ITapService tapService, ILogger<DashboardController> logger)
    {
        _aksesLogService = aksesLogService;
        _tapService = tapService;
        _logger = logger;
    }

    [HttpGet("stats")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<DashboardStatsDto>>> GetDashboardStats()
    {
        var response = await _aksesLogService.GetDashboardStats();
        return Ok(response);
    }

    [HttpGet("today-stats")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<TodayStatsDto>>> GetTodayStats()
    {
        var response = await _aksesLogService.GetTodayStats();
        return Ok(response);
    }

    [HttpGet("tap-stats")]
    [Authorize(Roles = "admin,operator")]
    public async Task<ActionResult<ApiResponse<object>>> GetTapStats()
    {
        var response = await _tapService.GetStats();
        return Ok(response);
    }

    [HttpGet("today-tap-stats")]
    [Authorize(Roles = "admin,operator")]
    public async Task<ActionResult<ApiResponse<object>>> GetTodayTapStats()
    {
        var response = await _tapService.GetStatsHariIni();
        return Ok(response);
    }
}