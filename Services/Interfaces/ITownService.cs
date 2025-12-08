using System.Collections.Generic;
using System.Threading.Tasks;
using knkwebapi_v2.Dtos;

namespace knkwebapi_v2.Services
{
    public interface ITownService
    {
        Task<IEnumerable<TownDto>> GetAllAsync();
        Task<TownDto?> GetByIdAsync(int id);
        Task<TownDto> CreateAsync(TownDto townDto);
        Task UpdateAsync(int id, TownDto townDto);
        Task DeleteAsync(int id);
        Task<PagedResultDto<TownListDto>> SearchAsync(PagedQueryDto query);
    }
}
