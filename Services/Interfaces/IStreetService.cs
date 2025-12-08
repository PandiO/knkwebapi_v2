using System.Collections.Generic;
using System.Threading.Tasks;
using knkwebapi_v2.Dtos;

namespace knkwebapi_v2.Services
{
    public interface IStreetService
    {
        Task<IEnumerable<StreetDto>> GetAllAsync();
        Task<StreetDto?> GetByIdAsync(int id);
        Task<StreetDto> CreateAsync(StreetDto streetDto);
        Task UpdateAsync(int id, StreetDto streetDto);
        Task DeleteAsync(int id);
        Task<PagedResultDto<StreetListDto>> SearchAsync(PagedQueryDto query);
    }
}
