using System.Collections.Generic;
using System.Threading.Tasks;
using knkwebapi_v2.Dtos;
using knkwebapi_v2.Models;

namespace knkwebapi_v2.Services.Interfaces
{
    public interface IEntityTypeConfigurationService
    {
        /// <summary>
        /// Get configuration by ID.
        /// </summary>
        Task<EntityTypeConfigurationReadDto?> GetByIdAsync(string id);

        /// <summary>
        /// Get configuration for a specific entity type.
        /// Returns null if no configuration exists (entity uses defaults).
        /// </summary>
        Task<EntityTypeConfigurationReadDto?> GetByEntityTypeNameAsync(string entityTypeName);

        /// <summary>
        /// Get all configurations.
        /// </summary>
        Task<List<EntityTypeConfigurationReadDto>> GetAllAsync();

        /// <summary>
        /// Create a new entity type configuration.
        /// Validates that entityTypeName is a valid form-configurable entity.
        /// </summary>
        Task<EntityTypeConfigurationReadDto> CreateAsync(EntityTypeConfigurationCreateDto dto);

        /// <summary>
        /// Update an existing configuration.
        /// </summary>
        Task<EntityTypeConfigurationReadDto> UpdateAsync(EntityTypeConfigurationUpdateDto dto);

        /// <summary>
        /// Delete a configuration.
        /// Entity will revert to default display settings.
        /// </summary>
        Task<bool> DeleteAsync(string id);

        /// <summary>
        /// Get merged metadata combining base EntityMetadata with configuration.
        /// Returns EntityMetadata enriched with configuration properties (icon, color, etc).
        /// If no configuration exists, returns base metadata with default display values.
        /// </summary>
        Task<MergedEntityMetadataDto> GetMergedMetadataByEntityTypeAsync(string entityTypeName);

        /// <summary>
        /// Get all merged metadata for all entities.
        /// Combines base metadata from MetadataService with all configurations.
        /// Useful for UI that needs to display all entities with their display settings.
        /// </summary>
        Task<List<MergedEntityMetadataDto>> GetAllMergedMetadataAsync();
    }
}
