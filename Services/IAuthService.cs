using testing.DTOs;

namespace testing.Services;

public interface IAuthService
{
    Task<ApiResponse<UserLoginResponse>> Login(UserLoginRequest request);
    Task<ApiResponse<UserDto>> GetProfile(int userId);
    Task<ApiResponse<List<object>>> GetRoles();
}