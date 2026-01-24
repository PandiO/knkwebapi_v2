using System;
using System.Collections.Generic;
using System.Linq;
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
        private readonly IFormTemplateValidationService _validationService;
        private readonly IMetadataService _metadataService;

        public FormConfigurationService(
            IFormConfigurationRepository repo,
            IMapper mapper,
            IFormTemplateValidationService validationService,
            IMetadataService metadataService)
        {
            _repo = repo;
            _mapper = mapper;
            _validationService = validationService;
            _metadataService = metadataService;
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

        public async Task<IEnumerable<FormConfigurationDto>> GetAllByEntityTypeNameAsync(string entityName, bool defaultOnly = false)
        {
            if (string.IsNullOrWhiteSpace(entityName)) return new List<FormConfigurationDto>();
            var list = await _repo.GetAllByEntityTypeNameAsync(entityName, defaultOnly);
            return _mapper.Map<IEnumerable<FormConfigurationDto>>(list);
        }

        public async Task<IEnumerable<FormConfigurationDto>> GetAllByEntityTypeNameAllAsync(string entityName)
        {
            if (string.IsNullOrWhiteSpace(entityName)) return new List<FormConfigurationDto>();
            var list = await _repo.GetAllByEntityTypeNameAllAsync(entityName);
            return _mapper.Map<IEnumerable<FormConfigurationDto>>(list);
        }

        public async Task<FormConfigurationDto> GetDefaultByEntityTypeNameAsync(string entityName)
        {
            if (string.IsNullOrWhiteSpace(entityName))
                throw new ArgumentException("EntityName is required.", nameof(entityName));

            var entity = await _repo.GetDefaultByEntityTypeNameAsync(entityName);
            if (entity == null)
                throw new KeyNotFoundException($"Default FormConfiguration for entity '{entityName}' not found.");

            return _mapper.Map<FormConfigurationDto>(entity);
        }

        public async Task<FormConfigurationDto> CreateAsync(FormConfigurationDto config)
        {
            if (config == null) throw new ArgumentNullException(nameof(config));
            if (string.IsNullOrWhiteSpace(config.EntityTypeName))
                throw new ArgumentException("EntityName is required.", nameof(config));
            if (config.Steps == null || config.Steps.Count == 0)
                throw new ArgumentException("At least one step is required.", nameof(config));

            var entity = _mapper.Map<FormConfiguration>(config);

            foreach (var step in entity.Steps)
            {
                if (string.IsNullOrWhiteSpace(step.StepName))
                    throw new ArgumentException("Step name is required for all steps.", nameof(config));
            }

            entity.CreatedAt = DateTime.UtcNow;
            entity.UpdatedAt = DateTime.UtcNow;

            // Validate configuration before saving
            var validationResult = await _validationService.ValidateConfigurationAsync(entity, _metadataService);
            if (!validationResult.IsValid)
            {
                throw new InvalidOperationException(
                    $"Cannot create FormConfiguration: {validationResult.Summary}");
            }

            await _repo.AddAsync(entity);
            return _mapper.Map<FormConfigurationDto>(entity);
        }

        public async Task UpdateAsync(int id, FormConfigurationDto config)
        {
            if (config == null) throw new ArgumentNullException(nameof(config));
            if (id <= 0) throw new ArgumentException("Invalid id.", nameof(id));

            var exists = await _repo.GetByIdAsync(id);
            if (exists == null) throw new KeyNotFoundException($"FormConfiguration with id {id} not found.");

            var incoming = _mapper.Map<FormConfiguration>(config);
            incoming.Id = id;
            incoming.UpdatedAt = DateTime.UtcNow;
            incoming.CreatedAt = exists.CreatedAt;
            
            // Preserve existing StepOrderJson if none provided
            if (string.IsNullOrWhiteSpace(incoming.StepOrderJson))
                incoming.StepOrderJson = exists.StepOrderJson;

            // Apply field ordering based on FieldOrderJson (parse GUID order and reorder fields)
            foreach (var step in incoming.Steps)
            {
                if (!string.IsNullOrWhiteSpace(step.FieldOrderJson))
                {
                    try
                    {
                        var guidOrder = System.Text.Json.JsonSerializer.Deserialize<List<string>>(step.FieldOrderJson) ?? new List<string>();
                        if (guidOrder.Count > 0 && step.Fields.Count > 0)
                        {
                            // Reorder fields based on the GUID order
                            var fieldsByGuid = step.Fields.ToDictionary(f => f.FieldGuid.ToString());
                            var reorderedFields = new List<FormField>();
                            
                            foreach (var guidStr in guidOrder)
                            {
                                if (fieldsByGuid.TryGetValue(guidStr, out var field))
                                {
                                    reorderedFields.Add(field);
                                }
                            }
                            
                            // Add any fields that weren't in the order list (shouldn't happen, but be safe)
                            foreach (var field in step.Fields)
                            {
                                if (!reorderedFields.Contains(field))
                                {
                                    reorderedFields.Add(field);
                                }
                            }
                            
                            // Replace the fields list with the reordered version
                            step.Fields.Clear();
                            foreach (var field in reorderedFields)
                            {
                                step.Fields.Add(field);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        // Log but don't fail if field reordering has issues
                        System.Diagnostics.Debug.WriteLine($"Warning: Failed to reorder fields: {ex.Message}");
                    }
                }
            }

            // Validate configuration before saving
            var validationResult = await _validationService.ValidateConfigurationAsync(incoming, _metadataService);
            if (!validationResult.IsValid)
            {
                throw new InvalidOperationException(
                    $"Cannot update FormConfiguration: {validationResult.Summary}");
            }

            await _repo.UpdateAsync(incoming);
        }

        public async Task DeleteAsync(int id)
        {
            if (id <= 0) throw new ArgumentException("Invalid id.", nameof(id));
            var existing = await _repo.GetByIdAsync(id);
            if (existing == null) throw new KeyNotFoundException($"FormConfiguration with id {id} not found.");

            await _repo.DeleteAsync(id);
        }

        public Task<IEnumerable<string>> GetEntityTypeNamesAsync()
        {
            return _repo.GetEntityTypeNamesAsync();
        }
    }
}
