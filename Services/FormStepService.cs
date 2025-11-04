using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using knkwebapi_v2.Models;
using knkwebapi_v2.Repositories;

namespace knkwebapi_v2.Services
{
    public class FormStepService : IFormStepService
    {
        private readonly IFormStepRepository _repo;

        public FormStepService(IFormStepRepository repo)
        {
            _repo = repo;
        }

        public async Task<IEnumerable<FormStep>> GetAllReusableAsync()
        {
            return await _repo.GetAllReusableAsync();
        }

        public async Task<FormStep?> GetByIdAsync(int id)
        {
            if (id <= 0) return null;
            return await _repo.GetByIdAsync(id);
        }

        public async Task<FormStep> CreateAsync(FormStep step)
        {
            if (step == null) throw new ArgumentNullException(nameof(step));
            if (string.IsNullOrWhiteSpace(step.StepName)) 
                throw new ArgumentException("StepName is required.", nameof(step));

            await _repo.AddAsync(step);
            return step;
        }

        public async Task UpdateAsync(int id, FormStep step)
        {
            if (step == null) throw new ArgumentNullException(nameof(step));
            if (id <= 0) throw new ArgumentException("Invalid id.", nameof(id));

            var existing = await _repo.GetByIdAsync(id);
            if (existing == null) throw new KeyNotFoundException($"FormStep with id {id} not found.");

            existing.StepName = step.StepName;
            existing.Description = step.Description;
            existing.IsReusable = step.IsReusable;
            existing.FieldOrderJson = step.FieldOrderJson;

            await _repo.UpdateAsync(existing);
        }

        public async Task DeleteAsync(int id)
        {
            if (id <= 0) throw new ArgumentException("Invalid id.", nameof(id));
            var existing = await _repo.GetByIdAsync(id);
            if (existing == null) throw new KeyNotFoundException($"FormStep with id {id} not found.");

            await _repo.DeleteAsync(id);
        }
    }
}
