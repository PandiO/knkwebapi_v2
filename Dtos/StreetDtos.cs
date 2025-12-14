using System.Text.Json.Serialization;

namespace knkwebapi_v2.Dtos
{
    public class StreetDto
    {
        [JsonPropertyName("id")]
        public int? Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; } = null!;
    }

    public class StreetListDto
    {
        [JsonPropertyName("id")]
        public int? id { get; set; }

        [JsonPropertyName("name")]
        public string name { get; set; } = null!;
    }
}

namespace knkwebapi_v2.Dtos
{
    // Lightweight District DTO for embedding in Street payloads
    public class StreetDistrictDto
    {
        [JsonPropertyName("id")]
        public int? Id { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("allowEntry")]
        public bool? AllowEntry { get; set; }

        [JsonPropertyName("allowExit")]
        public bool? AllowExit { get; set; }

        [JsonPropertyName("wgRegionId")]
        public string? WgRegionId { get; set; }
    }

    // Lightweight Structure DTO for embedding in Street payloads
    public class StreetStructureDto
    {
        [JsonPropertyName("id")]
        public int? Id { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("houseNumber")]
        public int? HouseNumber { get; set; }

        [JsonPropertyName("districtId")]
        public int? DistrictId { get; set; }
    }
}
