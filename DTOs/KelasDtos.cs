namespace testing.DTOs;

public class KelasCreateRequest
{
    public required string Nama { get; set; }
    public int? PeriodeId { get; set; }
    public string? PeriodeNama { get; set; }
}

public class KelasUpdateRequest
{
    public required string Nama { get; set; }
}

public class KelasDto
{
    public int Id { get; set; }
    public string Nama { get; set; } = string.Empty;
    public int? PeriodeId { get; set; }
    public string? PeriodeNama { get; set; }
}