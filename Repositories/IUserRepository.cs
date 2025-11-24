using testing.Models;

namespace testing.Repositories;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(int id);
    Task<User?> GetByUsernameAsync(string username);
    Task<User?> GetByKartuUidAsync(string kartuUid);
    Task<IEnumerable<User>> GetAllAsync();
    Task<IEnumerable<User>> GetPagedAsync(int page, int pageSize);
    Task<bool> IsUsernameExistAsync(string username, int? excludeId = null);
    Task AddAsync(User user);
    void Update(User user);
    void Remove(User user);
    Task<int> CountAsync();
    Task<int> CountAdminsAsync();
    Task<int> CountByRoleAsync(string role);
    Task<bool> SaveAsync();

    // Method baru
    Task<User?> GetUserWithKartuByUidAsync(string kartuUid);
    Task<IEnumerable<User>> GetUsersWithoutKartuAsync();
}