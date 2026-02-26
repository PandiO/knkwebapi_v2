using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace knkwebapi_v2.Dtos
{
    /// <summary>
    /// Read DTO for EntityTypeConfiguration.
    /// Returned from GET endpoints.
    /// </summary>
    public class EntityTypeConfigurationReadDto
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = null!;

        [JsonPropertyName("entityTypeName")]
        public string EntityTypeName { get; set; } = null!;

        [JsonPropertyName("iconKey")]
        public string? IconKey { get; set; }

        [JsonPropertyName("customIconUrl")]
        public string? CustomIconUrl { get; set; }

        [JsonPropertyName("displayColor")]
        public string? DisplayColor { get; set; }

        [JsonPropertyName("sortOrder")]
        public int SortOrder { get; set; }

        [JsonPropertyName("isVisible")]
        public bool IsVisible { get; set; }

        [JsonPropertyName("defaultTableColumns")]
        public List<string>? DefaultTableColumns { get; set; }

        [JsonPropertyName("createdAt")]
        public DateTime CreatedAt { get; set; }

        [JsonPropertyName("updatedAt")]
        public DateTime UpdatedAt { get; set; }
    }

    /// <summary>
    /// Create DTO for EntityTypeConfiguration.
    /// Sent with POST requests.
    /// </summary>
    public class EntityTypeConfigurationCreateDto
    {
        [JsonPropertyName("entityTypeName")]
        public string EntityTypeName { get; set; } = null!;

        [JsonPropertyName("iconKey")]
        public string? IconKey { get; set; }

        [JsonPropertyName("customIconUrl")]
        public string? CustomIconUrl { get; set; }

        [JsonPropertyName("displayColor")]
        public string? DisplayColor { get; set; }

        [JsonPropertyName("sortOrder")]
        public int SortOrder { get; set; } = 0;

        [JsonPropertyName("isVisible")]
        public bool IsVisible { get; set; } = true;

        [JsonPropertyName("defaultTableColumns")]
        public List<string>? DefaultTableColumns { get; set; }
    }

    /// <summary>
    /// Update DTO for EntityTypeConfiguration.
    /// Sent with PUT/PATCH requests.
    /// </summary>
    public class EntityTypeConfigurationUpdateDto
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = null!;

        [JsonPropertyName("entityTypeName")]
        public string EntityTypeName { get; set; } = null!;

        [JsonPropertyName("iconKey")]
        public string? IconKey { get; set; }

        [JsonPropertyName("customIconUrl")]
        public string? CustomIconUrl { get; set; }

        [JsonPropertyName("displayColor")]
        public string? DisplayColor { get; set; }

        [JsonPropertyName("sortOrder")]
        public int SortOrder { get; set; }

        [JsonPropertyName("isVisible")]
        public bool IsVisible { get; set; }

        [JsonPropertyName("defaultTableColumns")]
        public List<string>? DefaultTableColumns { get; set; }
    }

    /// <summary>
    /// Merged metadata combining base EntityMetadata with EntityTypeConfiguration.
    /// Returned from endpoints that need full entity information including display properties.
    /// </summary>
    public class MergedEntityMetadataDto : EntityMetadataDto
    {
        [JsonPropertyName("configuration")]
        public EntityTypeConfigurationReadDto? Configuration { get; set; }

        [JsonPropertyName("iconKey")]
        public string? IconKey { get; set; }

        [JsonPropertyName("customIconUrl")]
        public string? CustomIconUrl { get; set; }

        [JsonPropertyName("displayColor")]
        public string? DisplayColor { get; set; }

        [JsonPropertyName("sortOrder")]
        public int SortOrder { get; set; } = 0;

        [JsonPropertyName("isVisible")]
        public bool IsVisible { get; set; } = true;
    }
}
