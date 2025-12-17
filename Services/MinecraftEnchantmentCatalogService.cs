using knkwebapi_v2.Models;
using knkwebapi_v2.Services.Interfaces;
using System.Text.Json;

namespace knkwebapi_v2.Services;

public class MinecraftEnchantmentCatalogService : IMinecraftEnchantmentCatalogService
{
    private readonly ILogger<MinecraftEnchantmentCatalogService> _logger;
    private readonly Lazy<List<MinecraftEnchantmentCatalogEntry>> _catalog;

    public MinecraftEnchantmentCatalogService(ILogger<MinecraftEnchantmentCatalogService> logger)
    {
        _logger = logger;
        _catalog = new Lazy<List<MinecraftEnchantmentCatalogEntry>>(() => LoadCatalog());
    }

    public IEnumerable<MinecraftEnchantmentCatalogEntry> GetAll()
    {
        return _catalog.Value;
    }

    public MinecraftEnchantmentCatalogEntry? GetByNamespaceKey(string namespaceKey)
    {
        if (string.IsNullOrWhiteSpace(namespaceKey)) return null;
        return _catalog.Value.FirstOrDefault(e => 
            e.NamespaceKey.Equals(namespaceKey, StringComparison.OrdinalIgnoreCase));
    }

    public IEnumerable<MinecraftEnchantmentCatalogEntry> Search(string? query)
    {
        if (string.IsNullOrWhiteSpace(query))
            return _catalog.Value;

        var lowerQuery = query.ToLowerInvariant();
        return _catalog.Value.Where(e =>
            e.NamespaceKey.Contains(lowerQuery, StringComparison.OrdinalIgnoreCase) ||
            (e.DisplayName != null && e.DisplayName.Contains(lowerQuery, StringComparison.OrdinalIgnoreCase)) ||
            (e.LegacyName != null && e.LegacyName.Contains(lowerQuery, StringComparison.OrdinalIgnoreCase)) ||
            (e.Category != null && e.Category.Contains(lowerQuery, StringComparison.OrdinalIgnoreCase))
        );
    }

    private List<MinecraftEnchantmentCatalogEntry> LoadCatalog()
    {
        try
        {
            var catalogPath = Path.Combine(AppContext.BaseDirectory, "Data", "minecraft_enchantment_catalog.json");
            if (!File.Exists(catalogPath))
            {
                _logger.LogWarning("Enchantment catalog not found at {Path}", catalogPath);
                return new List<MinecraftEnchantmentCatalogEntry>();
            }

            var json = File.ReadAllText(catalogPath);
            var entries = JsonSerializer.Deserialize<List<MinecraftEnchantmentCatalogEntry>>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new();
            _logger.LogInformation("Loaded {Count} enchantment catalog entries", entries.Count);
            return entries;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load enchantment catalog");
            return new List<MinecraftEnchantmentCatalogEntry>();
        }
    }
}
