using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using testing.DTOs;
using testing.Services;
using System.Security.Claims;

namespace testing.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService; // Ganti dari IUserService ke IAuthService
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthService authService, ILogger<AuthController> logger) // Ganti parameter
    {
        _authService = authService;
        _logger = logger;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<UserLoginResponse>>> Login([FromBody] UserLoginRequest request)
    {
        var response = await _authService.Login(request); // Panggil IAuthService
        if (!response.Success)
            return Unauthorized(response);
        return Ok(response);
    }

    [HttpGet("profile")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<UserDto>>> GetProfile()
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var response = await _authService.GetProfile(userId); // Panggil IAuthService
        if (!response.Success)
            return NotFound(response);
        return Ok(response);
    }

    [HttpGet("roles")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<List<object>>>> GetRoles()
    {
        var response = await _authService.GetRoles(); // Panggil IAuthService
        return Ok(response);
    }
}