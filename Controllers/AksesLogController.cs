using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using testing.DTOs;
using testing.Services;

namespace testing.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AksesLogController : ControllerBase
{
    private readonly IAksesLogService _aksesLogService;
    private readonly ILogger<AksesLogController> _logger;

    public AksesLogController(IAksesLogService aksesLogService, ILogger<AksesLogController> logger)
    {
        _aksesLogService = aksesLogService;
        _logger = logger;
    }

    [HttpGet]
    [Authorize(Roles = "admin,operator")]
    public async Task<ActionResult<ApiResponse<List<AksesLogDto>>>> GetAll()
    {
        var response = await _aksesLogService.GetAllAksesLog();
        return Ok(response);
    }

    [HttpGet("paged")]
    [Authorize(Roles = "admin,operator")]
    public async Task<ActionResult<ApiResponse<PagedResponse<AksesLogDto>>>> GetPaged([FromQuery] PagedRequest request)
    {
        var response = await _aksesLogService.GetAksesLogPaged(request);
        return Ok(response);
    }

    [HttpGet("{id}")]
    [Authorize(Roles = "admin,operator")]
    public async Task<ActionResult<ApiResponse<AksesLogDto>>> GetById(int id)
    {
        var response = await _aksesLogService.GetAksesLogById(id);
        if (!response.Success)
            return NotFound(response);
        return Ok(response);
    }

    [HttpGet("kartu/{kartuId}")]
    [Authorize(Roles = "admin,operator")]
    public async Task<ActionResult<ApiResponse<List<AksesLogDto>>>> GetByKartuId(int kartuId)
    {
        var response = await _aksesLogService.GetAksesLogByKartuId(kartuId);
        return Ok(response);
    }

    [HttpGet("ruangan/{ruanganId}")]
    [Authorize(Roles = "admin,operator")]
    public async Task<ActionResult<ApiResponse<List<AksesLogDto>>>> GetByRuanganId(int ruanganId)
    {
        var response = await _aksesLogService.GetAksesLogByRuanganId(ruanganId);
        return Ok(response);
    }

    [HttpGet("latest/{count}")]
    [Authorize(Roles = "admin,operator")]
    public async Task<ActionResult<ApiResponse<List<AksesLogDto>>>> GetLatest(int count)
    {
        var response = await _aksesLogService.GetLatestAksesLog(count);
        return Ok(response);
    }

    [HttpGet("dashboard/stats")]
    // [Authorize(Roles = "admin,operator")]
    public async Task<ActionResult<ApiResponse<DashboardStatsDto>>> GetDashboardStats()
    {
        var response = await _aksesLogService.GetDashboardStats();
        return Ok(response);
    }

    [HttpGet("today/stats")]
    // [Authorize(Roles = "admin,operator")]
    public async Task<ActionResult<ApiResponse<TodayStatsDto>>> GetTodayStats()
    {
        var response = await _aksesLogService.GetTodayStats();
        return Ok(response);
    }

    // Tambahkan endpoint delete
    [HttpDelete("{id}")]
    [Authorize(Roles = "admin")]
    public async Task<ActionResult<ApiResponse<object>>> Delete(int id)
    {
        var response = await _aksesLogService.DeleteAksesLog(id);
        if (!response.Success)
            return NotFound(response);
        return Ok(response);
    }
}