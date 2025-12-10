using System.Collections.Generic;
using System.Threading.Tasks;
using knkwebapi_v2.Dtos;
using knkwebapi_v2.Enums;

namespace knkwebapi_v2.Services.Interfaces
{
    public interface IDisplayFieldService
    {
        Task<IEnumerable<DisplayFieldDto>> GetAllReusableAsync();
        Task<DisplayFieldDto?> GetByIdAsync(int id);
        Task<DisplayFieldDto> CreateReusableAsync(DisplayFieldDto dto);
        Task UpdateAsync(int id, DisplayFieldDto dto);
        Task DeleteAsync(int id);
        Task<DisplayFieldDto> CloneFieldAsync(
            int sourceFieldId, 
            ReuseLinkMode linkMode);
    }
}
