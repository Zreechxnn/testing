using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using testing.DTOs;
using testing.Services;

namespace testing.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TapController : ControllerBase
{
    private readonly ITapService _tapService;
    private readonly ILogger<TapController> _logger;

    public TapController(ITapService tapService, ILogger<TapController> logger)
    {
        _tapService = tapService;
        _logger = logger;
    }

    [HttpPost("process")]
    public async Task<ActionResult<ApiResponse<TapResponse>>> ProcessTap([FromBody] TapRequest request)
    {
        var response = await _tapService.ProcessTap(request);
        return Ok(response);
    }

    [HttpGet("logs")]
    [Authorize(Roles = "admin,operator")]
    public async Task<ActionResult<ApiResponse<List<object>>>> GetLogs([FromQuery] int? ruanganId = null)
    {
        var response = await _tapService.GetLogs(ruanganId);
        return Ok(response);
    }

    [HttpGet("kartu")]
    [Authorize(Roles = "admin,operator")]
    public async Task<ActionResult<ApiResponse<List<object>>>> GetKartu()
    {
        var response = await _tapService.GetKartu();
        return Ok(response);
    }

    [HttpGet("ruangan")]
    [Authorize(Roles = "admin,operator")]
    public async Task<ActionResult<ApiResponse<List<object>>>> GetRuangan()
    {
        var response = await _tapService.GetRuangan();
        return Ok(response);
    }

    [HttpGet("kelas")]
    [Authorize(Roles = "admin,operator")]
    public async Task<ActionResult<ApiResponse<List<object>>>> GetKelas()
    {
        var response = await _tapService.GetKelas();
        return Ok(response);
    }

    [HttpGet("stats")]
    [Authorize(Roles = "admin,operator")]
    public async Task<ActionResult<ApiResponse<object>>> GetStats()
    {
        var response = await _tapService.GetStats();
        return Ok(response);
    }

    [HttpGet("stats/hari-ini")]
    [Authorize(Roles = "admin,operator")]
    public async Task<ActionResult<ApiResponse<object>>> GetStatsHariIni()
    {
        var response = await _tapService.GetStatsHariIni();
        return Ok(response);
    }
}