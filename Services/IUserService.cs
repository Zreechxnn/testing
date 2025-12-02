using testing.DTOs;

namespace testing.Services;

public interface IUserService
{
    Task<ApiResponse<List<UserDto>>> GetAllUsers();
    Task<ApiResponse<PagedResponse<UserDto>>> GetUsersPaged(PagedRequest request);
    Task<ApiResponse<UserDto>> GetUserById(int id);
    Task<ApiResponse<UserDto>> CreateUser(UserCreateRequest request);
    Task<ApiResponse<UserDto>> UpdateUser(int id, UserUpdateRequest request);
    Task<ApiResponse<object>> DeleteUser(int id);
    Task<ApiResponse<List<UserDto>>> GetUsersWithoutKartu();
}