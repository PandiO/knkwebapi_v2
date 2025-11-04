using System.Collections.Generic;
using System.Threading.Tasks;
using knkwebapi_v2.Models;

namespace knkwebapi_v2.Services
{
    public interface IFormConfigurationService
    {
        Task<IEnumerable<FormConfiguration>> GetAllAsync();
        Task<FormConfiguration?> GetByIdAsync(int id);
        Task<FormConfiguration?> GetByEntityNameAsync(string entityName, bool defaultOnly = false);
        Task<IEnumerable<FormConfiguration>> GetByEntityNameAllAsync(string entityName);
        Task<FormConfiguration> CreateAsync(FormConfiguration config);
        Task UpdateAsync(int id, FormConfiguration config);
        Task DeleteAsync(int id);
    }
}
