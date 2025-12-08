using knkwebapi_v2.Models;

namespace knkwebapi_v2.Repositories
{
    public interface ITownRepository
    {
        Task<IEnumerable<Town>> GetAllAsync();
        Task<Town?> GetByIdAsync(int id);
        Task AddTownAsync(Town town);
        Task UpdateTownAsync(Town town);
        Task DeleteTownAsync(int id);
        Task<PagedResult<Town>> SearchAsync(PagedQuery query);
    }
}
