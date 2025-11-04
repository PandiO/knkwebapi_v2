using System.Collections.Generic;
using System.Threading.Tasks;
using knkwebapi_v2.Models;

namespace knkwebapi_v2.Services
{
    public interface IFormFieldService
    {
        Task<IEnumerable<FormField>> GetAllReusableAsync();
        Task<FormField?> GetByIdAsync(int id);
        Task<FormField> CreateAsync(FormField field);
        Task UpdateAsync(int id, FormField field);
        Task DeleteAsync(int id);
    }
}
