using System.ComponentModel.DataAnnotations;

namespace testing.DTOs;

public class JurusanDto
{
    public int Id { get; set; }
    public string Kode { get; set; } = string.Empty; // Contoh: RPL
    public string Nama { get; set; } = string.Empty; // Contoh: Rekayasa Perangkat Lunak
}

public class JurusanCreateRequest
{
    [Required]
    [StringLength(20, ErrorMessage = "Kode jurusan maksimal 20 karakter")]
    public required string Kode { get; set; }

    [Required]
    [StringLength(100, ErrorMessage = "Nama jurusan maksimal 100 karakter")]
    public required string Nama { get; set; }
}

public class JurusanUpdateRequest
{
    [Required]
    [StringLength(20)]
    public required string Kode { get; set; }

    [Required]
    [StringLength(100)]
    public required string Nama { get; set; }
}