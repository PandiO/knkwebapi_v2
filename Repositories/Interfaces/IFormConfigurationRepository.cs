using System.Collections.Generic;
using System.Threading.Tasks;
using knkwebapi_v2.Models;

namespace knkwebapi_v2.Repositories
{
    public interface IFormConfigurationRepository
    {
        Task<IEnumerable<FormConfiguration>> GetAllAsync();
        Task<FormConfiguration?> GetByIdAsync(int id);
        Task<FormConfiguration?> GetByEntityNameAsync(string entityName, bool defaultOnly = false);
        Task<IEnumerable<FormConfiguration>> GetByEntityNameAllAsync(string entityName);
        Task<IEnumerable<string>> GetEntityNamesAsync();
        Task AddAsync(FormConfiguration config);
        Task UpdateAsync(FormConfiguration config);
        Task DeleteAsync(int id);
    }
}
