using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using knkwebapi_v2.Models;
using knkwebapi_v2.Repositories;
using knkwebapi_v2.Dtos;
using AutoMapper;

namespace knkwebapi_v2.Services
{
    public class FormConfigurationService : IFormConfigurationService
    {
        private readonly IFormConfigurationRepository _repo;
        private readonly IMapper _mapper;

        public FormConfigurationService(IFormConfigurationRepository repo, IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }

        public async Task<IEnumerable<FormConfigurationDto>> GetAllAsync()
        {
            var list = await _repo.GetAllAsync();
            return _mapper.Map<IEnumerable<FormConfigurationDto>>(list);
        }

        public async Task<FormConfigurationDto?> GetByIdAsync(int id)
        {
            if (id <= 0) return null;
            var entity = await _repo.GetByIdAsync(id);
            return entity == null ? null : _mapper.Map<FormConfigurationDto>(entity);
        }

        public async Task<FormConfigurationDto?> GetByEntityNameAsync(string entityName, bool defaultOnly = false)
        {
            if (string.IsNullOrWhiteSpace(entityName)) return null;
            var entity = await _repo.GetByEntityNameAsync(entityName, defaultOnly);
            return entity == null ? null : _mapper.Map<FormConfigurationDto>(entity);
        }

        public async Task<IEnumerable<FormConfigurationDto>> GetByEntityNameAllAsync(string entityName)
        {
            if (string.IsNullOrWhiteSpace(entityName)) return new List<FormConfigurationDto>();
            var list = await _repo.GetByEntityNameAllAsync(entityName);
            return _mapper.Map<IEnumerable<FormConfigurationDto>>(list);
        }

        public async Task<FormConfigurationDto> CreateAsync(FormConfigurationDto config)
        {
            if (config == null) throw new ArgumentNullException(nameof(config));
            if (string.IsNullOrWhiteSpace(config.EntityName))
                throw new ArgumentException("EntityName is required.", nameof(config));
            if (config.Steps == null || config.Steps.Count == 0)
                throw new ArgumentException("At least one step is required.", nameof(config));

            var entity = _mapper.Map<FormConfiguration>(config);

            foreach (var step in entity.Steps)
            {
                if (string.IsNullOrWhiteSpace(step.StepName))
                    throw new ArgumentException("Step name is required for all steps.", nameof(config));
            }

            await _repo.AddAsync(entity);
            return _mapper.Map<FormConfigurationDto>(entity);
        }

        public async Task UpdateAsync(int id, FormConfigurationDto config)
        {
            if (config == null) throw new ArgumentNullException(nameof(config));
            if (id <= 0) throw new ArgumentException("Invalid id.", nameof(id));

            var existing = await _repo.GetByIdAsync(id);
            if (existing == null) throw new KeyNotFoundException($"FormConfiguration with id {id} not found.");

            var incoming = _mapper.Map<FormConfiguration>(config);
            existing.EntityName = incoming.EntityName;
            existing.IsDefault = incoming.IsDefault;
            existing.Description = incoming.Description;
            existing.StepOrderJson = incoming.StepOrderJson;
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
