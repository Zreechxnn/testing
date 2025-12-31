namespace testing.DTOs;

public class KelasCreateRequest
{
    public required string Nama { get; set; }
    // Hapus tanda tanya '?', jadikan wajib (int)
    public int PeriodeId { get; set; }
}

public class KelasUpdateRequest
{
    public required string Nama { get; set; }
    public int PeriodeId { get; set; }
}

public class KelasDto
{
    public int Id { get; set; }
    public string Nama { get; set; } = string.Empty;
    public int PeriodeId { get; set; } // Sesuaikan jadi int agar konsisten
    public string? PeriodeNama { get; set; }
}