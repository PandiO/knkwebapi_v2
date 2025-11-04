using System.Collections.Generic;
using System.Threading.Tasks;
using knkwebapi_v2.Dtos;

namespace knkwebapi_v2.Services
{
    public interface IFormConfigurationService
    {
        Task<IEnumerable<FormConfigurationDto>> GetAllAsync();
        Task<FormConfigurationDto?> GetByIdAsync(int id);
        Task<FormConfigurationDto?> GetByEntityNameAsync(string entityName, bool defaultOnly = false);
        Task<IEnumerable<FormConfigurationDto>> GetByEntityNameAllAsync(string entityName);
        Task<FormConfigurationDto> CreateAsync(FormConfigurationDto config);
        Task UpdateAsync(int id, FormConfigurationDto config);
        Task DeleteAsync(int id);
    }
}
