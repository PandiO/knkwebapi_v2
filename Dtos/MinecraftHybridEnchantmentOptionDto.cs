using System.Text.Json.Serialization;

namespace knkwebapi_v2.Dtos;

public class MinecraftHybridEnchantmentOptionDto
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty; // "PERSISTED" | "CATALOG"

    [JsonPropertyName("namespaceKey")]
    public string NamespaceKey { get; set; } = string.Empty;

    [JsonPropertyName("displayName")]
    public string? DisplayName { get; set; }

    [JsonPropertyName("legacyName")]
    public string? LegacyName { get; set; }

    [JsonPropertyName("category")]
    public string? Category { get; set; }

    [JsonPropertyName("iconUrl")]
    public string? IconUrl { get; set; }

    [JsonPropertyName("maxLevel")]
    public int? MaxLevel { get; set; }

    // Only populated if Type == "PERSISTED"
    [JsonPropertyName("id")]
    public int? Id { get; set; }

    [JsonPropertyName("isCustom")]
    public bool IsCustom { get; set; }
}
