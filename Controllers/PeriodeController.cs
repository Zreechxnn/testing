using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using testing.DTOs;
using testing.Services;

namespace testing.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PeriodeController : ControllerBase
{
    private readonly IPeriodeService _periodeService;

    public PeriodeController(IPeriodeService periodeService)
    {
        _periodeService = periodeService;
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetAll()
    {
        var result = await _periodeService.GetAll();
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> Create([FromBody] PeriodeCreateRequest request)
    {
        var result = await _periodeService.Create(request);
        return Ok(result);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _periodeService.Delete(id);
        if (!result.Success)
        {
            return BadRequest(result);
        }
        return Ok(result);
    }

    [HttpPut("{id}/active")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> SetActive(int id)
    {
        var result = await _periodeService.SetActive(id);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }
}