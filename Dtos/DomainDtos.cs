using System;
using System.Text.Json.Serialization;

namespace knkwebapi_v2.Dtos
{
    public class DomainDto
    {
        [JsonPropertyName("id")]
        public int? Id { get; set; }
        [JsonPropertyName("name")]
        public string Name { get; set; } = null!;
        [JsonPropertyName("description")]
        public string Description { get; set; } = null!;
        [JsonPropertyName("createdAt")]
        public DateTime? CreatedAt { get; set; }
        [JsonPropertyName("allowEntry")]
        public bool? AllowEntry { get; set; }
        [JsonPropertyName("allowExit")]
        public bool? AllowExit { get; set; }
        [JsonPropertyName("wgRegionId")]
        public string WgRegionId { get; set; } = null!;
        [JsonPropertyName("locationId")]
        public int? LocationId { get; set; }
    }

    public class DomainListDto
    {
        [JsonPropertyName("id")]
        public int? Id { get; set; }
        [JsonPropertyName("name")]
        public string Name { get; set; } = null!;
        [JsonPropertyName("description")]
        public string Description { get; set; } = null!;
        [JsonPropertyName("wgRegionId")]
        public string WgRegionId { get; set; } = null!;
    }
}
