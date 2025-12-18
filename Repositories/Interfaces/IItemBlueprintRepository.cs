using knkwebapi_v2.Models;
using knkwebapi_v2.Properties;

namespace knkwebapi_v2.Repositories.Interfaces
{
    public interface IItemBlueprintRepository
    {
        Task<IEnumerable<ItemBlueprint>> GetAllAsync();
        Task<ItemBlueprint?> GetByIdAsync(int id);
        Task AddAsync(ItemBlueprint entity);
        Task UpdateAsync(ItemBlueprint entity);
        Task DeleteAsync(int id);
        Task<PagedResult<ItemBlueprint>> SearchAsync(PagedQuery query);
    }
}
