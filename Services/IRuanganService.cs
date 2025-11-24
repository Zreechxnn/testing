using testing.DTOs;

namespace testing.Services;

public interface IRuanganService
{
    Task<ApiResponse<List<RuanganDto>>> GetAllRuangan();
    Task<ApiResponse<RuanganDto>> GetRuanganById(int id);
    Task<ApiResponse<RuanganDto>> CreateRuangan(RuanganCreateRequest request);
    Task<ApiResponse<RuanganDto>> UpdateRuangan(int id, RuanganUpdateRequest request);
    Task<ApiResponse<object>> DeleteRuangan(int id);
    Task<ApiResponse<RuanganStatsDto>> GetRuanganStats(int id);
}

// public class RuanganStatsDto
// {
//     public string Ruangan { get; set; } = string.Empty;
//     public int TotalAkses { get; set; }
//     public int AktifSekarang { get; set; }
// }