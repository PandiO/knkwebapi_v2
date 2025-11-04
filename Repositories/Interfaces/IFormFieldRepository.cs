using System.Collections.Generic;
using System.Threading.Tasks;
using knkwebapi_v2.Models;

namespace knkwebapi_v2.Repositories
{
    public interface IFormFieldRepository
    {
        Task<IEnumerable<FormField>> GetAllReusableAsync();
        Task<FormField?> GetByIdAsync(int id);
        Task AddAsync(FormField field);
        Task UpdateAsync(FormField field);
        Task DeleteAsync(int id);
    }
}
