using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using knkwebapi_v2.Dtos;
using knkwebapi_v2.Models;
using knkwebapi_v2.Repositories.Interfaces;
using knkwebapi_v2.Services.Interfaces;

namespace knkwebapi_v2.Services
{
    public class EntityTypeConfigurationService : IEntityTypeConfigurationService
    {
        private readonly IEntityTypeConfigurationRepository _repository;
        private readonly IMetadataService _metadataService;
        private readonly IMapper _mapper;

        public EntityTypeConfigurationService(
            IEntityTypeConfigurationRepository repository,
            IMetadataService metadataService,
            IMapper mapper)
        {
            _repository = repository;
            _metadataService = metadataService;
            _mapper = mapper;
        }

        public async Task<EntityTypeConfigurationReadDto?> GetByIdAsync(string id)
        {
            var config = await _repository.GetByIdAsync(id);
            return config != null ? _mapper.Map<EntityTypeConfigurationReadDto>(config) : null;
        }

        public async Task<EntityTypeConfigurationReadDto?> GetByEntityTypeNameAsync(string entityTypeName)
        {
            var config = await _repository.GetByEntityTypeNameAsync(entityTypeName);
            return config != null ? _mapper.Map<EntityTypeConfigurationReadDto>(config) : null;
        }

        public async Task<List<EntityTypeConfigurationReadDto>> GetAllAsync()
        {
            var configs = await _repository.GetAllAsync();
            return _mapper.Map<List<EntityTypeConfigurationReadDto>>(configs);
        }

        public async Task<EntityTypeConfigurationReadDto> CreateAsync(EntityTypeConfigurationCreateDto dto)
        {
            // Validate that the entity type exists
            var entityMetadata = _metadataService.GetEntityMetadata(dto.EntityTypeName);
            if (entityMetadata == null)
            {
                throw new ArgumentException(
                    $"Entity type '{dto.EntityTypeName}' is not a valid form-configurable entity."
                );
            }

            // Check if configuration already exists for this entity
            var existing = await _repository.GetByEntityTypeNameAsync(dto.EntityTypeName);
            if (existing != null)
            {
                throw new InvalidOperationException(
                    $"A configuration already exists for entity type '{dto.EntityTypeName}'."
                );
            }

            var config = _mapper.Map<EntityTypeConfiguration>(dto);
            var created = await _repository.CreateAsync(config);
            
            return _mapper.Map<EntityTypeConfigurationReadDto>(created);
        }

        public async Task<EntityTypeConfigurationReadDto> UpdateAsync(EntityTypeConfigurationUpdateDto dto)
        {
            var existing = await _repository.GetByIdAsync(dto.Id);
            if (existing == null)
            {
                throw new KeyNotFoundException($"Configuration with ID '{dto.Id}' not found.");
            }

            // Validate that the entity type exists
            var entityMetadata = _metadataService.GetEntityMetadata(dto.EntityTypeName);
            if (entityMetadata == null)
            {
                throw new ArgumentException(
                    $"Entity type '{dto.EntityTypeName}' is not a valid form-configurable entity."
                );
            }

            // If entity type name changed, ensure no other config uses the new name
            if (!existing.EntityTypeName.Equals(dto.EntityTypeName, StringComparison.OrdinalIgnoreCase))
            {
                var other = await _repository.GetByEntityTypeNameAsync(dto.EntityTypeName);
                if (other != null)
                {
                    throw new InvalidOperationException(
                        $"A configuration already exists for entity type '{dto.EntityTypeName}'."
                    );
                }
            }

            existing.EntityTypeName = dto.EntityTypeName;
            existing.IconKey = dto.IconKey;
            existing.CustomIconUrl = dto.CustomIconUrl;
            existing.DisplayColor = dto.DisplayColor;
            existing.SortOrder = dto.SortOrder;
            existing.IsVisible = dto.IsVisible;

            var updated = await _repository.UpdateAsync(existing);
            return _mapper.Map<EntityTypeConfigurationReadDto>(updated);
        }

        public async Task<bool> DeleteAsync(string id)
        {
            return await _repository.DeleteAsync(id);
        }

        public async Task<MergedEntityMetadataDto> GetMergedMetadataByEntityTypeAsync(string entityTypeName)
        {
            // Get base metadata
            var baseMetadata = _metadataService.GetEntityMetadata(entityTypeName);
            if (baseMetadata == null)
            {
                throw new KeyNotFoundException($"Entity type '{entityTypeName}' not found.");
            }

            // Get configuration (may be null)
            var config = await _repository.GetByEntityTypeNameAsync(entityTypeName);

            // Merge and return
            return MergeMetadata(baseMetadata, config);
        }

        public async Task<List<MergedEntityMetadataDto>> GetAllMergedMetadataAsync()
        {
            // Get all base metadata
            var allBaseMetadata = _metadataService.GetAllEntityMetadata();

            // Get all configurations
            var allConfigs = await _repository.GetAllAsync();
            var configsByEntity = allConfigs.ToDictionary(c => c.EntityTypeName, StringComparer.OrdinalIgnoreCase);

            // Merge all
            var merged = allBaseMetadata
                .Select(baseMetadata =>
                {
                    var config = configsByEntity.ContainsKey(baseMetadata.EntityName)
                        ? configsByEntity[baseMetadata.EntityName]
                        : null;
                    return MergeMetadata(baseMetadata, config);
                })
                .OrderBy(m => m.SortOrder)
                .ThenBy(m => m.DisplayName)
                .ToList();

            return merged;
        }

        /// <summary>
        /// Merges base EntityMetadata with EntityTypeConfiguration to create a complete
        /// metadata object with display properties.
        /// </summary>
        private MergedEntityMetadataDto MergeMetadata(EntityMetadataDto baseMetadata, EntityTypeConfiguration? config)
        {
            var merged = new MergedEntityMetadataDto
            {
                EntityName = baseMetadata.EntityName,
                DisplayName = baseMetadata.DisplayName,
                Fields = baseMetadata.Fields,
                Configuration = config != null ? _mapper.Map<EntityTypeConfigurationReadDto>(config) : null,
                IconKey = config?.IconKey,
                CustomIconUrl = config?.CustomIconUrl,
                DisplayColor = config?.DisplayColor,
                SortOrder = config?.SortOrder ?? 0,
                IsVisible = config?.IsVisible ?? true
            };

            return merged;
        }
    }
}
