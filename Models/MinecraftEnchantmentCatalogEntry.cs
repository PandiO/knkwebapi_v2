namespace knkwebapi_v2.Models;

public class MinecraftEnchantmentCatalogEntry
{
    public string NamespaceKey { get; set; } = string.Empty;
    public string? LegacyName { get; set; }
    public string? Category { get; set; }
    public int? MaxLevel { get; set; }
    public string? IconUrl { get; set; }
    public string? DisplayName { get; set; }
}
