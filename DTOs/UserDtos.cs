namespace testing.DTOs;

public class UserCreateRequest
{
    public required string Username { get; set; }
    public required string Password { get; set; }
    public required string Role { get; set; }
}

public class UserUpdateRequest
{
    public required string Username { get; set; }
    public string? Password { get; set; }
    public required string Role { get; set; }
}

public class UserLoginRequest
{
    public required string Username { get; set; }
    public required string Password { get; set; }
}

public class UserLoginResponse
{
    public int Id { get; set; }
    public required string Username { get; set; }
    public required string Role { get; set; }
    public required string Token { get; set; }
}

public class UserDto
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string? KartuUid { get; set; }
    public int? KartuId { get; set; }
}