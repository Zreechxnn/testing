using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using testing.DTOs;
using testing.Services;

namespace testing.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ILogger<UserController> _logger;

    public UserController(IUserService userService, ILogger<UserController> logger)
    {
        _userService = userService;
        _logger = logger;
    }

    [HttpGet]
    [Authorize(Roles = "admin")]
    public async Task<ActionResult<ApiResponse<List<UserDto>>>> GetAll()
    {
        var response = await _userService.GetAllUsers();
        return Ok(response);
    }

    [HttpGet("{id}")]
    [Authorize(Roles = "admin")]
    public async Task<ActionResult<ApiResponse<UserDto>>> GetById(int id)
    {
        var response = await _userService.GetUserById(id);
        if (!response.Success)
            return NotFound(response);
        return Ok(response);
    }

    [HttpPost]
    [Authorize(Roles = "admin")]
    public async Task<ActionResult<ApiResponse<UserDto>>> Create([FromBody] UserCreateRequest request)
    {
        var response = await _userService.CreateUser(request);
        if (!response.Success)
            return BadRequest(response);
        return CreatedAtAction(nameof(GetById), new { id = response.Data?.Id }, response);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "admin")]
    public async Task<ActionResult<ApiResponse<UserDto>>> Update(int id, [FromBody] UserUpdateRequest request)
    {
        var response = await _userService.UpdateUser(id, request);
        if (!response.Success)
            return BadRequest(response);
        return Ok(response);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "admin")]
    public async Task<ActionResult<ApiResponse<object>>> Delete(int id)
    {
        var response = await _userService.DeleteUser(id);
        if (!response.Success)
            return BadRequest(response);
        return Ok(response);
    }
}