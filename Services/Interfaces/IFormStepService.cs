using System.Collections.Generic;
using System.Threading.Tasks;
using knkwebapi_v2.Models;

namespace knkwebapi_v2.Services
{
    public interface IFormStepService
    {
        Task<IEnumerable<FormStep>> GetAllReusableAsync();
        Task<FormStep?> GetByIdAsync(int id);
        Task<FormStep> CreateAsync(FormStep step);
        Task UpdateAsync(int id, FormStep step);
        Task DeleteAsync(int id);
    }
}
