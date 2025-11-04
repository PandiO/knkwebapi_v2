using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using knkwebapi_v2.Models;
using knkwebapi_v2.Repositories;

namespace knkwebapi_v2.Services
{
    public class FormSubmissionProgressService : IFormSubmissionProgressService
    {
        private readonly IFormSubmissionProgressRepository _repo;

        public FormSubmissionProgressService(IFormSubmissionProgressRepository repo)
        {
            _repo = repo;
        }

        public async Task<IEnumerable<FormSubmissionProgress>> GetByUserIdAsync(int userId)
        {
            if (userId <= 0) return new List<FormSubmissionProgress>();
            return await _repo.GetByUserIdAsync(userId);
        }

        public async Task<FormSubmissionProgress?> GetByIdAsync(int id)
        {
            if (id <= 0) return null;
            return await _repo.GetByIdAsync(id);
        }

        public async Task<FormSubmissionProgress> SaveProgressAsync(FormSubmissionProgress progress)
        {
            if (progress == null) throw new ArgumentNullException(nameof(progress));
            if (progress.UserId <= 0) throw new ArgumentException("UserId is required.", nameof(progress));
            if (progress.FormConfigurationId <= 0) throw new ArgumentException("FormConfigurationId is required.", nameof(progress));

            await _repo.AddAsync(progress);
            return progress;
        }

        public async Task<FormSubmissionProgress> UpdateProgressAsync(int id, FormSubmissionProgress progress)
        {
            if (progress == null) throw new ArgumentNullException(nameof(progress));
            if (id <= 0) throw new ArgumentException("Invalid id.", nameof(id));

            var existing = await _repo.GetByIdAsync(id);
            if (existing == null) throw new KeyNotFoundException($"FormSubmissionProgress with id {id} not found.");

            existing.CurrentStepIndex = progress.CurrentStepIndex;
            existing.CurrentStepDataJson = progress.CurrentStepDataJson;
            existing.AllStepsDataJson = progress.AllStepsDataJson;
            existing.Status = progress.Status;
            
            if (progress.Status == "Completed")
            {
                existing.CompletedAt = DateTime.UtcNow;
            }

            await _repo.UpdateAsync(existing);
            return existing;
        }

        public async Task DeleteAsync(int id)
        {
            if (id <= 0) throw new ArgumentException("Invalid id.", nameof(id));
            var existing = await _repo.GetByIdAsync(id);
            if (existing == null) throw new KeyNotFoundException($"FormSubmissionProgress with id {id} not found.");

            await _repo.DeleteAsync(id);
        }
    }
}
