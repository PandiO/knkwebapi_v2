using System.Collections.Generic;
using System.Threading.Tasks;
using knkwebapi_v2.Models;

namespace knkwebapi_v2.Repositories.Interfaces
{
    public interface IEntityTypeConfigurationRepository
    {
        Task<EntityTypeConfiguration?> GetByIdAsync(string id);
        Task<EntityTypeConfiguration?> GetByEntityTypeNameAsync(string entityTypeName);
        Task<List<EntityTypeConfiguration>> GetAllAsync();
        Task<EntityTypeConfiguration> CreateAsync(EntityTypeConfiguration configuration);
        Task<EntityTypeConfiguration> UpdateAsync(EntityTypeConfiguration configuration);
        Task<bool> DeleteAsync(string id);
        Task<bool> ExistsByEntityTypeNameAsync(string entityTypeName);
    }
}
