using knkwebapi_v2.Dtos;

namespace knkwebapi_v2.Services
{
    public interface IMinecraftBlockRefService
    {
        Task<IEnumerable<MinecraftBlockRefDto>> GetAllAsync();
        Task<MinecraftBlockRefDto?> GetByIdAsync(int id);
        Task<MinecraftBlockRefDto> CreateAsync(MinecraftBlockRefCreateDto dto);
        Task UpdateAsync(int id, MinecraftBlockRefUpdateDto dto);
        Task DeleteAsync(int id);
        Task<PagedResultDto<MinecraftBlockRefListDto>> SearchAsync(PagedQueryDto queryDto);
    }
}
