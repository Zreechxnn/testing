using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using testing.DTOs;
using testing.Services;

namespace testing.Controllers;

[ApiController]
[Route("api/[controller]")]
public class KartuController : ControllerBase
{
    private readonly IKartuService _kartuService;
    private readonly ILogger<KartuController> _logger;

    public KartuController(IKartuService kartuService, ILogger<KartuController> logger)
    {
        _kartuService = kartuService;
        _logger = logger;
    }

    [HttpGet]
    [Authorize(Roles = "admin,operator,guru")]
    public async Task<ActionResult<ApiResponse<List<KartuDto>>>> GetAll()
    {
        var response = await _kartuService.GetAllKartu();
        return Ok(response);
    }

    [HttpGet("paged")]
    [Authorize(Roles = "admin,operator")]
    public async Task<ActionResult<ApiResponse<PagedResponse<KartuDto>>>> GetPaged([FromQuery] PagedRequest request)
    {
        var response = await _kartuService.GetKartuPaged(request);
        return Ok(response);
    }

    [HttpGet("{id}")]
    [Authorize(Roles = "admin,operator,guru")]
    public async Task<ActionResult<ApiResponse<KartuDto>>> GetById(int id)
    {
        var response = await _kartuService.GetKartuById(id);
        if (!response.Success)
            return NotFound(response);
        return Ok(response);
    }

    [HttpPost]
    [Authorize(Roles = "admin,operator")]
    public async Task<ActionResult<ApiResponse<KartuDto>>> Create([FromBody] KartuCreateDto request)
    {
        var response = await _kartuService.CreateKartu(request);
        if (!response.Success)
            return BadRequest(response);
        return CreatedAtAction(nameof(GetById), new { id = response.Data?.Id }, response);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "admin,operator")]
    public async Task<ActionResult<ApiResponse<KartuDto>>> Update(int id, [FromBody] KartuUpdateDto request)
    {
        var response = await _kartuService.UpdateKartu(id, request);
        if (!response.Success)
            return BadRequest(response);
        return Ok(response);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "admin")]
    public async Task<ActionResult<ApiResponse<object>>> Delete(int id)
    {
        var response = await _kartuService.DeleteKartu(id);
        if (!response.Success)
            return BadRequest(response);
        return Ok(response);
    }

    [HttpGet("check/{uid}")]
    public async Task<ActionResult<ApiResponse<KartuCheckDto>>> CheckCard(string uid)
    {
        var response = await _kartuService.CheckCard(uid);
        return Ok(response);
    }
}