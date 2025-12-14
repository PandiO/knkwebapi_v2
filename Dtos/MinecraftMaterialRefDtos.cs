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

        [JsonPropertyName("iconUrl")]
        public string? IconUrl { get; set; }
    }

    public class MinecraftMaterialRefCreateDto
    {
        [JsonPropertyName("namespaceKey")]
        public string NamespaceKey { get; set; } = null!;

        [JsonPropertyName("legacyName")]
        public string? LegacyName { get; set; }

        [JsonPropertyName("category")]
        public string Category { get; set; } = null!;

        [JsonPropertyName("iconUrl")]
        public string? IconUrl { get; set; }
    }

    public class MinecraftMaterialRefUpdateDto
    {
        [JsonPropertyName("namespaceKey")]
        public string NamespaceKey { get; set; } = null!;

        [JsonPropertyName("legacyName")]
        public string? LegacyName { get; set; }

        [JsonPropertyName("category")]
        public string Category { get; set; } = null!;

        [JsonPropertyName("iconUrl")]
        public string? IconUrl { get; set; }
    }

    public class MinecraftMaterialRefListDto
    {
        [JsonPropertyName("id")]
        public int? Id { get; set; }

        [JsonPropertyName("namespaceKey")]
        public string NamespaceKey { get; set; } = null!;

        [JsonPropertyName("category")]
        public string Category { get; set; } = null!;

        [JsonPropertyName("legacyName")]
        public string? LegacyName { get; set; }

        [JsonPropertyName("displayName")]
        public string? DisplayName { get; set; }

        [JsonPropertyName("isPersisted")]
        public bool? IsPersisted { get; set; }

        [JsonPropertyName("iconUrl")]
        public string? IconUrl { get; set; }
    }
}
