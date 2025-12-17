using knkwebapi_v2.Attributes;
using knkwebapi_v2.Models;

[FormConfigurableEntity("EnchantmentDefinition")]
public class EnchantmentDefinition
{
    public int Id { get; set; }

    // "minecraft:sharpness" or "knk:lifesteal"
    public string Key { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public bool IsCustom { get; set; } = false;
    public int MaxLevel { get; set; } = 1;

    [NavigationPair(nameof(MinecraftEnchantmentRef))]
    [RelatedEntityField(typeof(MinecraftEnchantmentRef))]
    public List<int> DefaultForBlueprintIds { get; set; } = new();
    [RelatedEntityField(typeof(ItemBlueprintDefaultEnchantment))]
    public List<ItemBlueprintDefaultEnchantment> DefaultForBlueprints { get; set; } = new();
    // TODO: Add ItemInstanceEnchantment when item instances are implemented
    // public List<ItemInstanceEnchantment> AppliedToInstances { get; set; } = new();
}
