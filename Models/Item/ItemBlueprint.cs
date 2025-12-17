using knkwebapi_v2.Attributes;

namespace knkwebapi_v2.Models;

[FormConfigurableEntity("ItemBlueprint")]
public class ItemBlueprint
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    [RelatedEntityField(typeof(MinecraftMaterialRef))]
    [NavigationPair("IconMaterial")]
    public int? IconMaterialRefId { get; set; }
    [RelatedEntityField(typeof(MinecraftMaterialRef))]
    public MinecraftMaterialRef? IconMaterial { get; set; } = null;

    public string DefaultDisplayName { get; set; } = string.Empty;
    public string DefaultDisplayDescription { get; set; } = string.Empty;

    public int DefaultQuantity { get; set; } = 1;
    public int MaxStackSize { get; set; } = 64;

    [NavigationPair(nameof(ItemBlueprintDefaultEnchantment))]
    [RelatedEntityField(typeof(ItemBlueprintDefaultEnchantment))]
    public ICollection<int> DefaultEnchantmentIds { get; set; } = new List<int>();
    [RelatedEntityField(typeof(ItemBlueprintDefaultEnchantment))]
    public ICollection<ItemBlueprintDefaultEnchantment> DefaultEnchantments { get; set; } = new List<ItemBlueprintDefaultEnchantment>();
}