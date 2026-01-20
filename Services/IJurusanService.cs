using testing.DTOs;

namespace testing.Services;

public interface IJurusanService
{
    Task<ApiResponse<List<JurusanDto>>> GetAll();
    Task<ApiResponse<JurusanDto>> GetById(int id);
    Task<ApiResponse<JurusanDto>> Create(JurusanCreateRequest request);
    Task<ApiResponse<JurusanDto>> Update(int id, JurusanUpdateRequest request);
    Task<ApiResponse<object>> Delete(int id);
}