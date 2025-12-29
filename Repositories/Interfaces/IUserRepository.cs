using knkwebapi_v2.Models;

namespace knkwebapi_v2.Repositories
{
    public interface IUserRepository
    {
        Task<IEnumerable<User>> GetAllAsync();
        Task<User?> GetByIdAsync(int id);
        Task<User?> GetByUuidAsync(string uuid);
        Task<User?> GetByUsernameAsync(string username);
        Task AddUserAsync(User user);
        Task UpdateUserAsync(User user);
        Task UpdateUserCoinsAsync(int id, int coins);
        Task UpdateUserCoinsByUuidAsync(string uuid, int coins);
        Task DeleteUserAsync(int id);
        Task<PagedResult<User>> SearchAsync(PagedQuery query);
    }
}
