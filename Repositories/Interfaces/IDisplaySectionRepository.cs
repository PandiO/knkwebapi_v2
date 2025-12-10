using System.Collections.Generic;
using System.Threading.Tasks;
using knkwebapi_v2.Models;

namespace knkwebapi_v2.Repositories.Interfaces
{
    public interface IDisplaySectionRepository
    {
        Task<IEnumerable<DisplaySection>> GetAllReusableAsync();
        Task<DisplaySection?> GetByIdAsync(int id, bool includeRelated = true);
        Task<DisplaySection> CreateAsync(DisplaySection section);
        Task UpdateAsync(DisplaySection section);
        Task DeleteAsync(int id);
        Task<DisplaySection?> GetSourceSectionAsync(int sourceSectionId);
    }
}
