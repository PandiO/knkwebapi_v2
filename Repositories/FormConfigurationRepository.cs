using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using knkwebapi_v2.Models;
using knkwebapi_v2.Properties;
using Microsoft.EntityFrameworkCore;

namespace knkwebapi_v2.Repositories
{
    public class FormConfigurationRepository : IFormConfigurationRepository
    {
        private readonly KnKDbContext _context;

        public FormConfigurationRepository(KnKDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<FormConfiguration>> GetAllAsync()
        {
            return await _context.FormConfigurations
                .Include(fc => fc.Steps)
                    .ThenInclude(s => s.Fields)
                        .ThenInclude(f => f.Validations)
                .Include(fc => fc.Steps)
                    .ThenInclude(s => s.StepConditions)
                .ToListAsync();
        }

        public async Task<FormConfiguration?> GetByIdAsync(int id)
        {
            return await _context.FormConfigurations
                .Include(fc => fc.Steps)
                    .ThenInclude(s => s.Fields)
                        .ThenInclude(f => f.Validations)
                .Include(fc => fc.Steps)
                    .ThenInclude(s => s.StepConditions)
                .FirstOrDefaultAsync(fc => fc.Id == id);
        }

        public async Task<FormConfiguration?> GetByEntityNameAsync(string entityName, bool defaultOnly = false)
        {
            var query = _context.FormConfigurations
                .Include(fc => fc.Steps)
                    .ThenInclude(s => s.Fields)
                        .ThenInclude(f => f.Validations)
                .Include(fc => fc.Steps)
                    .ThenInclude(s => s.StepConditions)
                .Where(fc => fc.EntityName == entityName);

            if (defaultOnly)
            {
                query = query.Where(fc => fc.IsDefault);
            }

            return await query.FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<FormConfiguration>> GetByEntityNameAllAsync(string entityName)
        {
            return await _context.FormConfigurations
                .Include(fc => fc.Steps)
                    .ThenInclude(s => s.Fields)
                        .ThenInclude(f => f.Validations)
                .Include(fc => fc.Steps)
                    .ThenInclude(s => s.StepConditions)
                .Where(fc => fc.EntityName == entityName)
                .ToListAsync();
        }

        public async Task AddAsync(FormConfiguration config)
        {
            await _context.FormConfigurations.AddAsync(config);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(FormConfiguration config)
        {
            _context.FormConfigurations.Update(config);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var config = await _context.FormConfigurations.FindAsync(id);
            if (config != null)
            {
                _context.FormConfigurations.Remove(config);
                await _context.SaveChangesAsync();
            }
        }
    }
}
