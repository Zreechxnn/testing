using AutoMapper;
using Microsoft.AspNetCore.SignalR;
using testing.DTOs;
using testing.Hubs;
using testing.Models;
using testing.Repositories;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace testing.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IKartuRepository _kartuRepository;
    private readonly IKelasRepository _kelasRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<UserService> _logger;
    private readonly IConfiguration _configuration;
    private readonly IHubContext<LogHub> _hubContext;
    private readonly IBroadcastService _broadcastService; // 1. Inject BroadcastService

    private readonly string _jwtSecretKey;
    private readonly string _jwtIssuer;
    private readonly string _jwtAudience;
    private readonly int _jwtExpireMinutes;

    public UserService(
        IUserRepository userRepository,
        IKartuRepository kartuRepository,
        IKelasRepository kelasRepository,
        IMapper mapper,
        ILogger<UserService> logger,
        IConfiguration configuration,
        IHubContext<LogHub> hubContext,
        IBroadcastService broadcastService) // 2. Tambahkan di Constructor
    {
        _userRepository = userRepository;
        _kartuRepository = kartuRepository;
        _kelasRepository = kelasRepository;
        _mapper = mapper;
        _logger = logger;
        _configuration = configuration;
        _hubContext = hubContext;
        _broadcastService = broadcastService; // 3. Assign

        _jwtSecretKey = Environment.GetEnvironmentVariable("JwtSettings__SecretKey")
                        ?? configuration["JwtSettings:SecretKey"]
                        ?? throw new InvalidOperationException("JWT SecretKey not found!");

        _jwtIssuer = configuration["JwtSettings:Issuer"] ?? "LabAccessAPI";
        _jwtAudience = configuration["JwtSettings:Audience"] ?? "LabAccessClient";
        _jwtExpireMinutes = int.Parse(configuration["JwtSettings:ExpireMinutes"] ?? "1440");
    }

    public async Task<ApiResponse<List<UserDto>>> GetAllUsers()
    {
        var users = await _userRepository.GetAllAsync();
        return ApiResponse<List<UserDto>>.SuccessResult(_mapper.Map<List<UserDto>>(users));
    }

    public async Task<ApiResponse<PagedResponse<UserDto>>> GetUsersPaged(PagedRequest request)
    {
        if (!request.IsValid()) return ApiResponse<PagedResponse<UserDto>>.ErrorResult("Invalid pagination");

        var users = await _userRepository.GetPagedAsync(request.Page, request.PageSize);
        var totalCount = await _userRepository.CountAsync();

        var dtos = _mapper.Map<List<UserDto>>(users);
        return ApiResponse<PagedResponse<UserDto>>.SuccessResult(new PagedResponse<UserDto>(dtos, request.Page, request.PageSize, totalCount));
    }

    public async Task<ApiResponse<UserDto>> GetUserById(int id)
    {
        var user = await _userRepository.GetByIdAsync(id);
        return user == null
            ? ApiResponse<UserDto>.ErrorResult("User tidak ditemukan")
            : ApiResponse<UserDto>.SuccessResult(_mapper.Map<UserDto>(user));
    }

    public async Task<ApiResponse<UserDto>> CreateUser(UserCreateRequest request)
    {
        try
        {
            if (await _userRepository.IsUsernameExistAsync(request.Username))
                return ApiResponse<UserDto>.ErrorResult("Username sudah digunakan");

            var user = _mapper.Map<User>(request);
            user.PasswordHash = HashPassword(request.Password);

            // Fix Logic Relasi Kelas
            user.KelasId = request.KelasId;

            if (request.KelasId.HasValue && request.KelasId > 0)
            {
                var kelasExists = await _kelasRepository.GetByIdAsync(request.KelasId.Value);
                if (kelasExists == null) return ApiResponse<UserDto>.ErrorResult("Kelas tidak valid");
            }

            await _userRepository.AddAsync(user);
            await _userRepository.SaveAsync();

            var createdUser = await _userRepository.GetByIdAsync(user.Id);
            var createdDto = _mapper.Map<UserDto>(createdUser);

            // --- SIGNALR UPDATES ---
            await SendUserNotification("USER_CREATED", createdDto);
            await _broadcastService.PushDashboardStatsAsync(); // Update Counter User
            // -----------------------

            return ApiResponse<UserDto>.SuccessResult(createdDto, "User berhasil dibuat");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Create user failed");
            return ApiResponse<UserDto>.ErrorResult("Gagal membuat user");
        }
    }

    public async Task<ApiResponse<UserDto>> UpdateUser(int id, UserUpdateRequest request)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null) return ApiResponse<UserDto>.ErrorResult("User tidak ditemukan");

            if (request.Username != user.Username && await _userRepository.IsUsernameExistAsync(request.Username))
                return ApiResponse<UserDto>.ErrorResult("Username sudah digunakan");

            user.Username = request.Username;
            user.Role = request.Role;

            if (!string.IsNullOrWhiteSpace(request.Password))
                user.PasswordHash = HashPassword(request.Password);

            // Fix Logic Relasi Kelas (Update)
            if (request.KelasId.HasValue)
            {
                if (request.KelasId.Value > 0)
                {
                    var kelasExists = await _kelasRepository.GetByIdAsync(request.KelasId.Value);
                    if (kelasExists == null) return ApiResponse<UserDto>.ErrorResult("Kelas tidak valid");
                    user.KelasId = request.KelasId.Value;
                }
                else
                {
                    // ID 0 atau negatif berarti hapus kelas
                    user.KelasId = null;
                }
            }

            _userRepository.Update(user);
            await _userRepository.SaveAsync();

            var updatedUser = await _userRepository.GetByIdAsync(id);
            var userDto = _mapper.Map<UserDto>(updatedUser);

            await SendUserNotification("USER_UPDATED", userDto);

            await _hubContext.Clients.All.SendAsync("ReceiveCheckIn");

            return ApiResponse<UserDto>.SuccessResult(userDto, "User berhasil diupdate");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Update user failed: {Id}", id);
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
                return ApiResponse<object>.ErrorResult("Hapus kartu user terlebih dahulu");

            if (user.Role == "admin" && await _userRepository.CountAdminsAsync() <= 1)
                return ApiResponse<object>.ErrorResult("Tidak dapat menghapus admin terakhir");

            var dto = new UserDto { Id = user.Id, Username = user.Username, Role = user.Role };

            _userRepository.Remove(user);
            await _userRepository.SaveAsync();

            // --- SIGNALR UPDATES ---
            await SendUserNotification("USER_DELETED", dto);
            await _broadcastService.PushDashboardStatsAsync(); // Update Counter User
            // -----------------------

            return ApiResponse<object>.SuccessResult(new { }, "User berhasil dihapus");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Delete user failed");
            return ApiResponse<object>.ErrorResult("Gagal menghapus user");
        }
    }

    public async Task<ApiResponse<List<UserDto>>> GetUsersWithoutKartu()
    {
        var users = await _userRepository.GetUsersWithoutKartuAsync();
        return ApiResponse<List<UserDto>>.SuccessResult(_mapper.Map<List<UserDto>>(users));
    }

    // --- Helpers ---
    private string HashPassword(string p) => BCrypt.Net.BCrypt.HashPassword(p);

    private async Task SendUserNotification(string type, UserDto data)
    {
        try
        {
            var payload = new
            {
                EventId = Guid.NewGuid(),
                EventType = type,
                Timestamp = DateTime.UtcNow,
                Data = data,
                Message = $"User {data.Username} telah {type.Split('_')[1].ToLower()}"
            };
            await _hubContext.Clients.All.SendAsync("UserNotification", payload);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Gagal mengirim notifikasi user");
        }
    }

    // METHOD WAJIB (Karena UserService juga handle Auth di Controller)
    // Biasanya dipanggil dari AuthService atau Controller login
    public string GenerateJwtToken(User user)
    {
        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.UniqueName, user.Username),
            new Claim("role", user.Role), // Penting untuk otorisasi di FE
            new Claim("name", user.Username)
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSecretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expires = DateTime.UtcNow.AddMinutes(_jwtExpireMinutes);

        var token = new JwtSecurityToken(
            _jwtIssuer,
            _jwtAudience,
            claims,
            expires: expires,
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}