using knkwebapi_v2.Attributes;

namespace knkwebapi_v2.Models;

[FormConfigurableEntity("ItemBlueprintDefaultEnchantment")]
public class ItemBlueprintDefaultEnchantment
{
    [NavigationPair(nameof(ItemBlueprint))]
    [RelatedEntityField(typeof(ItemBlueprint))]
    public int ItemBlueprintId { get; set; }
    [RelatedEntityField(typeof(ItemBlueprint))]
    public ItemBlueprint ItemBlueprint { get; set; } = null!;
    [NavigationPair(nameof(EnchantmentDefinition))]
    [RelatedEntityField(typeof(EnchantmentDefinition))]
    public int EnchantmentDefinitionId { get; set; }
    [RelatedEntityField(typeof(EnchantmentDefinition))]
    public EnchantmentDefinition EnchantmentDefinition { get; set; } = null!;

    public int Level { get; set; } = 1;
}
