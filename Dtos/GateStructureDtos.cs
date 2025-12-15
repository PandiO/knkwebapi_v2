using System.Text.Json.Serialization;

namespace knkwebapi_v2.Dtos
{
    public class GateStructureDto
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

        [JsonPropertyName("streetId")]
        public int StreetId { get; set; }

        [JsonPropertyName("districtId")]
        public int DistrictId { get; set; }

        [JsonPropertyName("houseNumber")]
        public int HouseNumber { get; set; }

        // GateStructure-specific fields
        [JsonPropertyName("isActive")]
        public bool? IsActive { get; set; }

        [JsonPropertyName("canRespawn")]
        public bool? CanRespawn { get; set; }

        [JsonPropertyName("isDestroyed")]
        public bool? IsDestroyed { get; set; }

        [JsonPropertyName("isInvincible")]
        public bool? IsInvincible { get; set; }

        [JsonPropertyName("isOpened")]
        public bool? IsOpened { get; set; }

        [JsonPropertyName("healthCurrent")]
        public double? HealthCurrent { get; set; }

        [JsonPropertyName("healthMax")]
        public double? HealthMax { get; set; }

        [JsonPropertyName("faceDirection")]
        public string FaceDirection { get; set; } = "north";

        [JsonPropertyName("respawnRateSeconds")]
        public int? RespawnRateSeconds { get; set; }

        [JsonPropertyName("iconMaterialRefId")]
        public int? IconMaterialRefId { get; set; }

        [JsonPropertyName("regionClosedId")]
        public string RegionClosedId { get; set; } = string.Empty;

        [JsonPropertyName("regionOpenedId")]
        public string RegionOpenedId { get; set; } = string.Empty;

        // Optional embedded lightweight navigations
        [JsonPropertyName("street")]
        public GateStructureStreetDto? Street { get; set; }

        [JsonPropertyName("district")]
        public GateStructureDistrictDto? District { get; set; }

        [JsonPropertyName("iconMaterialRef")]
        public MinecraftMaterialRefDto? IconMaterialRef { get; set; }
    }

    public class GateStructureListDto
    {
        [JsonPropertyName("id")]
        public int? id { get; set; }

        [JsonPropertyName("name")]
        public string name { get; set; } = null!;

        [JsonPropertyName("description")]
        public string description { get; set; } = null!;

        [JsonPropertyName("wgRegionId")]
        public string wgRegionId { get; set; } = null!;

        [JsonPropertyName("houseNumber")]
        public int houseNumber { get; set; }

        [JsonPropertyName("streetId")]
        public int streetId { get; set; }

        [JsonPropertyName("streetName")]
        public string? streetName { get; set; }

        [JsonPropertyName("districtId")]
        public int districtId { get; set; }

        [JsonPropertyName("districtName")]
        public string? districtName { get; set; }

        [JsonPropertyName("isActive")]
        public bool isActive { get; set; }

        [JsonPropertyName("healthCurrent")]
        public double healthCurrent { get; set; }

        [JsonPropertyName("isDestroyed")]
        public bool isDestroyed { get; set; }
    }
}

namespace knkwebapi_v2.Dtos
{
    // Lightweight Street DTO for embedding in GateStructure payloads
    public class GateStructureStreetDto
    {
        [JsonPropertyName("id")]
        public int? Id { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }
    }

    // Lightweight District DTO for embedding in GateStructure payloads
    public class GateStructureDistrictDto
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
}
