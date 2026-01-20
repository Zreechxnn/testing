using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using testing.DTOs;
using testing.Services;

namespace testing.Controllers;

[ApiController]
[Route("api/[controller]")]
public class KelasController : ControllerBase
{
    private readonly IKelasService _kelasService;
    private readonly ILogger<KelasController> _logger;

    public KelasController(IKelasService kelasService, ILogger<KelasController> logger)
    {
        _kelasService = kelasService;
        _logger = logger;
    }

    [HttpGet]
    [Authorize(Roles = "admin,operator,guru,siswa")]
    public async Task<ActionResult<ApiResponse<List<KelasDto>>>> GetAll()
    {
        var response = await _kelasService.GetAllKelas();
        return Ok(response);
    }

    [HttpGet("{id}")]
    [Authorize(Roles = "admin,operator,guru,siswa")]
    public async Task<ActionResult<ApiResponse<KelasDto>>> GetById(int id)
    {
        var response = await _kelasService.GetKelasById(id);
        if (!response.Success)
            return NotFound(response);
        return Ok(response);
    }

    [HttpPost]
    [Authorize(Roles = "admin,guru")]
    public async Task<ActionResult<ApiResponse<KelasDto>>> Create([FromBody] KelasCreateRequest request)
    {
        var response = await _kelasService.CreateKelas(request);
        if (!response.Success)
            return BadRequest(response);
        return CreatedAtAction(nameof(GetById), new { id = response.Data?.Id }, response);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "admin,guru")]
    public async Task<ActionResult<ApiResponse<KelasDto>>> Update(int id, [FromBody] KelasUpdateRequest request)
    {
        var response = await _kelasService.UpdateKelas(id, request);
        if (!response.Success)
            return BadRequest(response);
        return Ok(response);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "admin,guru")]
    public async Task<ActionResult<ApiResponse<object>>> Delete(int id)
    {
        var response = await _kelasService.DeleteKelas(id);
        if (!response.Success)
            return BadRequest(response);
        return Ok(response);
    }

    [HttpGet("stats/{id}")]
    [Authorize(Roles = "admin,operator,guru,siswa")]
    public async Task<ActionResult<ApiResponse<KelasStatsDto>>> GetStats(int id)
    {
        var response = await _kelasService.GetKelasStats(id);
        if (!response.Success)
            return NotFound(response);
        return Ok(response);
    }

    [HttpGet("periode/{periodeId}")]
    [Authorize(Roles = "admin,guru")]
    public async Task<IActionResult> GetByPeriode(int periodeId)
    {
        var response = await _kelasService.GetKelasByPeriode(periodeId);
        return Ok(response);
    }

    [HttpGet("jurusan/{jurusanId}")]
    [Authorize(Roles = "admin,operator,guru")]
    public async Task<IActionResult> GetByJurusan(int jurusanId)
    {
        var response = await _kelasService.GetKelasByJurusan(jurusanId);
        return Ok(response);
    }

    // 2. Ambil kelas spesifik berdasarkan Jurusan DAN Tingkat (Misal: 10 RPL)
    [HttpGet("jurusan/{jurusanId}/tingkat/{tingkat}")]
    [Authorize(Roles = "admin,operator,guru")]
    public async Task<IActionResult> GetByJurusanAndTingkat(int jurusanId, int tingkat)
    {
        var response = await _kelasService.GetKelasByJurusanAndTingkat(jurusanId, tingkat);
        return Ok(response);
    }
}