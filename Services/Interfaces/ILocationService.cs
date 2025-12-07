using System.Collections.Generic;
using System.Threading.Tasks;
using knkwebapi_v2.Dtos;

namespace knkwebapi_v2.Services
{
    public interface ILocationService
    {
        Task<IEnumerable<LocationDto>> GetAllAsync();
        Task<LocationDto?> GetByIdAsync(int id);
        Task<LocationDto> CreateAsync(LocationDto locationDto);
        Task UpdateAsync(int id, LocationDto locationDto);
        Task DeleteAsync(int id);
        Task<PagedResultDto<LocationDto>> SearchAsync(PagedQueryDto query);
    }
}
