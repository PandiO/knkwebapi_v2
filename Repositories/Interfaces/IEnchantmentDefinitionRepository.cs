using knkwebapi_v2.Models;
using knkwebapi_v2.Properties;

namespace knkwebapi_v2.Repositories.Interfaces
{
    public interface IEnchantmentDefinitionRepository
    {
        Task<IEnumerable<EnchantmentDefinition>> GetAllAsync();
        Task<EnchantmentDefinition?> GetByIdAsync(int id);
        Task AddAsync(EnchantmentDefinition entity);
        Task UpdateAsync(EnchantmentDefinition entity);
        Task DeleteAsync(int id);
        Task<PagedResult<EnchantmentDefinition>> SearchAsync(PagedQuery query);
    }
}
