using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using testing.DTOs;
using testing.Services;

namespace testing.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RuanganController : ControllerBase
{
    private readonly IRuanganService _ruanganService;
    private readonly ILogger<RuanganController> _logger;

    public RuanganController(IRuanganService ruanganService, ILogger<RuanganController> logger)
    {
        _ruanganService = ruanganService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<RuanganDto>>>> GetAll()
    {
        var response = await _ruanganService.GetAllRuangan();
        return Ok(response);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<RuanganDto>>> GetById(int id)
    {
        var response = await _ruanganService.GetRuanganById(id);
        if (!response.Success)
            return NotFound(response);
        return Ok(response);
    }

    [HttpPost]
    [Authorize(Roles = "admin")]
    public async Task<ActionResult<ApiResponse<RuanganDto>>> Create([FromBody] RuanganCreateRequest request)
    {
        var response = await _ruanganService.CreateRuangan(request);
        if (!response.Success)
            return BadRequest(response);
        return CreatedAtAction(nameof(GetById), new { id = response.Data?.Id }, response);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "admin")]
    public async Task<ActionResult<ApiResponse<RuanganDto>>> Update(int id, [FromBody] RuanganUpdateRequest request)
    {
        var response = await _ruanganService.UpdateRuangan(id, request);
        if (!response.Success)
            return BadRequest(response);
        return Ok(response);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "admin")]
    public async Task<ActionResult<ApiResponse<object>>> Delete(int id)
    {
        var response = await _ruanganService.DeleteRuangan(id);
        if (!response.Success)
            return BadRequest(response);
        return Ok(response);
    }

    [HttpGet("stats/{id}")]
    public async Task<ActionResult<ApiResponse<RuanganStatsDto>>> GetStats(int id)
    {
        var response = await _ruanganService.GetRuanganStats(id);
        if (!response.Success)
            return NotFound(response);
        return Ok(response);
    }
}