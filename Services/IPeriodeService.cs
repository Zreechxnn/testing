using testing.DTOs;
namespace testing.Services;

public interface IPeriodeService
{
    Task<ApiResponse<object>> SetActive(int id);
    Task<ApiResponse<List<PeriodeDto>>> GetAll();
    Task<ApiResponse<PeriodeDto>> Create(PeriodeCreateRequest request);
}