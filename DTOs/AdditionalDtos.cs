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
    public DateTime TimestampMasuk { get; set; }  // Sekarang sudah WIB
    public DateTime? TimestampKeluar { get; set; } // Sekarang sudah WIB
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
}