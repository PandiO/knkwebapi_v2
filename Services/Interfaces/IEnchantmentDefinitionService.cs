using knkwebapi_v2.Dtos;
using knkwebapi_v2.Models;

namespace knkwebapi_v2.Services.Interfaces
{
    public interface IEnchantmentDefinitionService
    {
        Task<IEnumerable<EnchantmentDefinitionReadDto>> GetAllAsync();
        Task<EnchantmentDefinitionReadDto?> GetByIdAsync(int id);
        Task<EnchantmentDefinitionReadDto> CreateAsync(EnchantmentDefinitionCreateDto dto);
        Task UpdateAsync(int id, EnchantmentDefinitionUpdateDto dto);
        Task DeleteAsync(int id);
        Task<PagedResultDto<EnchantmentDefinitionListDto>> SearchAsync(PagedQueryDto query);
    }
}
