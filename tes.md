perbaiki waktu tap/response dan tap/requestnya agar sesuai dengan apa yang diinput (kalau timestamp asal, tetap jadikan now) , bagaimana menurutmu? D:\sakat\testing>cd controllers

D:\sakat\testing\Controllers>ls
AksesLogController.cs  RuanganController.cs  UserController.cs
KartuController.cs     ScanController.cs
KelasController.cs     TapController.cs

D:\sakat\testing\Controllers>cat AksesLogController.cs  RuanganController.cs  UserController.cs KartuController.cs     ScanController.cs KelasController.cs     TapController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using testing.DTOs;
using testing.Services;

namespace testing.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AksesLogController : ControllerBase
{
    private readonly IAksesLogService _aksesLogService;
    private readonly ILogger<AksesLogController> _logger;

    public AksesLogController(IAksesLogService aksesLogService, ILogger<AksesLogController> logger)
    {
        _aksesLogService = aksesLogService;
        _logger = logger;
    }

    [HttpGet]
    // [Authorize(Roles = "admin,operator")]
    public async Task<ActionResult<ApiResponse<List<AksesLogDto>>>> GetAll()
    {
        var response = await _aksesLogService.GetAllAksesLog();
        return Ok(response);
    }

    [HttpGet("paged")]
    // [Authorize(Roles = "admin,operator")]
    public async Task<ActionResult<ApiResponse<PagedResponse<AksesLogDto>>>> GetPaged([FromQuery] PagedRequest request)
    {
        var response = await _aksesLogService.GetAksesLogPaged(request);
        return Ok(response);
    }

    [HttpGet("{id}")]
    // [Authorize(Roles = "admin,operator")]
    public async Task<ActionResult<ApiResponse<AksesLogDto>>> GetById(int id)
    {
        var response = await _aksesLogService.GetAksesLogById(id);
        if (!response.Success)
            return NotFound(response);
        return Ok(response);
    }

    [HttpGet("kartu/{kartuId}")]
    // [Authorize(Roles = "admin,operator")]
    public async Task<ActionResult<ApiResponse<List<AksesLogDto>>>> GetByKartuId(int kartuId)
    {
        var response = await _aksesLogService.GetAksesLogByKartuId(kartuId);
        return Ok(response);
    }

    [HttpGet("ruangan/{ruanganId}")]
    // [Authorize(Roles = "admin,operator")]
    public async Task<ActionResult<ApiResponse<List<AksesLogDto>>>> GetByRuanganId(int ruanganId)
    {
        var response = await _aksesLogService.GetAksesLogByRuanganId(ruanganId);
        return Ok(response);
    }

    [HttpGet("latest/{count}")]
    // [Authorize(Roles = "admin,operator")]
    public async Task<ActionResult<ApiResponse<List<AksesLogDto>>>> GetLatest(int count)
    {
        var response = await _aksesLogService.GetLatestAksesLog(count);
        return Ok(response);
    }

    [HttpGet("dashboard/stats")]
    // [Authorize(Roles = "admin,operator")]
    public async Task<ActionResult<ApiResponse<DashboardStatsDto>>> GetDashboardStats()
    {
        var response = await _aksesLogService.GetDashboardStats();
        return Ok(response);
    }

    [HttpGet("today/stats")]
    // [Authorize(Roles = "admin,operator")]
    public async Task<ActionResult<ApiResponse<TodayStatsDto>>> GetTodayStats()
    {
        var response = await _aksesLogService.GetTodayStats();
        return Ok(response);
    }
}using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using testing.DTOs;
using testing.Services;

namespace testing.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RuanganController : ControllerBase
{
    private readonly IRuanganService _ruanganService;
    private readonly ILogger<RuanganController> _logger;

    public RuanganController(IRuanganService ruanganService, ILogger<RuanganController> logger)
    {
        _ruanganService = ruanganService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<RuanganDto>>>> GetAll()
    {
        var response = await _ruanganService.GetAllRuangan();
        return Ok(response);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<RuanganDto>>> GetById(int id)
    {
        var response = await _ruanganService.GetRuanganById(id);
        if (!response.Success)
            return NotFound(response);
        return Ok(response);
    }

    [HttpPost]
    // [Authorize(Roles = "admin")]
    public async Task<ActionResult<ApiResponse<RuanganDto>>> Create([FromBody] RuanganCreateRequest request)
    {
        var response = await _ruanganService.CreateRuangan(request);
        if (!response.Success)
            return BadRequest(response);
        return CreatedAtAction(nameof(GetById), new { id = response.Data?.Id }, response);
    }

    [HttpPut("{id}")]
    // [Authorize(Roles = "admin")]
    public async Task<ActionResult<ApiResponse<RuanganDto>>> Update(int id, [FromBody] RuanganUpdateRequest request)
    {
        var response = await _ruanganService.UpdateRuangan(id, request);
        if (!response.Success)
            return BadRequest(response);
        return Ok(response);
    }

    [HttpDelete("{id}")]
    // [Authorize(Roles = "admin")]
    public async Task<ActionResult<ApiResponse<object>>> Delete(int id)
    {
        var response = await _ruanganService.DeleteRuangan(id);
        if (!response.Success)
            return BadRequest(response);
        return Ok(response);
    }

    [HttpGet("stats/{id}")]
    public async Task<ActionResult<ApiResponse<RuanganStatsDto>>> GetStats(int id)
    {
        var response = await _ruanganService.GetRuanganStats(id);
        if (!response.Success)
            return NotFound(response);
        return Ok(response);
    }
}using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using testing.DTOs;
using testing.Services;
using System.Security.Claims;

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

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<UserLoginResponse>>> Login([FromBody] UserLoginRequest request)
    {
        var response = await _userService.Login(request);
        if (!response.Success)
            return Unauthorized(response);
        return Ok(response);
    }

    [HttpGet("profile")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<UserDto>>> GetProfile()
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var response = await _userService.GetProfile(userId);
        if (!response.Success)
            return NotFound(response);
        return Ok(response);
    }

    [HttpGet]
    // [Authorize(Roles = "admin")]
    public async Task<ActionResult<ApiResponse<List<UserDto>>>> GetAll()
    {
        var response = await _userService.GetAllUsers();
        return Ok(response);
    }

    [HttpGet("{id}")]
    // [Authorize(Roles = "admin")]
    public async Task<ActionResult<ApiResponse<UserDto>>> GetById(int id)
    {
        var response = await _userService.GetUserById(id);
        if (!response.Success)
            return NotFound(response);
        return Ok(response);
    }

    [HttpPost]
    // [Authorize(Roles = "admin")]
    public async Task<ActionResult<ApiResponse<UserDto>>> Create([FromBody] UserCreateRequest request)
    {
        var response = await _userService.CreateUser(request);
        if (!response.Success)
            return BadRequest(response);
        return CreatedAtAction(nameof(GetById), new { id = response.Data?.Id }, response);
    }

    [HttpPut("{id}")]
    // [Authorize(Roles = "admin")]
    public async Task<ActionResult<ApiResponse<UserDto>>> Update(int id, [FromBody] UserUpdateRequest request)
    {
        var response = await _userService.UpdateUser(id, request);
        if (!response.Success)
            return BadRequest(response);
        return Ok(response);
    }

    [HttpDelete("{id}")]
    // [Authorize(Roles = "admin")]
    public async Task<ActionResult<ApiResponse<object>>> Delete(int id)
    {
        var response = await _userService.DeleteUser(id);
        if (!response.Success)
            return BadRequest(response);
        return Ok(response);
    }

    [HttpGet("Roles")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<List<object>>>> GetRoles()
    {
        var response = await _userService.GetRoles();
        return Ok(response);
    }
}using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using testing.DTOs;
using testing.Services;

namespace testing.Controllers;

[ApiController]
[Route("api/[controller]")]
public class KartuController : ControllerBase
{
    private readonly IKartuService _kartuService;
    private readonly ILogger<KartuController> _logger;

    public KartuController(IKartuService kartuService, ILogger<KartuController> logger)
    {
        _kartuService = kartuService;
        _logger = logger;
    }

    [HttpGet]
    // [Authorize(Roles = "admin,operator")]
    public async Task<ActionResult<ApiResponse<List<KartuDto>>>> GetAll()
    {
        var response = await _kartuService.GetAllKartu();
        return Ok(response);
    }

    [HttpGet("paged")]
    // [Authorize(Roles = "admin,operator")]
    public async Task<ActionResult<ApiResponse<PagedResponse<KartuDto>>>> GetPaged([FromQuery] PagedRequest request)
    {
        var response = await _kartuService.GetKartuPaged(request);
        return Ok(response);
    }

    [HttpGet("{id}")]
    // [Authorize(Roles = "admin,operator")]
    public async Task<ActionResult<ApiResponse<KartuDto>>> GetById(int id)
    {
        var response = await _kartuService.GetKartuById(id);
        if (!response.Success)
            return NotFound(response);
        return Ok(response);
    }

    [HttpPost]
    // [Authorize(Roles = "admin,operator")]
    public async Task<ActionResult<ApiResponse<KartuDto>>> Create([FromBody] KartuCreateDto request)
    {
        var response = await _kartuService.CreateKartu(request);
        if (!response.Success)
            return BadRequest(response);
        return CreatedAtAction(nameof(GetById), new { id = response.Data?.Id }, response);
    }

    [HttpPut("{id}")]
    // [Authorize(Roles = "admin,operator")]
    public async Task<ActionResult<ApiResponse<KartuDto>>> Update(int id, [FromBody] KartuUpdateDto request)
    {
        var response = await _kartuService.UpdateKartu(id, request);
        if (!response.Success)
            return BadRequest(response);
        return Ok(response);
    }

    [HttpDelete("{id}")]
    // [Authorize(Roles = "admin")]
    public async Task<ActionResult<ApiResponse<object>>> Delete(int id)
    {
        var response = await _kartuService.DeleteKartu(id);
        if (!response.Success)
            return BadRequest(response);
        return Ok(response);
    }

    [HttpGet("check/{uid}")]
    public async Task<ActionResult<ApiResponse<KartuCheckDto>>> CheckCard(string uid)
    {
        var response = await _kartuService.CheckCard(uid);
        return Ok(response);
    }
}using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using testing.DTOs;
using testing.Services;

namespace testing.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ScanController : ControllerBase
{
    private readonly IScanService _scanService;
    private readonly ILogger<ScanController> _logger;

    public ScanController(IScanService scanService, ILogger<ScanController> logger)
    {
        _scanService = scanService;
        _logger = logger;
    }

    [HttpPost("register")]
    public async Task<ActionResult<ApiResponse<ScanResponse>>> RegisterCard([FromBody] ScanRequest request)
    {
        var response = await _scanService.RegisterCard(request);
        if (!response.Success)
            return BadRequest(response);
        return Ok(response);
    }

    [HttpPost("check")]
    public async Task<ActionResult<ApiResponse<ScanCheckResponse>>> CheckCard([FromBody] ScanCheckRequest request)
    {
        var response = await _scanService.CheckCard(request);
        return Ok(response);
    }

    [HttpGet("kartu-terdaftar")]
    [Authorize(Roles = "admin,operator")]
    public async Task<ActionResult<ApiResponse<List<object>>>> GetKartuTerdaftar()
    {
        var response = await _scanService.GetKartuTerdaftar();
        return Ok(response);
    }

    [HttpDelete("kartu/{id}")]
    [Authorize(Roles = "admin")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteKartu(int id)
    {
        var response = await _scanService.DeleteKartu(id);
        if (!response.Success)
            return BadRequest(response);
        return Ok(response);
    }
}using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using testing.DTOs;
using testing.Services;

namespace testing.Controllers;

[ApiController]
[Route("api/[controller]")]
public class KelasController : ControllerBase
{
    private readonly IKelasService _kelasService;
    private readonly ILogger<KelasController> _logger;

    public KelasController(IKelasService kelasService, ILogger<KelasController> logger)
    {
        _kelasService = kelasService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<KelasDto>>>> GetAll()
    {
        var response = await _kelasService.GetAllKelas();
        return Ok(response);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<KelasDto>>> GetById(int id)
    {
        var response = await _kelasService.GetKelasById(id);
        if (!response.Success)
            return NotFound(response);
        return Ok(response);
    }

    [HttpPost]
    // [Authorize(Roles = "admin")]
    public async Task<ActionResult<ApiResponse<KelasDto>>> Create([FromBody] KelasCreateRequest request)
    {
        var response = await _kelasService.CreateKelas(request);
        if (!response.Success)
            return BadRequest(response);
        return CreatedAtAction(nameof(GetById), new { id = response.Data?.Id }, response);
    }

    [HttpPut("{id}")]
    // [Authorize(Roles = "admin")]
    public async Task<ActionResult<ApiResponse<KelasDto>>> Update(int id, [FromBody] KelasUpdateRequest request)
    {
        var response = await _kelasService.UpdateKelas(id, request);
        if (!response.Success)
            return BadRequest(response);
        return Ok(response);
    }

    [HttpDelete("{id}")]
    // [Authorize(Roles = "admin")]
    public async Task<ActionResult<ApiResponse<object>>> Delete(int id)
    {
        var response = await _kelasService.DeleteKelas(id);
        if (!response.Success)
            return BadRequest(response);
        return Ok(response);
    }

    [HttpGet("stats/{id}")]
    public async Task<ActionResult<ApiResponse<KelasStatsDto>>> GetStats(int id)
    {
        var response = await _kelasService.GetKelasStats(id);
        if (!response.Success)
            return NotFound(response);
        return Ok(response);
    }
}using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using testing.DTOs;
using testing.Services;

namespace testing.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TapController : ControllerBase
{
    private readonly ITapService _tapService;
    private readonly ILogger<TapController> _logger;

    public TapController(ITapService tapService, ILogger<TapController> logger)
    {
        _tapService = tapService;
        _logger = logger;
    }

    [HttpPost("process")]
    public async Task<ActionResult<ApiResponse<TapResponse>>> ProcessTap([FromBody] TapRequest request)
    {
        var response = await _tapService.ProcessTap(request);
        return Ok(response);
    }

    [HttpGet("logs")]
    [Authorize(Roles = "admin,operator")]
    public async Task<ActionResult<ApiResponse<List<object>>>> GetLogs([FromQuery] int? ruanganId = null)
    {
        var response = await _tapService.GetLogs(ruanganId);
        return Ok(response);
    }

    [HttpGet("kartu")]
    [Authorize(Roles = "admin,operator")]
    public async Task<ActionResult<ApiResponse<List<object>>>> GetKartu()
    {
        var response = await _tapService.GetKartu();
        return Ok(response);
    }

    [HttpGet("ruangan")]
    [Authorize(Roles = "admin,operator")]
    public async Task<ActionResult<ApiResponse<List<object>>>> GetRuangan()
    {
        var response = await _tapService.GetRuangan();
        return Ok(response);
    }

    [HttpGet("kelas")]
    [Authorize(Roles = "admin,operator")]
    public async Task<ActionResult<ApiResponse<List<object>>>> GetKelas()
    {
        var response = await _tapService.GetKelas();
        return Ok(response);
    }

    [HttpGet("stats")]
    [Authorize(Roles = "admin,operator")]
    public async Task<ActionResult<ApiResponse<object>>> GetStats()
    {
        var response = await _tapService.GetStats();
        return Ok(response);
    }

    [HttpGet("stats/hari-ini")]
    [Authorize(Roles = "admin,operator")]
    public async Task<ActionResult<ApiResponse<object>>> GetStatsHariIni()
    {
        var response = await _tapService.GetStatsHariIni();
        return Ok(response);
    }
}
D:\sakat\testing\Controllers>cd ../dtos

D:\sakat\testing\DTOs>ls
AdditionalDtos.cs  KartuCreateDto.cs  KartuUpdateDto.cs
ApiResponse.cs     KartuDto.cs

D:\sakat\testing\DTOs>cat AdditionalDtos.cs  KartuCreateDto.cs  KartuUpdateDto.cs ApiResponse.cs     KartuDto.cs
namespace testing.DTOs;

// Existing DTOs
public class KelasCreateRequest
{
    public required string Nama { get; set; }
}

public class KelasUpdateRequest
{
    public required string Nama { get; set; }
}

public class RuanganCreateRequest
{
    public required string Nama { get; set; }
}

public class RuanganUpdateRequest
{
    public required string Nama { get; set; }
}

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

// DTOs for responses
public class KelasDto
{
    public int Id { get; set; }
    public string Nama { get; set; } = string.Empty;
}

public class RuanganDto
{
    public int Id { get; set; }
    public string Nama { get; set; } = string.Empty;
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

public class AksesLogDto
{
    public int Id { get; set; }
    public int KartuId { get; set; }
    public int RuanganId { get; set; }
    public DateTime TimestampMasuk { get; set; }
    public DateTime? TimestampKeluar { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? KartuUid { get; set; }
    public string? RuanganNama { get; set; }
    public string? UserUsername { get; set; }
    public string? KelasNama { get; set; }
}

public class DashboardStatsDto
{
    public int TotalAkses { get; set; }
    public int AktifSekarang { get; set; }
    public int TotalKartu { get; set; }
    public int TotalKelas { get; set; }
    public int TotalRuangan { get; set; }
    public int TotalUsers { get; set; }
}

public class TodayStatsDto
{
    public string Tanggal { get; set; } = string.Empty;
    public int TotalCheckin { get; set; }
    public int TotalCheckout { get; set; }
    public int MasihAktif { get; set; }
    public object KartuPalingAktif { get; set; } = new(); // Ubah ke object
}

public class KelasStatsDto
{
    public string Kelas { get; set; } = string.Empty;
    public int TotalAkses { get; set; }
    public int AktifSekarang { get; set; }
}

public class RuanganStatsDto
{
    public string Ruangan { get; set; } = string.Empty;
    public int TotalAkses { get; set; }
    public int AksesHariIni { get; set; }
    public int AktifSekarang { get; set; }
}using System.ComponentModel.DataAnnotations;

namespace testing.DTOs;

public class KartuCreateDto
{
    [Required(ErrorMessage = "UID kartu wajib diisi")]
    [StringLength(50, MinimumLength = 5, ErrorMessage = "UID harus 5-50 karakter")]
    [RegularExpression("^[a-zA-Z0-9:]*$", ErrorMessage = "UID hanya boleh alfanumerik dan titik dua")]
    public required string Uid { get; set; }

    [RegularExpression("^(AKTIF|NONAKTIF)$", ErrorMessage = "Status harus AKTIF atau NONAKTIF")]
    public string? Status { get; set; }

    [StringLength(500, ErrorMessage = "Keterangan maksimal 500 karakter")]
    public string? Keterangan { get; set; }

    public int? UserId { get; set; }
    public int? KelasId { get; set; }
}using System.ComponentModel.DataAnnotations;

namespace testing.DTOs
{
    public class KartuUpdateDto
    {
        [Required(ErrorMessage = "UID kartu wajib diisi")]
        [StringLength(50, MinimumLength = 5, ErrorMessage = "UID harus 5-50 karakter")]
        [RegularExpression("^[a-zA-Z0-9:]*$", ErrorMessage = "UID hanya boleh alfanumerik dan titik dua")]
        public required string Uid { get; set; }

        [Required(ErrorMessage = "Status wajib diisi")]
        [RegularExpression("^(AKTIF|NONAKTIF)$", ErrorMessage = "Status harus AKTIF atau NONAKTIF")]
        public required string Status { get; set; }

        [StringLength(500, ErrorMessage = "Keterangan maksimal 500 karakter")]
        public string? Keterangan { get; set; }

        public int? UserId { get; set; }
        public int? KelasId { get; set; }
    }
}namespace testing.DTOs;

public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string Message { get; set; }
    public T? Data { get; set; }
    public string? ErrorCode { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    public ApiResponse(T data, string message = "Success")
    {
        Success = true;
        Message = message;
        Data = data;
    }

    public ApiResponse(string errorMessage, string? errorCode = null)
    {
        Success = false;
        Message = errorMessage;
        ErrorCode = errorCode;
        Data = default;
    }

    public static ApiResponse<T> SuccessResult(T data, string message = "Success")
        => new ApiResponse<T>(data, message);

    public static ApiResponse<T> ErrorResult(string message, string? errorCode = null)
        => new ApiResponse<T>(message, errorCode);
}

public class PagedResponse<T> : ApiResponse<IEnumerable<T>>
{
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasPrevious => Page > 1;
    public bool HasNext => Page < TotalPages;

    public PagedResponse(
        IEnumerable<T> data,
        int page,
        int pageSize,
        int totalCount,
        string message = "Success")
        : base(data, message)
    {
        Page = page;
        PageSize = pageSize;
        TotalCount = totalCount;
    }
}

public class PagedRequest
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 50;

    public virtual bool IsValid()
    {
        return Page > 0 && PageSize > 0 && PageSize <= 1000;
    }
}namespace testing.DTOs
{
    public class KartuDto
    {
        public int Id { get; set; }
        public required string Uid { get; set; }
        public string? Status { get; set; }
        public string? Keterangan { get; set; }
        public int? UserId { get; set; }
        public int? KelasId { get; set; }
        public DateTime? CreatedAt { get; set; }
        public string? UserUsername { get; set; }
        public string? KelasNama { get; set; }
    }
}
D:\sakat\testing\DTOs>cd ../data

D:\sakat\testing\Data>ls
LabDbContext.cs

D:\sakat\testing\Data>cat LabDbContext.cs
using Microsoft.EntityFrameworkCore;
using testing.Models;

namespace testing.Data
{
    public class LabDbContext : DbContext
    {
        public LabDbContext(DbContextOptions<LabDbContext> options) : base(options) { }

        public DbSet<Kartu> Kartu => Set<Kartu>();
        public DbSet<Kelas> Kelas => Set<Kelas>();
        public DbSet<User> Users => Set<User>();
        public DbSet<Ruangan> Ruangan => Set<Ruangan>();
        public DbSet<AksesLog> AksesLog => Set<AksesLog>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Konfigurasi Kartu
            modelBuilder.Entity<Kartu>(entity =>
            {
                entity.Property(k => k.CreatedAt)
                    .HasDefaultValueSql("NOW()");

                entity.Property(k => k.Status)
                    .HasDefaultValue("AKTIF");

                entity.HasIndex(k => k.Uid)
                    .IsUnique();

                // Relasi dengan User
                entity.HasOne(k => k.User)
                    .WithMany(u => u.Kartu)
                    .HasForeignKey(k => k.UserId)
                    .OnDelete(DeleteBehavior.SetNull);

                // Relasi dengan Kelas
                entity.HasOne(k => k.Kelas)
                    .WithMany(k => k.Kartu)
                    .HasForeignKey(k => k.KelasId)
                    .OnDelete(DeleteBehavior.SetNull);

                // Constraint untuk memastikan hanya satu yang terisi - PERBAIKAN: Gunakan ToTable().HasCheckConstraint
                entity.ToTable(t => t.HasCheckConstraint("CK_Kartu_SingleOwner",
                    @"""UserId"" IS NULL AND ""KelasId"" IS NOT NULL OR
                      ""UserId"" IS NOT NULL AND ""KelasId"" IS NULL OR
                      ""UserId"" IS NULL AND ""KelasId"" IS NULL"));
            });

            // Konfigurasi User
            modelBuilder.Entity<User>(entity =>
            {
                entity.Property(u => u.CreatedAt)
                    .HasDefaultValueSql("NOW()");

                entity.HasIndex(u => u.Username)
                    .IsUnique();
            });

            // Konfigurasi AksesLog
            modelBuilder.Entity<AksesLog>(entity =>
            {
                entity.HasOne(a => a.Kartu)
                    .WithMany(k => k.AksesLogs)
                    .HasForeignKey(a => a.KartuId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(a => a.Ruangan)
                    .WithMany(r => r.AksesLogs)
                    .HasForeignKey(a => a.RuanganId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
D:\sakat\testing\Data>cd
D:\sakat\testing\Data

D:\sakat\testing\Data>
D:\sakat\testing\Data>cd ..

D:\sakat\testing>ls
Controllers        Properties                    db
DTOs               Repositories                  obj
Data               Services                      rebuild.bat
Hubs               Validators                    run.bat
MappingProfile.cs  add.bat                       testing.csproj
Middleware         api                           testing.http
Migrations         appsettings.Development.json  testing.sln
Models             appsettings.json
Program.cs         bin

D:\sakat\testing>cd models

D:\sakat\testing\Models>ls
AksesLog.cs  Kelas.cs    TapRequest.cs   User.cs
Kartu.cs     Ruangan.cs  TapResponse.cs

D:\sakat\testing\Models>cat AksesLog.cs  Kelas.cs    TapRequest.cs   User.cs Kartu.cs     Ruangan.cs  TapResponse.cs
namespace testing.Models
{
    public class AksesLog
    {
        public int Id { get; set; }
        public int KartuId { get; set; }
        public int RuanganId { get; set; }
        public required DateTime TimestampMasuk { get; set; }
        public DateTime? TimestampKeluar { get; set; }
        public required string Status { get; set; }

        public virtual Kartu? Kartu { get; set; }
        public virtual Ruangan? Ruangan { get; set; }
    }
}namespace testing.Models
{
    public class Kelas
    {
        public int Id { get; set; }
        public required string Nama { get; set; }

        public virtual ICollection<Kartu>? Kartu { get; set; }
    }
}using System.Text.Json.Serialization;

namespace testing.Models
{
    public class TapRequest
    {
        [JsonPropertyName("uid")]
        public required string Uid { get; set; }

        [JsonPropertyName("id_ruangan")]
        public int IdRuangan { get; set; }

        [JsonPropertyName("timestamp")]
        public required string Timestamp { get; set; }
    }
}namespace testing.Models
{
    public class User
    {
        public int Id { get; set; }
        public required string Username { get; set; }
        public required string PasswordHash { get; set; }
        public required string Role { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation property - satu user bisa memiliki banyak kartu
        public virtual ICollection<Kartu>? Kartu { get; set; }
    }
}namespace testing.Models
{
    public class Kartu
    {
        public int Id { get; set; }
        public required string Uid { get; set; }
        public string? Status { get; set; } = "AKTIF";
        public string? Keterangan { get; set; }
        public int? UserId { get; set; }
        public int? KelasId { get; set; }
        public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;

        public virtual User? User { get; set; }
        public virtual Kelas? Kelas { get; set; }
        public virtual ICollection<AksesLog>? AksesLogs { get; set; }
    }
}namespace testing.Models
{
    public class Ruangan
    {
        public int Id { get; set; }
        public required string Nama { get; set; }
        public virtual ICollection<AksesLog>? AksesLogs { get; set; }
    }
}namespace testing.Models
{
    public class TapResponse
    {
        public required string Status { get; set; }
        public required string Message { get; set; }
        public string? NamaKelas { get; set; }
        public string? Ruangan { get; set; }
        public string? Waktu { get; set; }
    }
}
D:\sakat\testing\Models>cd ..

D:\sakat\testing>cd repositories

D:\sakat\testing\Repositories>ls
AksesLogRepository.cs   IUserRepository.cs
IAksesLogRepository.cs  KartuRepository.cs
IKartuRepository.cs     KelasRepository.cs
IKelasRepository.cs     RuanganRepository.cs
IRuanganRepository.cs   UserRepository.cs

D:\sakat\testing\Repositories>cat AksesLogRepository.cs   IUserRepository.cs IAksesLogRepository.cs  KartuRepository.cs IKartuRepository.cs     KelasRepository.cs IKelasRepository.cs     RuanganRepository.cs IRuanganRepository.cs   UserRepository.cs
using Microsoft.EntityFrameworkCore;
using testing.Data;
using testing.Models;

namespace testing.Repositories;

public class AksesLogRepository : IAksesLogRepository
{
    private readonly LabDbContext _context;

    public AksesLogRepository(LabDbContext context)
    {
        _context = context;
    }

    public async Task<AksesLog?> GetByIdAsync(int id)
    {
        return await _context.AksesLog
            .Include(a => a.Kartu)
            .ThenInclude(k => k!.User)
            .Include(a => a.Kartu)
            .ThenInclude(k => k!.Kelas)
            .Include(a => a.Ruangan)
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<IEnumerable<AksesLog>> GetAllAsync()
    {
        return await _context.AksesLog
            .Include(a => a.Kartu)
            .ThenInclude(k => k!.User)
            .Include(a => a.Kartu)
            .ThenInclude(k => k!.Kelas)
            .Include(a => a.Ruangan)
            .AsNoTracking()
            .OrderByDescending(a => a.TimestampMasuk)
            .ToListAsync();
    }

    public async Task<IEnumerable<AksesLog>> GetPagedAsync(int page, int pageSize)
    {
        return await _context.AksesLog
            .Include(a => a.Kartu)
            .ThenInclude(k => k!.User)
            .Include(a => a.Kartu)
            .ThenInclude(k => k!.Kelas)
            .Include(a => a.Ruangan)
            .AsNoTracking()
            .OrderByDescending(a => a.TimestampMasuk)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<IEnumerable<AksesLog>> GetByKartuIdAsync(int kartuId)
    {
        return await _context.AksesLog
            .Include(a => a.Kartu)
            .ThenInclude(k => k!.User)
            .Include(a => a.Kartu)
            .ThenInclude(k => k!.Kelas)
            .Include(a => a.Ruangan)
            .Where(a => a.KartuId == kartuId)
            .AsNoTracking()
            .OrderByDescending(a => a.TimestampMasuk)
            .ToListAsync();
    }

    public async Task<IEnumerable<AksesLog>> GetByRuanganIdAsync(int ruanganId)
    {
        return await _context.AksesLog
            .Include(a => a.Kartu)
            .ThenInclude(k => k!.User)
            .Include(a => a.Kartu)
            .ThenInclude(k => k!.Kelas)
            .Include(a => a.Ruangan)
            .Where(a => a.RuanganId == ruanganId)
            .AsNoTracking()
            .OrderByDescending(a => a.TimestampMasuk)
            .ToListAsync();
    }

    public async Task<IEnumerable<AksesLog>> GetLatestAsync(int count)
    {
        return await _context.AksesLog
            .Include(a => a.Kartu)
            .ThenInclude(k => k!.User)
            .Include(a => a.Kartu)
            .ThenInclude(k => k!.Kelas)
            .Include(a => a.Ruangan)
            .AsNoTracking()
            .OrderByDescending(a => a.TimestampMasuk)
            .Take(count)
            .ToListAsync();
    }

    public async Task<AksesLog?> GetActiveLogByKartuIdAsync(int kartuId)
    {
        return await _context.AksesLog
            .Include(a => a.Kartu)
            .ThenInclude(k => k!.User)
            .Include(a => a.Kartu)
            .ThenInclude(k => k!.Kelas)
            .Include(a => a.Ruangan)
            .Where(a => a.KartuId == kartuId && a.TimestampKeluar == null)
            .FirstOrDefaultAsync();
    }

    public async Task<bool> AnyByKartuIdAsync(int kartuId)
    {
        return await _context.AksesLog
            .AnyAsync(a => a.KartuId == kartuId);
    }

    public async Task<bool> AnyByRuanganIdAsync(int ruanganId)
    {
        return await _context.AksesLog
            .AnyAsync(a => a.RuanganId == ruanganId);
    }

    public async Task AddAsync(AksesLog aksesLog)
    {
        await _context.AksesLog.AddAsync(aksesLog);
    }

    public void Update(AksesLog aksesLog)
    {
        _context.AksesLog.Update(aksesLog);
    }

    public async Task<int> CountAsync()
    {
        return await _context.AksesLog.CountAsync();
    }

    public async Task<int> CountByRuanganIdAsync(int ruanganId)
    {
        return await _context.AksesLog
            .CountAsync(a => a.RuanganId == ruanganId);
    }

    public async Task<int> CountActiveByRuanganIdAsync(int ruanganId)
    {
        return await _context.AksesLog
            .CountAsync(a => a.RuanganId == ruanganId && a.TimestampKeluar == null);
    }

    public async Task<int> CountByDateRangeAsync(DateTime start, DateTime end)
    {
        return await _context.AksesLog
            .CountAsync(a => a.TimestampMasuk >= start && a.TimestampMasuk < end);
    }

    public async Task<IEnumerable<AksesLog>> GetByDateRangeAsync(DateTime start, DateTime end)
    {
        return await _context.AksesLog
            .Include(a => a.Kartu)
            .ThenInclude(k => k!.User)
            .Include(a => a.Kartu)
            .ThenInclude(k => k!.Kelas)
            .Include(a => a.Ruangan)
            .Where(a => a.TimestampMasuk >= start && a.TimestampMasuk < end)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<bool> SaveAsync()
    {
        try
        {
            var result = await _context.SaveChangesAsync();
            return result > 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving AksesLog: {ex.Message}");
            throw;
        }
    }

    public async Task<int> CountByKelasIdAsync(int kelasId)
    {
        return await _context.AksesLog
            .Include(a => a.Kartu)
            .CountAsync(a => a.Kartu != null && a.Kartu.KelasId == kelasId);
    }

    public async Task<int> CountActiveByKelasIdAsync(int kelasId)
    {
        return await _context.AksesLog
            .Include(a => a.Kartu)
            .CountAsync(a => a.Kartu != null && a.Kartu.KelasId == kelasId && a.TimestampKeluar == null);
    }

    public async Task<int> CountByRuanganIdAndDateAsync(int ruanganId, DateTime date)
    {
        var start = date.Date;
        var end = start.AddDays(1);
        return await _context.AksesLog
            .CountAsync(a => a.RuanganId == ruanganId && a.TimestampMasuk >= start && a.TimestampMasuk < end);
    }
}using testing.Models;

namespace testing.Repositories;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(int id);
    Task<User?> GetByUsernameAsync(string username);
    Task<User?> GetByKartuUidAsync(string kartuUid);
    Task<IEnumerable<User>> GetAllAsync();
    Task<IEnumerable<User>> GetPagedAsync(int page, int pageSize);
    Task<bool> IsUsernameExistAsync(string username, int? excludeId = null);
    Task AddAsync(User user);
    void Update(User user);
    void Remove(User user);
    Task<int> CountAsync();
    Task<int> CountAdminsAsync();
    Task<int> CountByRoleAsync(string role);
    Task<bool> SaveAsync();

    // Method baru
    Task<User?> GetUserWithKartuByUidAsync(string kartuUid);
    Task<IEnumerable<User>> GetUsersWithoutKartuAsync();
}using testing.Models;

namespace testing.Repositories;

public interface IAksesLogRepository
{
    Task<AksesLog?> GetByIdAsync(int id);
    Task<IEnumerable<AksesLog>> GetAllAsync();
    Task<IEnumerable<AksesLog>> GetPagedAsync(int page, int pageSize);
    Task<IEnumerable<AksesLog>> GetByKartuIdAsync(int kartuId);
    Task<IEnumerable<AksesLog>> GetByRuanganIdAsync(int ruanganId);
    Task<IEnumerable<AksesLog>> GetLatestAsync(int count);
    Task<AksesLog?> GetActiveLogByKartuIdAsync(int kartuId);
    Task<bool> AnyByKartuIdAsync(int kartuId);
    Task<bool> AnyByRuanganIdAsync(int ruanganId);
    Task AddAsync(AksesLog aksesLog);
    void Update(AksesLog aksesLog);
    Task<int> CountAsync();
    Task<int> CountByRuanganIdAsync(int ruanganId);
    Task<int> CountActiveByRuanganIdAsync(int ruanganId);
    Task<int> CountByDateRangeAsync(DateTime start, DateTime end);

    // Method tambahan
    Task<IEnumerable<AksesLog>> GetByDateRangeAsync(DateTime start, DateTime end);
    Task<bool> SaveAsync();

    // Method untuk stats
    Task<int> CountByKelasIdAsync(int kelasId);
    Task<int> CountActiveByKelasIdAsync(int kelasId);
    Task<int> CountByRuanganIdAndDateAsync(int ruanganId, DateTime date);
}using Microsoft.EntityFrameworkCore;
using testing.Data;
using testing.Models;

namespace testing.Repositories;

public class KartuRepository : IKartuRepository
{
    private readonly LabDbContext _context;

    public KartuRepository(LabDbContext context)
    {
        _context = context;
    }

    public async Task<Kartu?> GetByIdAsync(int id)
    {
        return await _context.Kartu
            .Include(k => k.User)
            .Include(k => k.Kelas)
            .AsNoTracking()
            .FirstOrDefaultAsync(k => k.Id == id);
    }

    public async Task<Kartu?> GetByUidAsync(string uid)
    {
        return await _context.Kartu
            .Include(k => k.User)
            .Include(k => k.Kelas)
            .AsNoTracking()
            .FirstOrDefaultAsync(k => k.Uid == uid);
    }

    public async Task<Kartu?> GetByUserIdAsync(int userId)
    {
        return await _context.Kartu
            .Include(k => k.User)
            .Include(k => k.Kelas)
            .AsNoTracking()
            .FirstOrDefaultAsync(k => k.UserId == userId);
    }

    public async Task<Kartu?> GetByKelasIdAsync(int kelasId)
    {
        return await _context.Kartu
            .Include(k => k.User)
            .Include(k => k.Kelas)
            .AsNoTracking()
            .FirstOrDefaultAsync(k => k.KelasId == kelasId);
    }

    public async Task<IEnumerable<Kartu>> GetAllAsync()
    {
        return await _context.Kartu
            .Include(k => k.User)
            .Include(k => k.Kelas)
            .AsNoTracking()
            .OrderByDescending(k => k.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Kartu>> GetPagedAsync(int page, int pageSize)
    {
        return await _context.Kartu
            .Include(k => k.User)
            .Include(k => k.Kelas)
            .AsNoTracking()
            .OrderByDescending(k => k.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<bool> IsUidExistAsync(string uid, int? excludeId = null)
    {
        return await _context.Kartu
            .AnyAsync(k => k.Uid == uid && (excludeId == null || k.Id != excludeId));
    }

    public async Task<bool> IsUserHasCardAsync(int userId)
    {
        return await _context.Kartu
            .AnyAsync(k => k.UserId == userId);
    }

    public async Task<bool> IsKelasHasCardAsync(int kelasId)
    {
        return await _context.Kartu
            .AnyAsync(k => k.KelasId == kelasId);
    }

    public async Task AddAsync(Kartu kartu)
    {
        await _context.Kartu.AddAsync(kartu);
    }

    public void Update(Kartu kartu)
    {
        _context.Kartu.Update(kartu);
    }

    public void Remove(Kartu kartu)
    {
        _context.Kartu.Remove(kartu);
    }

    public async Task<int> CountAsync()
    {
        return await _context.Kartu.CountAsync();
    }

    public async Task<bool> SaveAsync()
    {
        return await _context.SaveChangesAsync() > 0;
    }
}using testing.Models;

namespace testing.Repositories;

public interface IKartuRepository
{
    Task<Kartu?> GetByIdAsync(int id);
    Task<Kartu?> GetByUidAsync(string uid);
    Task<Kartu?> GetByUserIdAsync(int userId);
    Task<Kartu?> GetByKelasIdAsync(int kelasId);
    Task<IEnumerable<Kartu>> GetAllAsync();
    Task<IEnumerable<Kartu>> GetPagedAsync(int page, int pageSize);
    Task<bool> IsUidExistAsync(string uid, int? excludeId = null);
    Task<bool> IsUserHasCardAsync(int userId);
    Task<bool> IsKelasHasCardAsync(int kelasId);
    Task AddAsync(Kartu kartu);
    void Update(Kartu kartu);
    void Remove(Kartu kartu);
    Task<int> CountAsync();
    Task<bool> SaveAsync();
}using Microsoft.EntityFrameworkCore;
using testing.Data;
using testing.Models;

namespace testing.Repositories;

public class KelasRepository : IKelasRepository
{
    private readonly LabDbContext _context;

    public KelasRepository(LabDbContext context)
    {
        _context = context;
    }

    public async Task<Kelas?> GetByIdAsync(int id)
    {
        return await _context.Kelas
            .AsNoTracking()
            .FirstOrDefaultAsync(k => k.Id == id);
    }

    public async Task<IEnumerable<Kelas>> GetAllAsync()
    {
        return await _context.Kelas
            .AsNoTracking()
            .OrderBy(k => k.Nama)
            .ToListAsync();
    }

    public async Task<bool> IsNamaExistAsync(string nama, int? excludeId = null)
    {
        return await _context.Kelas
            .AnyAsync(k => k.Nama.ToLower() == nama.ToLower() && (excludeId == null || k.Id != excludeId));
    }

    public async Task AddAsync(Kelas kelas)
    {
        await _context.Kelas.AddAsync(kelas);
    }

    public void Update(Kelas kelas)
    {
        _context.Kelas.Update(kelas);
    }

    public void Remove(Kelas kelas)
    {
        _context.Kelas.Remove(kelas);
    }

    public async Task<int> CountAsync()
    {
        return await _context.Kelas.CountAsync();
    }

    public async Task<bool> SaveAsync()
    {
        return await _context.SaveChangesAsync() > 0;
    }
}using testing.Models;

namespace testing.Repositories;

public interface IKelasRepository
{
    Task<Kelas?> GetByIdAsync(int id);
    Task<IEnumerable<Kelas>> GetAllAsync();
    Task<bool> IsNamaExistAsync(string nama, int? excludeId = null);
    Task AddAsync(Kelas kelas);
    void Update(Kelas kelas);
    void Remove(Kelas kelas);
    Task<int> CountAsync();
    Task<bool> SaveAsync();
}using Microsoft.EntityFrameworkCore;
using testing.Data;
using testing.Models;

namespace testing.Repositories;

public class RuanganRepository : IRuanganRepository
{
    private readonly LabDbContext _context;

    public RuanganRepository(LabDbContext context)
    {
        _context = context;
    }

    public async Task<Ruangan?> GetByIdAsync(int id)
    {
        return await _context.Ruangan
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task<Ruangan?> GetByIdWithAksesLogsAsync(int id)
    {
        return await _context.Ruangan
            .Include(r => r.AksesLogs)
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task<IEnumerable<Ruangan>> GetAllAsync()
    {
        return await _context.Ruangan
            .AsNoTracking()
            .OrderBy(r => r.Nama)
            .ToListAsync();
    }

    public async Task<bool> IsNamaExistAsync(string nama, int? excludeId = null)
    {
        return await _context.Ruangan
            .AnyAsync(r => r.Nama.ToLower() == nama.ToLower() && (excludeId == null || r.Id != excludeId));
    }

    public async Task AddAsync(Ruangan ruangan)
    {
        await _context.Ruangan.AddAsync(ruangan);
    }

    public void Update(Ruangan ruangan)
    {
        _context.Ruangan.Update(ruangan);
    }

    public void Remove(Ruangan ruangan)
    {
        _context.Ruangan.Remove(ruangan);
    }

    public async Task<int> CountAsync()
    {
        return await _context.Ruangan.CountAsync();
    }

    public async Task<bool> SaveAsync()
    {
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<int> GetTotalAksesCountAsync(int ruanganId)
    {
        return await _context.AksesLog
            .CountAsync(a => a.RuanganId == ruanganId);
    }

    public async Task<int> GetActiveAksesCountAsync(int ruanganId)
    {
        return await _context.AksesLog
            .CountAsync(a => a.RuanganId == ruanganId && a.TimestampKeluar == null);
    }
}using testing.Models;

namespace testing.Repositories;

public interface IRuanganRepository
{
    Task<Ruangan?> GetByIdAsync(int id);
    Task<Ruangan?> GetByIdWithAksesLogsAsync(int id);
    Task<IEnumerable<Ruangan>> GetAllAsync();
    Task<bool> IsNamaExistAsync(string nama, int? excludeId = null);
    Task AddAsync(Ruangan ruangan);
    void Update(Ruangan ruangan);
    void Remove(Ruangan ruangan);
    Task<int> CountAsync();
    Task<bool> SaveAsync();
    Task<int> GetTotalAksesCountAsync(int ruanganId);
    Task<int> GetActiveAksesCountAsync(int ruanganId);
}using Microsoft.EntityFrameworkCore;
using testing.Data;
using testing.Models;

namespace testing.Repositories;

public class UserRepository : IUserRepository
{
    private readonly LabDbContext _context;

    public UserRepository(LabDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetByIdAsync(int id)
    {
        return await _context.Users
            .Include(u => u.Kartu)  // Include kartu yang dimiliki user
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == id);
    }

    public async Task<User?> GetByUsernameAsync(string username)
    {
        return await _context.Users
            .Include(u => u.Kartu)  // Include kartu yang dimiliki user
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Username.ToLower() == username.ToLower());
    }

    public async Task<User?> GetByKartuUidAsync(string kartuUid)
    {
        // Sekarang mencari melalui relasi Kartu
        return await _context.Users
            .Include(u => u.Kartu)
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Kartu != null && u.Kartu.Any(k => k.Uid == kartuUid));
    }

    public async Task<IEnumerable<User>> GetAllAsync()
    {
        return await _context.Users
            .Include(u => u.Kartu)  // Include kartu yang dimiliki user
            .AsNoTracking()
            .OrderBy(u => u.Username)
            .ToListAsync();
    }

    public async Task<IEnumerable<User>> GetPagedAsync(int page, int pageSize)
    {
        return await _context.Users
            .Include(u => u.Kartu)
            .AsNoTracking()
            .OrderBy(u => u.Username)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<bool> IsUsernameExistAsync(string username, int? excludeId = null)
    {
        return await _context.Users
            .AnyAsync(u => u.Username.ToLower() == username.ToLower() && (excludeId == null || u.Id != excludeId));
    }

    // Hapus method IsKartuUidExistAsync karena sekarang relasi ada di tabel Kartu
    // public async Task<bool> IsKartuUidExistAsync(string kartuUid, int? excludeId = null)

    public async Task AddAsync(User user)
    {
        await _context.Users.AddAsync(user);
    }

    public void Update(User user)
    {
        _context.Users.Update(user);
    }

    public void Remove(User user)
    {
        _context.Users.Remove(user);
    }

    public async Task<int> CountAsync()
    {
        return await _context.Users.CountAsync();
    }

    public async Task<int> CountAdminsAsync()
    {
        return await _context.Users
            .CountAsync(u => u.Role == "admin");
    }

    public async Task<int> CountByRoleAsync(string role)
    {
        return await _context.Users
            .CountAsync(u => u.Role == role);
    }

    public async Task<bool> SaveAsync()
    {
        return await _context.SaveChangesAsync() > 0;
    }

    // Method baru untuk mendapatkan user dengan kartu tertentu
    public async Task<User?> GetUserWithKartuByUidAsync(string kartuUid)
    {
        return await _context.Users
            .Include(u => u.Kartu)
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Kartu != null && u.Kartu.Any(k => k.Uid == kartuUid));
    }

    // Method untuk mendapatkan user yang tidak memiliki kartu
    public async Task<IEnumerable<User>> GetUsersWithoutKartuAsync()
    {
        return await _context.Users
            .Where(u => u.Kartu == null || !u.Kartu.Any())
            .AsNoTracking()
            .OrderBy(u => u.Username)
            .ToListAsync();
    }
}
D:\sakat\testing\Repositories>cd ..

D:\sakat\testing>ls
Controllers        Properties                    db
DTOs               Repositories                  obj
Data               Services                      rebuild.bat
Hubs               Validators                    run.bat
MappingProfile.cs  add.bat                       testing.csproj
Middleware         api                           testing.http
Migrations         appsettings.Development.json  testing.sln
Models             appsettings.json
Program.cs         bin

D:\sakat\testing>cd services

D:\sakat\testing\Services>ls
AksesLogService.cs   IScanService.cs  RuanganService.cs
IAksesLogService.cs  ITapService.cs   ScanService.cs
IKartuService.cs     IUserService.cs  TapService.cs
IKelasService.cs     KartuService.cs  UserService.cs
IRuanganService.cs   KelasService.cs

D:\sakat\testing\Services>cat AksesLogService.cs   IScanService.cs  RuanganService.cs IAksesLogService.cs  ITapService.cs   ScanService.cs IKartuService.cs     IUserService.cs  TapService.cs IKelasService.cs     KartuService.cs  UserService.cs IRuanganService.cs   KelasService.cs
using AutoMapper;
using testing.DTOs;
using testing.Models;
using testing.Repositories;

namespace testing.Services;

public class AksesLogService : IAksesLogService
{
    private readonly IAksesLogRepository _aksesLogRepository;
    private readonly IKartuRepository _kartuRepository;
    private readonly IKelasRepository _kelasRepository;
    private readonly IRuanganRepository _ruanganRepository;
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<AksesLogService> _logger;

    public AksesLogService(
        IAksesLogRepository aksesLogRepository,
        IKartuRepository kartuRepository,
        IKelasRepository kelasRepository,
        IRuanganRepository ruanganRepository,
        IUserRepository userRepository,
        IMapper mapper,
        ILogger<AksesLogService> logger)
    {
        _aksesLogRepository = aksesLogRepository;
        _kartuRepository = kartuRepository;
        _kelasRepository = kelasRepository;
        _ruanganRepository = ruanganRepository;
        _userRepository = userRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<ApiResponse<List<AksesLogDto>>> GetAllAksesLog()
    {
        try
        {
            var aksesLogs = await _aksesLogRepository.GetAllAsync();
            var aksesLogDtos = _mapper.Map<List<AksesLogDto>>(aksesLogs);
            return ApiResponse<List<AksesLogDto>>.SuccessResult(aksesLogDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all akses log");
            return ApiResponse<List<AksesLogDto>>.ErrorResult("Gagal mengambil data akses log");
        }
    }

    public async Task<ApiResponse<PagedResponse<AksesLogDto>>> GetAksesLogPaged(PagedRequest request)
    {
        try
        {
            if (!request.IsValid())
            {
                return ApiResponse<PagedResponse<AksesLogDto>>.ErrorResult("Parameter pagination tidak valid");
            }

            var aksesLogs = await _aksesLogRepository.GetPagedAsync(request.Page, request.PageSize);
            var totalCount = await _aksesLogRepository.CountAsync();
            var aksesLogDtos = _mapper.Map<List<AksesLogDto>>(aksesLogs);

            var pagedResponse = new PagedResponse<AksesLogDto>(
                aksesLogDtos,
                request.Page,
                request.PageSize,
                totalCount
            );

            return ApiResponse<PagedResponse<AksesLogDto>>.SuccessResult(pagedResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving paged akses log");
            return ApiResponse<PagedResponse<AksesLogDto>>.ErrorResult("Gagal mengambil data akses log");
        }
    }

    public async Task<ApiResponse<AksesLogDto>> GetAksesLogById(int id)
    {
        try
        {
            var aksesLog = await _aksesLogRepository.GetByIdAsync(id);
            if (aksesLog == null)
            {
                return ApiResponse<AksesLogDto>.ErrorResult("Akses log tidak ditemukan");
            }

            var aksesLogDto = _mapper.Map<AksesLogDto>(aksesLog);
            return ApiResponse<AksesLogDto>.SuccessResult(aksesLogDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving akses log by id: {Id}", id);
            return ApiResponse<AksesLogDto>.ErrorResult("Gagal mengambil data akses log");
        }
    }

    public async Task<ApiResponse<List<AksesLogDto>>> GetAksesLogByKartuId(int kartuId)
    {
        try
        {
            var aksesLogs = await _aksesLogRepository.GetByKartuIdAsync(kartuId);
            var aksesLogDtos = _mapper.Map<List<AksesLogDto>>(aksesLogs);
            return ApiResponse<List<AksesLogDto>>.SuccessResult(aksesLogDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving akses log by kartu id: {KartuId}", kartuId);
            return ApiResponse<List<AksesLogDto>>.ErrorResult("Gagal mengambil data akses log");
        }
    }

    public async Task<ApiResponse<List<AksesLogDto>>> GetAksesLogByRuanganId(int ruanganId)
    {
        try
        {
            var aksesLogs = await _aksesLogRepository.GetByRuanganIdAsync(ruanganId);
            var aksesLogDtos = _mapper.Map<List<AksesLogDto>>(aksesLogs);
            return ApiResponse<List<AksesLogDto>>.SuccessResult(aksesLogDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving akses log by ruangan id: {RuanganId}", ruanganId);
            return ApiResponse<List<AksesLogDto>>.ErrorResult("Gagal mengambil data akses log");
        }
    }

    public async Task<ApiResponse<List<AksesLogDto>>> GetLatestAksesLog(int count)
    {
        try
        {
            if (count <= 0 || count > 1000)
            {
                return ApiResponse<List<AksesLogDto>>.ErrorResult("Count harus antara 1 dan 1000");
            }

            var aksesLogs = await _aksesLogRepository.GetLatestAsync(count);
            var aksesLogDtos = _mapper.Map<List<AksesLogDto>>(aksesLogs);
            return ApiResponse<List<AksesLogDto>>.SuccessResult(aksesLogDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving latest akses log: {Count}", count);
            return ApiResponse<List<AksesLogDto>>.ErrorResult("Gagal mengambil data akses log");
        }
    }

    public async Task<ApiResponse<AksesLogDto?>> GetActiveAksesLogByKartuId(int kartuId)
    {
        try
        {
            var aksesLog = await _aksesLogRepository.GetActiveLogByKartuIdAsync(kartuId);
            if (aksesLog == null)
            {
                return ApiResponse<AksesLogDto?>.SuccessResult(null, "Tidak ada akses log aktif");
            }

            var aksesLogDto = _mapper.Map<AksesLogDto>(aksesLog);
            return ApiResponse<AksesLogDto?>.SuccessResult(aksesLogDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving active akses log by kartu id: {KartuId}", kartuId);
            return ApiResponse<AksesLogDto?>.ErrorResult("Gagal mengambil data akses log");
        }
    }

    public async Task<ApiResponse<DashboardStatsDto>> GetDashboardStats()
    {
        try
        {
            var totalAkses = await _aksesLogRepository.CountAsync();

            var semuaAkses = await _aksesLogRepository.GetAllAsync();
            var aktifSekarang = semuaAkses.Count(a => a.TimestampKeluar == null);

            var totalKartu = await _kartuRepository.CountAsync();
            var totalKelas = await _kelasRepository.CountAsync();
            var totalRuangan = await _ruanganRepository.CountAsync();
            var totalUsers = await _userRepository.CountAsync();

            var stats = new DashboardStatsDto
            {
                TotalAkses = totalAkses,
                AktifSekarang = aktifSekarang,
                TotalKartu = totalKartu,
                TotalKelas = totalKelas,
                TotalRuangan = totalRuangan,
                TotalUsers = totalUsers
            };

            return ApiResponse<DashboardStatsDto>.SuccessResult(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting dashboard stats");
            return ApiResponse<DashboardStatsDto>.ErrorResult("Gagal mengambil statistik dashboard");
        }
    }

    public async Task<ApiResponse<TodayStatsDto>> GetTodayStats()
    {
        try
        {
            var hariIni = DateTime.Today;
            var besok = hariIni.AddDays(1);

            var aksesHariIni = await _aksesLogRepository.GetByDateRangeAsync(hariIni, besok);

            var checkinHariIni = aksesHariIni.Count();
            var checkoutHariIni = aksesHariIni.Count(a => a.TimestampKeluar.HasValue);
            var masihAktif = aksesHariIni.Count(a => !a.TimestampKeluar.HasValue);

            var kartuAktif = aksesHariIni
                .Where(a => a.Kartu != null)
                .GroupBy(a => a.Kartu!.Uid)
                .Select(g => new { KartuUID = g.Key, Jumlah = g.Count() })
                .OrderByDescending(x => x.Jumlah)
                .Take(5)
                .ToList();

            var stats = new TodayStatsDto
            {
                Tanggal = hariIni.ToString("yyyy-MM-dd"),
                TotalCheckin = checkinHariIni,
                TotalCheckout = checkoutHariIni,
                MasihAktif = masihAktif,
                KartuPalingAktif = kartuAktif
            };

            return ApiResponse<TodayStatsDto>.SuccessResult(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting today's stats");
            return ApiResponse<TodayStatsDto>.ErrorResult("Gagal mengambil statistik hari ini");
        }
    }
}using testing.DTOs;

namespace testing.Services;

public interface IScanService
{
    Task<ApiResponse<ScanResponse>> RegisterCard(ScanRequest request);
    Task<ApiResponse<ScanCheckResponse>> CheckCard(ScanCheckRequest request);
    Task<ApiResponse<List<object>>> GetKartuTerdaftar();
    Task<ApiResponse<object>> DeleteKartu(int id);
}

public class ScanRequest
{
    public required string Uid { get; set; }
}

public class ScanResponse
{
    public bool Success { get; set; }
    public required string Status { get; set; }
    public required string Message { get; set; }
    public string? Uid { get; set; }
    public string? Timestamp { get; set; }
}

public class ScanCheckRequest
{
    public required string Uid { get; set; }
}

public class ScanCheckResponse
{
    public bool Success { get; set; }
    public required string Status { get; set; }
    public required string Message { get; set; }
    public string? Uid { get; set; }
    public bool Terdaftar { get; set; }
    public string? StatusKartu { get; set; }
}using AutoMapper;
using testing.DTOs;
using testing.Models;
using testing.Repositories;

namespace testing.Services;

public class RuanganService : IRuanganService
{
    private readonly IRuanganRepository _ruanganRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<RuanganService> _logger;

    public RuanganService(
        IRuanganRepository ruanganRepository,
        IMapper mapper,
        ILogger<RuanganService> logger)
    {
        _ruanganRepository = ruanganRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<ApiResponse<List<RuanganDto>>> GetAllRuangan()
    {
        try
        {
            var ruanganList = await _ruanganRepository.GetAllAsync();
            var ruanganDtos = _mapper.Map<List<RuanganDto>>(ruanganList);
            return ApiResponse<List<RuanganDto>>.SuccessResult(ruanganDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all ruangan");
            return ApiResponse<List<RuanganDto>>.ErrorResult("Gagal mengambil data ruangan");
        }
    }

    public async Task<ApiResponse<RuanganDto>> GetRuanganById(int id)
    {
        try
        {
            var ruangan = await _ruanganRepository.GetByIdAsync(id);
            if (ruangan == null)
            {
                return ApiResponse<RuanganDto>.ErrorResult("Ruangan tidak ditemukan");
            }

            var ruanganDto = _mapper.Map<RuanganDto>(ruangan);
            return ApiResponse<RuanganDto>.SuccessResult(ruanganDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving ruangan by id: {Id}", id);
            return ApiResponse<RuanganDto>.ErrorResult("Gagal mengambil data ruangan");
        }
    }

    public async Task<ApiResponse<RuanganDto>> CreateRuangan(RuanganCreateRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Nama))
            {
                return ApiResponse<RuanganDto>.ErrorResult("Nama ruangan harus diisi");
            }

            var existingRuangan = await _ruanganRepository.IsNamaExistAsync(request.Nama);
            if (existingRuangan)
            {
                return ApiResponse<RuanganDto>.ErrorResult("Ruangan dengan nama tersebut sudah terdaftar");
            }

            var ruangan = _mapper.Map<Ruangan>(request);
            await _ruanganRepository.AddAsync(ruangan);
            var saved = await _ruanganRepository.SaveAsync();

            if (!saved)
            {
                return ApiResponse<RuanganDto>.ErrorResult("Gagal menyimpan ruangan");
            }

            _logger.LogInformation("Ruangan created: {Nama}", ruangan.Nama);

            var ruanganDto = _mapper.Map<RuanganDto>(ruangan);
            return ApiResponse<RuanganDto>.SuccessResult(ruanganDto, "Ruangan berhasil ditambahkan");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating ruangan: {Nama}", request.Nama);
            return ApiResponse<RuanganDto>.ErrorResult("Gagal membuat ruangan");
        }
    }

    public async Task<ApiResponse<RuanganDto>> UpdateRuangan(int id, RuanganUpdateRequest request)
    {
        try
        {
            var ruangan = await _ruanganRepository.GetByIdAsync(id);
            if (ruangan == null)
            {
                return ApiResponse<RuanganDto>.ErrorResult("Ruangan tidak ditemukan");
            }

            if (string.IsNullOrWhiteSpace(request.Nama))
            {
                return ApiResponse<RuanganDto>.ErrorResult("Nama ruangan harus diisi");
            }

            var existingRuangan = await _ruanganRepository.IsNamaExistAsync(request.Nama, id);
            if (existingRuangan)
            {
                return ApiResponse<RuanganDto>.ErrorResult("Ruangan dengan nama tersebut sudah terdaftar");
            }

            _mapper.Map(request, ruangan);
            _ruanganRepository.Update(ruangan);
            var saved = await _ruanganRepository.SaveAsync();

            if (!saved)
            {
                return ApiResponse<RuanganDto>.ErrorResult("Gagal mengupdate ruangan");
            }

            _logger.LogInformation("Ruangan updated: {Id} - {Nama}", ruangan.Id, ruangan.Nama);

            var ruanganDto = _mapper.Map<RuanganDto>(ruangan);
            return ApiResponse<RuanganDto>.SuccessResult(ruanganDto, "Ruangan berhasil diupdate");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating ruangan: {Id}", id);
            return ApiResponse<RuanganDto>.ErrorResult("Gagal mengupdate ruangan");
        }
    }

    public async Task<ApiResponse<object>> DeleteRuangan(int id)
    {
        try
        {
            var ruangan = await _ruanganRepository.GetByIdWithAksesLogsAsync(id);
            if (ruangan == null)
            {
                return ApiResponse<object>.ErrorResult("Ruangan tidak ditemukan");
            }

            if (ruangan.AksesLogs != null && ruangan.AksesLogs.Any())
            {
                return ApiResponse<object>.ErrorResult("Tidak dapat menghapus ruangan karena memiliki riwayat akses");
            }

            _ruanganRepository.Remove(ruangan);
            var saved = await _ruanganRepository.SaveAsync();

            if (!saved)
            {
                return ApiResponse<object>.ErrorResult("Gagal menghapus ruangan");
            }

            _logger.LogInformation("Ruangan deleted: {Id} - {Nama}", ruangan.Id, ruangan.Nama);
            return ApiResponse<object>.SuccessResult(null!, "Ruangan berhasil dihapus"); // Fixed: menggunakan null!
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting ruangan: {Id}", id);
            return ApiResponse<object>.ErrorResult("Gagal menghapus ruangan");
        }
    }

    public async Task<ApiResponse<RuanganStatsDto>> GetRuanganStats(int id)
    {
        try
        {
            var ruangan = await _ruanganRepository.GetByIdAsync(id);
            if (ruangan == null)
            {
                return ApiResponse<RuanganStatsDto>.ErrorResult("Ruangan tidak ditemukan");
            }

            var totalAkses = await _ruanganRepository.GetTotalAksesCountAsync(id);
            var aktifSekarang = await _ruanganRepository.GetActiveAksesCountAsync(id);

            var stats = new RuanganStatsDto
            {
                Ruangan = ruangan.Nama,
                TotalAkses = totalAkses,
                AktifSekarang = aktifSekarang
            };

            return ApiResponse<RuanganStatsDto>.SuccessResult(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting ruangan stats: {Id}", id);
            return ApiResponse<RuanganStatsDto>.ErrorResult("Gagal mengambil statistik ruangan");
        }
    }
}using testing.DTOs;

namespace testing.Services;

public interface IAksesLogService
{
    Task<ApiResponse<List<AksesLogDto>>> GetAllAksesLog();
    Task<ApiResponse<PagedResponse<AksesLogDto>>> GetAksesLogPaged(PagedRequest request);
    Task<ApiResponse<AksesLogDto>> GetAksesLogById(int id);
    Task<ApiResponse<List<AksesLogDto>>> GetAksesLogByKartuId(int kartuId);
    Task<ApiResponse<List<AksesLogDto>>> GetAksesLogByRuanganId(int ruanganId);
    Task<ApiResponse<List<AksesLogDto>>> GetLatestAksesLog(int count);
    Task<ApiResponse<AksesLogDto?>> GetActiveAksesLogByKartuId(int kartuId);
    Task<ApiResponse<DashboardStatsDto>> GetDashboardStats();
    Task<ApiResponse<TodayStatsDto>> GetTodayStats();
}using testing.DTOs;

namespace testing.Services;

public interface ITapService
{
    Task<ApiResponse<TapResponse>> ProcessTap(TapRequest request);
    Task<ApiResponse<List<object>>> GetLogs(int? ruanganId = null);
    Task<ApiResponse<List<object>>> GetKartu();
    Task<ApiResponse<List<object>>> GetRuangan();
    Task<ApiResponse<List<object>>> GetKelas();
    Task<ApiResponse<object>> GetStats();
    Task<ApiResponse<object>> GetStatsHariIni();
}

public class TapRequest
{
    public required string Uid { get; set; }
    public int IdRuangan { get; set; }
    public required string Timestamp { get; set; }
}

public class TapResponse
{
    public required string Status { get; set; }
    public required string Message { get; set; }
    public string? NamaKelas { get; set; }
    public string? Ruangan { get; set; }
    public string? Waktu { get; set; }
}using AutoMapper;
using Microsoft.AspNetCore.SignalR;
using testing.Hubs;
using testing.DTOs;
using testing.Models;
using testing.Repositories;

namespace testing.Services;

public class ScanService : IScanService
{
    private readonly IKartuRepository _kartuRepository;
    private readonly IAksesLogRepository _aksesLogRepository;
    private readonly IUserRepository _userRepository;
    private readonly IHubContext<LogHub> _hubContext;
    private readonly IMapper _mapper;
    private readonly ILogger<ScanService> _logger;

    public ScanService(
        IKartuRepository kartuRepository,
        IAksesLogRepository aksesLogRepository,
        IUserRepository userRepository,
        IHubContext<LogHub> hubContext,
        IMapper mapper,
        ILogger<ScanService> logger)
    {
        _kartuRepository = kartuRepository;
        _aksesLogRepository = aksesLogRepository;
        _userRepository = userRepository;
        _hubContext = hubContext;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<ApiResponse<ScanResponse>> RegisterCard(ScanRequest request)
    {
        try
        {
            _logger.LogInformation("Received card registration: UID={Uid}", request.Uid);

            if (string.IsNullOrWhiteSpace(request.Uid))
            {
                return ApiResponse<ScanResponse>.ErrorResult("UID kartu tidak boleh kosong");
            }

            var existingKartu = await _kartuRepository.GetByUidAsync(request.Uid);
            if (existingKartu != null)
            {
                _logger.LogWarning("Kartu sudah terdaftar: UID={Uid}", request.Uid);

                var response = new ScanResponse
                {
                    Success = false,
                    Status = "KARTU SUDAH TERDAFTAR",
                    Message = $"Kartu dengan UID {request.Uid} sudah terdaftar dalam sistem",
                    Uid = request.Uid
                };

                return ApiResponse<ScanResponse>.SuccessResult(response);
            }

            var kartu = new Kartu
            {
                Uid = request.Uid.Trim().ToUpper(),
                Status = "AKTIF"
            };

            await _kartuRepository.AddAsync(kartu);
            var saved = await _kartuRepository.SaveAsync();

            if (!saved)
            {
                return ApiResponse<ScanResponse>.ErrorResult("Gagal mendaftarkan kartu");
            }

            _logger.LogInformation("Kartu berhasil didaftarkan: UID={Uid}", kartu.Uid);

            var logData = new
            {
                Type = "CARD_REGISTERED",
                Uid = kartu.Uid,
                Timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                Message = $"Kartu berhasil didaftarkan: {kartu.Uid}"
            };
            await _hubContext.Clients.All.SendAsync("ReceiveCardRegistration", logData);

            var scanResponse = new ScanResponse
            {
                Success = true,
                Status = "SUKSES",
                Message = "Kartu berhasil didaftarkan",
                Uid = kartu.Uid,
                Timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
            };

            return ApiResponse<ScanResponse>.SuccessResult(scanResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error registering card: UID={Uid}", request.Uid);
            return ApiResponse<ScanResponse>.ErrorResult("Terjadi kesalahan internal server");
        }
    }

    public async Task<ApiResponse<ScanCheckResponse>> CheckCard(ScanCheckRequest request)
    {
        try
        {
            _logger.LogInformation("Checking card: UID={Uid}", request.Uid);

            if (string.IsNullOrWhiteSpace(request.Uid))
            {
                return ApiResponse<ScanCheckResponse>.ErrorResult("UID kartu tidak boleh kosong");
            }

            var kartu = await _kartuRepository.GetByUidAsync(request.Uid);

            if (kartu != null)
            {
                _logger.LogInformation("Kartu ditemukan: UID={Uid}", request.Uid);

                var response = new ScanCheckResponse
                {
                    Success = true,
                    Status = "KARTU TERDAFTAR",
                    Message = "Kartu sudah terdaftar dalam sistem",
                    Uid = kartu.Uid,
                    Terdaftar = true,
                    StatusKartu = kartu.Status
                };

                return ApiResponse<ScanCheckResponse>.SuccessResult(response);
            }
            else
            {
                _logger.LogInformation("Kartu tidak terdaftar: UID={Uid}", request.Uid);

                var response = new ScanCheckResponse
                {
                    Success = true,
                    Status = "KARTU TIDAK TERDAFTAR",
                    Message = "Kartu belum terdaftar dalam sistem",
                    Uid = request.Uid,
                    Terdaftar = false
                };

                return ApiResponse<ScanCheckResponse>.SuccessResult(response);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking card: UID={Uid}", request.Uid);
            return ApiResponse<ScanCheckResponse>.ErrorResult("Terjadi kesalahan internal server");
        }
    }

    public async Task<ApiResponse<List<object>>> GetKartuTerdaftar()
    {
        try
        {
            var kartuTerdaftar = await _kartuRepository.GetAllAsync();
            var result = kartuTerdaftar.Select(k => new
            {
                Id = k.Id,
                Uid = k.Uid,
                Status = k.Status,
                Keterangan = k.Keterangan,
                TanggalDaftar = k.CreatedAt.HasValue ? k.CreatedAt.Value.ToString("yyyy-MM-dd HH:mm:ss") : "Unknown"
            }).OrderBy(k => k.Uid).Cast<object>().ToList();

            return ApiResponse<List<object>>.SuccessResult(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting kartu terdaftar");
            return ApiResponse<List<object>>.ErrorResult("Error retrieving kartu terdaftar");
        }
    }

    public async Task<ApiResponse<object>> DeleteKartu(int id)
    {
        try
        {
            var kartu = await _kartuRepository.GetByIdAsync(id);
            if (kartu == null)
            {
                return ApiResponse<object>.ErrorResult("Kartu tidak ditemukan");
            }

            var hasAccessLog = await _aksesLogRepository.AnyByKartuIdAsync(id);
            if (hasAccessLog)
            {
                return ApiResponse<object>.ErrorResult("Tidak dapat menghapus kartu karena memiliki riwayat akses");
            }

            var userWithCard = await _userRepository.GetByKartuUidAsync(kartu.Uid);
            if (userWithCard != null)
            {
                return ApiResponse<object>.ErrorResult("Tidak dapat menghapus kartu karena terdaftar pada user");
            }

            _kartuRepository.Remove(kartu);
            var saved = await _kartuRepository.SaveAsync();

            if (!saved)
            {
                return ApiResponse<object>.ErrorResult("Gagal menghapus kartu");
            }

            _logger.LogInformation("Kartu deleted: UID={Uid}", kartu.Uid);

            var logData = new
            {
                Type = "CARD_DELETED",
                Uid = kartu.Uid,
                Timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                Message = $"Kartu {kartu.Uid} berhasil dihapus"
            };
            await _hubContext.Clients.All.SendAsync("ReceiveCardRegistration", logData);

            return ApiResponse<object>.SuccessResult(null!, "Kartu berhasil dihapus");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting kartu: {Id}", id);
            return ApiResponse<object>.ErrorResult("Error deleting kartu");
        }
    }
}using testing.DTOs;

namespace testing.Services;

public interface IKartuService
{
    Task<ApiResponse<List<KartuDto>>> GetAllKartu();
    Task<ApiResponse<PagedResponse<KartuDto>>> GetKartuPaged(PagedRequest request);
    Task<ApiResponse<KartuDto>> GetKartuById(int id);
    Task<ApiResponse<KartuDto>> CreateKartu(KartuCreateDto request);
    Task<ApiResponse<KartuDto>> UpdateKartu(int id, KartuUpdateDto request);
    Task<ApiResponse<object>> DeleteKartu(int id);
    Task<ApiResponse<KartuCheckDto>> CheckCard(string uid);
}

public class KartuCheckDto
{
    public string Uid { get; set; } = string.Empty;
    public bool Terdaftar { get; set; }
    public string? Status { get; set; }
    public string? Keterangan { get; set; }
    public string Message { get; set; } = string.Empty;
    public int? UserId { get; set; }
    public int? KelasId { get; set; }
    public string? UserUsername { get; set; }
    public string? KelasNama { get; set; }
}using testing.DTOs;

namespace testing.Services;

public interface IUserService
{
    Task<ApiResponse<UserLoginResponse>> Login(UserLoginRequest request);
    Task<ApiResponse<UserDto>> GetProfile(int userId);
    Task<ApiResponse<List<UserDto>>> GetAllUsers();
    Task<ApiResponse<PagedResponse<UserDto>>> GetUsersPaged(PagedRequest request);
    Task<ApiResponse<UserDto>> GetUserById(int id);
    Task<ApiResponse<UserDto>> CreateUser(UserCreateRequest request);
    Task<ApiResponse<UserDto>> UpdateUser(int id, UserUpdateRequest request);
    Task<ApiResponse<object>> DeleteUser(int id);
    Task<ApiResponse<List<object>>> GetRoles();
    Task<ApiResponse<List<UserDto>>> GetUsersWithoutKartu();
}using AutoMapper;
using Microsoft.AspNetCore.SignalR;
using testing.Hubs;
using testing.DTOs;
using testing.Models;
using testing.Repositories;

namespace testing.Services;

public class TapService : ITapService
{
    private readonly IKartuRepository _kartuRepository;
    private readonly IRuanganRepository _ruanganRepository;
    private readonly IAksesLogRepository _aksesLogRepository;
    private readonly IKelasRepository _kelasRepository;
    private readonly IHubContext<LogHub> _hubContext;
    private readonly IMapper _mapper;
    private readonly ILogger<TapService> _logger;

    public TapService(
        IKartuRepository kartuRepository,
        IRuanganRepository ruanganRepository,
        IAksesLogRepository aksesLogRepository,
        IKelasRepository kelasRepository,
        IHubContext<LogHub> hubContext,
        IMapper mapper,
        ILogger<TapService> logger)
    {
        _kartuRepository = kartuRepository;
        _ruanganRepository = ruanganRepository;
        _aksesLogRepository = aksesLogRepository;
        _kelasRepository = kelasRepository;
        _hubContext = hubContext;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<ApiResponse<TapResponse>> ProcessTap(TapRequest request)
    {
        try
        {
            _logger.LogInformation("Received tap: UID={Uid}, Ruangan={Ruangan}, Time={Time}",
                request.Uid, request.IdRuangan, request.Timestamp);

            var kartu = await _kartuRepository.GetByUidAsync(request.Uid);
            if (kartu == null)
            {
                _logger.LogWarning("Kartu tidak terdaftar: {Uid}", request.Uid);
                return ApiResponse<TapResponse>.SuccessResult(new TapResponse
                {
                    Status = "KARTU TIDAK TERDAFTAR",
                    Message = "Kartu tidak terdaftar dalam sistem"
                });
            }

            if (kartu.Status != "AKTIF")
            {
                _logger.LogWarning("Kartu tidak aktif: {Uid} - Status: {Status}", request.Uid, kartu.Status);
                return ApiResponse<TapResponse>.SuccessResult(new TapResponse
                {
                    Status = "KARTU TIDAK AKTIF",
                    Message = $"Kartu tidak aktif. Status: {kartu.Status}"
                });
            }

            var ruangan = await _ruanganRepository.GetByIdAsync(request.IdRuangan);
            if (ruangan == null)
            {
                return ApiResponse<TapResponse>.ErrorResult("Ruangan tidak valid");
            }

            // PERBAIKAN: Handle timestamp parsing dengan benar untuk PostgreSQL
            DateTime tapTime;
            if (!DateTime.TryParse(request.Timestamp, out tapTime))
            {
                tapTime = DateTime.UtcNow;
            }

            // PERBAIKAN KRITIS: Pastikan DateTime selalu UTC untuk PostgreSQL
            if (tapTime.Kind == DateTimeKind.Unspecified)
            {
                // Jika timezone tidak specified, assume UTC
                tapTime = DateTime.SpecifyKind(tapTime, DateTimeKind.Utc);
            }
            else if (tapTime.Kind == DateTimeKind.Local)
            {
                // Convert local time to UTC
                tapTime = tapTime.ToUniversalTime();
            }
            // Jika sudah UTC, biarkan saja

            _logger.LogInformation("Processed timestamp: Original={Original}, UTC={UtcTime}, Kind={Kind}",
                request.Timestamp, tapTime, tapTime.Kind);

            var lastAccess = await _aksesLogRepository.GetActiveLogByKartuIdAsync(kartu.Id);
            TapResponse response;

            if (lastAccess == null)
            {
                // CHECK-IN
                var aksesLog = new AksesLog
                {
                    KartuId = kartu.Id,
                    RuanganId = request.IdRuangan,
                    TimestampMasuk = tapTime, // Sudah UTC
                    Status = "CHECKIN"
                };

                await _aksesLogRepository.AddAsync(aksesLog);
                var saved = await _aksesLogRepository.SaveAsync();

                if (!saved)
                {
                    _logger.LogError("Gagal menyimpan data check-in untuk kartu {KartuId}", kartu.Id);
                    return ApiResponse<TapResponse>.ErrorResult("Gagal melakukan check-in");
                }

                response = new TapResponse
                {
                    Status = "SUKSES CHECK-IN",
                    Message = "Check-in berhasil",
                    Ruangan = ruangan.Nama,
                    Waktu = tapTime.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss")
                };

                _logger.LogInformation("Check-in berhasil: Kartu {Uid} di {Ruangan}", kartu.Uid, ruangan.Nama);

                var logData = new
                {
                    Id = aksesLog.Id,
                    KartuUid = kartu.Uid,
                    Ruangan = ruangan.Nama,
                    Masuk = aksesLog.TimestampMasuk.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss"),
                    Keluar = (string?)null,
                    Status = aksesLog.Status,
                    Durasi = "Masih aktif"
                };
                await _hubContext.Clients.All.SendAsync("ReceiveNewLog", logData);
            }
            else
            {
                // CHECK-OUT
                if (lastAccess.RuanganId != request.IdRuangan)
                {
                    var ruanganCheckIn = await _ruanganRepository.GetByIdAsync(lastAccess.RuanganId);
                    var ruanganNama = ruanganCheckIn?.Nama ?? "Unknown";

                    response = new TapResponse
                    {
                        Status = "ERROR: BEDA LAB",
                        Message = $"Check-out harus dilakukan di ruangan yang sama dengan check-in (Ruangan: {ruanganNama})",
                        Ruangan = ruangan.Nama
                    };

                    _logger.LogWarning("Check-out ditolak: Kartu {Uid} mencoba check-out di {RuanganSekarang} sedangkan check-in di {RuanganCheckIn}",
                        kartu.Uid, ruangan.Nama, ruanganNama);
                }
                else
                {
                    _logger.LogInformation("Memproses check-out: LogId={LogId}, Kartu={Uid}, Ruangan={Ruangan}",
                        lastAccess.Id, kartu.Uid, ruangan.Nama);

                    // PERBAIKAN KRITIS: Pastikan TimestampMasuk yang sudah ada juga UTC
                    // Karena data yang sudah ada di database mungkin memiliki DateTimeKind.Unspecified
                    if (lastAccess.TimestampMasuk.Kind == DateTimeKind.Unspecified)
                    {
                        _logger.LogWarning("Converting existing TimestampMasuk from Unspecified to UTC");
                        lastAccess.TimestampMasuk = DateTime.SpecifyKind(lastAccess.TimestampMasuk, DateTimeKind.Utc);
                    }

                    lastAccess.TimestampKeluar = tapTime;
                    lastAccess.Status = "CHECKOUT";

                    _aksesLogRepository.Update(lastAccess);
                    var saved = await _aksesLogRepository.SaveAsync();

                    if (!saved)
                    {
                        _logger.LogError("Gagal menyimpan data check-out: LogId={LogId}, Kartu={Uid}",
                            lastAccess.Id, kartu.Uid);
                        return ApiResponse<TapResponse>.ErrorResult("Gagal melakukan check-out: Data tidak dapat disimpan");
                    }

                    response = new TapResponse
                    {
                        Status = "SUKSES CHECK-OUT",
                        Message = "Check-out berhasil",
                        Ruangan = ruangan.Nama,
                        Waktu = tapTime.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss")
                    };

                    _logger.LogInformation("Check-out berhasil: Kartu {Uid} di {Ruangan}", kartu.Uid, ruangan.Nama);

                    // PERBAIKAN: Handle nullable timestamp dengan benar
                    var durasi = lastAccess.TimestampKeluar.HasValue ?
                                (lastAccess.TimestampKeluar.Value - lastAccess.TimestampMasuk).TotalMinutes.ToString("F1") + " menit" :
                                "0 menit";

                    var logData = new
                    {
                        Id = lastAccess.Id,
                        KartuUid = kartu.Uid,
                        Ruangan = ruangan.Nama,
                        Masuk = lastAccess.TimestampMasuk.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss"),
                        Keluar = lastAccess.TimestampKeluar?.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss"),
                        Status = lastAccess.Status,
                        Durasi = durasi
                    };
                    await _hubContext.Clients.All.SendAsync("ReceiveUpdatedLog", logData);
                }
            }

            return ApiResponse<TapResponse>.SuccessResult(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing tap: UID={Uid}, Ruangan={Ruangan}", request.Uid, request.IdRuangan);
            return ApiResponse<TapResponse>.ErrorResult("Terjadi kesalahan internal server");
        }
    }

    public async Task<ApiResponse<List<object>>> GetLogs(int? ruanganId = null)
    {
        try
        {
            IEnumerable<AksesLog> logs;
            if (ruanganId.HasValue)
            {
                logs = await _aksesLogRepository.GetByRuanganIdAsync(ruanganId.Value);
            }
            else
            {
                logs = await _aksesLogRepository.GetLatestAsync(50);
            }

            var result = logs.Select(a => new
            {
                Id = a.Id,
                KartuUid = a.Kartu?.Uid ?? "Unknown",
                Ruangan = a.Ruangan?.Nama ?? "Unknown",
                Masuk = a.TimestampMasuk.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss"),
                Keluar = a.TimestampKeluar.HasValue ? a.TimestampKeluar.Value.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss") : null,
                Status = a.Status,
                Durasi = a.TimestampKeluar.HasValue ?
                         (a.TimestampKeluar.Value - a.TimestampMasuk).TotalMinutes.ToString("F1") + " menit" :
                         "Masih aktif"
            }).Cast<object>().ToList();

            return ApiResponse<List<object>>.SuccessResult(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting logs");
            return ApiResponse<List<object>>.ErrorResult("Error retrieving logs");
        }
    }

    public async Task<ApiResponse<List<object>>> GetKartu()
    {
        try
        {
            var kartuList = await _kartuRepository.GetAllAsync();
            var result = kartuList.Select(k => new
            {
                Id = k.Id,
                UID = k.Uid,
                Status = k.Status,
                Keterangan = k.Keterangan
            }).Cast<object>().ToList();

            return ApiResponse<List<object>>.SuccessResult(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting kartu list");
            return ApiResponse<List<object>>.ErrorResult("Error retrieving kartu list");
        }
    }

    public async Task<ApiResponse<List<object>>> GetRuangan()
    {
        try
        {
            var ruanganList = await _ruanganRepository.GetAllAsync();
            var result = ruanganList.Select(r => new
            {
                Id = r.Id,
                Nama = r.Nama
            }).Cast<object>().ToList();

            return ApiResponse<List<object>>.SuccessResult(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting ruangan list");
            return ApiResponse<List<object>>.ErrorResult("Error retrieving ruangan list");
        }
    }

    public async Task<ApiResponse<List<object>>> GetKelas()
    {
        try
        {
            var kelasList = await _kelasRepository.GetAllAsync();
            var result = kelasList.Select(k => new
            {
                Id = k.Id,
                Nama = k.Nama
            }).Cast<object>().ToList();

            return ApiResponse<List<object>>.SuccessResult(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting kelas list");
            return ApiResponse<List<object>>.ErrorResult("Error retrieving kelas list");
        }
    }

    public async Task<ApiResponse<object>> GetStats()
    {
        try
        {
            var totalAkses = await _aksesLogRepository.CountAsync();
            var aktifSekarang = await _aksesLogRepository.GetLatestAsync(1000)
                .ContinueWith(t => t.Result.Count(a => a.TimestampKeluar == null));
            var totalKartu = await _kartuRepository.CountAsync();
            var totalKelas = await _kelasRepository.CountAsync();

            var stats = new
            {
                TotalAkses = totalAkses,
                AktifSekarang = aktifSekarang,
                TotalKartu = totalKartu,
                TotalKelas = totalKelas
            };

            return ApiResponse<object>.SuccessResult(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting stats");
            return ApiResponse<object>.ErrorResult("Error retrieving stats");
        }
    }

    public async Task<ApiResponse<object>> GetStatsHariIni()
    {
        try
        {
            var hariIni = DateTime.Today;
            var besok = hariIni.AddDays(1);

            var semuaAkses = await _aksesLogRepository.GetAllAsync();
            var aksesHariIni = semuaAkses.Where(a => a.TimestampMasuk >= hariIni && a.TimestampMasuk < besok).ToList();

            var checkinHariIni = aksesHariIni.Count;
            var checkoutHariIni = aksesHariIni.Count(a => a.TimestampKeluar.HasValue);
            var masihAktif = aksesHariIni.Count(a => !a.TimestampKeluar.HasValue);

            var kartuAktif = aksesHariIni
                .GroupBy(a => a.Kartu?.Uid ?? "Unknown")
                .Select(g => new { KartuUID = g.Key, Jumlah = g.Count() })
                .OrderByDescending(x => x.Jumlah)
                .Take(5)
                .ToList();

            var stats = new
            {
                Tanggal = hariIni.ToString("yyyy-MM-dd"),
                TotalCheckin = checkinHariIni,
                TotalCheckout = checkoutHariIni,
                MasihAktif = masihAktif,
                KartuPalingAktif = kartuAktif
            };

            return ApiResponse<object>.SuccessResult(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting today's stats");
            return ApiResponse<object>.ErrorResult("Error retrieving today's stats");
        }
    }
}using testing.DTOs;

namespace testing.Services;

public interface IKelasService
{
    Task<ApiResponse<List<KelasDto>>> GetAllKelas();
    Task<ApiResponse<KelasDto>> GetKelasById(int id);
    Task<ApiResponse<KelasDto>> CreateKelas(KelasCreateRequest request);
    Task<ApiResponse<KelasDto>> UpdateKelas(int id, KelasUpdateRequest request);
    Task<ApiResponse<object>> DeleteKelas(int id);
    Task<ApiResponse<KelasStatsDto>> GetKelasStats(int id);
}

// public class KelasStatsDto
// {
//     public string Kelas { get; set; } = string.Empty;
//     public int TotalAkses { get; set; }
//     public int AktifSekarang { get; set; }
// }using AutoMapper;
using testing.DTOs;
using testing.Models;
using testing.Repositories;

namespace testing.Services;

public class KartuService : IKartuService
{
    private readonly IKartuRepository _kartuRepository;
    private readonly IUserRepository _userRepository;
    private readonly IKelasRepository _kelasRepository;
    private readonly IAksesLogRepository _aksesLogRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<KartuService> _logger;

    public KartuService(
        IKartuRepository kartuRepository,
        IUserRepository userRepository,
        IKelasRepository kelasRepository,
        IAksesLogRepository aksesLogRepository,
        IMapper mapper,
        ILogger<KartuService> logger)
    {
        _kartuRepository = kartuRepository;
        _userRepository = userRepository;
        _kelasRepository = kelasRepository;
        _aksesLogRepository = aksesLogRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<ApiResponse<List<KartuDto>>> GetAllKartu()
    {
        try
        {
            var kartuList = await _kartuRepository.GetAllAsync();
            var kartuDtos = kartuList.Select(k => new KartuDto
            {
                Id = k.Id,
                Uid = k.Uid,
                Status = k.Status,
                Keterangan = k.Keterangan,
                UserId = k.UserId,
                KelasId = k.KelasId,
                CreatedAt = k.CreatedAt,
                UserUsername = k.User?.Username,
                KelasNama = k.Kelas?.Nama
            }).ToList();

            return ApiResponse<List<KartuDto>>.SuccessResult(kartuDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all kartu");
            return ApiResponse<List<KartuDto>>.ErrorResult("Gagal mengambil data kartu");
        }
    }

    public async Task<ApiResponse<PagedResponse<KartuDto>>> GetKartuPaged(PagedRequest request)
    {
        try
        {
            if (!request.IsValid())
            {
                return ApiResponse<PagedResponse<KartuDto>>.ErrorResult("Parameter pagination tidak valid");
            }

            var kartuList = await _kartuRepository.GetPagedAsync(request.Page, request.PageSize);
            var totalCount = await _kartuRepository.CountAsync();

            var kartuDtos = kartuList.Select(k => new KartuDto
            {
                Id = k.Id,
                Uid = k.Uid,
                Status = k.Status,
                Keterangan = k.Keterangan,
                UserId = k.UserId,
                KelasId = k.KelasId,
                CreatedAt = k.CreatedAt,
                UserUsername = k.User?.Username,
                KelasNama = k.Kelas?.Nama
            }).ToList();

            var pagedResponse = new PagedResponse<KartuDto>(
                kartuDtos,
                request.Page,
                request.PageSize,
                totalCount
            );

            return ApiResponse<PagedResponse<KartuDto>>.SuccessResult(pagedResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving paged kartu");
            return ApiResponse<PagedResponse<KartuDto>>.ErrorResult("Gagal mengambil data kartu");
        }
    }

    public async Task<ApiResponse<KartuDto>> GetKartuById(int id)
    {
        try
        {
            var kartu = await _kartuRepository.GetByIdAsync(id);
            if (kartu == null)
            {
                return ApiResponse<KartuDto>.ErrorResult("Kartu tidak ditemukan");
            }

            var kartuDto = new KartuDto
            {
                Id = kartu.Id,
                Uid = kartu.Uid,
                Status = kartu.Status,
                Keterangan = kartu.Keterangan,
                UserId = kartu.UserId,
                KelasId = kartu.KelasId,
                CreatedAt = kartu.CreatedAt,
                UserUsername = kartu.User?.Username,
                KelasNama = kartu.Kelas?.Nama
            };

            return ApiResponse<KartuDto>.SuccessResult(kartuDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving kartu by id: {Id}", id);
            return ApiResponse<KartuDto>.ErrorResult("Gagal mengambil data kartu");
        }
    }

    public async Task<ApiResponse<KartuDto>> CreateKartu(KartuCreateDto request)
    {
        try
        {
            // Validasi UID
            if (string.IsNullOrWhiteSpace(request.Uid))
            {
                return ApiResponse<KartuDto>.ErrorResult("UID kartu harus diisi");
            }

            // Cek duplikasi UID
            var existingKartu = await _kartuRepository.GetByUidAsync(request.Uid.Trim().ToUpper());
            if (existingKartu != null)
            {
                return ApiResponse<KartuDto>.ErrorResult("Kartu dengan UID tersebut sudah terdaftar");
            }

            // Validasi relasi User
            if (request.UserId.HasValue)
            {
                var user = await _userRepository.GetByIdAsync(request.UserId.Value);
                if (user == null)
                {
                    return ApiResponse<KartuDto>.ErrorResult("User tidak ditemukan");
                }

                // Cek apakah user sudah memiliki kartu
                var userHasCard = await _kartuRepository.IsUserHasCardAsync(request.UserId.Value);
                if (userHasCard)
                {
                    return ApiResponse<KartuDto>.ErrorResult("User sudah memiliki kartu");
                }
            }

            // Validasi relasi Kelas
            if (request.KelasId.HasValue)
            {
                var kelas = await _kelasRepository.GetByIdAsync(request.KelasId.Value);
                if (kelas == null)
                {
                    return ApiResponse<KartuDto>.ErrorResult("Kelas tidak ditemukan");
                }

                // Cek apakah kelas sudah memiliki kartu
                var kelasHasCard = await _kartuRepository.IsKelasHasCardAsync(request.KelasId.Value);
                if (kelasHasCard)
                {
                    return ApiResponse<KartuDto>.ErrorResult("Kelas sudah memiliki kartu");
                }
            }

            // Validasi constraint: UserId dan KelasId tidak boleh berdua-duanya terisi
            if (request.UserId.HasValue && request.KelasId.HasValue)
            {
                return ApiResponse<KartuDto>.ErrorResult("Kartu tidak dapat memiliki User dan Kelas secara bersamaan");
            }

            var kartu = new Kartu
            {
                Uid = request.Uid.Trim().ToUpper(),
                Status = request.Status ?? "AKTIF",
                Keterangan = request.Keterangan,
                UserId = request.UserId,
                KelasId = request.KelasId
            };

            await _kartuRepository.AddAsync(kartu);
            var saved = await _kartuRepository.SaveAsync();

            if (!saved)
            {
                return ApiResponse<KartuDto>.ErrorResult("Gagal menyimpan kartu");
            }

            // Reload data dengan include
            var createdKartu = await _kartuRepository.GetByIdAsync(kartu.Id);
            var kartuDto = new KartuDto
            {
                Id = createdKartu!.Id,
                Uid = createdKartu.Uid,
                Status = createdKartu.Status,
                Keterangan = createdKartu.Keterangan,
                UserId = createdKartu.UserId,
                KelasId = createdKartu.KelasId,
                CreatedAt = createdKartu.CreatedAt,
                UserUsername = createdKartu.User?.Username,
                KelasNama = createdKartu.Kelas?.Nama
            };

            _logger.LogInformation("Kartu created: UID={Uid}", kartu.Uid);
            return ApiResponse<KartuDto>.SuccessResult(kartuDto, "Kartu berhasil ditambahkan");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating kartu: {Uid}", request.Uid);
            return ApiResponse<KartuDto>.ErrorResult("Gagal membuat kartu");
        }
    }

    public async Task<ApiResponse<KartuDto>> UpdateKartu(int id, KartuUpdateDto request)
    {
        try
        {
            var kartu = await _kartuRepository.GetByIdAsync(id);
            if (kartu == null)
            {
                return ApiResponse<KartuDto>.ErrorResult("Kartu tidak ditemukan");
            }

            // Validasi UID
            if (string.IsNullOrWhiteSpace(request.Uid))
            {
                return ApiResponse<KartuDto>.ErrorResult("UID kartu harus diisi");
            }

            // Cek duplikasi UID (kecuali kartu ini sendiri)
            var existingKartu = await _kartuRepository.GetByUidAsync(request.Uid.Trim().ToUpper());
            if (existingKartu != null && existingKartu.Id != id)
            {
                return ApiResponse<KartuDto>.ErrorResult("Kartu dengan UID tersebut sudah terdaftar");
            }

            // Validasi relasi User
            if (request.UserId.HasValue && request.UserId.Value != kartu.UserId)
            {
                var user = await _userRepository.GetByIdAsync(request.UserId.Value);
                if (user == null)
                {
                    return ApiResponse<KartuDto>.ErrorResult("User tidak ditemukan");
                }

                var userHasCard = await _kartuRepository.IsUserHasCardAsync(request.UserId.Value);
                if (userHasCard)
                {
                    return ApiResponse<KartuDto>.ErrorResult("User sudah memiliki kartu");
                }
            }

            // Validasi relasi Kelas
            if (request.KelasId.HasValue && request.KelasId.Value != kartu.KelasId)
            {
                var kelas = await _kelasRepository.GetByIdAsync(request.KelasId.Value);
                if (kelas == null)
                {
                    return ApiResponse<KartuDto>.ErrorResult("Kelas tidak ditemukan");
                }

                var kelasHasCard = await _kartuRepository.IsKelasHasCardAsync(request.KelasId.Value);
                if (kelasHasCard)
                {
                    return ApiResponse<KartuDto>.ErrorResult("Kelas sudah memiliki kartu");
                }
            }

            // Validasi constraint: UserId dan KelasId tidak boleh berdua-duanya terisi
            if (request.UserId.HasValue && request.KelasId.HasValue)
            {
                return ApiResponse<KartuDto>.ErrorResult("Kartu tidak dapat memiliki User dan Kelas secara bersamaan");
            }

            // Update data
            kartu.Uid = request.Uid.Trim().ToUpper();
            kartu.Status = request.Status;
            kartu.Keterangan = request.Keterangan;
            kartu.UserId = request.UserId;
            kartu.KelasId = request.KelasId;

            _kartuRepository.Update(kartu);
            var saved = await _kartuRepository.SaveAsync();

            if (!saved)
            {
                return ApiResponse<KartuDto>.ErrorResult("Gagal mengupdate kartu");
            }

            // Reload data dengan include
            var updatedKartu = await _kartuRepository.GetByIdAsync(id);
            var kartuDto = new KartuDto
            {
                Id = updatedKartu!.Id,
                Uid = updatedKartu.Uid,
                Status = updatedKartu.Status,
                Keterangan = updatedKartu.Keterangan,
                UserId = updatedKartu.UserId,
                KelasId = updatedKartu.KelasId,
                CreatedAt = updatedKartu.CreatedAt,
                UserUsername = updatedKartu.User?.Username,
                KelasNama = updatedKartu.Kelas?.Nama
            };

            _logger.LogInformation("Kartu updated: {Id} - {Uid}", kartu.Id, kartu.Uid);
            return ApiResponse<KartuDto>.SuccessResult(kartuDto, "Kartu berhasil diupdate");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating kartu: {Id}", id);
            return ApiResponse<KartuDto>.ErrorResult("Gagal mengupdate kartu");
        }
    }

    public async Task<ApiResponse<object>> DeleteKartu(int id)
    {
        try
        {
            var kartu = await _kartuRepository.GetByIdAsync(id);
            if (kartu == null)
            {
                return ApiResponse<object>.ErrorResult("Kartu tidak ditemukan");
            }

            var hasAccessLog = await _aksesLogRepository.AnyByKartuIdAsync(id);
            if (hasAccessLog)
            {
                return ApiResponse<object>.ErrorResult("Tidak dapat menghapus kartu karena memiliki riwayat akses");
            }

            _kartuRepository.Remove(kartu);
            var saved = await _kartuRepository.SaveAsync();

            if (!saved)
            {
                return ApiResponse<object>.ErrorResult("Gagal menghapus kartu");
            }

            _logger.LogInformation("Kartu deleted: {Id} - {Uid}", kartu.Id, kartu.Uid);
            return ApiResponse<object>.SuccessResult(null!, "Kartu berhasil dihapus");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting kartu: {Id}", id);
            return ApiResponse<object>.ErrorResult("Gagal menghapus kartu");
        }
    }

    public async Task<ApiResponse<KartuCheckDto>> CheckCard(string uid)
    {
        try
        {
            var kartu = await _kartuRepository.GetByUidAsync(uid);

            if (kartu == null)
            {
                var notFoundResponse = new KartuCheckDto
                {
                    Uid = uid,
                    Terdaftar = false,
                    Message = "Kartu tidak terdaftar"
                };
                return ApiResponse<KartuCheckDto>.SuccessResult(notFoundResponse);
            }

            var foundResponse = new KartuCheckDto
            {
                Uid = kartu.Uid,
                Terdaftar = true,
                Status = kartu.Status,
                Keterangan = kartu.Keterangan,
                Message = "Kartu terdaftar",
                UserId = kartu.UserId,
                KelasId = kartu.KelasId,
                UserUsername = kartu.User?.Username,
                KelasNama = kartu.Kelas?.Nama
            };

            return ApiResponse<KartuCheckDto>.SuccessResult(foundResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking card: {Uid}", uid);
            return ApiResponse<KartuCheckDto>.ErrorResult("Gagal memeriksa kartu");
        }
    }
}using AutoMapper;
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

            // Verify password (using BCrypt)
            if (!VerifyPassword(request.Password, user.PasswordHash))
            {
                return ApiResponse<UserLoginResponse>.ErrorResult("Username atau password salah");
            }

            var token = GenerateJwtToken(user);

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

            // GUNAKAN AUTOMAPPER DI SINI
            var userDto = _mapper.Map<UserDto>(user);
            return ApiResponse<UserDto>.SuccessResult(userDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting profile for user: {UserId}", userId);
            return ApiResponse<UserDto>.ErrorResult("Gagal mengambil profil user");
        }
    }

    public async Task<ApiResponse<List<UserDto>>> GetAllUsers()
    {
        try
        {
            var users = await _userRepository.GetAllAsync();
            // GUNAKAN AUTOMAPPER DI SINI
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

            // GUNAKAN AUTOMAPPER DI SINI
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

            // GUNAKAN AUTOMAPPER DI SINI
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
            // Validasi input
            if (string.IsNullOrWhiteSpace(request.Username))
            {
                return ApiResponse<UserDto>.ErrorResult("Username harus diisi");
            }

            if (string.IsNullOrWhiteSpace(request.Password))
            {
                return ApiResponse<UserDto>.ErrorResult("Password harus diisi");
            }

            if (string.IsNullOrWhiteSpace(request.Role))
            {
                return ApiResponse<UserDto>.ErrorResult("Role harus diisi");
            }

            // Cek duplikasi username
            var existingUser = await _userRepository.GetByUsernameAsync(request.Username);
            if (existingUser != null)
            {
                return ApiResponse<UserDto>.ErrorResult("Username sudah digunakan");
            }

            // Hash password
            var passwordHash = HashPassword(request.Password);

            // GUNAKAN AUTOMAPPER DI SINI
            var user = _mapper.Map<User>(request);
            user.PasswordHash = passwordHash;

            await _userRepository.AddAsync(user);
            var saved = await _userRepository.SaveAsync();

            if (!saved)
            {
                return ApiResponse<UserDto>.ErrorResult("Gagal menyimpan user");
            }

            // Reload user dengan data lengkap
            var createdUser = await _userRepository.GetByIdAsync(user.Id);
            // GUNAKAN AUTOMAPPER DI SINI
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
            if (user == null)
            {
                return ApiResponse<UserDto>.ErrorResult("User tidak ditemukan");
            }

            // Validasi input
            if (string.IsNullOrWhiteSpace(request.Username))
            {
                return ApiResponse<UserDto>.ErrorResult("Username harus diisi");
            }

            if (string.IsNullOrWhiteSpace(request.Role))
            {
                return ApiResponse<UserDto>.ErrorResult("Role harus diisi");
            }

            // Cek duplikasi username (kecuali user ini sendiri)
            var existingUser = await _userRepository.GetByUsernameAsync(request.Username);
            if (existingUser != null && existingUser.Id != id)
            {
                return ApiResponse<UserDto>.ErrorResult("Username sudah digunakan");
            }

            // Update data menggunakan AutoMapper
            _mapper.Map(request, user);

            // Update password jika diisi
            if (!string.IsNullOrWhiteSpace(request.Password))
            {
                user.PasswordHash = HashPassword(request.Password);
            }

            _userRepository.Update(user);
            var saved = await _userRepository.SaveAsync();

            if (!saved)
            {
                return ApiResponse<UserDto>.ErrorResult("Gagal mengupdate user");
            }

            // Reload user dengan data lengkap
            var updatedUser = await _userRepository.GetByIdAsync(id);
            // GUNAKAN AUTOMAPPER DI SINI
            var userDto = _mapper.Map<UserDto>(updatedUser!);

            _logger.LogInformation("User updated: {Id} - {Username}", user.Id, user.Username);
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
            if (user == null)
            {
                return ApiResponse<object>.ErrorResult("User tidak ditemukan");
            }

            // Cek apakah user memiliki kartu
            if (user.Kartu != null && user.Kartu.Any())
            {
                return ApiResponse<object>.ErrorResult("Tidak dapat menghapus user karena memiliki kartu terdaftar");
            }

            // Cek jika user adalah admin terakhir
            if (user.Role == "admin")
            {
                var adminCount = await _userRepository.CountAdminsAsync();
                if (adminCount <= 1)
                {
                    return ApiResponse<object>.ErrorResult("Tidak dapat menghapus admin terakhir");
                }
            }

            _userRepository.Remove(user);
            var saved = await _userRepository.SaveAsync();

            if (!saved)
            {
                return ApiResponse<object>.ErrorResult("Gagal menghapus user");
            }

            _logger.LogInformation("User deleted: {Id} - {Username}", user.Id, user.Username);
            return ApiResponse<object>.SuccessResult(null!, "User berhasil dihapus");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting user: {Id}", id);
            return ApiResponse<object>.ErrorResult("Gagal menghapus user");
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

    public async Task<ApiResponse<List<UserDto>>> GetUsersWithoutKartu()
    {
        try
        {
            var users = await _userRepository.GetUsersWithoutKartuAsync();
            // GUNAKAN AUTOMAPPER DI SINI
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

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim("username", user.Username),
                new Claim("role", user.Role)
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(_jwtExpireMinutes), // Gunakan menit dari config
                IssuedAt = DateTime.UtcNow,
                NotBefore = DateTime.UtcNow,
                Issuer = _jwtIssuer,
                Audience = _jwtAudience,
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);

            // Debug: Print token details
            _logger.LogInformation("JWT Token generated successfully for {Username}", user.Username);
            _logger.LogInformation("Token expires at: {Expires}", tokenDescriptor.Expires);
            _logger.LogInformation("Token issuer: {Issuer}", _jwtIssuer);
            _logger.LogInformation("Token audience: {Audience}", _jwtAudience);

            return tokenString;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating JWT token for user {Username}", user.Username);
            throw new Exception($"Gagal generate token: {ex.Message}");
        }
    }
}using testing.DTOs;

namespace testing.Services;

public interface IRuanganService
{
    Task<ApiResponse<List<RuanganDto>>> GetAllRuangan();
    Task<ApiResponse<RuanganDto>> GetRuanganById(int id);
    Task<ApiResponse<RuanganDto>> CreateRuangan(RuanganCreateRequest request);
    Task<ApiResponse<RuanganDto>> UpdateRuangan(int id, RuanganUpdateRequest request);
    Task<ApiResponse<object>> DeleteRuangan(int id);
    Task<ApiResponse<RuanganStatsDto>> GetRuanganStats(int id);
}

// public class RuanganStatsDto
// {
//     public string Ruangan { get; set; } = string.Empty;
//     public int TotalAkses { get; set; }
//     public int AktifSekarang { get; set; }
// }using AutoMapper;
using testing.DTOs;
using testing.Models;
using testing.Repositories;

namespace testing.Services;

public class KelasService : IKelasService
{
    private readonly IKelasRepository _kelasRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<KelasService> _logger;

    public KelasService(
        IKelasRepository kelasRepository,
        IMapper mapper,
        ILogger<KelasService> logger)
    {
        _kelasRepository = kelasRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<ApiResponse<List<KelasDto>>> GetAllKelas()
    {
        try
        {
            var kelasList = await _kelasRepository.GetAllAsync();
            var kelasDtos = _mapper.Map<List<KelasDto>>(kelasList);
            return ApiResponse<List<KelasDto>>.SuccessResult(kelasDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all kelas");
            return ApiResponse<List<KelasDto>>.ErrorResult("Gagal mengambil data kelas");
        }
    }

    public async Task<ApiResponse<KelasDto>> GetKelasById(int id)
    {
        try
        {
            var kelas = await _kelasRepository.GetByIdAsync(id);
            if (kelas == null)
            {
                return ApiResponse<KelasDto>.ErrorResult("Kelas tidak ditemukan");
            }

            var kelasDto = _mapper.Map<KelasDto>(kelas);
            return ApiResponse<KelasDto>.SuccessResult(kelasDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving kelas by id: {Id}", id);
            return ApiResponse<KelasDto>.ErrorResult("Gagal mengambil data kelas");
        }
    }

    public async Task<ApiResponse<KelasDto>> CreateKelas(KelasCreateRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Nama))
            {
                return ApiResponse<KelasDto>.ErrorResult("Nama kelas harus diisi");
            }

            var existingKelas = await _kelasRepository.IsNamaExistAsync(request.Nama);
            if (existingKelas)
            {
                return ApiResponse<KelasDto>.ErrorResult("Kelas dengan nama tersebut sudah terdaftar");
            }

            var kelas = _mapper.Map<Kelas>(request);
            await _kelasRepository.AddAsync(kelas);
            var saved = await _kelasRepository.SaveAsync();

            if (!saved)
            {
                return ApiResponse<KelasDto>.ErrorResult("Gagal menyimpan kelas");
            }

            _logger.LogInformation("Kelas created: {Nama}", kelas.Nama);

            var kelasDto = _mapper.Map<KelasDto>(kelas);
            return ApiResponse<KelasDto>.SuccessResult(kelasDto, "Kelas berhasil ditambahkan");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating kelas: {Nama}", request.Nama);
            return ApiResponse<KelasDto>.ErrorResult("Gagal membuat kelas");
        }
    }

    public async Task<ApiResponse<KelasDto>> UpdateKelas(int id, KelasUpdateRequest request)
    {
        try
        {
            var kelas = await _kelasRepository.GetByIdAsync(id);
            if (kelas == null)
            {
                return ApiResponse<KelasDto>.ErrorResult("Kelas tidak ditemukan");
            }

            if (string.IsNullOrWhiteSpace(request.Nama))
            {
                return ApiResponse<KelasDto>.ErrorResult("Nama kelas harus diisi");
            }

            var existingKelas = await _kelasRepository.IsNamaExistAsync(request.Nama, id);
            if (existingKelas)
            {
                return ApiResponse<KelasDto>.ErrorResult("Kelas dengan nama tersebut sudah terdaftar");
            }

            _mapper.Map(request, kelas);
            _kelasRepository.Update(kelas);
            var saved = await _kelasRepository.SaveAsync();

            if (!saved)
            {
                return ApiResponse<KelasDto>.ErrorResult("Gagal mengupdate kelas");
            }

            _logger.LogInformation("Kelas updated: {Id} - {Nama}", kelas.Id, kelas.Nama);

            var kelasDto = _mapper.Map<KelasDto>(kelas);
            return ApiResponse<KelasDto>.SuccessResult(kelasDto, "Kelas berhasil diupdate");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating kelas: {Id}", id);
            return ApiResponse<KelasDto>.ErrorResult("Gagal mengupdate kelas");
        }
    }

    public async Task<ApiResponse<object>> DeleteKelas(int id)
    {
        try
        {
            var kelas = await _kelasRepository.GetByIdAsync(id);
            if (kelas == null)
            {
                return ApiResponse<object>.ErrorResult("Kelas tidak ditemukan");
            }

            _kelasRepository.Remove(kelas);
            var saved = await _kelasRepository.SaveAsync();

            if (!saved)
            {
                return ApiResponse<object>.ErrorResult("Gagal menghapus kelas");
            }

            _logger.LogInformation("Kelas deleted: {Id} - {Nama}", kelas.Id, kelas.Nama);
            return ApiResponse<object>.SuccessResult(null!, "Kelas berhasil dihapus");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting kelas: {Id}", id);
            return ApiResponse<object>.ErrorResult("Gagal menghapus kelas");
        }
    }

    public async Task<ApiResponse<KelasStatsDto>> GetKelasStats(int id)
    {
        try
        {
            var kelas = await _kelasRepository.GetByIdAsync(id);
            if (kelas == null)
            {
                return ApiResponse<KelasStatsDto>.ErrorResult("Kelas tidak ditemukan");
            }

            // Karena kartu sekarang independen, stats untuk kelas mungkin berbeda
            var stats = new KelasStatsDto
            {
                Kelas = kelas.Nama,
                TotalAkses = 0, // Tidak bisa dihitung karena kartu independen
                AktifSekarang = 0 // Tidak bisa dihitung karena kartu independen
            };

            return ApiResponse<KelasStatsDto>.SuccessResult(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting kelas stats: {Id}", id);
            return ApiResponse<KelasStatsDto>.ErrorResult("Gagal mengambil statistik kelas");
        }
    }
}
D:\sakat\testing\Services>cd ..

D:\sakat\testing>ls
Controllers        Properties                    db
DTOs               Repositories                  obj
Data               Services                      rebuild.bat
Hubs               Validators                    run.bat
MappingProfile.cs  add.bat                       testing.csproj
Middleware         api                           testing.http
Migrations         appsettings.Development.json  testing.sln
Models             appsettings.json
Program.cs         bin

D:\sakat\testing>cd repositories

D:\sakat\testing\Repositories>ls
AksesLogRepository.cs   IUserRepository.cs
IAksesLogRepository.cs  KartuRepository.cs
IKartuRepository.cs     KelasRepository.cs
IKelasRepository.cs     RuanganRepository.cs
IRuanganRepository.cs   UserRepository.cs

D:\sakat\testing\Repositories>cat AksesLogRepository.cs   IUserRepository.cs IAksesLogRepository.cs  KartuRepository.cs IKartuRepository.cs     KelasRepository.cs IKelasRepository.cs     RuanganRepository.cs IRuanganRepository.cs   UserRepository.cs
using Microsoft.EntityFrameworkCore;
using testing.Data;
using testing.Models;

namespace testing.Repositories;

public class AksesLogRepository : IAksesLogRepository
{
    private readonly LabDbContext _context;

    public AksesLogRepository(LabDbContext context)
    {
        _context = context;
    }

    public async Task<AksesLog?> GetByIdAsync(int id)
    {
        return await _context.AksesLog
            .Include(a => a.Kartu)
            .ThenInclude(k => k!.User)
            .Include(a => a.Kartu)
            .ThenInclude(k => k!.Kelas)
            .Include(a => a.Ruangan)
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<IEnumerable<AksesLog>> GetAllAsync()
    {
        return await _context.AksesLog
            .Include(a => a.Kartu)
            .ThenInclude(k => k!.User)
            .Include(a => a.Kartu)
            .ThenInclude(k => k!.Kelas)
            .Include(a => a.Ruangan)
            .AsNoTracking()
            .OrderByDescending(a => a.TimestampMasuk)
            .ToListAsync();
    }

    public async Task<IEnumerable<AksesLog>> GetPagedAsync(int page, int pageSize)
    {
        return await _context.AksesLog
            .Include(a => a.Kartu)
            .ThenInclude(k => k!.User)
            .Include(a => a.Kartu)
            .ThenInclude(k => k!.Kelas)
            .Include(a => a.Ruangan)
            .AsNoTracking()
            .OrderByDescending(a => a.TimestampMasuk)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<IEnumerable<AksesLog>> GetByKartuIdAsync(int kartuId)
    {
        return await _context.AksesLog
            .Include(a => a.Kartu)
            .ThenInclude(k => k!.User)
            .Include(a => a.Kartu)
            .ThenInclude(k => k!.Kelas)
            .Include(a => a.Ruangan)
            .Where(a => a.KartuId == kartuId)
            .AsNoTracking()
            .OrderByDescending(a => a.TimestampMasuk)
            .ToListAsync();
    }

    public async Task<IEnumerable<AksesLog>> GetByRuanganIdAsync(int ruanganId)
    {
        return await _context.AksesLog
            .Include(a => a.Kartu)
            .ThenInclude(k => k!.User)
            .Include(a => a.Kartu)
            .ThenInclude(k => k!.Kelas)
            .Include(a => a.Ruangan)
            .Where(a => a.RuanganId == ruanganId)
            .AsNoTracking()
            .OrderByDescending(a => a.TimestampMasuk)
            .ToListAsync();
    }

    public async Task<IEnumerable<AksesLog>> GetLatestAsync(int count)
    {
        return await _context.AksesLog
            .Include(a => a.Kartu)
            .ThenInclude(k => k!.User)
            .Include(a => a.Kartu)
            .ThenInclude(k => k!.Kelas)
            .Include(a => a.Ruangan)
            .AsNoTracking()
            .OrderByDescending(a => a.TimestampMasuk)
            .Take(count)
            .ToListAsync();
    }

    public async Task<AksesLog?> GetActiveLogByKartuIdAsync(int kartuId)
    {
        return await _context.AksesLog
            .Include(a => a.Kartu)
            .ThenInclude(k => k!.User)
            .Include(a => a.Kartu)
            .ThenInclude(k => k!.Kelas)
            .Include(a => a.Ruangan)
            .Where(a => a.KartuId == kartuId && a.TimestampKeluar == null)
            .FirstOrDefaultAsync();
    }

    public async Task<bool> AnyByKartuIdAsync(int kartuId)
    {
        return await _context.AksesLog
            .AnyAsync(a => a.KartuId == kartuId);
    }

    public async Task<bool> AnyByRuanganIdAsync(int ruanganId)
    {
        return await _context.AksesLog
            .AnyAsync(a => a.RuanganId == ruanganId);
    }

    public async Task AddAsync(AksesLog aksesLog)
    {
        await _context.AksesLog.AddAsync(aksesLog);
    }

    public void Update(AksesLog aksesLog)
    {
        _context.AksesLog.Update(aksesLog);
    }

    public async Task<int> CountAsync()
    {
        return await _context.AksesLog.CountAsync();
    }

    public async Task<int> CountByRuanganIdAsync(int ruanganId)
    {
        return await _context.AksesLog
            .CountAsync(a => a.RuanganId == ruanganId);
    }

    public async Task<int> CountActiveByRuanganIdAsync(int ruanganId)
    {
        return await _context.AksesLog
            .CountAsync(a => a.RuanganId == ruanganId && a.TimestampKeluar == null);
    }

    public async Task<int> CountByDateRangeAsync(DateTime start, DateTime end)
    {
        return await _context.AksesLog
            .CountAsync(a => a.TimestampMasuk >= start && a.TimestampMasuk < end);
    }

    public async Task<IEnumerable<AksesLog>> GetByDateRangeAsync(DateTime start, DateTime end)
    {
        return await _context.AksesLog
            .Include(a => a.Kartu)
            .ThenInclude(k => k!.User)
            .Include(a => a.Kartu)
            .ThenInclude(k => k!.Kelas)
            .Include(a => a.Ruangan)
            .Where(a => a.TimestampMasuk >= start && a.TimestampMasuk < end)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<bool> SaveAsync()
    {
        try
        {
            var result = await _context.SaveChangesAsync();
            return result > 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving AksesLog: {ex.Message}");
            throw;
        }
    }

    public async Task<int> CountByKelasIdAsync(int kelasId)
    {
        return await _context.AksesLog
            .Include(a => a.Kartu)
            .CountAsync(a => a.Kartu != null && a.Kartu.KelasId == kelasId);
    }

    public async Task<int> CountActiveByKelasIdAsync(int kelasId)
    {
        return await _context.AksesLog
            .Include(a => a.Kartu)
            .CountAsync(a => a.Kartu != null && a.Kartu.KelasId == kelasId && a.TimestampKeluar == null);
    }

    public async Task<int> CountByRuanganIdAndDateAsync(int ruanganId, DateTime date)
    {
        var start = date.Date;
        var end = start.AddDays(1);
        return await _context.AksesLog
            .CountAsync(a => a.RuanganId == ruanganId && a.TimestampMasuk >= start && a.TimestampMasuk < end);
    }
}using testing.Models;

namespace testing.Repositories;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(int id);
    Task<User?> GetByUsernameAsync(string username);
    Task<User?> GetByKartuUidAsync(string kartuUid);
    Task<IEnumerable<User>> GetAllAsync();
    Task<IEnumerable<User>> GetPagedAsync(int page, int pageSize);
    Task<bool> IsUsernameExistAsync(string username, int? excludeId = null);
    Task AddAsync(User user);
    void Update(User user);
    void Remove(User user);
    Task<int> CountAsync();
    Task<int> CountAdminsAsync();
    Task<int> CountByRoleAsync(string role);
    Task<bool> SaveAsync();

    // Method baru
    Task<User?> GetUserWithKartuByUidAsync(string kartuUid);
    Task<IEnumerable<User>> GetUsersWithoutKartuAsync();
}using testing.Models;

namespace testing.Repositories;

public interface IAksesLogRepository
{
    Task<AksesLog?> GetByIdAsync(int id);
    Task<IEnumerable<AksesLog>> GetAllAsync();
    Task<IEnumerable<AksesLog>> GetPagedAsync(int page, int pageSize);
    Task<IEnumerable<AksesLog>> GetByKartuIdAsync(int kartuId);
    Task<IEnumerable<AksesLog>> GetByRuanganIdAsync(int ruanganId);
    Task<IEnumerable<AksesLog>> GetLatestAsync(int count);
    Task<AksesLog?> GetActiveLogByKartuIdAsync(int kartuId);
    Task<bool> AnyByKartuIdAsync(int kartuId);
    Task<bool> AnyByRuanganIdAsync(int ruanganId);
    Task AddAsync(AksesLog aksesLog);
    void Update(AksesLog aksesLog);
    Task<int> CountAsync();
    Task<int> CountByRuanganIdAsync(int ruanganId);
    Task<int> CountActiveByRuanganIdAsync(int ruanganId);
    Task<int> CountByDateRangeAsync(DateTime start, DateTime end);

    // Method tambahan
    Task<IEnumerable<AksesLog>> GetByDateRangeAsync(DateTime start, DateTime end);
    Task<bool> SaveAsync();

    // Method untuk stats
    Task<int> CountByKelasIdAsync(int kelasId);
    Task<int> CountActiveByKelasIdAsync(int kelasId);
    Task<int> CountByRuanganIdAndDateAsync(int ruanganId, DateTime date);
}using Microsoft.EntityFrameworkCore;
using testing.Data;
using testing.Models;

namespace testing.Repositories;

public class KartuRepository : IKartuRepository
{
    private readonly LabDbContext _context;

    public KartuRepository(LabDbContext context)
    {
        _context = context;
    }

    public async Task<Kartu?> GetByIdAsync(int id)
    {
        return await _context.Kartu
            .Include(k => k.User)
            .Include(k => k.Kelas)
            .AsNoTracking()
            .FirstOrDefaultAsync(k => k.Id == id);
    }

    public async Task<Kartu?> GetByUidAsync(string uid)
    {
        return await _context.Kartu
            .Include(k => k.User)
            .Include(k => k.Kelas)
            .AsNoTracking()
            .FirstOrDefaultAsync(k => k.Uid == uid);
    }

    public async Task<Kartu?> GetByUserIdAsync(int userId)
    {
        return await _context.Kartu
            .Include(k => k.User)
            .Include(k => k.Kelas)
            .AsNoTracking()
            .FirstOrDefaultAsync(k => k.UserId == userId);
    }

    public async Task<Kartu?> GetByKelasIdAsync(int kelasId)
    {
        return await _context.Kartu
            .Include(k => k.User)
            .Include(k => k.Kelas)
            .AsNoTracking()
            .FirstOrDefaultAsync(k => k.KelasId == kelasId);
    }

    public async Task<IEnumerable<Kartu>> GetAllAsync()
    {
        return await _context.Kartu
            .Include(k => k.User)
            .Include(k => k.Kelas)
            .AsNoTracking()
            .OrderByDescending(k => k.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Kartu>> GetPagedAsync(int page, int pageSize)
    {
        return await _context.Kartu
            .Include(k => k.User)
            .Include(k => k.Kelas)
            .AsNoTracking()
            .OrderByDescending(k => k.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<bool> IsUidExistAsync(string uid, int? excludeId = null)
    {
        return await _context.Kartu
            .AnyAsync(k => k.Uid == uid && (excludeId == null || k.Id != excludeId));
    }

    public async Task<bool> IsUserHasCardAsync(int userId)
    {
        return await _context.Kartu
            .AnyAsync(k => k.UserId == userId);
    }

    public async Task<bool> IsKelasHasCardAsync(int kelasId)
    {
        return await _context.Kartu
            .AnyAsync(k => k.KelasId == kelasId);
    }

    public async Task AddAsync(Kartu kartu)
    {
        await _context.Kartu.AddAsync(kartu);
    }

    public void Update(Kartu kartu)
    {
        _context.Kartu.Update(kartu);
    }

    public void Remove(Kartu kartu)
    {
        _context.Kartu.Remove(kartu);
    }

    public async Task<int> CountAsync()
    {
        return await _context.Kartu.CountAsync();
    }

    public async Task<bool> SaveAsync()
    {
        return await _context.SaveChangesAsync() > 0;
    }
}using testing.Models;

namespace testing.Repositories;

public interface IKartuRepository
{
    Task<Kartu?> GetByIdAsync(int id);
    Task<Kartu?> GetByUidAsync(string uid);
    Task<Kartu?> GetByUserIdAsync(int userId);
    Task<Kartu?> GetByKelasIdAsync(int kelasId);
    Task<IEnumerable<Kartu>> GetAllAsync();
    Task<IEnumerable<Kartu>> GetPagedAsync(int page, int pageSize);
    Task<bool> IsUidExistAsync(string uid, int? excludeId = null);
    Task<bool> IsUserHasCardAsync(int userId);
    Task<bool> IsKelasHasCardAsync(int kelasId);
    Task AddAsync(Kartu kartu);
    void Update(Kartu kartu);
    void Remove(Kartu kartu);
    Task<int> CountAsync();
    Task<bool> SaveAsync();
}using Microsoft.EntityFrameworkCore;
using testing.Data;
using testing.Models;

namespace testing.Repositories;

public class KelasRepository : IKelasRepository
{
    private readonly LabDbContext _context;

    public KelasRepository(LabDbContext context)
    {
        _context = context;
    }

    public async Task<Kelas?> GetByIdAsync(int id)
    {
        return await _context.Kelas
            .AsNoTracking()
            .FirstOrDefaultAsync(k => k.Id == id);
    }

    public async Task<IEnumerable<Kelas>> GetAllAsync()
    {
        return await _context.Kelas
            .AsNoTracking()
            .OrderBy(k => k.Nama)
            .ToListAsync();
    }

    public async Task<bool> IsNamaExistAsync(string nama, int? excludeId = null)
    {
        return await _context.Kelas
            .AnyAsync(k => k.Nama.ToLower() == nama.ToLower() && (excludeId == null || k.Id != excludeId));
    }

    public async Task AddAsync(Kelas kelas)
    {
        await _context.Kelas.AddAsync(kelas);
    }

    public void Update(Kelas kelas)
    {
        _context.Kelas.Update(kelas);
    }

    public void Remove(Kelas kelas)
    {
        _context.Kelas.Remove(kelas);
    }

    public async Task<int> CountAsync()
    {
        return await _context.Kelas.CountAsync();
    }

    public async Task<bool> SaveAsync()
    {
        return await _context.SaveChangesAsync() > 0;
    }
}using testing.Models;

namespace testing.Repositories;

public interface IKelasRepository
{
    Task<Kelas?> GetByIdAsync(int id);
    Task<IEnumerable<Kelas>> GetAllAsync();
    Task<bool> IsNamaExistAsync(string nama, int? excludeId = null);
    Task AddAsync(Kelas kelas);
    void Update(Kelas kelas);
    void Remove(Kelas kelas);
    Task<int> CountAsync();
    Task<bool> SaveAsync();
}using Microsoft.EntityFrameworkCore;
using testing.Data;
using testing.Models;

namespace testing.Repositories;

public class RuanganRepository : IRuanganRepository
{
    private readonly LabDbContext _context;

    public RuanganRepository(LabDbContext context)
    {
        _context = context;
    }

    public async Task<Ruangan?> GetByIdAsync(int id)
    {
        return await _context.Ruangan
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task<Ruangan?> GetByIdWithAksesLogsAsync(int id)
    {
        return await _context.Ruangan
            .Include(r => r.AksesLogs)
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task<IEnumerable<Ruangan>> GetAllAsync()
    {
        return await _context.Ruangan
            .AsNoTracking()
            .OrderBy(r => r.Nama)
            .ToListAsync();
    }

    public async Task<bool> IsNamaExistAsync(string nama, int? excludeId = null)
    {
        return await _context.Ruangan
            .AnyAsync(r => r.Nama.ToLower() == nama.ToLower() && (excludeId == null || r.Id != excludeId));
    }

    public async Task AddAsync(Ruangan ruangan)
    {
        await _context.Ruangan.AddAsync(ruangan);
    }

    public void Update(Ruangan ruangan)
    {
        _context.Ruangan.Update(ruangan);
    }

    public void Remove(Ruangan ruangan)
    {
        _context.Ruangan.Remove(ruangan);
    }

    public async Task<int> CountAsync()
    {
        return await _context.Ruangan.CountAsync();
    }

    public async Task<bool> SaveAsync()
    {
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<int> GetTotalAksesCountAsync(int ruanganId)
    {
        return await _context.AksesLog
            .CountAsync(a => a.RuanganId == ruanganId);
    }

    public async Task<int> GetActiveAksesCountAsync(int ruanganId)
    {
        return await _context.AksesLog
            .CountAsync(a => a.RuanganId == ruanganId && a.TimestampKeluar == null);
    }
}using testing.Models;

namespace testing.Repositories;

public interface IRuanganRepository
{
    Task<Ruangan?> GetByIdAsync(int id);
    Task<Ruangan?> GetByIdWithAksesLogsAsync(int id);
    Task<IEnumerable<Ruangan>> GetAllAsync();
    Task<bool> IsNamaExistAsync(string nama, int? excludeId = null);
    Task AddAsync(Ruangan ruangan);
    void Update(Ruangan ruangan);
    void Remove(Ruangan ruangan);
    Task<int> CountAsync();
    Task<bool> SaveAsync();
    Task<int> GetTotalAksesCountAsync(int ruanganId);
    Task<int> GetActiveAksesCountAsync(int ruanganId);
}using Microsoft.EntityFrameworkCore;
using testing.Data;
using testing.Models;

namespace testing.Repositories;

public class UserRepository : IUserRepository
{
    private readonly LabDbContext _context;

    public UserRepository(LabDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetByIdAsync(int id)
    {
        return await _context.Users
            .Include(u => u.Kartu)  // Include kartu yang dimiliki user
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == id);
    }

    public async Task<User?> GetByUsernameAsync(string username)
    {
        return await _context.Users
            .Include(u => u.Kartu)  // Include kartu yang dimiliki user
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Username.ToLower() == username.ToLower());
    }

    public async Task<User?> GetByKartuUidAsync(string kartuUid)
    {
        // Sekarang mencari melalui relasi Kartu
        return await _context.Users
            .Include(u => u.Kartu)
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Kartu != null && u.Kartu.Any(k => k.Uid == kartuUid));
    }

    public async Task<IEnumerable<User>> GetAllAsync()
    {
        return await _context.Users
            .Include(u => u.Kartu)  // Include kartu yang dimiliki user
            .AsNoTracking()
            .OrderBy(u => u.Username)
            .ToListAsync();
    }

    public async Task<IEnumerable<User>> GetPagedAsync(int page, int pageSize)
    {
        return await _context.Users
            .Include(u => u.Kartu)
            .AsNoTracking()
            .OrderBy(u => u.Username)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<bool> IsUsernameExistAsync(string username, int? excludeId = null)
    {
        return await _context.Users
            .AnyAsync(u => u.Username.ToLower() == username.ToLower() && (excludeId == null || u.Id != excludeId));
    }

    // Hapus method IsKartuUidExistAsync karena sekarang relasi ada di tabel Kartu
    // public async Task<bool> IsKartuUidExistAsync(string kartuUid, int? excludeId = null)

    public async Task AddAsync(User user)
    {
        await _context.Users.AddAsync(user);
    }

    public void Update(User user)
    {
        _context.Users.Update(user);
    }

    public void Remove(User user)
    {
        _context.Users.Remove(user);
    }

    public async Task<int> CountAsync()
    {
        return await _context.Users.CountAsync();
    }

    public async Task<int> CountAdminsAsync()
    {
        return await _context.Users
            .CountAsync(u => u.Role == "admin");
    }

    public async Task<int> CountByRoleAsync(string role)
    {
        return await _context.Users
            .CountAsync(u => u.Role == role);
    }

    public async Task<bool> SaveAsync()
    {
        return await _context.SaveChangesAsync() > 0;
    }

    // Method baru untuk mendapatkan user dengan kartu tertentu
    public async Task<User?> GetUserWithKartuByUidAsync(string kartuUid)
    {
        return await _context.Users
            .Include(u => u.Kartu)
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Kartu != null && u.Kartu.Any(k => k.Uid == kartuUid));
    }

    // Method untuk mendapatkan user yang tidak memiliki kartu
    public async Task<IEnumerable<User>> GetUsersWithoutKartuAsync()
    {
        return await _context.Users
            .Where(u => u.Kartu == null || !u.Kartu.Any())
            .AsNoTracking()
            .OrderBy(u => u.Username)
            .ToListAsync();
    }
}
D:\sakat\testing\Repositories>cd ..

D:\sakat\testing>ls
Controllers        Properties                    db
DTOs               Repositories                  obj
Data               Services                      rebuild.bat
Hubs               Validators                    run.bat
MappingProfile.cs  add.bat                       testing.csproj
Middleware         api                           testing.http
Migrations         appsettings.Development.json  testing.sln
Models             appsettings.json
Program.cs         bin

D:\sakat\testing>cat program.cs .env mappingprofile.cs
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;
using testing.Hubs;
using testing.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi.Models;
using testing.Middleware;
using testing.Services;
using testing.Repositories;
using FluentValidation;
using FluentValidation.AspNetCore;
using DotNetEnv;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Logging; // PENTING

// [CRITICAL] BUKA SENSOR ERROR AGAR KITA BISA LIHAT ISINYA
IdentityModelEventSource.ShowPII = true;
JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

var builder = WebApplication.CreateBuilder(args);
Env.Load();
builder.Configuration.AddEnvironmentVariables();

// DB & Service Setup (Standar)
var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
    ?? builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<LabDbContext>(options => options.UseNpgsql(connectionString));

// JWT Settings
var jwtSecretKey = Environment.GetEnvironmentVariable("JwtSettings__SecretKey")?.Trim() ?? "SuperStrongSecretKeyForLabAccessSMKN1Katapang2024!";
var key = Encoding.UTF8.GetBytes(jwtSecretKey);

// [DIAGNOSTIC] PRINT TOKEN SAAT STARTUP
Console.WriteLine("\n🔑 --- ADMIN TOKEN (VALID 24 JAM) --- 🔑");
var tokenDescriptor = new SecurityTokenDescriptor
{
    Subject = new ClaimsIdentity(new[] { new Claim("id", "1"), new Claim("name", "admin"), new Claim("role", "admin") }),
    Expires = DateTime.UtcNow.AddHours(24),
    Issuer = "LabAccessAPI",
    Audience = "LabAccessClient",
    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
};
var startupToken = new JwtSecurityTokenHandler().WriteToken(new JwtSecurityTokenHandler().CreateToken(tokenDescriptor));
Console.WriteLine(startupToken);
Console.WriteLine("---------------------------------------\n");

builder.Services.AddControllers().AddJsonOptions(opts =>
{
    opts.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    opts.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Lab Access API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement { { new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" } }, new string[] { } } });
});

builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddFluentValidationClientsideAdapters();
builder.Services.AddValidatorsFromAssemblyContaining<Program>();
builder.Services.AddSignalR();
builder.Services.AddAutoMapper(typeof(Program));

// Services
builder.Services.AddScoped<IKartuRepository, KartuRepository>();
builder.Services.AddScoped<IAksesLogRepository, AksesLogRepository>();
builder.Services.AddScoped<IKelasRepository, KelasRepository>();
builder.Services.AddScoped<IRuanganRepository, RuanganRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IKartuService, KartuService>();
builder.Services.AddScoped<IAksesLogService, AksesLogService>();
builder.Services.AddScoped<IKelasService, KelasService>();
builder.Services.AddScoped<IRuanganService, RuanganService>();
builder.Services.AddScoped<IScanService, ScanService>();
builder.Services.AddScoped<ITapService, TapService>();
builder.Services.AddScoped<IUserService, UserService>();

var corsOrigins = Environment.GetEnvironmentVariable("CORS__Origins") ?? "*";
builder.Services.AddCors(options => options.AddPolicy("AllowFrontend", p => p.WithOrigins(corsOrigins.Split(',')).AllowAnyHeader().AllowAnyMethod().AllowCredentials()));


builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
.AddJwtBearer(options =>
{
    // Kita simpan parameter validasi di variabel agar bisa dipakai di dalam event
    var validationParams = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = "LabAccessAPI",
        ValidAudience = "LabAccessClient",
        IssuerSigningKey = new SymmetricSecurityKey(key),
        NameClaimType = "name",
        RoleClaimType = "role",
        ClockSkew = TimeSpan.Zero // Strict expiration
    };

    options.TokenValidationParameters = validationParams;

    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var authHeader = context.Request.Headers["Authorization"].ToString();

            // 1. Cek apakah ada header Authorization
            if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                // 2. Ambil token murni
                var token = authHeader.Substring("Bearer ".Length).Trim();

                try
                {
                    var handler = new JwtSecurityTokenHandler();

                    // 3. LAKUKAN VALIDASI MANUAL DI SINI (BYPASS SISTEM OTOMATIS)
                    // Kita paksa validasi menggunakan token yang sudah kita bersihkan
                    var principal = handler.ValidateToken(token, validationParams, out var validatedToken);

                    // 4. JIKA LOLOS: TEMPELKAN HASILNYA KE CONTEXT
                    context.Principal = principal;
                    context.Success(); // BERITAHU SISTEM: "SUDAH SELESAI, JANGAN CEK LAGI!"

                    Console.WriteLine($"[🎉 MANUAL OVERRIDE] Token Valid! User: {principal.Identity.Name}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[❌ MANUAL FAIL] Token ditolak oleh validasi manual: {ex.Message}");
                    // Biarkan lanjut agar sistem melempar 401 standar
                }
            }
            return Task.CompletedTask;
        },

        OnAuthenticationFailed = context =>
        {
            // Event ini hanya akan terpanggil jika Manual Override di atas gagal/dilewati
            Console.WriteLine($"[🔥 SYSTEM FAIL] {context.Exception.Message}");
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddAuthorization();

var app = builder.Build();

if (app.Environment.IsDevelopment()) { app.UseSwagger(); app.UseSwaggerUI(); }
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseCors("AllowFrontend");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHub<LogHub>("/logHub");
await app.RunAsync();

# Database Configuration
ConnectionStrings__DefaultConnection=Host=localhost;Port=5432;Database=akses_lab;Username=postgres;Password=101001010100;Timeout=300;CommandTimeout=300

# JWT Configuration
JwtSettings__SecretKey=SuperStrongSecretKeyForLabAccessSMKN1Katapang2024!
JwtSettings__Issuer=LabAccessAPI
JwtSettings__Audience=LabAccessClient
JwtSettings__ExpireMinutes=1440

# CORS Configuration
CORS__Origins=http://localhost:3000,http://localhost:8000,http://localhost:5500,http://192.168.1.8:5500,http://10.238.146.219:5500,http://localhost:5173,http://192.168.1.6:5500

# Application Settings
AppSettings__Environment=Development
AppSettings__LogLevel=Information
AppSettings__ApiUrl=http://localhost:5292

# Swagger Settings (optional)
Swagger__Enabled=true
Swagger__Title=Lab Access API
Swagger__Version=v1using AutoMapper;
using testing.DTOs;
using testing.Models;

namespace testing;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // User mappings
        CreateMap<User, UserDto>()
            .ForMember(dest => dest.KartuUid, opt => opt.MapFrom(src =>
                src.Kartu != null && src.Kartu.Any() ? src.Kartu.First().Uid : null))
            .ForMember(dest => dest.KartuId, opt => opt.MapFrom(src =>
                src.Kartu != null && src.Kartu.Any() ? src.Kartu.First().Id : (int?)null));

        CreateMap<UserCreateRequest, User>()
            .ForMember(dest => dest.PasswordHash, opt => opt.Ignore()); // Password akan di-hash di service

        CreateMap<UserUpdateRequest, User>()
            .ForMember(dest => dest.PasswordHash, opt => opt.Ignore()); // Password akan di-hash di service

        // Kartu mappings
        CreateMap<Kartu, KartuDto>()
            .ForMember(dest => dest.UserUsername, opt => opt.MapFrom(src => src.User != null ? src.User.Username : null))
            .ForMember(dest => dest.KelasNama, opt => opt.MapFrom(src => src.Kelas != null ? src.Kelas.Nama : null));

        CreateMap<KartuCreateDto, Kartu>();
        CreateMap<KartuUpdateDto, Kartu>();

        // Kelas mappings
        CreateMap<Kelas, KelasDto>();
        CreateMap<KelasCreateRequest, Kelas>();
        CreateMap<KelasUpdateRequest, Kelas>();

        // Ruangan mappings
        CreateMap<Ruangan, RuanganDto>();
        CreateMap<RuanganCreateRequest, Ruangan>();
        CreateMap<RuanganUpdateRequest, Ruangan>();

        // AksesLog mappings
        CreateMap<AksesLog, AksesLogDto>()
            .ForMember(dest => dest.KartuUid, opt => opt.MapFrom(src => src.Kartu != null ? src.Kartu.Uid : null))
            .ForMember(dest => dest.RuanganNama, opt => opt.MapFrom(src => src.Ruangan != null ? src.Ruangan.Nama : null))
            .ForMember(dest => dest.UserUsername, opt => opt.MapFrom(src =>
                src.Kartu != null && src.Kartu.User != null ? src.Kartu.User.Username : null))
            .ForMember(dest => dest.KelasNama, opt => opt.MapFrom(src =>
                src.Kartu != null && src.Kartu.Kelas != null ? src.Kartu.Kelas.Nama : null));
    }
}
D:\sakat\testing>