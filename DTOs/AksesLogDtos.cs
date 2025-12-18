namespace testing.DTOs;

public class AksesLogDto
{
    public int Id { get; set; }
    public int KartuId { get; set; }
    public int RuanganId { get; set; }
    public DateTime TimestampMasuk { get; set; }
    public DateTime? TimestampKeluar { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Keterangan { get; set; }
    public string? KartuUid { get; set; }
    public string? RuanganNama { get; set; }
    public string? UserUsername { get; set; }
    public string? KelasNama { get; set; }
}
public class AksesLogUpdateRequest
{
    public string? Keterangan { get; set; }
}