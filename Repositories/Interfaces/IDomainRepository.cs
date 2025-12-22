using knkwebapi_v2.Models;

namespace knkwebapi_v2.Repositories
{
    public interface IDomainRepository
    {
        Task<IEnumerable<Domain>> GetAllAsync();
        Task<Domain?> GetByIdAsync(int id);
        Task AddDomainAsync(Domain domain);
        Task UpdateDomainAsync(Domain domain);
        Task DeleteDomainAsync(int id);
    }
}
