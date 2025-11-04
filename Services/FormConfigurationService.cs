using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using knkwebapi_v2.Models;
using knkwebapi_v2.Repositories;

namespace knkwebapi_v2.Services
{
    public class FormConfigurationService : IFormConfigurationService
    {
        private readonly IFormConfigurationRepository _repo;

        public FormConfigurationService(IFormConfigurationRepository repo)
        {
            _repo = repo;
        }

        public async Task<IEnumerable<FormConfiguration>> GetAllAsync()
        {
            return await _repo.GetAllAsync();
        }

        public async Task<FormConfiguration?> GetByIdAsync(int id)
        {
            if (id <= 0) return null;
            return await _repo.GetByIdAsync(id);
        }

        public async Task<FormConfiguration?> GetByEntityNameAsync(string entityName, bool defaultOnly = false)
        {
            if (string.IsNullOrWhiteSpace(entityName)) return null;
            return await _repo.GetByEntityNameAsync(entityName, defaultOnly);
        }

        public async Task<IEnumerable<FormConfiguration>> GetByEntityNameAllAsync(string entityName)
        {
            if (string.IsNullOrWhiteSpace(entityName)) return new List<FormConfiguration>();
            return await _repo.GetByEntityNameAllAsync(entityName);
        }

        public async Task<FormConfiguration> CreateAsync(FormConfiguration config)
        {
            if (config == null) throw new ArgumentNullException(nameof(config));
            if (string.IsNullOrWhiteSpace(config.EntityName)) 
                throw new ArgumentException("EntityName is required.", nameof(config));
            if (config.Steps == null || config.Steps.Count == 0) 
                throw new ArgumentException("At least one step is required.", nameof(config));

            // Validate each step has fields
            foreach (var step in config.Steps)
            {
                if (string.IsNullOrWhiteSpace(step.StepName))
                    throw new ArgumentException($"Step name is required for all steps.", nameof(config));
            }

            await _repo.AddAsync(config);
            return config;
        }

        public async Task UpdateAsync(int id, FormConfiguration config)
        {
            if (config == null) throw new ArgumentNullException(nameof(config));
            if (id <= 0) throw new ArgumentException("Invalid id.", nameof(id));

            var existing = await _repo.GetByIdAsync(id);
            if (existing == null) throw new KeyNotFoundException($"FormConfiguration with id {id} not found.");

            existing.EntityName = config.EntityName;
            existing.IsDefault = config.IsDefault;
            existing.Description = config.Description;
            existing.StepOrderJson = config.StepOrderJson;
            existing.UpdatedAt = DateTime.UtcNow;

            await _repo.UpdateAsync(existing);
        }

        public async Task DeleteAsync(int id)
        {
            if (id <= 0) throw new ArgumentException("Invalid id.", nameof(id));
            var existing = await _repo.GetByIdAsync(id);
            if (existing == null) throw new KeyNotFoundException($"FormConfiguration with id {id} not found.");

            await _repo.DeleteAsync(id);
        }
    }
}
