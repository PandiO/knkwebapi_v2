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

        [JsonPropertyName("parentCategoryId")]
        public int? ParentCategoryId { get; set; }
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
    }
}