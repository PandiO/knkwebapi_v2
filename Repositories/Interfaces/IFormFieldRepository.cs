using System.Collections.Generic;
using System.Threading.Tasks;
using knkwebapi_v2.Models;

namespace knkwebapi_v2.Repositories
{
    public interface IFormFieldRepository
    {
        Task<IEnumerable<FormField>> GetAllReusableAsync();
        
        /// <summary>
        /// Get all reusable fields that were originally designed for a specific entity type.
        /// These are library templates that can be applied to steps for that entity.
        /// </summary>
        Task<IEnumerable<FormField>> GetAllReusableByEntityTypeAsync(string entityTypeName);
        
        Task<FormField?> GetByIdAsync(int id);
        Task AddAsync(FormField field);
        Task UpdateAsync(FormField field);
        Task DeleteAsync(int id);
    }
}
