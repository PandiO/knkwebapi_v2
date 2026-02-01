using knkwebapi_v2.Models;
using knkwebapi_v2.Properties;
using Microsoft.EntityFrameworkCore;

namespace knkwebapi_v2.Repositories
{
    public class DomainRepository : IDomainRepository
    {
        private readonly KnKDbContext _context;

        public DomainRepository(KnKDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Domain>> GetAllAsync()
        {
            // Domain has no ParentDomain navigation; return plain list
            return await _context.Domains.ToListAsync();
        }

        public async Task<Domain?> GetByIdAsync(int id)
        {
            // Domain has no ParentDomain navigation; query by Id only
            return await _context.Domains.FirstOrDefaultAsync(d => d.Id == id);
        }

        public async Task<Domain?> GetByWgRegionNameAsync(string regionName)
        {
            if (string.IsNullOrWhiteSpace(regionName)) return null;
            var v = regionName.ToLower().Trim();
            var domain = await _context.Domains
                .FirstOrDefaultAsync(d => d.WgRegionId != null && d.WgRegionId.ToLower() == v);
            
            if (domain == null) return null;

            // Explicitly load the entire ParentDomain chain (unknown depth)
            Domain? current = domain;
            while (current != null)
            {
                if (current is Town) {
                    break;
                } else if (current is District district)
                {
                    await _context.Entry(district).Reference(d => d.Town).LoadAsync();
                    if (district.TownId <= 0) break;
                    current = district.Town;
                    continue;
                }  else if (current is Structure structure) {
                    await _context.Entry(structure).Reference(s => s.District).LoadAsync();
                    if (structure.DistrictId <= 0) break;
                    current = structure.District;
                    continue;
                }
                break;
            }

            return domain;
        }

        public async Task AddDomainAsync(Domain domain)
        {
            await _context.Domains.AddAsync(domain);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateDomainAsync(Domain domain)
        {
            _context.Domains.Update(domain);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteDomainAsync(int id)
        {
            var domain = await _context.Domains.FindAsync(id);
            if (domain != null)
            {
                _context.Domains.Remove(domain);
                await _context.SaveChangesAsync();
            }
        }
    }
}
