using System.Text.Json.Serialization;

namespace knkwebapi_v2.Dtos
{
    public class MinecraftMaterialRefDto
    {
        [JsonPropertyName("id")]
        public int? Id { get; set; }

        [JsonPropertyName("namespaceKey")]
        public string NamespaceKey { get; set; } = null!;

        [JsonPropertyName("legacyName")]
        public string? LegacyName { get; set; }

        [JsonPropertyName("category")]
        public string Category { get; set; } = null!;
    }

    public class MinecraftMaterialRefCreateDto
    {
        [JsonPropertyName("namespaceKey")]
        public string NamespaceKey { get; set; } = null!;

        [JsonPropertyName("legacyName")]
        public string? LegacyName { get; set; }

        [JsonPropertyName("category")]
        public string Category { get; set; } = null!;
    }

    public class MinecraftMaterialRefUpdateDto
    {
        [JsonPropertyName("namespaceKey")]
        public string NamespaceKey { get; set; } = null!;

        [JsonPropertyName("legacyName")]
        public string? LegacyName { get; set; }

        [JsonPropertyName("category")]
        public string Category { get; set; } = null!;
    }

    public class MinecraftMaterialRefListDto
    {
        [JsonPropertyName("id")]
        public int? Id { get; set; }

        [JsonPropertyName("namespaceKey")]
        public string NamespaceKey { get; set; } = null!;

        [JsonPropertyName("category")]
        public string Category { get; set; } = null!;
    }
}
