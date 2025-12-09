using System.Text.Json.Serialization;

namespace knkwebapi_v2.Dtos
{
    public class CategoryDto
    {
        [JsonPropertyName("id")]
        public int? Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; } = null!;

        [JsonPropertyName("itemtypeId")]
        public int? ItemtypeId { get; set; }

        [JsonPropertyName("iconMaterialRefId")]
        public int? IconMaterialRefId { get; set; }

        [JsonPropertyName("parentCategoryId")]
        public int? ParentCategoryId { get; set; }
        [JsonPropertyName("parentCategory")]
        public CategoryDto? ParentCategory { get; set; }
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
        [JsonPropertyName("itemtypeId")]
        public int? itemtypeId { get; set; }
        [JsonPropertyName("itemtypeName")]
        public string? itemtypeName { get; set; }

        [JsonPropertyName("iconMaterialRefId")]
        public int? iconMaterialRefId { get; set; }

        [JsonPropertyName("iconNamespaceKey")]
        public string? iconNamespaceKey { get; set; }
    }
}