namespace testing.DTOs;

public class PeriodeDto
{
    public int Id { get; set; }
    public string Nama { get; set; } = string.Empty; // Contoh: "2024/2025 Ganjil"
    public bool IsAktif { get; set; }
}

public class PeriodeCreateRequest
{
    public string Nama { get; set; } = string.Empty;
    public bool IsAktif { get; set; } = false;
}

public class PeriodeUpdateRequest
{
    public string Nama { get; set; } = string.Empty;
    public bool IsAktif { get; set; }
}