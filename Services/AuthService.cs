using AutoMapper;
using Microsoft.AspNetCore.SignalR;
using testing.Hubs;
using testing.DTOs;
using testing.Models;
using testing.Repositories;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace testing.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<AuthService> _logger;
    private readonly IHubContext<LogHub> _hubContext;

    // JWT Settings
    private readonly string _jwtSecretKey;
    private readonly string _jwtIssuer;
    private readonly string _jwtAudience;
    private readonly int _jwtExpireMinutes;

    public AuthService(
        IUserRepository userRepository,
        IMapper mapper,
        ILogger<AuthService> logger,
        IConfiguration configuration,
        IHubContext<LogHub> hubContext)
    {
        _userRepository = userRepository;
        _mapper = mapper;
        _logger = logger;
        _hubContext = hubContext;

        // Load JWT settings
        _jwtSecretKey = Environment.GetEnvironmentVariable("JwtSettings__SecretKey")
                        ?? configuration["JwtSettings:SecretKey"]
                        ?? throw new InvalidOperationException("JWT SecretKey tidak ditemukan!");

        _jwtIssuer = Environment.GetEnvironmentVariable("JwtSettings__Issuer")
                    ?? configuration["JwtSettings:Issuer"]
                    ?? "LabAccessAPI";

        _jwtAudience = Environment.GetEnvironmentVariable("JwtSettings__Audience")
                      ?? configuration["JwtSettings:Audience"]
                      ?? "LabAccessClient";

        _jwtExpireMinutes = int.Parse(Environment.GetEnvironmentVariable("JwtSettings__ExpireMinutes")
                                    ?? configuration["JwtSettings:ExpireMinutes"]
                                    ?? "1440");
    }

    public async Task<ApiResponse<UserLoginResponse>> Login(UserLoginRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
            {
                return ApiResponse<UserLoginResponse>.ErrorResult("Username dan password harus diisi");
            }

            var user = await _userRepository.GetByUsernameAsync(request.Username);
            if (user == null)
            {
                return ApiResponse<UserLoginResponse>.ErrorResult("Username atau password salah");
            }

            if (!VerifyPassword(request.Password, user.PasswordHash))
            {
                return ApiResponse<UserLoginResponse>.ErrorResult("Username atau password salah");
            }

            var token = GenerateJwtToken(user);

            // Kirim notifikasi ke SignalR
            await _hubContext.Clients.All.SendAsync("UserLoggedIn", new
            {
                userId = user.Id,
                username = user.Username,
                timestamp = DateTime.UtcNow
            });

            var response = new UserLoginResponse
            {
                Id = user.Id,
                Username = user.Username,
                Role = user.Role,
                Token = token
            };

            _logger.LogInformation("User logged in successfully: {Username}", user.Username);
            return ApiResponse<UserLoginResponse>.SuccessResult(response, "Login berhasil");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login for user: {Username}", request.Username);
            return ApiResponse<UserLoginResponse>.ErrorResult("Terjadi kesalahan saat login");
        }
    }

    public async Task<ApiResponse<UserDto>> GetProfile(int userId)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                return ApiResponse<UserDto>.ErrorResult("User tidak ditemukan");
            }

            var userDto = _mapper.Map<UserDto>(user);
            return ApiResponse<UserDto>.SuccessResult(userDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting profile for user: {UserId}", userId);
            return ApiResponse<UserDto>.ErrorResult("Gagal mengambil profil user");
        }
    }

    public async Task<ApiResponse<List<object>>> GetRoles()
    {
        try
        {
            var roles = new List<object>
            {
                new { Value = "admin", Label = "Administrator" },
                new { Value = "guru", Label = "Guru" },
                new { Value = "operator", Label = "Operator" }
            };
            return ApiResponse<List<object>>.SuccessResult(roles);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting roles");
            return ApiResponse<List<object>>.ErrorResult("Gagal mengambil data roles");
        }
    }

    // Helper methods
    private string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password);
    }

    private bool VerifyPassword(string password, string passwordHash)
    {
        return BCrypt.Net.BCrypt.Verify(password, passwordHash);
    }

    private string GenerateJwtToken(User user)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_jwtSecretKey);

            var claims = new[]
            {
                new Claim("id", user.Id.ToString()),
                new Claim("name", user.Username),
                new Claim("role", user.Role),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                NotBefore = DateTime.UtcNow.AddMinutes(-1),
                Expires = DateTime.UtcNow.AddMinutes(_jwtExpireMinutes),
                IssuedAt = DateTime.UtcNow,
                Issuer = _jwtIssuer,
                Audience = _jwtAudience,
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating JWT token for user {Username}", user.Username);
            throw new Exception($"Gagal generate token: {ex.Message}");
        }
    }
}