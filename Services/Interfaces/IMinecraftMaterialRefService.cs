using knkwebapi_v2.Dtos;

namespace knkwebapi_v2.Services
{
    public interface IMinecraftMaterialRefService
    {
        Task<IEnumerable<MinecraftMaterialRefDto>> GetAllAsync();
        Task<MinecraftMaterialRefDto?> GetByIdAsync(int id);
        Task<MinecraftMaterialRefDto> CreateAsync(MinecraftMaterialRefCreateDto dto);
        Task<MinecraftMaterialRefDto> GetOrCreateAsync(string namespaceKey, string category, string? legacyName = null);
        Task<IEnumerable<MinecraftHybridMaterialOptionDto>> GetHybridAsync(string? search = null, string? category = null, int? take = null);
        Task<PagedResultDto<MinecraftMaterialRefListDto>> SearchHybridAsync(PagedQueryDto queryDto);
        Task UpdateAsync(int id, MinecraftMaterialRefUpdateDto dto);
        Task DeleteAsync(int id);
        Task<PagedResultDto<MinecraftMaterialRefListDto>> SearchAsync(PagedQueryDto queryDto);
    }
}
