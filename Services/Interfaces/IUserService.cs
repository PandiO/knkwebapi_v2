using knkwebapi_v2.Dtos;
using knkwebapi_v2.Models;

namespace knkwebapi_v2.Services
{
    public interface IUserService
    {
        Task<IEnumerable<UserDto>> GetAllAsync();
        Task<UserDto?> GetByIdAsync(int id);
        Task<UserDto?> GetByUuidAsync(string uuid);
        Task<UserDto?> GetByUsernameAsync(string username);
        Task<UserDto> CreateAsync(UserCreateDto user);
        Task UpdateAsync(int id, UserDto user);
        Task UpdateCoinsAsync(int id, int coins);
        Task UpdateCoinsByUuidAsync(string uuid, int coins);
        Task DeleteAsync(int id);
        Task<PagedResultDto<UserListDto>> SearchAsync(PagedQueryDto query);
    }
}
