using System;
using knkwebapi_v2.Attributes;

namespace knkwebapi_v2.Models;

[FormConfigurableEntity("GateStructure")]
public class GateStructure : Structure
{
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
}
