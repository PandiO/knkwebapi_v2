using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using knkwebapi_v2.Models;
using knkwebapi_v2.Properties;
using knkwebapi_v2.Repositories.Interfaces;

namespace knkwebapi_v2.Repositories
{
    public class DisplayConfigurationRepository : IDisplayConfigurationRepository
    {
        private readonly KnKDbContext _context;

        public DisplayConfigurationRepository(KnKDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<DisplayConfiguration>> GetAllAsync(bool includeDrafts = true)
        {
            var query = _context.DisplayConfigurations
                .Include(dc => dc.Sections)
                    .ThenInclude(s => s.Fields)
                .Include(dc => dc.Sections)
                    .ThenInclude(s => s.SubSections)
                        .ThenInclude(ss => ss.Fields)
                .AsQueryable();

            if (!includeDrafts)
            {
                query = query.Where(dc => !dc.IsDraft);
            }

            return await query.ToListAsync();
        }

        public async Task<DisplayConfiguration?> GetByIdAsync(int id, bool includeRelated = true)
        {
            if (!includeRelated)
            {
                return await _context.DisplayConfigurations
                    .FirstOrDefaultAsync(dc => dc.Id == id);
            }

            return await _context.DisplayConfigurations
                .Include(dc => dc.Sections)
                    .ThenInclude(s => s.Fields)
                .Include(dc => dc.Sections)
                    .ThenInclude(s => s.SubSections)
                        .ThenInclude(ss => ss.Fields)
                .FirstOrDefaultAsync(dc => dc.Id == id);
        }

        public async Task<DisplayConfiguration?> GetByEntityTypeNameAsync(
            string entityTypeName, 
            bool defaultOnly = false,
            bool includeDrafts = true)
        {
            var query = _context.DisplayConfigurations
                .Include(dc => dc.Sections)
                    .ThenInclude(s => s.Fields)
                .Include(dc => dc.Sections)
                    .ThenInclude(s => s.SubSections)
                        .ThenInclude(ss => ss.Fields)
                .Where(dc => dc.EntityTypeName == entityTypeName);

            if (defaultOnly)
            {
                query = query.Where(dc => dc.IsDefault);
            }

            if (!includeDrafts)
            {
                query = query.Where(dc => !dc.IsDraft);
            }

            return await query.FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<DisplayConfiguration>> GetAllByEntityTypeNameAsync(
            string entityTypeName,
            bool includeDrafts = true)
        {
            var query = _context.DisplayConfigurations
                .Include(dc => dc.Sections)
                    .ThenInclude(s => s.Fields)
                .Include(dc => dc.Sections)
                    .ThenInclude(s => s.SubSections)
                        .ThenInclude(ss => ss.Fields)
                .Where(dc => dc.EntityTypeName == entityTypeName);

            if (!includeDrafts)
            {
                query = query.Where(dc => !dc.IsDraft);
            }

            return await query.ToListAsync();
        }

        public async Task<DisplayConfiguration> CreateAsync(DisplayConfiguration config)
        {
            await _context.DisplayConfigurations.AddAsync(config);
            await _context.SaveChangesAsync();
            return config;
        }

        public async Task UpdateAsync(DisplayConfiguration config)
        {
            // Detach any existing tracked entity with the same key
            var trackedEntity = _context.ChangeTracker.Entries<DisplayConfiguration>()
                .FirstOrDefault(e => e.Entity.Id == config.Id);
            
            if (trackedEntity != null)
            {
                trackedEntity.State = EntityState.Detached;
            }
            
            _context.DisplayConfigurations.Update(config);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var config = await _context.DisplayConfigurations.FindAsync(id);
            if (config != null)
            {
                _context.DisplayConfigurations.Remove(config);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> IsDefaultExistsAsync(string entityTypeName, int? excludeId = null)
        {
            var query = _context.DisplayConfigurations
                .Where(dc => dc.EntityTypeName == entityTypeName && dc.IsDefault);

            if (excludeId.HasValue)
            {
                query = query.Where(dc => dc.Id != excludeId.Value);
            }

            return await query.AnyAsync();
        }

        public async Task<IEnumerable<string>> GetEntityTypeNamesAsync()
        {
            return await _context.DisplayConfigurations
                .Select(dc => dc.EntityTypeName)
                .Distinct()
                .ToListAsync();
        }
    }
}
