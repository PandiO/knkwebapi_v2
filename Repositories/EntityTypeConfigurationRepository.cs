using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using knkwebapi_v2.Models;
using knkwebapi_v2.Repositories.Interfaces;
using knkwebapi_v2.Properties;
using Microsoft.EntityFrameworkCore;

namespace knkwebapi_v2.Repositories
{
    public class EntityTypeConfigurationRepository : IEntityTypeConfigurationRepository
    {
        private readonly KnKDbContext _context;

        public EntityTypeConfigurationRepository(KnKDbContext context)
        {
            _context = context;
        }

        public async Task<EntityTypeConfiguration?> GetByIdAsync(string id)
        {
            return await _context.EntityTypeConfigurations
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<EntityTypeConfiguration?> GetByEntityTypeNameAsync(string entityTypeName)
        {
            return await _context.EntityTypeConfigurations
                .FirstOrDefaultAsync(c => c.EntityTypeName.Equals(entityTypeName, StringComparison.OrdinalIgnoreCase));
        }

        public async Task<List<EntityTypeConfiguration>> GetAllAsync()
        {
            return await _context.EntityTypeConfigurations
                .OrderBy(c => c.SortOrder)
                .ThenBy(c => c.EntityTypeName)
                .ToListAsync();
        }

        public async Task<EntityTypeConfiguration> CreateAsync(EntityTypeConfiguration configuration)
        {
            configuration.Id = Guid.NewGuid().ToString();
            configuration.CreatedAt = DateTime.UtcNow;
            configuration.UpdatedAt = DateTime.UtcNow;
            
            _context.EntityTypeConfigurations.Add(configuration);
            await _context.SaveChangesAsync();
            
            return configuration;
        }

        public async Task<EntityTypeConfiguration> UpdateAsync(EntityTypeConfiguration configuration)
        {
            configuration.UpdatedAt = DateTime.UtcNow;
            
            _context.EntityTypeConfigurations.Update(configuration);
            await _context.SaveChangesAsync();
            
            return configuration;
        }

        public async Task<bool> DeleteAsync(string id)
        {
            var config = await GetByIdAsync(id);
            if (config == null)
                return false;

            _context.EntityTypeConfigurations.Remove(config);
            await _context.SaveChangesAsync();
            
            return true;
        }

        public async Task<bool> ExistsByEntityTypeNameAsync(string entityTypeName)
        {
            return await _context.EntityTypeConfigurations
                .AnyAsync(c => c.EntityTypeName.Equals(entityTypeName, StringComparison.OrdinalIgnoreCase));
        }
    }
}
