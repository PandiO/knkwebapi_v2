using knkwebapi_v2.Models;

namespace knkwebapi_v2.Repositories
{
    public interface IStreetRepository
    {
        Task<IEnumerable<Street>> GetAllAsync();
        Task<Street?> GetByIdAsync(int id);
        Task AddStreetAsync(Street street);
        Task UpdateStreetAsync(Street street);
        Task DeleteStreetAsync(int id);
        Task<PagedResult<Street>> SearchAsync(PagedQuery query);
    }
}
