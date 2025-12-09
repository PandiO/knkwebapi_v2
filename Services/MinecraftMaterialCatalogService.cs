using System.Text.Json;
using knkwebapi_v2.Models;
using knkwebapi_v2.Services.Interfaces;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace knkwebapi_v2.Services
{
    /// <summary>
    /// Provides the full catalog of Minecraft materials from a static source (JSON asset), decoupled from DB state.
    /// </summary>
    public class MinecraftMaterialCatalogService : IMinecraftMaterialCatalogService
    {
        private readonly ILogger<MinecraftMaterialCatalogService> _logger;
        private readonly Lazy<IReadOnlyList<MinecraftMaterialCatalogEntry>> _catalog;

        public MinecraftMaterialCatalogService(IHostEnvironment environment, ILogger<MinecraftMaterialCatalogService> logger)
        {
            _logger = logger;
            _catalog = new Lazy<IReadOnlyList<MinecraftMaterialCatalogEntry>>(() => LoadCatalog(environment));
        }

        public IEnumerable<MinecraftMaterialCatalogEntry> GetAll() => _catalog.Value;

        public IEnumerable<MinecraftMaterialCatalogEntry> GetByCategory(string category)
        {
            if (string.IsNullOrWhiteSpace(category)) return _catalog.Value;
            return _catalog.Value.Where(x => string.Equals(x.Category, category, StringComparison.OrdinalIgnoreCase));
        }

        public IEnumerable<MinecraftMaterialCatalogEntry> Search(string? search = null, string? category = null)
        {
            var query = _catalog.Value.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(category))
            {
                query = query.Where(x => string.Equals(x.Category, category, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim();
                query = query.Where(x =>
                    x.DisplayName.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                    x.NamespaceKey.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                    (!string.IsNullOrWhiteSpace(x.LegacyName) && x.LegacyName.Contains(term, StringComparison.OrdinalIgnoreCase)));
            }

            return query;
        }

        private IReadOnlyList<MinecraftMaterialCatalogEntry> LoadCatalog(IHostEnvironment environment)
        {
            var path = Path.Combine(environment.ContentRootPath, "Data", "minecraft_material_catalog.json");

            if (File.Exists(path))
            {
                try
                {
                    var json = File.ReadAllText(path);
                    var items = JsonSerializer.Deserialize<List<MinecraftMaterialCatalogEntry>>(json, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (items != null && items.Count > 0)
                    {
                        return items;
                    }

                    _logger.LogWarning("Material catalog file at {Path} was empty; falling back to built-in defaults.", path);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to read material catalog from {Path}. Falling back to built-in defaults.", path);
                }
            }
            else
            {
                _logger.LogWarning("Material catalog file not found at {Path}. Falling back to built-in defaults.", path);
            }

            // Minimal fallback list; extend as needed.
            return new List<MinecraftMaterialCatalogEntry>
            {
                new() { NamespaceKey = "minecraft:stone", DisplayName = "Stone", Category = "BLOCK", LegacyName = "STONE" },
                new() { NamespaceKey = "minecraft:oak_planks", DisplayName = "Oak Planks", Category = "BLOCK", LegacyName = "WOOD" },
                new() { NamespaceKey = "minecraft:stick", DisplayName = "Stick", Category = "ICON", LegacyName = "STICK" },
                new() { NamespaceKey = "minecraft:white_banner", DisplayName = "White Banner", Category = "BANNER" },
                new() { NamespaceKey = "minecraft:torch", DisplayName = "Torch", Category = "ICON" },
            };
        }
    }
}
