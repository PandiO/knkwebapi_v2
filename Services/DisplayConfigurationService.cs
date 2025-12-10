using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using knkwebapi_v2.Dtos;
using knkwebapi_v2.Models;
using knkwebapi_v2.Repositories.Interfaces;
using knkwebapi_v2.Services.Interfaces;

namespace knkwebapi_v2.Services
{
    public class DisplayConfigurationService : IDisplayConfigurationService
    {
        private readonly IDisplayConfigurationRepository _repo;
        private readonly IMapper _mapper;

        public DisplayConfigurationService(
            IDisplayConfigurationRepository repo,
            IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }

        public async Task<IEnumerable<DisplayConfigurationDto>> GetAllAsync(bool includeDrafts = true)
        {
            var configs = await _repo.GetAllAsync(includeDrafts);
            return _mapper.Map<IEnumerable<DisplayConfigurationDto>>(configs);
        }

        public async Task<DisplayConfigurationDto?> GetByIdAsync(int id)
        {
            if (id <= 0) return null;
            var config = await _repo.GetByIdAsync(id);
            return config == null ? null : _mapper.Map<DisplayConfigurationDto>(config);
        }

        public async Task<DisplayConfigurationDto?> GetDefaultByEntityTypeNameAsync(
            string entityTypeName,
            bool includeDrafts = false)
        {
            var config = await _repo.GetByEntityTypeNameAsync(
                entityTypeName, 
                defaultOnly: true, 
                includeDrafts: includeDrafts);
            return config == null ? null : _mapper.Map<DisplayConfigurationDto>(config);
        }

        public async Task<IEnumerable<DisplayConfigurationDto>> GetAllByEntityTypeNameAsync(
            string entityTypeName,
            bool includeDrafts = true)
        {
            var configs = await _repo.GetAllByEntityTypeNameAsync(entityTypeName, includeDrafts);
            return _mapper.Map<IEnumerable<DisplayConfigurationDto>>(configs);
        }

        public async Task<IEnumerable<string>> GetEntityTypeNamesAsync()
        {
            return await _repo.GetEntityTypeNamesAsync();
        }

        public async Task<DisplayConfigurationDto> CreateAsync(DisplayConfigurationDto dto)
        {
            if (dto == null) 
                throw new ArgumentNullException(nameof(dto));
            if (string.IsNullOrWhiteSpace(dto.EntityTypeName))
                throw new ArgumentException("EntityTypeName is required.", nameof(dto));

            // Check if default already exists
            if (dto.IsDefault)
            {
                var defaultExists = await _repo.IsDefaultExistsAsync(dto.EntityTypeName);
                if (defaultExists)
                {
                    throw new InvalidOperationException(
                        $"A default DisplayConfiguration for '{dto.EntityTypeName}' already exists.");
                }
            }

            var entity = _mapper.Map<DisplayConfiguration>(dto);
            entity.CreatedAt = DateTime.UtcNow;
            entity.IsDraft = true; // Always start as draft

            var created = await _repo.CreateAsync(entity);
            return _mapper.Map<DisplayConfigurationDto>(created);
        }

        public async Task UpdateAsync(int id, DisplayConfigurationDto dto)
        {
            if (dto == null) 
                throw new ArgumentNullException(nameof(dto));
            if (id <= 0) 
                throw new ArgumentException("Invalid id.", nameof(id));

            var existing = await _repo.GetByIdAsync(id, includeRelated: false);
            if (existing == null)
                throw new KeyNotFoundException($"DisplayConfiguration with id {id} not found.");

            // Check if default already exists (excluding current)
            if (dto.IsDefault)
            {
                var defaultExists = await _repo.IsDefaultExistsAsync(
                    dto.EntityTypeName, 
                    excludeId: id);
                if (defaultExists)
                {
                    throw new InvalidOperationException(
                        $"Another default DisplayConfiguration for '{dto.EntityTypeName}' already exists.");
                }
            }

            var entity = _mapper.Map<DisplayConfiguration>(dto);
            entity.Id = id;
            entity.CreatedAt = existing.CreatedAt;
            entity.UpdatedAt = DateTime.UtcNow;
            entity.IsDraft = existing.IsDraft; // Preserve draft status

            await _repo.UpdateAsync(entity);
        }

        public async Task DeleteAsync(int id)
        {
            if (id <= 0)
                throw new ArgumentException("Invalid id.", nameof(id));

            await _repo.DeleteAsync(id);
        }

        public async Task<DisplayConfigurationDto> PublishAsync(int id)
        {
            var config = await _repo.GetByIdAsync(id, includeRelated: false);
            if (config == null)
                throw new KeyNotFoundException($"DisplayConfiguration with id {id} not found.");

            if (!config.IsDraft)
            {
                throw new InvalidOperationException(
                    $"DisplayConfiguration {id} is already published.");
            }

            // Basic validation: check if has sections
            if (string.IsNullOrEmpty(config.SectionOrderJson) || config.SectionOrderJson == "[]")
            {
                throw new InvalidOperationException(
                    $"Cannot publish DisplayConfiguration {id}: No sections configured.");
            }

            // Set to published state
            config.IsDraft = false;
            config.UpdatedAt = DateTime.UtcNow;

            await _repo.UpdateAsync(config);
            
            // Reload with relationships
            var published = await _repo.GetByIdAsync(id);
            return _mapper.Map<DisplayConfigurationDto>(published);
        }
    }
}
