using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace knkwebapi_v2.Dtos
{
    public class EntityMetadataDto
    {
        [JsonPropertyName("entityName")]
        public string EntityName { get; set; } = null!;
        [JsonPropertyName("displayName")]
        public string DisplayName { get; set; } = null!;
        [JsonPropertyName("fields")]
        public List<FieldMetadataDto> Fields { get; set; } = new();
    }

    public class FieldMetadataDto
    {
        [JsonPropertyName("fieldName")]
        public string FieldName { get; set; } = null!;
        [JsonPropertyName("fieldType")]
        public string FieldType { get; set; } = null!;
        [JsonPropertyName("isNullable")]
        public bool IsNullable { get; set; }
        [JsonPropertyName("isRelatedEntity")]
        public bool IsRelatedEntity { get; set; }
        [JsonPropertyName("relatedEntityType")]
        public string? RelatedEntityType { get; set; }
    }
}
