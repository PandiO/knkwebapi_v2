using System.Collections.Generic;
using System.Threading.Tasks;
using knkwebapi_v2.Dtos;

namespace knkwebapi_v2.Services
{
    public interface IFormConfigurationService
    {
        Task<IEnumerable<FormConfigurationDto>> GetAllAsync();
        Task<FormConfigurationDto?> GetByIdAsync(int id);
        Task<IEnumerable<FormConfigurationDto>> GetAllByEntityTypeNameAsync(string entityName, bool defaultOnly = false);
        Task<IEnumerable<FormConfigurationDto>> GetAllByEntityTypeNameAllAsync(string entityName);
        Task<FormConfigurationDto> GetDefaultByEntityTypeNameAsync(string entityName);
        Task<IEnumerable<string>> GetEntityTypeNamesAsync();
        Task<FormConfigurationDto> CreateAsync(FormConfigurationDto config);
        Task UpdateAsync(int id, FormConfigurationDto config);
        Task DeleteAsync(int id);
    }
}
