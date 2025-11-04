using System.Collections.Generic;
using System.Threading.Tasks;
using knkwebapi_v2.Models;

namespace knkwebapi_v2.Services
{
    public interface IFormSubmissionProgressService
    {
        Task<IEnumerable<FormSubmissionProgress>> GetByUserIdAsync(int userId);
        Task<FormSubmissionProgress?> GetByIdAsync(int id);
        Task<FormSubmissionProgress> SaveProgressAsync(FormSubmissionProgress progress);
        Task<FormSubmissionProgress> UpdateProgressAsync(int id, FormSubmissionProgress progress);
        Task DeleteAsync(int id);
    }
}
