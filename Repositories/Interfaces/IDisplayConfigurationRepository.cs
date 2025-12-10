using System.Collections.Generic;
using System.Threading.Tasks;
using knkwebapi_v2.Models;

namespace knkwebapi_v2.Repositories.Interfaces
{
    public interface IDisplayConfigurationRepository
    {
        Task<IEnumerable<DisplayConfiguration>> GetAllAsync(bool includeDrafts = true);
        Task<DisplayConfiguration?> GetByIdAsync(int id, bool includeRelated = true);
        Task<DisplayConfiguration?> GetByEntityTypeNameAsync(
            string entityTypeName, 
            bool defaultOnly = false,
            bool includeDrafts = true);
        Task<IEnumerable<DisplayConfiguration>> GetAllByEntityTypeNameAsync(
            string entityTypeName,
            bool includeDrafts = true);
        Task<DisplayConfiguration> CreateAsync(DisplayConfiguration config);
        Task UpdateAsync(DisplayConfiguration config);
        Task DeleteAsync(int id);
        Task<bool> IsDefaultExistsAsync(
            string entityTypeName, 
            int? excludeId = null);
        Task<IEnumerable<string>> GetEntityTypeNamesAsync();
    }
}
