using knkwebapi_v2.Properties;
using knkwebapi_v2.Models;
using knkwebapi_v2.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace knkwebapi_v2.Repositories;

public class MinecraftEnchantmentRefRepository : IMinecraftEnchantmentRefRepository
{
    private readonly KnKDbContext _context;

    public MinecraftEnchantmentRefRepository(KnKDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<MinecraftEnchantmentRef>> GetAllAsync()
    {
        return await _context.MinecraftEnchantmentRefs.ToListAsync();
    }

    public async Task<MinecraftEnchantmentRef?> GetByIdAsync(int id)
    {
        return await _context.MinecraftEnchantmentRefs.FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<MinecraftEnchantmentRef?> GetByNamespaceKeyAsync(string namespaceKey)
    {
        if (string.IsNullOrWhiteSpace(namespaceKey))
            return null;

        return await _context.MinecraftEnchantmentRefs
            .FirstOrDefaultAsync(e => e.NamespaceKey == namespaceKey);
    }

    public async Task AddAsync(MinecraftEnchantmentRef entity)
    {
        await _context.MinecraftEnchantmentRefs.AddAsync(entity);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(MinecraftEnchantmentRef entity)
    {
        _context.MinecraftEnchantmentRefs.Update(entity);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(MinecraftEnchantmentRef entity)
    {
        _context.MinecraftEnchantmentRefs.Remove(entity);
        await _context.SaveChangesAsync();
    }
}
