using knkwebapi_v2.Models;
using knkwebapi_v2.Properties;
using knkwebapi_v2.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace knkwebapi_v2.Repositories
{
    public class ItemBlueprintRepository : IItemBlueprintRepository
    {
        private readonly KnKDbContext _context;

        public ItemBlueprintRepository(KnKDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ItemBlueprint>> GetAllAsync()
        {
            return await _context.ItemBlueprints
                .Include(ib => ib.IconMaterial)
                .Include(ib => ib.DefaultEnchantments)
                    .ThenInclude(de => de.EnchantmentDefinition)
                        .ThenInclude(ed => ed.BaseEnchantmentRef)
                .ToListAsync();
        }

        public async Task<ItemBlueprint?> GetByIdAsync(int id)
        {
            return await _context.ItemBlueprints
                .Include(ib => ib.IconMaterial)
                .Include(ib => ib.DefaultEnchantments)
                    .ThenInclude(de => de.EnchantmentDefinition)
                        .ThenInclude(ed => ed.BaseEnchantmentRef)
                .FirstOrDefaultAsync(ib => ib.Id == id);
        }

        public async Task AddAsync(ItemBlueprint entity)
        {
            await _context.ItemBlueprints.AddAsync(entity);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(ItemBlueprint entity)
        {
            _context.ItemBlueprints.Update(entity);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var entity = await _context.ItemBlueprints.FindAsync(id);
            if (entity != null)
            {
                _context.ItemBlueprints.Remove(entity);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<PagedResult<ItemBlueprint>> SearchAsync(PagedQuery query)
        {
            var queryable = _context.ItemBlueprints.AsQueryable();

            // Apply search term filter
            if (!string.IsNullOrWhiteSpace(query.SearchTerm))
            {
                var searchLower = query.SearchTerm.ToLower();
                queryable = queryable.Where(ib =>
                    ib.Name.ToLower().Contains(searchLower) ||
                    ib.Description.ToLower().Contains(searchLower) ||
                    ib.DefaultDisplayName.ToLower().Contains(searchLower));
            }

            // Get total count before pagination
            var totalCount = await queryable.CountAsync();

            // Apply sorting
            queryable = query.SortBy switch
            {
                "name" => query.SortDescending ? queryable.OrderByDescending(ib => ib.Name) : queryable.OrderBy(ib => ib.Name),
                "defaultDisplayName" => query.SortDescending ? queryable.OrderByDescending(ib => ib.DefaultDisplayName) : queryable.OrderBy(ib => ib.DefaultDisplayName),
                _ => query.SortDescending ? queryable.OrderByDescending(ib => ib.Id) : queryable.OrderBy(ib => ib.Id)
            };

            // Apply pagination
            var items = await queryable
                .Skip((query.PageNumber - 1) * query.PageSize)
                .Take(query.PageSize)
                .Include(ib => ib.IconMaterial)
                .Include(ib => ib.DefaultEnchantments)
                .ToListAsync();

            return new PagedResult<ItemBlueprint>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = query.PageNumber,
                PageSize = query.PageSize
            };
        }
    }
}

using knkwebapi_v2.Models;
using knkwebapi_v2.Properties;
using Microsoft.EntityFrameworkCore;

namespace knkwebapi_v2.Repositories
{
    public class ItemBlueprintRepository : IItemBlueprintRepository
    {
        private readonly KnKDbContext _context;

        public ItemBlueprintRepository(KnKDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ItemBlueprint>> GetAllAsync()
        {
            return await _context.ItemBlueprints
                .Include(ib => ib.IconMaterial)
                .ToListAsync();
        }

        public async Task<ItemBlueprint?> GetByIdAsync(int id)
        {
            return await _context.ItemBlueprints
                .Include(ib => ib.IconMaterial)
                .FirstOrDefaultAsync(ib => ib.Id == id);
        }

        public async Task AddAsync(ItemBlueprint itemBlueprint)
        {
            await _context.ItemBlueprints.AddAsync(itemBlueprint);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(ItemBlueprint itemBlueprint)
        {
            _context.ItemBlueprints.Update(itemBlueprint);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var itemBlueprint = await _context.ItemBlueprints.FindAsync(id);
            if (itemBlueprint != null)
            {
                _context.ItemBlueprints.Remove(itemBlueprint);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<PagedResult<ItemBlueprint>> SearchAsync(PagedQuery query)
        {
            var queryable = _context.ItemBlueprints.AsQueryable();

            // Apply search term filter
            if (!string.IsNullOrWhiteSpace(query.SearchTerm))
            {
                var searchLower = query.SearchTerm.ToLower();
                queryable = queryable.Where(ib =>
                    ib.Name.ToLower().Contains(searchLower) ||
                    ib.Description.ToLower().Contains(searchLower) ||
                    ib.DefaultDisplayName.ToLower().Contains(searchLower));
            }

            // Apply filters from dictionary
            if (query.Filters != null)
            {
                if (query.Filters.TryGetValue("iconMaterialRefId", out var iconMaterialRefIdStr))
                {
                    if (int.TryParse(iconMaterialRefIdStr, out var iconMaterialRefId))
                    {
                        queryable = queryable.Where(ib => ib.IconMaterialRefId == iconMaterialRefId);
                    }
                }
            }

            // Get total count before pagination
            var totalCount = await queryable.CountAsync();

            // Apply pagination
            var items = await queryable
                .Include(ib => ib.IconMaterial)
                .Skip((query.PageNumber - 1) * query.PageSize)
                .Take(query.PageSize)
                .ToListAsync();

            return new PagedResult<ItemBlueprint>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = query.PageNumber,
                PageSize = query.PageSize
            };
        }
    }
}
