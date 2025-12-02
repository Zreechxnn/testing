using AutoMapper;
using testing.DTOs;
using testing.Models;
using testing.Repositories;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace testing.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IKartuRepository _kartuRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<UserService> _logger;
    private readonly IConfiguration _configuration;

    // JWT Settings
    private readonly string _jwtSecretKey;
    private readonly string _jwtIssuer;
    private readonly string _jwtAudience;
    private readonly int _jwtExpireMinutes;

    public UserService(
       IUserRepository userRepository,
       IKartuRepository kartuRepository,
       IMapper mapper,
       ILogger<UserService> logger,
       IConfiguration configuration)
    {
        _userRepository = userRepository;
        _kartuRepository = kartuRepository;
        _mapper = mapper;
        _logger = logger;
        _configuration = configuration;

        // Load JWT settings dari environment variables
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

        _logger.LogInformation("JWT Settings loaded - Issuer: {Issuer}, Audience: {Audience}, Expire: {ExpireMinutes}m",
            _jwtIssuer, _jwtAudience, _jwtExpireMinutes);
    }

    public async Task<ApiResponse<List<UserDto>>> GetAllUsers()
    {
        try
        {
            var users = await _userRepository.GetAllAsync();
            var userDtos = _mapper.Map<List<UserDto>>(users);
            return ApiResponse<List<UserDto>>.SuccessResult(userDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all users");
            return ApiResponse<List<UserDto>>.ErrorResult("Gagal mengambil data user");
        }
    }

    public async Task<ApiResponse<PagedResponse<UserDto>>> GetUsersPaged(PagedRequest request)
    {
        try
        {
            if (!request.IsValid())
            {
                return ApiResponse<PagedResponse<UserDto>>.ErrorResult("Parameter pagination tidak valid");
            }

            var users = await _userRepository.GetPagedAsync(request.Page, request.PageSize);
            var totalCount = await _userRepository.CountAsync();

            var userDtos = _mapper.Map<List<UserDto>>(users);
            var pagedResponse = new PagedResponse<UserDto>(
                userDtos,
                request.Page,
                request.PageSize,
                totalCount
            );

            return ApiResponse<PagedResponse<UserDto>>.SuccessResult(pagedResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving paged users");
            return ApiResponse<PagedResponse<UserDto>>.ErrorResult("Gagal mengambil data user");
        }
    }

    public async Task<ApiResponse<UserDto>> GetUserById(int id)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
            {
                return ApiResponse<UserDto>.ErrorResult("User tidak ditemukan");
            }

            var userDto = _mapper.Map<UserDto>(user);
            return ApiResponse<UserDto>.SuccessResult(userDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user by id: {Id}", id);
            return ApiResponse<UserDto>.ErrorResult("Gagal mengambil data user");
        }
    }

    public async Task<ApiResponse<UserDto>> CreateUser(UserCreateRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Username))
                return ApiResponse<UserDto>.ErrorResult("Username harus diisi");
            if (string.IsNullOrWhiteSpace(request.Password))
                return ApiResponse<UserDto>.ErrorResult("Password harus diisi");
            if (string.IsNullOrWhiteSpace(request.Role))
                return ApiResponse<UserDto>.ErrorResult("Role harus diisi");

            var existingUser = await _userRepository.GetByUsernameAsync(request.Username);
            if (existingUser != null)
                return ApiResponse<UserDto>.ErrorResult("Username sudah digunakan");

            var passwordHash = HashPassword(request.Password);
            var user = _mapper.Map<User>(request);
            user.PasswordHash = passwordHash;

            await _userRepository.AddAsync(user);
            var saved = await _userRepository.SaveAsync();

            if (!saved) return ApiResponse<UserDto>.ErrorResult("Gagal menyimpan user");

            var createdUser = await _userRepository.GetByIdAsync(user.Id);
            var userDto = _mapper.Map<UserDto>(createdUser!);

            _logger.LogInformation("User created: {Username}", user.Username);
            return ApiResponse<UserDto>.SuccessResult(userDto, "User berhasil dibuat");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating user: {Username}", request.Username);
            return ApiResponse<UserDto>.ErrorResult("Gagal membuat user");
        }
    }

    public async Task<ApiResponse<UserDto>> UpdateUser(int id, UserUpdateRequest request)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null) return ApiResponse<UserDto>.ErrorResult("User tidak ditemukan");

            if (string.IsNullOrWhiteSpace(request.Username))
                return ApiResponse<UserDto>.ErrorResult("Username harus diisi");
            if (string.IsNullOrWhiteSpace(request.Role))
                return ApiResponse<UserDto>.ErrorResult("Role harus diisi");

            var existingUser = await _userRepository.GetByUsernameAsync(request.Username);
            if (existingUser != null && existingUser.Id != id)
                return ApiResponse<UserDto>.ErrorResult("Username sudah digunakan");

            _mapper.Map(request, user);

            if (!string.IsNullOrWhiteSpace(request.Password))
            {
                user.PasswordHash = HashPassword(request.Password);
            }

            _userRepository.Update(user);
            var saved = await _userRepository.SaveAsync();

            if (!saved) return ApiResponse<UserDto>.ErrorResult("Gagal mengupdate user");

            var updatedUser = await _userRepository.GetByIdAsync(id);
            var userDto = _mapper.Map<UserDto>(updatedUser!);

            return ApiResponse<UserDto>.SuccessResult(userDto, "User berhasil diupdate");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user: {Id}", id);
            return ApiResponse<UserDto>.ErrorResult("Gagal mengupdate user");
        }
    }

    public async Task<ApiResponse<object>> DeleteUser(int id)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null) return ApiResponse<object>.ErrorResult("User tidak ditemukan");

            if (user.Kartu != null && user.Kartu.Any())
                return ApiResponse<object>.ErrorResult("Tidak dapat menghapus user karena memiliki kartu terdaftar");

            if (user.Role == "admin")
            {
                var adminCount = await _userRepository.CountAdminsAsync();
                if (adminCount <= 1)
                    return ApiResponse<object>.ErrorResult("Tidak dapat menghapus admin terakhir");
            }

            _userRepository.Remove(user);
            var saved = await _userRepository.SaveAsync();

            if (!saved) return ApiResponse<object>.ErrorResult("Gagal menghapus user");

            _logger.LogInformation("User deleted: {Id} - {Username}", user.Id, user.Username);
            return ApiResponse<object>.SuccessResult(null!, "User berhasil dihapus");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting user: {Id}", id);
            return ApiResponse<object>.ErrorResult("Gagal menghapus user");
        }
    }

    public async Task<ApiResponse<List<UserDto>>> GetUsersWithoutKartu()
    {
        try
        {
            var users = await _userRepository.GetUsersWithoutKartuAsync();
            var userDtos = _mapper.Map<List<UserDto>>(users);
            return ApiResponse<List<UserDto>>.SuccessResult(userDtos, "Berhasil mengambil user tanpa kartu");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting users without kartu");
            return ApiResponse<List<UserDto>>.ErrorResult("Gagal mengambil user tanpa kartu");
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
            _logger.LogInformation("Generating JWT token for user: {Username}, Role: {Role}", user.Username, user.Role);

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_jwtSecretKey);

            // [PENTING] Gunakan nama claim standar string literal ("id", "role") 
            // agar match dengan konfigurasi Program.cs
            var claims = new[]
            {
                new Claim("id", user.Id.ToString()),
                new Claim("name", user.Username),
                new Claim("role", user.Role), // Lowercase 'role'

                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                // [PENTING] Tambahkan buffer mundur 1 menit untuk sinkronisasi waktu server
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
            var tokenString = tokenHandler.WriteToken(token);

            return tokenString;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating JWT token for user {Username}", user.Username);
            throw new Exception($"Gagal generate token: {ex.Message}");
        }
    }
}