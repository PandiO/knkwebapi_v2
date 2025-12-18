using knkwebapi_v2.Dtos;
using knkwebapi_v2.Models;

namespace knkwebapi_v2.Services.Interfaces
{
    public interface IItemBlueprintService
    {
        Task<IEnumerable<ItemBlueprintReadDto>> GetAllAsync();
        Task<ItemBlueprintReadDto?> GetByIdAsync(int id);
        Task<ItemBlueprintReadDto> CreateAsync(ItemBlueprintCreateDto dto);
        Task UpdateAsync(int id, ItemBlueprintUpdateDto dto);
        Task DeleteAsync(int id);
        Task<PagedResultDto<ItemBlueprintListDto>> SearchAsync(PagedQueryDto query);
    }
}
