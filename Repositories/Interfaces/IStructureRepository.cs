using knkwebapi_v2.Models;

namespace knkwebapi_v2.Repositories
{
    public interface IStructureRepository
    {
        Task<IEnumerable<Structure>> GetAllAsync();
        Task<Structure?> GetByIdAsync(int id);
        Task AddStructureAsync(Structure structure);
        Task UpdateStructureAsync(Structure structure);
        Task DeleteStructureAsync(int id);
        Task<PagedResult<Structure>> SearchAsync(PagedQuery query);
    }
}
