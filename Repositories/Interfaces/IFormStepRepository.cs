using System.Collections.Generic;
using System.Threading.Tasks;
using knkwebapi_v2.Models;

namespace knkwebapi_v2.Repositories
{
    public interface IFormStepRepository
    {
        Task<IEnumerable<FormStep>> GetAllReusableAsync();
        Task<FormStep?> GetByIdAsync(int id);
        Task AddAsync(FormStep step);
        Task UpdateAsync(FormStep step);
        Task DeleteAsync(int id);
    }
}
