using knkwebapi_v2.Models;
using knkwebapi_v2.Properties;
using Microsoft.EntityFrameworkCore;

namespace knkwebapi_v2.Repositories
{
    public class GateStructureRepository : IGateStructureRepository
    {
        private readonly KnKDbContext _context;

        public GateStructureRepository(KnKDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<GateStructure>> GetAllAsync()
        {
            return await _context.Set<GateStructure>()
                .Include(gs => gs.Location)
                .Include(gs => gs.Street)
                .Include(gs => gs.District)
                .Include(gs => gs.IconMaterial)
                .Include(gs => gs.FallbackMaterial)
                .ToListAsync();
        }

        public async Task<GateStructure?> GetByIdAsync(int id)
        {
            return await _context.Set<GateStructure>()
                .Include(gs => gs.Location)
                .Include(gs => gs.Street)
                .Include(gs => gs.District)
                .Include(gs => gs.IconMaterial)
                .Include(gs => gs.FallbackMaterial)
                .FirstOrDefaultAsync(gs => gs.Id == id);
        }

        public async Task<GateStructure?> GetByIdWithSnapshotsAsync(int id)
        {
            return await _context.Set<GateStructure>()
                .Include(gs => gs.Location)
                .Include(gs => gs.Street)
                .Include(gs => gs.District)
                .Include(gs => gs.IconMaterial)
                .Include(gs => gs.FallbackMaterial)
                .Include(gs => gs.BlockSnapshots)
                .FirstOrDefaultAsync(gs => gs.Id == id);
        }

        public async Task AddGateStructureAsync(GateStructure gateStructure)
        {
            await _context.Set<GateStructure>().AddAsync(gateStructure);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateGateStructureAsync(GateStructure gateStructure)
        {
            _context.Set<GateStructure>().Update(gateStructure);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteGateStructureAsync(int id)
        {
            var gateStructure = await _context.Set<GateStructure>().FindAsync(id);
            if (gateStructure != null)
            {
                _context.Set<GateStructure>().Remove(gateStructure);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<GateStructure>> GetGatesByDomainAsync(int domainId)
        {
            // Domain is inherited through Structure, need to query via LocationId
            // For now, return all gates (will be refined when Domain relationship is clearer)
            return await _context.Set<GateStructure>()
                .Include(gs => gs.Street)
                .Include(gs => gs.District)
                .Include(gs => gs.IconMaterial)
                .ToListAsync();
        }

        public async Task<IEnumerable<GateStructure>> GetActiveGatesAsync()
        {
            return await _context.Set<GateStructure>()
                .Where(gs => gs.IsActive)
                .Include(gs => gs.Street)
                .Include(gs => gs.District)
                .Include(gs => gs.IconMaterial)
                .ToListAsync();
        }

        public async Task<bool> IsGateNameUniqueAsync(string name, int domainId, int? excludeId = null)
        {
            var query = _context.Set<GateStructure>()
                .Where(gs => gs.Name == name);

            if (excludeId.HasValue)
            {
                query = query.Where(gs => gs.Id != excludeId.Value);
            }

            return !await query.AnyAsync();
        }

        public async Task<GateStructure?> FindGateByRegionAsync(string regionId)
        {
            return await _context.Set<GateStructure>()
                .Include(gs => gs.IconMaterial)
                .FirstOrDefaultAsync(gs => 
                    gs.RegionClosedId == regionId || 
                    gs.RegionOpenedId == regionId);
        }

        public async Task UpdateGateHealthAsync(int id, double newHealth)
        {
            var gate = await _context.Set<GateStructure>().FindAsync(id);
            if (gate != null)
            {
                gate.HealthCurrent = newHealth;
                if (newHealth <= 0)
                {
                    gate.IsDestroyed = true;
                }
                await _context.SaveChangesAsync();
            }
        }

        public async Task UpdateGateStateAsync(int id, bool isOpened, bool isDestroyed)
        {
            var gate = await _context.Set<GateStructure>().FindAsync(id);
            if (gate != null)
            {
                gate.IsOpened = isOpened;
                gate.IsDestroyed = isDestroyed;
                await _context.SaveChangesAsync();
            }
        }

        // Block snapshot operations
        public async Task<IEnumerable<GateBlockSnapshot>> GetBlockSnapshotsByGateIdAsync(int gateId)
        {
            return await _context.Set<GateBlockSnapshot>()
                .Where(bs => bs.GateStructureId == gateId)
                .OrderBy(bs => bs.SortOrder)
                .ToListAsync();
        }

        public async Task AddBlockSnapshotAsync(GateBlockSnapshot snapshot)
        {
            await _context.Set<GateBlockSnapshot>().AddAsync(snapshot);
            await _context.SaveChangesAsync();
        }

        public async Task AddBlockSnapshotsAsync(IEnumerable<GateBlockSnapshot> snapshots)
        {
            await _context.Set<GateBlockSnapshot>().AddRangeAsync(snapshots);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteBlockSnapshotsByGateIdAsync(int gateId)
        {
            var snapshots = await _context.Set<GateBlockSnapshot>()
                .Where(bs => bs.GateStructureId == gateId)
                .ToListAsync();
            
            _context.Set<GateBlockSnapshot>().RemoveRange(snapshots);
            await _context.SaveChangesAsync();
        }

        public async Task<PagedResult<GateStructure>> SearchAsync(PagedQuery query)
        {
            var queryable = _context.Set<GateStructure>().AsQueryable();

            if (!string.IsNullOrWhiteSpace(query.SearchTerm))
            {
                var searchLower = query.SearchTerm.ToLower();
                queryable = queryable.Where(gs => gs.Name.ToLower().Contains(searchLower) || 
                                                   gs.Description.ToLower().Contains(searchLower));
            }

            if (query.Filters != null)
            {
                if (query.Filters.TryGetValue("streetId", out var streetIdStr) && int.TryParse(streetIdStr, out var streetId))
                {
                    queryable = queryable.Where(gs => gs.StreetId == streetId);
                }
                if (query.Filters.TryGetValue("districtId", out var districtIdStr) && int.TryParse(districtIdStr, out var districtId))
                {
                    queryable = queryable.Where(gs => gs.DistrictId == districtId);
                }
                if (query.Filters.TryGetValue("isActive", out var isActiveStr) && bool.TryParse(isActiveStr, out var isActive))
                {
                    queryable = queryable.Where(gs => gs.IsActive == isActive);
                }
                if (query.Filters.TryGetValue("gateType", out var gateType))
                {
                    queryable = queryable.Where(gs => gs.GateType == gateType);
                }
                if (query.Filters.TryGetValue("isOpened", out var isOpenedStr) && bool.TryParse(isOpenedStr, out var isOpened))
                {
                    queryable = queryable.Where(gs => gs.IsOpened == isOpened);
                }
            }

            queryable = ApplySorting(queryable, query.SortBy, query.SortDescending);

            var totalCount = await queryable.CountAsync();

            var items = await queryable
                .Include(gs => gs.Location)
                .Include(gs => gs.Street)
                .Include(gs => gs.District)
                .Include(gs => gs.IconMaterial)
                .Include(gs => gs.FallbackMaterial)
                .Skip((query.PageNumber - 1) * query.PageSize)
                .Take(query.PageSize)
                .ToListAsync();

            return new PagedResult<GateStructure>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = query.PageNumber,
                PageSize = query.PageSize
            };
        }

        private IQueryable<GateStructure> ApplySorting(IQueryable<GateStructure> queryable, string? sortBy, bool sortDescending)
        {
            if (string.IsNullOrWhiteSpace(sortBy))
                return queryable.OrderBy(gs => gs.Name);

            return sortBy.ToLower() switch
            {
                "name" => sortDescending ? queryable.OrderByDescending(gs => gs.Name) : queryable.OrderBy(gs => gs.Name),
                "id" => sortDescending ? queryable.OrderByDescending(gs => gs.Id) : queryable.OrderBy(gs => gs.Id),
                "housenumber" => sortDescending ? queryable.OrderByDescending(gs => gs.HouseNumber) : queryable.OrderBy(gs => gs.HouseNumber),
                "createdat" => sortDescending ? queryable.OrderByDescending(gs => gs.CreatedAt) : queryable.OrderBy(gs => gs.CreatedAt),
                "isactive" => sortDescending ? queryable.OrderByDescending(gs => gs.IsActive) : queryable.OrderBy(gs => gs.IsActive),
                "gatetype" => sortDescending ? queryable.OrderByDescending(gs => gs.GateType) : queryable.OrderBy(gs => gs.GateType),
                "healthcurrent" => sortDescending ? queryable.OrderByDescending(gs => gs.HealthCurrent) : queryable.OrderBy(gs => gs.HealthCurrent),
                _ => queryable.OrderBy(gs => gs.Name)
            };
        }
    }
}
