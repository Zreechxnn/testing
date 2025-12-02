using testing.DTOs;

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
}