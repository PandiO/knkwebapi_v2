using System.Collections.Generic;
using System.Threading.Tasks;
using knkwebapi_v2.Dtos;

namespace knkwebapi_v2.Services
{
    public interface IDistrictService
    {
        Task<IEnumerable<DistrictDto>> GetAllAsync();
        Task<DistrictDto?> GetByIdAsync(int id);
        Task<DistrictDto> CreateAsync(DistrictDto districtDto);
        Task UpdateAsync(int id, DistrictDto districtDto);
        Task DeleteAsync(int id);
        Task<PagedResultDto<DistrictListDto>> SearchAsync(PagedQueryDto query);
    }
}
