using System;
using System.Text.Json.Serialization;

namespace knkwebapi_v2.Dtos
{
    public class GateStructureReadDto
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;

        [JsonPropertyName("createdAt")]
        public DateTime CreatedAt { get; set; }

        [JsonPropertyName("allowEntry")]
        public bool AllowEntry { get; set; }

        [JsonPropertyName("allowExit")]
        public bool AllowExit { get; set; }

        [JsonPropertyName("wgRegionId")]
        public string WgRegionId { get; set; } = string.Empty;

        [JsonPropertyName("locationId")]
        public int? LocationId { get; set; }

        [JsonPropertyName("streetId")]
        public int StreetId { get; set; }

        [JsonPropertyName("districtId")]
        public int DistrictId { get; set; }

        [JsonPropertyName("houseNumber")]
        public int HouseNumber { get; set; }

        // === Core Gate State & Health System ===
        [JsonPropertyName("isActive")]
        public bool IsActive { get; set; }

        [JsonPropertyName("canRespawn")]
        public bool CanRespawn { get; set; }

        [JsonPropertyName("isDestroyed")]
        public bool IsDestroyed { get; set; }

        [JsonPropertyName("isInvincible")]
        public bool IsInvincible { get; set; }

        [JsonPropertyName("isOpened")]
        public bool IsOpened { get; set; }

        [JsonPropertyName("healthCurrent")]
        public double HealthCurrent { get; set; }

        [JsonPropertyName("healthMax")]
        public double HealthMax { get; set; }

        [JsonPropertyName("faceDirection")]
        public string FaceDirection { get; set; } = string.Empty;

        [JsonPropertyName("respawnRateSeconds")]
        public int RespawnRateSeconds { get; set; }

        [JsonPropertyName("iconMaterialRefId")]
        public int? IconMaterialRefId { get; set; }

        [JsonPropertyName("regionClosedId")]
        public string RegionClosedId { get; set; } = string.Empty;

        [JsonPropertyName("regionOpenedId")]
        public string RegionOpenedId { get; set; } = string.Empty;

        // === Gate Type & Animation Configuration ===
        [JsonPropertyName("gateType")]
        public string GateType { get; set; } = string.Empty;

        [JsonPropertyName("geometryDefinitionMode")]
        public string GeometryDefinitionMode { get; set; } = string.Empty;

        [JsonPropertyName("motionType")]
        public string MotionType { get; set; } = string.Empty;

        [JsonPropertyName("animationDurationTicks")]
        public int AnimationDurationTicks { get; set; }

        [JsonPropertyName("animationTickRate")]
        public int AnimationTickRate { get; set; }

        // === Geometry Definition (PLANE_GRID mode) ===
        [JsonPropertyName("anchorPoint")]
        public string AnchorPoint { get; set; } = string.Empty;

        [JsonPropertyName("referencePoint1")]
        public string ReferencePoint1 { get; set; } = string.Empty;

        [JsonPropertyName("referencePoint2")]
        public string ReferencePoint2 { get; set; } = string.Empty;

        [JsonPropertyName("geometryWidth")]
        public int GeometryWidth { get; set; }

        [JsonPropertyName("geometryHeight")]
        public int GeometryHeight { get; set; }

        [JsonPropertyName("geometryDepth")]
        public int GeometryDepth { get; set; }

        // === Geometry Definition (FLOOD_FILL mode) ===
        [JsonPropertyName("seedBlocks")]
        public string SeedBlocks { get; set; } = string.Empty;

        [JsonPropertyName("scanMaxBlocks")]
        public int ScanMaxBlocks { get; set; }

        [JsonPropertyName("scanMaxRadius")]
        public int ScanMaxRadius { get; set; }

        [JsonPropertyName("scanMaterialWhitelist")]
        public string ScanMaterialWhitelist { get; set; } = string.Empty;

        [JsonPropertyName("scanMaterialBlacklist")]
        public string ScanMaterialBlacklist { get; set; } = string.Empty;

        [JsonPropertyName("scanPlaneConstraint")]
        public bool ScanPlaneConstraint { get; set; }

        // === Block Management ===
        [JsonPropertyName("fallbackMaterialRefId")]
        public int? FallbackMaterialRefId { get; set; }

        [JsonPropertyName("tileEntityPolicy")]
        public string TileEntityPolicy { get; set; } = string.Empty;

        // === Rotation-Specific Fields ===
        [JsonPropertyName("rotationMaxAngleDegrees")]
        public int RotationMaxAngleDegrees { get; set; }

        [JsonPropertyName("hingeAxis")]
        public string HingeAxis { get; set; } = string.Empty;

        // === Double Doors Specific ===
        [JsonPropertyName("leftDoorSeedBlock")]
        public string LeftDoorSeedBlock { get; set; } = string.Empty;

        [JsonPropertyName("rightDoorSeedBlock")]
        public string RightDoorSeedBlock { get; set; } = string.Empty;

        [JsonPropertyName("mirrorRotation")]
        public bool MirrorRotation { get; set; }

        // === Pass-Through System ===
        [JsonPropertyName("allowPassThrough")]
        public bool AllowPassThrough { get; set; }

        [JsonPropertyName("passThroughDurationSeconds")]
        public int PassThroughDurationSeconds { get; set; }

        [JsonPropertyName("passThroughConditionsJson")]
        public string PassThroughConditionsJson { get; set; } = string.Empty;

        // === Guard & Defense System ===
        [JsonPropertyName("guardSpawnLocationsJson")]
        public string GuardSpawnLocationsJson { get; set; } = string.Empty;

        [JsonPropertyName("guardCount")]
        public int GuardCount { get; set; }

        [JsonPropertyName("guardNpcTemplateId")]
        public int? GuardNpcTemplateId { get; set; }

        // === Health Display Configuration ===
        [JsonPropertyName("showHealthDisplay")]
        public bool ShowHealthDisplay { get; set; }

        [JsonPropertyName("healthDisplayMode")]
        public string HealthDisplayMode { get; set; } = string.Empty;

        [JsonPropertyName("healthDisplayYOffset")]
        public int HealthDisplayYOffset { get; set; }

        // === Siege Integration ===
        [JsonPropertyName("isOverridable")]
        public bool IsOverridable { get; set; }

        [JsonPropertyName("animateDuringSiege")]
        public bool AnimateDuringSiege { get; set; }

        [JsonPropertyName("currentSiegeId")]
        public int? CurrentSiegeId { get; set; }

        [JsonPropertyName("isSiegeObjective")]
        public bool IsSiegeObjective { get; set; }

        // === Combat System: Continuous Damage ===
        [JsonPropertyName("allowContinuousDamage")]
        public bool AllowContinuousDamage { get; set; }

        [JsonPropertyName("continuousDamageMultiplier")]
        public double ContinuousDamageMultiplier { get; set; }

        [JsonPropertyName("continuousDamageDurationSeconds")]
        public int ContinuousDamageDurationSeconds { get; set; }

        // === Navigation Properties ===
        [JsonPropertyName("blockSnapshots")]
        public List<GateBlockSnapshotDto>? BlockSnapshots { get; set; }

        [JsonPropertyName("street")]
        public GateStructureStreetDto? Street { get; set; }

        [JsonPropertyName("district")]
        public GateStructureDistrictDto? District { get; set; }

        [JsonPropertyName("iconMaterialRef")]
        public MinecraftMaterialRefDto? IconMaterialRef { get; set; }

        [JsonPropertyName("fallbackMaterialRef")]
        public MinecraftMaterialRefDto? FallbackMaterialRef { get; set; }
    }

    public class GateStructureCreateDto
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;

        [JsonPropertyName("allowEntry")]
        public bool AllowEntry { get; set; } = true;

        [JsonPropertyName("allowExit")]
        public bool AllowExit { get; set; } = true;

        [JsonPropertyName("wgRegionId")]
        public string WgRegionId { get; set; } = string.Empty;

        [JsonPropertyName("locationId")]
        public int? LocationId { get; set; }

        [JsonPropertyName("streetId")]
        public int StreetId { get; set; }

        [JsonPropertyName("districtId")]
        public int DistrictId { get; set; }

        [JsonPropertyName("houseNumber")]
        public int HouseNumber { get; set; }

        [JsonPropertyName("iconMaterialRefId")]
        public int? IconMaterialRefId { get; set; }

        [JsonPropertyName("gateType")]
        public string GateType { get; set; } = "SLIDING";

        [JsonPropertyName("geometryDefinitionMode")]
        public string GeometryDefinitionMode { get; set; } = "PLANE_GRID";

        [JsonPropertyName("motionType")]
        public string MotionType { get; set; } = "VERTICAL";

        [JsonPropertyName("faceDirection")]
        public string FaceDirection { get; set; } = "north";

        [JsonPropertyName("anchorPoint")]
        public string AnchorPoint { get; set; } = string.Empty;

        [JsonPropertyName("referencePoint1")]
        public string ReferencePoint1 { get; set; } = string.Empty;

        [JsonPropertyName("referencePoint2")]
        public string ReferencePoint2 { get; set; } = string.Empty;

        [JsonPropertyName("geometryWidth")]
        public int GeometryWidth { get; set; }

        [JsonPropertyName("geometryHeight")]
        public int GeometryHeight { get; set; }

        [JsonPropertyName("geometryDepth")]
        public int GeometryDepth { get; set; }

        [JsonPropertyName("seedBlocks")]
        public string SeedBlocks { get; set; } = string.Empty;

        [JsonPropertyName("scanMaxBlocks")]
        public int ScanMaxBlocks { get; set; } = 500;

        [JsonPropertyName("scanMaxRadius")]
        public int ScanMaxRadius { get; set; } = 20;

        [JsonPropertyName("scanMaterialWhitelist")]
        public string ScanMaterialWhitelist { get; set; } = string.Empty;

        [JsonPropertyName("scanMaterialBlacklist")]
        public string ScanMaterialBlacklist { get; set; } = string.Empty;

        [JsonPropertyName("scanPlaneConstraint")]
        public bool ScanPlaneConstraint { get; set; }

        [JsonPropertyName("animationDurationTicks")]
        public int AnimationDurationTicks { get; set; } = 60;

        [JsonPropertyName("animationTickRate")]
        public int AnimationTickRate { get; set; } = 1;

        [JsonPropertyName("fallbackMaterialRefId")]
        public int? FallbackMaterialRefId { get; set; }

        [JsonPropertyName("tileEntityPolicy")]
        public string TileEntityPolicy { get; set; } = "DECORATIVE_ONLY";

        [JsonPropertyName("rotationMaxAngleDegrees")]
        public int RotationMaxAngleDegrees { get; set; } = 90;

        [JsonPropertyName("hingeAxis")]
        public string HingeAxis { get; set; } = string.Empty;

        [JsonPropertyName("leftDoorSeedBlock")]
        public string LeftDoorSeedBlock { get; set; } = string.Empty;

        [JsonPropertyName("rightDoorSeedBlock")]
        public string RightDoorSeedBlock { get; set; } = string.Empty;

        [JsonPropertyName("mirrorRotation")]
        public bool MirrorRotation { get; set; } = true;

        [JsonPropertyName("healthMax")]
        public double HealthMax { get; set; } = 500.0;

        [JsonPropertyName("isInvincible")]
        public bool IsInvincible { get; set; } = true;

        [JsonPropertyName("canRespawn")]
        public bool CanRespawn { get; set; } = true;

        [JsonPropertyName("respawnRateSeconds")]
        public int RespawnRateSeconds { get; set; } = 300;

        [JsonPropertyName("regionClosedId")]
        public string RegionClosedId { get; set; } = string.Empty;

        [JsonPropertyName("regionOpenedId")]
        public string RegionOpenedId { get; set; } = string.Empty;
    }

    public class GateStructureUpdateDto
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;

        [JsonPropertyName("isActive")]
        public bool IsActive { get; set; }

        [JsonPropertyName("healthMax")]
        public double HealthMax { get; set; }

        [JsonPropertyName("isInvincible")]
        public bool IsInvincible { get; set; }

        [JsonPropertyName("canRespawn")]
        public bool CanRespawn { get; set; }

        [JsonPropertyName("respawnRateSeconds")]
        public int RespawnRateSeconds { get; set; }

        [JsonPropertyName("animationDurationTicks")]
        public int AnimationDurationTicks { get; set; }

        [JsonPropertyName("animationTickRate")]
        public int AnimationTickRate { get; set; }

        [JsonPropertyName("regionClosedId")]
        public string RegionClosedId { get; set; } = string.Empty;

        [JsonPropertyName("regionOpenedId")]
        public string RegionOpenedId { get; set; } = string.Empty;
    }

    public class GateStateUpdateDto
    {
        [JsonPropertyName("isOpened")]
        public bool IsOpened { get; set; }

        [JsonPropertyName("isDestroyed")]
        public bool IsDestroyed { get; set; }
    }

    public class GateStructureNavDto
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("gateType")]
        public string GateType { get; set; } = string.Empty;

        [JsonPropertyName("isOpened")]
        public bool IsOpened { get; set; }

        [JsonPropertyName("healthCurrent")]
        public double HealthCurrent { get; set; }
    }

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

        // === Core Gate State & Health System ===
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

        // === Gate Type & Animation Configuration ===
        [JsonPropertyName("gateType")]
        public string GateType { get; set; } = "SLIDING";

        [JsonPropertyName("geometryDefinitionMode")]
        public string GeometryDefinitionMode { get; set; } = "PLANE_GRID";

        [JsonPropertyName("motionType")]
        public string MotionType { get; set; } = "VERTICAL";

        [JsonPropertyName("animationDurationTicks")]
        public int? AnimationDurationTicks { get; set; }

        [JsonPropertyName("animationTickRate")]
        public int? AnimationTickRate { get; set; }

        // === Geometry Definition (PLANE_GRID mode) ===
        [JsonPropertyName("anchorPoint")]
        public string AnchorPoint { get; set; } = string.Empty;

        [JsonPropertyName("referencePoint1")]
        public string ReferencePoint1 { get; set; } = string.Empty;

        [JsonPropertyName("referencePoint2")]
        public string ReferencePoint2 { get; set; } = string.Empty;

        [JsonPropertyName("geometryWidth")]
        public int? GeometryWidth { get; set; }

        [JsonPropertyName("geometryHeight")]
        public int? GeometryHeight { get; set; }

        [JsonPropertyName("geometryDepth")]
        public int? GeometryDepth { get; set; }

        // === Geometry Definition (FLOOD_FILL mode) ===
        [JsonPropertyName("seedBlocks")]
        public string SeedBlocks { get; set; } = string.Empty;

        [JsonPropertyName("scanMaxBlocks")]
        public int? ScanMaxBlocks { get; set; }

        [JsonPropertyName("scanMaxRadius")]
        public int? ScanMaxRadius { get; set; }

        [JsonPropertyName("scanMaterialWhitelist")]
        public string ScanMaterialWhitelist { get; set; } = string.Empty;

        [JsonPropertyName("scanMaterialBlacklist")]
        public string ScanMaterialBlacklist { get; set; } = string.Empty;

        [JsonPropertyName("scanPlaneConstraint")]
        public bool? ScanPlaneConstraint { get; set; }

        // === Block Management ===
        [JsonPropertyName("fallbackMaterialRefId")]
        public int? FallbackMaterialRefId { get; set; }

        [JsonPropertyName("tileEntityPolicy")]
        public string TileEntityPolicy { get; set; } = "DECORATIVE_ONLY";

        // === Rotation-Specific Fields ===
        [JsonPropertyName("rotationMaxAngleDegrees")]
        public int? RotationMaxAngleDegrees { get; set; }

        [JsonPropertyName("hingeAxis")]
        public string HingeAxis { get; set; } = string.Empty;

        // === Double Doors Specific ===
        [JsonPropertyName("leftDoorSeedBlock")]
        public string LeftDoorSeedBlock { get; set; } = string.Empty;

        [JsonPropertyName("rightDoorSeedBlock")]
        public string RightDoorSeedBlock { get; set; } = string.Empty;

        [JsonPropertyName("mirrorRotation")]
        public bool? MirrorRotation { get; set; }

        // === Pass-Through System ===
        [JsonPropertyName("allowPassThrough")]
        public bool? AllowPassThrough { get; set; }

        [JsonPropertyName("passThroughDurationSeconds")]
        public int? PassThroughDurationSeconds { get; set; }

        [JsonPropertyName("passThroughConditionsJson")]
        public string PassThroughConditionsJson { get; set; } = string.Empty;

        // === Guard & Defense System ===
        [JsonPropertyName("guardSpawnLocationsJson")]
        public string GuardSpawnLocationsJson { get; set; } = string.Empty;

        [JsonPropertyName("guardCount")]
        public int? GuardCount { get; set; }

        [JsonPropertyName("guardNpcTemplateId")]
        public int? GuardNpcTemplateId { get; set; }

        // === Health Display Configuration ===
        [JsonPropertyName("showHealthDisplay")]
        public bool? ShowHealthDisplay { get; set; }

        [JsonPropertyName("healthDisplayMode")]
        public string HealthDisplayMode { get; set; } = "ALWAYS";

        [JsonPropertyName("healthDisplayYOffset")]
        public int? HealthDisplayYOffset { get; set; }

        // === Siege Integration ===
        [JsonPropertyName("isOverridable")]
        public bool? IsOverridable { get; set; }

        [JsonPropertyName("animateDuringSiege")]
        public bool? AnimateDuringSiege { get; set; }

        [JsonPropertyName("currentSiegeId")]
        public int? CurrentSiegeId { get; set; }

        [JsonPropertyName("isSiegeObjective")]
        public bool? IsSiegeObjective { get; set; }

        // === Combat System: Continuous Damage ===
        [JsonPropertyName("allowContinuousDamage")]
        public bool? AllowContinuousDamage { get; set; }

        [JsonPropertyName("continuousDamageMultiplier")]
        public double? ContinuousDamageMultiplier { get; set; }

        [JsonPropertyName("continuousDamageDurationSeconds")]
        public int? ContinuousDamageDurationSeconds { get; set; }

        // === Navigation Properties ===
        [JsonPropertyName("blockSnapshots")]
        public List<GateBlockSnapshotDto>? BlockSnapshots { get; set; }

        [JsonPropertyName("street")]
        public GateStructureStreetDto? Street { get; set; }

        [JsonPropertyName("district")]
        public GateStructureDistrictDto? District { get; set; }

        [JsonPropertyName("iconMaterialRef")]
        public MinecraftMaterialRefDto? IconMaterialRef { get; set; }

        [JsonPropertyName("fallbackMaterialRef")]
        public MinecraftMaterialRefDto? FallbackMaterialRef { get; set; }
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

        [JsonPropertyName("healthMax")]
        public double healthMax { get; set; }

        [JsonPropertyName("isDestroyed")]
        public bool isDestroyed { get; set; }

        [JsonPropertyName("isOpened")]
        public bool isOpened { get; set; }

        [JsonPropertyName("gateType")]
        public string gateType { get; set; } = "SLIDING";

        [JsonPropertyName("faceDirection")]
        public string faceDirection { get; set; } = "north";
    }

    // GateBlockSnapshot DTO
    public class GateBlockSnapshotDto
    {
        [JsonPropertyName("id")]
        public int? Id { get; set; }

        [JsonPropertyName("gateStructureId")]
        public int GateStructureId { get; set; }

        [JsonPropertyName("relativeX")]
        public int RelativeX { get; set; }

        [JsonPropertyName("relativeY")]
        public int RelativeY { get; set; }

        [JsonPropertyName("relativeZ")]
        public int RelativeZ { get; set; }

        [JsonPropertyName("worldX")]
        public int WorldX { get; set; }

        [JsonPropertyName("worldY")]
        public int WorldY { get; set; }

        [JsonPropertyName("worldZ")]
        public int WorldZ { get; set; }

        [JsonPropertyName("materialName")]
        public string MaterialName { get; set; } = null!;

        [JsonPropertyName("blockDataJson")]
        public string BlockDataJson { get; set; } = "{}";

        [JsonPropertyName("tileEntityJson")]
        public string TileEntityJson { get; set; } = "{}";

        [JsonPropertyName("sortOrder")]
        public int SortOrder { get; set; }
    }

    public class GateBlockSnapshotCreateDto
    {
        [JsonPropertyName("relativeX")]
        public int RelativeX { get; set; }

        [JsonPropertyName("relativeY")]
        public int RelativeY { get; set; }

        [JsonPropertyName("relativeZ")]
        public int RelativeZ { get; set; }

        [JsonPropertyName("worldX")]
        public int WorldX { get; set; }

        [JsonPropertyName("worldY")]
        public int WorldY { get; set; }

        [JsonPropertyName("worldZ")]
        public int WorldZ { get; set; }

        [JsonPropertyName("materialName")]
        public string MaterialName { get; set; } = null!;

        [JsonPropertyName("blockDataJson")]
        public string BlockDataJson { get; set; } = "{}";

        [JsonPropertyName("tileEntityJson")]
        public string TileEntityJson { get; set; } = "{}";

        [JsonPropertyName("sortOrder")]
        public int SortOrder { get; set; }
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
