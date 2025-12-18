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
    Task<ApiResponse<List<KelasDto>>> GetKelasByPeriode(int periodeId);
}