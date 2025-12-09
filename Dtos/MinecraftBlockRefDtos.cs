using System.Text.Json.Serialization;

namespace knkwebapi_v2.Dtos
{
    public class MinecraftBlockRefDto
    {
        [JsonPropertyName("id")]
        public int? Id { get; set; }

        [JsonPropertyName("namespaceKey")]
        public string NamespaceKey { get; set; } = null!;

        [JsonPropertyName("blockStateString")]
        public string? BlockStateString { get; set; }

        [JsonPropertyName("logicalType")]
        public string? LogicalType { get; set; }
    }

    public class MinecraftBlockRefCreateDto
    {
        [JsonPropertyName("namespaceKey")]
        public string NamespaceKey { get; set; } = null!;

        [JsonPropertyName("blockStateString")]
        public string? BlockStateString { get; set; }

        [JsonPropertyName("logicalType")]
        public string? LogicalType { get; set; }
    }

    public class MinecraftBlockRefUpdateDto
    {
        [JsonPropertyName("namespaceKey")]
        public string NamespaceKey { get; set; } = null!;

        [JsonPropertyName("blockStateString")]
        public string? BlockStateString { get; set; }

        [JsonPropertyName("logicalType")]
        public string? LogicalType { get; set; }
    }

    public class MinecraftBlockRefListDto
    {
        [JsonPropertyName("id")]
        public int? Id { get; set; }

        [JsonPropertyName("namespaceKey")]
        public string NamespaceKey { get; set; } = null!;

        [JsonPropertyName("logicalType")]
        public string? LogicalType { get; set; }
    }
}
