using System.Collections.Generic;
using System.Threading.Tasks;
using knkwebapi_v2.Dtos;

namespace knkwebapi_v2.Services
{
    public interface IFormFieldService
    {
        Task<IEnumerable<FormFieldDto>> GetAllReusableAsync();
        Task<FormFieldDto?> GetByIdAsync(int id);
        Task<FormFieldDto> CreateAsync(FormFieldDto field);
        Task UpdateAsync(int id, FormFieldDto field);
        Task DeleteAsync(int id);
    }
}
