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
            return await _context.Domains.Include(d => d.ParentDomain).ToListAsync();
        }

        public async Task<Domain?> GetByIdAsync(int id)
        {
            return await _context.Domains.Include(d => d.ParentDomain).FirstOrDefaultAsync(d => d.Id == id);
        }

        public async Task<Domain?> GetByWgRegionNameAsync(string regionName)
        {
            if (string.IsNullOrWhiteSpace(regionName)) return null;
            var v = regionName.ToLower();
            var domain = await _context.Domains.FirstOrDefaultAsync(d => d.WgRegionId.ToLower() == v);
            
            if (domain == null) return null;

            // Explicitly load the entire ParentDomain chain (unknown depth)
            var current = domain;
            while (current.ParentDomainId.HasValue)
            {
                await _context.Entry(current).Reference(d => d.ParentDomain).LoadAsync();
                if (current.ParentDomain == null) break;
                current = current.ParentDomain;
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
