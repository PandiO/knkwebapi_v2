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
}