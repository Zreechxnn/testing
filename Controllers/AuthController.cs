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
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthService authService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<UserLoginResponse>>> Login([FromBody] UserLoginRequest request)
    {
        var response = await _authService.Login(request);
        if (!response.Success)
            return Unauthorized(response);
        return Ok(response);
    }

    [HttpGet("profile")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<UserDto>>> GetProfile()
    {
        var userId = GetUserIdFromToken();

        if (userId == 0)
        {
            return Unauthorized(ApiResponse<UserDto>.ErrorResult("Gagal membaca ID User dari token. Silakan login ulang."));
        }

        var response = await _authService.GetProfile(userId);
        if (!response.Success)
            return NotFound(response);

        return Ok(response);
    }

    [HttpPost("change-password")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<bool>>> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        var userId = GetUserIdFromToken();

        if (userId == 0)
        {
            return Unauthorized(ApiResponse<bool>.ErrorResult("Token tidak valid."));
        }

        var response = await _authService.ChangePassword(userId, request);

        if (!response.Success)
            return BadRequest(response);

        return Ok(response);
    }

    [HttpGet("roles")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<List<object>>>> GetRoles()
    {
        var response = await _authService.GetRoles();
        return Ok(response);
    }

    private int GetUserIdFromToken()
    {
        var user = HttpContext.User;

        var idClaim = user.FindFirst("id");

        if (idClaim == null) idClaim = user.FindFirst("nameid");

        if (idClaim == null) idClaim = user.FindFirst(ClaimTypes.NameIdentifier);

        if (idClaim != null && int.TryParse(idClaim.Value, out int userId))
        {
            return userId;
        }

        return 0;
    }
}