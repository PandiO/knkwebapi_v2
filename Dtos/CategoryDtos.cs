using System.Text.Json.Serialization;

namespace knkwebapi_v2.Dtos
{
    public class CategoryDto
    {
        [JsonPropertyName("id")]
        public int? Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; } = null!;

        [JsonPropertyName("iconMaterialRefId")]
        public int? IconMaterialRefId { get; set; }
        // Optional namespace key for hybrid material picker selections when the material is not yet persisted
        [JsonPropertyName("iconNamespaceKey")]
        public string? IconNamespaceKey { get; set; }

        [JsonPropertyName("parentCategoryId")]
        public int? ParentCategoryId { get; set; }
        [JsonPropertyName("parentCategory")]
        public CategoryDto? ParentCategory { get; set; }

        // Children of this category
        [JsonPropertyName("childCategories")]
        public List<RelatedCategoryDto> ChildCategories { get; set; } = new();

        // Optional embedded icon material reference when available
        [JsonPropertyName("iconMaterialRef")]
        public MinecraftMaterialRefDto? IconMaterialRef { get; set; }
    }

    public class RelatedCategoryDto
    {
        [JsonPropertyName("id")]
        public int? Id { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; } = null!;

        [JsonPropertyName("iconMaterialRefId")]
        public int? IconMaterialRefId { get; set; }

        [JsonPropertyName("iconMaterialRef")]
        public MinecraftMaterialRefDto? IconMaterialRef { get; set; }
    }
    
    public class CategoryListDto
    {
        [JsonPropertyName("id")]
        public int? id { get; set; }
        [JsonPropertyName("name")]
        public string name { get; set; } = null!;
        [JsonPropertyName("parentCategoryName")]
        public string? parentCategoryName { get; set; }
        [JsonPropertyName("parentCategoryId")]
        public int? parentCategoryId { get; set; }

        [JsonPropertyName("iconMaterialRefId")]
        public int? iconMaterialRefId { get; set; }
        [JsonPropertyName("iconMaterialRefName")]
        public string? iconMaterialRefName { get; set; }

        [JsonPropertyName("iconNamespaceKey")]
        public string? iconNamespaceKey { get; set; }

        // Convenience: number of direct children
        [JsonPropertyName("childrenCount")]
        public int childrenCount { get; set; }
    }
}