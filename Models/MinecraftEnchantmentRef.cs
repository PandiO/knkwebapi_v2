using System.ComponentModel.DataAnnotations;
using knkwebapi_v2.Attributes;

namespace knkwebapi_v2.Models;

[FormConfigurableEntity("MinecraftEnchantmentRef")]
public class MinecraftEnchantmentRef
{
    public int Id { get; set; }

    [Required]
    [MaxLength(191)]
    public string NamespaceKey { get; set; } = string.Empty;

    public string? LegacyName { get; set; }

    public string? Category { get; set; }

    public string? IconUrl { get; set; }

    public int? MaxLevel { get; set; }

    public string? DisplayName { get; set; }

    public bool IsCustom { get; set; } = false;

    // TODO: Future extension—custom enchantment metadata/rules
    // - EnchantmentDefinition with effects/conflicts/allowed item types
    // - Plugin-driven custom enchantment engine (knk:* namespace)
}
