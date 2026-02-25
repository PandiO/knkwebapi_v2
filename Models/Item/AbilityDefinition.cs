using knkwebapi_v2.Attributes;

namespace knkwebapi_v2.Models;

[FormConfigurableEntity("AbilityDefinition")]
public class AbilityDefinition
{
    public int Id { get; set; }

    [NavigationPair(nameof(EnchantmentDefinition))]
    [RelatedEntityField(typeof(EnchantmentDefinition))]
    public int EnchantmentDefinitionId { get; set; }

    [RelatedEntityField(typeof(EnchantmentDefinition))]
    public EnchantmentDefinition EnchantmentDefinition { get; set; } = null!;

    public string AbilityKey { get; set; } = string.Empty;
    public string? RuntimeConfigJson { get; set; }
    public string? FutureUserAssignmentContract { get; set; }
}