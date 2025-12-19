using Microsoft.AspNetCore.Mvc;
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
    public async Task<IActionResult> GetAll()
    {
        var result = await _periodeService.GetAll();
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] PeriodeCreateRequest request)
    {
        var result = await _periodeService.Create(request);
        return Ok(result);
    }
}