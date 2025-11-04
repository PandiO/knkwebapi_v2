using System.Collections.Generic;
using System.Threading.Tasks;
using knkwebapi_v2.Dtos;

namespace knkwebapi_v2.Services
{
    public interface IFormStepService
    {
        Task<IEnumerable<FormStepDto>> GetAllReusableAsync();
        Task<FormStepDto?> GetByIdAsync(int id);
        Task<FormStepDto> CreateAsync(FormStepDto step);
        Task UpdateAsync(int id, FormStepDto step);
        Task DeleteAsync(int id);
    }
}
