using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using testing.DTOs;
using testing.Services;

namespace testing.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ScanController : ControllerBase
{
    private readonly IScanService _scanService;
    private readonly ILogger<ScanController> _logger;

    public ScanController(IScanService scanService, ILogger<ScanController> logger)
    {
        _scanService = scanService;
        _logger = logger;
    }

    [HttpPost("register")]
    public async Task<ActionResult<ApiResponse<ScanResponse>>> RegisterCard([FromBody] ScanRequest request)
    {
        var response = await _scanService.RegisterCard(request);
        if (!response.Success)
            return BadRequest(response);
        return Ok(response);
    }

    [HttpPost("check")]
    public async Task<ActionResult<ApiResponse<ScanCheckResponse>>> CheckCard([FromBody] ScanCheckRequest request)
    {
        var response = await _scanService.CheckCard(request);
        return Ok(response);
    }

    [HttpGet("kartu-terdaftar")]
    [Authorize(Roles = "admin,operator")]
    public async Task<ActionResult<ApiResponse<List<object>>>> GetKartuTerdaftar()
    {
        var response = await _scanService.GetKartuTerdaftar();
        return Ok(response);
    }

    [HttpDelete("kartu/{id}")]
    [Authorize(Roles = "admin")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteKartu(int id)
    {
        var response = await _scanService.DeleteKartu(id);
        if (!response.Success)
            return BadRequest(response);
        return Ok(response);
    }
}