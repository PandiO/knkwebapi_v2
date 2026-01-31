using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using knkwebapi_v2.Attributes;

namespace knkwebapi_v2.Models;

[FormConfigurableEntity("GateStructure")]
public class GateStructure : Structure
{
    // === Core Gate State & Health System ===
    public bool IsActive { get; set; } = false;
    public bool CanRespawn { get; set; } = true;
    public bool IsDestroyed { get; set; } = false; 
    public bool IsInvincible { get; set; } = true;
    public bool IsOpened { get; set; } = false;
    public double HealthCurrent { get; set; } = 500.0;
    public double HealthMax { get; set; } = 500.0;
    public string FaceDirection { get; set; } = "north";
    public int RespawnRateSeconds { get; set; } = 300;
    
    [RelatedEntityField(typeof(MinecraftMaterialRef))]
    public int? IconMaterialRefId { get; set; }
    
    [RelatedEntityField(typeof(MinecraftMaterialRef))]
    public MinecraftMaterialRef? IconMaterial { get; set; } = null;
    
    public string RegionClosedId { get; set; } = string.Empty;
    public string RegionOpenedId { get; set; } = string.Empty;

    // === Gate Type & Animation Configuration ===
    [MaxLength(50)]
    public string GateType { get; set; } = "SLIDING";  // SLIDING, TRAP, DRAWBRIDGE, DOUBLE_DOORS
    
    [MaxLength(50)]
    public string GeometryDefinitionMode { get; set; } = "PLANE_GRID";  // PLANE_GRID, FLOOD_FILL
    
    [MaxLength(50)]
    public string MotionType { get; set; } = "VERTICAL";  // VERTICAL, LATERAL, ROTATION
    
    public int AnimationDurationTicks { get; set; } = 60;  // Default 3 seconds @ 20 TPS
    public int AnimationTickRate { get; set; } = 1;  // Frames per tick

    // === Geometry Definition (PLANE_GRID mode) ===
    [MaxLength(200)]
    public string AnchorPoint { get; set; } = string.Empty;  // JSON: {x, y, z} - p0
    
    [MaxLength(200)]
    public string ReferencePoint1 { get; set; } = string.Empty;  // JSON: {x, y, z} - p1
    
    [MaxLength(200)]
    public string ReferencePoint2 { get; set; } = string.Empty;  // JSON: {x, y, z} - p2
    
    public int GeometryWidth { get; set; } = 0;
    public int GeometryHeight { get; set; } = 0;
    public int GeometryDepth { get; set; } = 0;

    // === Geometry Definition (FLOOD_FILL mode) ===
    [MaxLength(2000)]
    public string SeedBlocks { get; set; } = string.Empty;  // JSON array: [{x,y,z}, ...]
    
    public int ScanMaxBlocks { get; set; } = 500;
    public int ScanMaxRadius { get; set; } = 20;
    
    [MaxLength(1000)]
    public string ScanMaterialWhitelist { get; set; } = string.Empty;  // JSON: [materialIds]
    
    [MaxLength(1000)]
    public string ScanMaterialBlacklist { get; set; } = string.Empty;  // JSON: [materialIds]
    
    public bool ScanPlaneConstraint { get; set; } = false;

    // === Block Management ===
    [RelatedEntityField(typeof(MinecraftMaterialRef))]
    public int? FallbackMaterialRefId { get; set; }
    
    [RelatedEntityField(typeof(MinecraftMaterialRef))]
    public MinecraftMaterialRef? FallbackMaterial { get; set; } = null;
    
    [MaxLength(50)]
    public string TileEntityPolicy { get; set; } = "DECORATIVE_ONLY";  // NONE, DECORATIVE_ONLY, ALL

    // === Rotation-Specific Fields (Drawbridge, Double Doors) ===
    public int RotationMaxAngleDegrees { get; set; } = 90;
    
    [MaxLength(200)]
    public string HingeAxis { get; set; } = string.Empty;  // JSON: {x,y,z} - rotation axis vector

    // === Double Doors Specific ===
    [MaxLength(200)]
    public string LeftDoorSeedBlock { get; set; } = string.Empty;  // JSON: {x,y,z}
    
    [MaxLength(200)]
    public string RightDoorSeedBlock { get; set; } = string.Empty;  // JSON: {x,y,z}
    
    public bool MirrorRotation { get; set; } = true;

    // === Pass-Through System ===
    public bool AllowPassThrough { get; set; } = false;
    public int PassThroughDurationSeconds { get; set; } = 4;
    
    [MaxLength(2000)]
    public string PassThroughConditionsJson { get; set; } = string.Empty;  // Complex conditions

    // === Guard & Defense System (Future Feature) ===
    [MaxLength(2000)]
    public string GuardSpawnLocationsJson { get; set; } = string.Empty;  // JSON: [{x,y,z,yaw,pitch}, ...]
    
    public int GuardCount { get; set; } = 0;
    public int? GuardNpcTemplateId { get; set; }  // FK to NpcTemplate (future)

    // === Health Display Configuration ===
    public bool ShowHealthDisplay { get; set; } = true;
    
    [MaxLength(50)]
    public string HealthDisplayMode { get; set; } = "ALWAYS";  // ALWAYS, DAMAGED_ONLY, NEVER, SIEGE_ONLY
    
    public int HealthDisplayYOffset { get; set; } = 2;

    // === Siege Integration ===
    public bool IsOverridable { get; set; } = true;
    public bool AnimateDuringSiege { get; set; } = true;
    public int? CurrentSiegeId { get; set; }  // FK to Siege (future)
    public bool IsSiegeObjective { get; set; } = false;

    // === Combat System: Continuous Damage ===
    public bool AllowContinuousDamage { get; set; } = true;
    public double ContinuousDamageMultiplier { get; set; } = 1.0;
    public int ContinuousDamageDurationSeconds { get; set; } = 5;

    // === Navigation Properties ===
    public virtual ICollection<GateBlockSnapshot> BlockSnapshots { get; set; } = new List<GateBlockSnapshot>();
}
