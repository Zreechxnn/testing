using testing.DTOs;

namespace testing.Services;

public interface IKelasService
{
    Task<ApiResponse<List<KelasDto>>> GetAllKelas();
    Task<ApiResponse<KelasDto>> GetKelasById(int id);
    Task<ApiResponse<KelasDto>> CreateKelas(KelasCreateRequest request);
    Task<ApiResponse<KelasDto>> UpdateKelas(int id, KelasUpdateRequest request);
    Task<ApiResponse<object>> DeleteKelas(int id);
    Task<ApiResponse<KelasStatsDto>> GetKelasStats(int id);
}

// public class KelasStatsDto
// {
//     public string Kelas { get; set; } = string.Empty;
//     public int TotalAkses { get; set; }
//     public int AktifSekarang { get; set; }
// }