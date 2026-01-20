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
        IHubContext<LogHub> hubContext)
    {
        _userRepository = userRepository;
        _kartuRepository = kartuRepository;
        _kelasRepository = kelasRepository;
        _mapper = mapper;
        _logger = logger;
        _configuration = configuration;
        _hubContext = hubContext;

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

            // --- FIX KELAS LOGIC ---
            // Langsung set KelasId, tidak perlu List AnggotaKelas
            user.KelasId = request.KelasId;

            // Validasi keberadaan kelas jika ID dikirim
            if (request.KelasId.HasValue && request.KelasId > 0)
            {
                var kelasExists = await _kelasRepository.GetByIdAsync(request.KelasId.Value);
                if (kelasExists == null) return ApiResponse<UserDto>.ErrorResult("Kelas tidak valid");
            }

            await _userRepository.AddAsync(user);
            await _userRepository.SaveAsync();

            // Reload user untuk mendapatkan data relasi lengkap (Nama Kelas)
            var createdUser = await _userRepository.GetByIdAsync(user.Id);
            var createdDto = _mapper.Map<UserDto>(createdUser);

            await SendUserNotification("USER_CREATED", createdDto);
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

            // Update Field Dasar
            user.Username = request.Username;
            user.Role = request.Role;

            if (!string.IsNullOrWhiteSpace(request.Password))
                user.PasswordHash = HashPassword(request.Password);

            // --- FIX KELAS LOGIC ---
            // Cukup update KelasId
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
                    // Jika dikirim 0 atau negatif, anggap remove kelas
                    user.KelasId = null;
                }
            }
            // Note: Jika request.KelasId == null, kita biarkan data lama (sesuai pola PATCH/Update parsial)
            // Atau jika Anda ingin null berarti menghapus kelas, ubah logika di atas.

            _userRepository.Update(user);
            await _userRepository.SaveAsync();

            // Refresh data response
            var updatedUser = await _userRepository.GetByIdAsync(id);
            var userDto = _mapper.Map<UserDto>(updatedUser);

            await SendUserNotification("USER_UPDATED", userDto);
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

            await SendUserNotification("USER_DELETED", dto);

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

    // ... (GenerateJwtToken & SendUserNotification TETAP SAMA, tidak perlu dicopy ulang jika tidak berubah) ...
    // ... Pastikan copy method GenerateJwtToken dan SendUserNotification dari kode lamamu ...

    private async Task SendUserNotification(string type, UserDto data)
    {
        try
        {
            await _hubContext.Clients.All.SendAsync("UserNotification", new { EventId = Guid.NewGuid(), EventType = type, Timestamp = DateTime.UtcNow, Data = data });
        }
        catch { /* Ignore */ }
    }
}