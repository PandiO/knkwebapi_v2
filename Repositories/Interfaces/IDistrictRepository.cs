using knkwebapi_v2.Models;

namespace knkwebapi_v2.Repositories
{
    public interface IDistrictRepository
    {
        Task<IEnumerable<District>> GetAllAsync();
        Task<District?> GetByIdAsync(int id);
        Task AddDistrictAsync(District district);
        Task UpdateDistrictAsync(District district);
        Task DeleteDistrictAsync(int id);
        Task<PagedResult<District>> SearchAsync(PagedQuery query);
    }
}
