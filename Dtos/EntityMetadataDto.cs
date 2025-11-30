using System.Collections.Generic;

namespace knkwebapi_v2.Dtos
{
    public class EntityMetadataDto
    {
        public string EntityName { get; set; } = null!;
        public string DisplayName { get; set; } = null!;
        public List<FieldMetadataDto> Fields { get; set; } = new();
    }

    public class FieldMetadataDto
    {
        public string FieldName { get; set; } = null!;
        public string FieldType { get; set; } = null!;
        public bool IsNullable { get; set; }
        public bool IsRelatedEntity { get; set; }
        public string? RelatedEntityType { get; set; }
    }
}
