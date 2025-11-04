using knkwebapi_v2.Models;

namespace knkwebapi_v2.Services
{
    public interface IDomainService
    {
        Task<IEnumerable<Domain>> GetAllAsync();
        Task<Domain?> GetByIdAsync(int id);
        Task<Domain> CreateAsync(Domain domain);
        Task UpdateAsync(int id, Domain domain);
        Task DeleteAsync(int id);
    }
}
