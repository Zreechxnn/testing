using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using testing.DTOs;
using testing.Services;

namespace testing.Controllers;

[ApiController]
[Route("api/[controller]")]
public class JurusanController : ControllerBase
{
    private readonly IJurusanService _jurusanService;
    private readonly ILogger<JurusanController> _logger;

    public JurusanController(IJurusanService jurusanService, ILogger<JurusanController> logger)
    {
        _jurusanService = jurusanService;
        _logger = logger;
    }

    [HttpGet]
    [Authorize(Roles = "admin,operator,guru,siswa")]
    public async Task<ActionResult<ApiResponse<List<JurusanDto>>>> GetAll()
    {
        var response = await _jurusanService.GetAll();
        return Ok(response);
    }

    [HttpGet("{id}")]
    [Authorize(Roles = "admin,operator,guru")]
    public async Task<ActionResult<ApiResponse<JurusanDto>>> GetById(int id)
    {
        var response = await _jurusanService.GetById(id);
        if (!response.Success) return NotFound(response);
        return Ok(response);
    }

    [HttpPost]
    [Authorize(Roles = "admin")]
    public async Task<ActionResult<ApiResponse<JurusanDto>>> Create([FromBody] JurusanCreateRequest request)
    {
        var response = await _jurusanService.Create(request);
        if (!response.Success) return BadRequest(response);
        return CreatedAtAction(nameof(GetById), new { id = response.Data?.Id }, response);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "admin")]
    public async Task<ActionResult<ApiResponse<JurusanDto>>> Update(int id, [FromBody] JurusanUpdateRequest request)
    {
        var response = await _jurusanService.Update(id, request);
        if (!response.Success) return BadRequest(response);
        return Ok(response);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "admin")]
    public async Task<ActionResult<ApiResponse<object>>> Delete(int id)
    {
        var response = await _jurusanService.Delete(id);
        if (!response.Success) return BadRequest(response);
        return Ok(response);
    }
}