using System.Collections.Generic;
using System.Threading.Tasks;
using knkwebapi_v2.Models;

namespace knkwebapi_v2.Repositories
{
    public interface IFormStepRepository
    {
        Task<IEnumerable<FormStep>> GetAllReusableAsync();
        
        /// <summary>
        /// Get all reusable steps that were originally designed for a specific entity type.
        /// These are library templates that can be applied to configurations for that entity.
        /// </summary>
        Task<IEnumerable<FormStep>> GetAllReusableByEntityTypeAsync(string entityTypeName);
        
        Task<FormStep?> GetByIdAsync(int id);
        Task AddAsync(FormStep step);
        Task UpdateAsync(FormStep step);
        Task DeleteAsync(int id);
    }
}
