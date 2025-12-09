using knkwebapi_v2.Models;

namespace knkwebapi_v2.Services.Interfaces
{
    public interface IMinecraftMaterialCatalogService
    {
        IEnumerable<MinecraftMaterialCatalogEntry> GetAll();
        IEnumerable<MinecraftMaterialCatalogEntry> GetByCategory(string category);
        IEnumerable<MinecraftMaterialCatalogEntry> Search(string? search = null, string? category = null);
    }
}
