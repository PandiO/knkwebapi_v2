using knkwebapi_v2.Models;

namespace knkwebapi_v2.Services.Interfaces;

public interface IMinecraftEnchantmentCatalogService
{
    IEnumerable<MinecraftEnchantmentCatalogEntry> GetAll();
    MinecraftEnchantmentCatalogEntry? GetByNamespaceKey(string namespaceKey);
    IEnumerable<MinecraftEnchantmentCatalogEntry> Search(string? query);
}
