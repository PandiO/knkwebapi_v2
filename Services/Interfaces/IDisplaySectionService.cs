using System.Collections.Generic;
using System.Threading.Tasks;
using knkwebapi_v2.Dtos;
using knkwebapi_v2.Enums;

namespace knkwebapi_v2.Services.Interfaces
{
    public interface IDisplaySectionService
    {
        Task<IEnumerable<DisplaySectionDto>> GetAllReusableAsync();
        Task<DisplaySectionDto?> GetByIdAsync(int id);
        Task<DisplaySectionDto> CreateReusableAsync(DisplaySectionDto dto);
        Task UpdateAsync(int id, DisplaySectionDto dto);
        Task DeleteAsync(int id);
        Task<DisplaySectionDto> CloneSectionAsync(
            int sourceSectionId, 
            ReuseLinkMode linkMode);
    }
}
