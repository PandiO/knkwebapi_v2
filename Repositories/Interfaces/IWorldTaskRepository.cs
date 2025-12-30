using knkwebapi_v2.Models;

namespace knkwebapi_v2.Repositories.Interfaces
{
    public interface IWorldTaskRepository
    {
        Task<WorldTask?> GetByIdAsync(int id);
        Task<WorldTask?> GetByLinkCodeAsync(string linkCode);
        Task<List<WorldTask>> ListByStatusAsync(string status, string? serverId = null);
        Task AddAsync(WorldTask task);
        Task UpdateAsync(WorldTask task);
        Task DeleteAsync(int id);

        Task<PagedResult<WorldTask>> SearchAsync(PagedQuery query);
        Task<List<WorldTask>> GetBySessionAsync(int sessionId);
        Task<PagedResult<WorldTask>> GetByUserAsync(int userId, PagedQuery query);
    }
}
