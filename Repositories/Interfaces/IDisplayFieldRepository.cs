using System.Collections.Generic;
using System.Threading.Tasks;
using knkwebapi_v2.Models;

namespace knkwebapi_v2.Repositories.Interfaces
{
    public interface IDisplayFieldRepository
    {
        Task<IEnumerable<DisplayField>> GetAllReusableAsync();
        Task<DisplayField?> GetByIdAsync(int id);
        Task<DisplayField> CreateAsync(DisplayField field);
        Task UpdateAsync(DisplayField field);
        Task DeleteAsync(int id);
        Task<DisplayField?> GetSourceFieldAsync(int sourceFieldId);
    }
}
