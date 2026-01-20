using System.ComponentModel.DataAnnotations;

namespace testing.DTOs;

public class KelasCreateRequest
{
    [Required]
    public required string Nama { get; set; } // Contoh: "10 PPLG 1"

    [Required]
    [Range(10, 13, ErrorMessage = "Tingkat harus antara 10-13")]
    public int Tingkat { get; set; } // Baru: 10, 11, 12

    [Required]
    public int JurusanId { get; set; } // Baru: ID Jurusan

    [Required]
    public int PeriodeId { get; set; }
}

public class KelasUpdateRequest
{
    [Required]
    public required string Nama { get; set; }

    [Required]
    [Range(10, 13)]
    public int Tingkat { get; set; }

    [Required]
    public int JurusanId { get; set; }

    [Required]
    public int PeriodeId { get; set; }
}

public class KelasDto
{
    public int Id { get; set; }
    public string Nama { get; set; } = string.Empty;
    public int Tingkat { get; set; } // Baru

    // Info Jurusan
    public int? JurusanId { get; set; }
    public string? JurusanKode { get; set; } // Baru: RPL
    public string? JurusanNama { get; set; } // Baru: Rekayasa Perangkat Lunak

    // Info Periode
    public int PeriodeId { get; set; }
    public string? PeriodeNama { get; set; }
}