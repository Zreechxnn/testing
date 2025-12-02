using testing.DTOs;

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
}