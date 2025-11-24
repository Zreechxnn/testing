using testing.DTOs;

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
}