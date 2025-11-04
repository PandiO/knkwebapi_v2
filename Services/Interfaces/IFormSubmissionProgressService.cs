using System.Collections.Generic;
using System.Threading.Tasks;
using knkwebapi_v2.Dtos;

namespace knkwebapi_v2.Services
{
    public interface IFormSubmissionProgressService
    {
        Task<IEnumerable<FormSubmissionProgressDto>> GetByUserIdAsync(int userId);
        Task<FormSubmissionProgressDto?> GetByIdAsync(int id);
        Task<FormSubmissionProgressDto> SaveProgressAsync(FormSubmissionProgressDto progress);
        Task<FormSubmissionProgressDto> UpdateProgressAsync(int id, FormSubmissionProgressDto progress);
        Task DeleteAsync(int id);
    }
}
