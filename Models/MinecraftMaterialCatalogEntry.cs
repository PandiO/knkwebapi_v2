using System.Text.Json.Serialization;

namespace knkwebapi_v2.Models
{
    public class MinecraftMaterialCatalogEntry
    {
        [JsonPropertyName("namespaceKey")]
        public string NamespaceKey { get; set; } = null!;

        [JsonPropertyName("displayName")]
        public string DisplayName { get; set; } = null!;

        [JsonPropertyName("category")]
        public string Category { get; set; } = null!;

        [JsonPropertyName("legacyName")]
        public string? LegacyName { get; set; }

        [JsonPropertyName("texturePath")]
        public string TexturePath { get; set; } = null!;

        [JsonPropertyName("iconUrl")]
        public string? IconUrl { get; set; }
    }
}
