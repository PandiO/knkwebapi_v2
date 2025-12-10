using System.Collections.Generic;
using System.Threading.Tasks;
using knkwebapi_v2.Dtos;

namespace knkwebapi_v2.Services.Interfaces
{
    public interface IDisplayConfigurationService
    {
        Task<IEnumerable<DisplayConfigurationDto>> GetAllAsync(bool includeDrafts = true);
        Task<DisplayConfigurationDto?> GetByIdAsync(int id);
        Task<DisplayConfigurationDto?> GetDefaultByEntityTypeNameAsync(
            string entityTypeName,
            bool includeDrafts = false);
        Task<IEnumerable<DisplayConfigurationDto>> GetAllByEntityTypeNameAsync(
            string entityTypeName,
            bool includeDrafts = true);
        Task<IEnumerable<string>> GetEntityTypeNamesAsync();
        Task<DisplayConfigurationDto> CreateAsync(DisplayConfigurationDto dto);
        Task UpdateAsync(int id, DisplayConfigurationDto dto);
        Task DeleteAsync(int id);
        Task<DisplayConfigurationDto> PublishAsync(int id);
    }
}
