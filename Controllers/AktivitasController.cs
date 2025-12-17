using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using testing.DTOs;
using testing.Services;

namespace testing.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AktivitasController : ControllerBase
{
    private readonly IAksesLogService _aksesLogService;
    private readonly ITapService _tapService;
    private readonly ILogger<AktivitasController> _logger;

    public AktivitasController(IAksesLogService aksesLogService, ITapService tapService, ILogger<AktivitasController> logger)
    {
        _aksesLogService = aksesLogService;
        _tapService = tapService;
        _logger = logger;
    }

    // Dari AksesLogController
    [HttpGet]
    [Authorize(Roles = "admin,operator,guru")]
    public async Task<ActionResult<ApiResponse<List<AksesLogDto>>>> GetAll()
    {
        var response = await _aksesLogService.GetAllAksesLog();
        return Ok(response);
    }

    [HttpGet("paged")]
    [Authorize(Roles = "admin,operator,guru")]
    public async Task<ActionResult<ApiResponse<PagedResponse<AksesLogDto>>>> GetPaged([FromQuery] PagedRequest request)
    {
        var response = await _aksesLogService.GetAksesLogPaged(request);
        return Ok(response);
    }

    [HttpGet("{id}")]
    [Authorize(Roles = "admin,operator,guru")]
    public async Task<ActionResult<ApiResponse<AksesLogDto>>> GetById(int id)
    {
        var response = await _aksesLogService.GetAksesLogById(id);
        if (!response.Success)
            return NotFound(response);
        return Ok(response);
    }

    [HttpGet("kartu/{kartuId}")]
    [Authorize(Roles = "admin,operator,guru")]
    public async Task<ActionResult<ApiResponse<List<AksesLogDto>>>> GetByKartuId(int kartuId)
    {
        var response = await _aksesLogService.GetAksesLogByKartuId(kartuId);
        return Ok(response);
    }

    [HttpGet("ruangan/{ruanganId}")]
    [Authorize(Roles = "admin,operator,guru")]
    public async Task<ActionResult<ApiResponse<List<AksesLogDto>>>> GetByRuanganId(int ruanganId)
    {
        var response = await _aksesLogService.GetAksesLogByRuanganId(ruanganId);
        return Ok(response);
    }

    [HttpGet("latest/{count}")]
    [Authorize(Roles = "admin,operator,guru")]
    public async Task<ActionResult<ApiResponse<List<AksesLogDto>>>> GetLatest(int count)
    {
        var response = await _aksesLogService.GetLatestAksesLog(count);
        return Ok(response);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "admin")]
    public async Task<ActionResult<ApiResponse<object>>> Delete(int id)
    {
        var response = await _aksesLogService.DeleteAksesLog(id);
        if (!response.Success)
            return NotFound(response);
        return Ok(response);
    }

    // Dari TapController (kecuali stats yang pindah ke Dashboard)
    [HttpPost("tap")]
    public async Task<ActionResult<ApiResponse<TapResponse>>> ProcessTap([FromBody] TapRequest request)
    {
        var response = await _tapService.ProcessTap(request);
        return Ok(response);
    }

    [HttpGet("tap-logs")]
    [Authorize(Roles = "admin,operator,guru")]
    public async Task<ActionResult<ApiResponse<List<object>>>> GetTapLogs([FromQuery] int? ruanganId = null)
    {
        var response = await _tapService.GetLogs(ruanganId);
        return Ok(response);
    }

    [HttpDelete("all")]
    [Authorize(Roles = "admin")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteAll()
    {
        var response = await _aksesLogService.DeleteAllAksesLog();
        return Ok(response);
    }
}