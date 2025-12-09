using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using knkwebapi_v2.Models;
using knkwebapi_v2.Properties;
using Microsoft.EntityFrameworkCore;

namespace knkwebapi_v2.Repositories
{
    public class FormStepRepository : IFormStepRepository
    {
        private readonly KnKDbContext _context;

        public FormStepRepository(KnKDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<FormStep>> GetAllReusableAsync()
        {
            return await _context.FormSteps
                .Where(s => s.IsReusable)
                .Include(s => s.Fields)
                    .ThenInclude(f => f.Validations)
                .Include(s => s.StepConditions)
                .ToListAsync();
        }

        /// <summary>
        /// Get all reusable steps for a specific entity type.
        /// A step belongs to an entity type through its associated FormConfiguration.
        /// </summary>
        public async Task<IEnumerable<FormStep>> GetAllReusableByEntityTypeAsync(string entityTypeName)
        {
            if (string.IsNullOrWhiteSpace(entityTypeName))
                return new List<FormStep>();

            // Reusable steps exist in the library (FormConfigurationId = null).
            // We can track entity type via a future tag/category mechanism.
            // For now, return all reusable steps since they're generic templates.
            // TODO: When entity type tagging is added, filter by that.
            return await _context.FormSteps
                .Where(s => s.IsReusable && s.FormConfigurationId == null)
                .Include(s => s.Fields)
                    .ThenInclude(f => f.Validations)
                .Include(s => s.StepConditions)
                .ToListAsync();
        }

        public async Task<FormStep?> GetByIdAsync(int id)
        {
            return await _context.FormSteps
                .Include(s => s.Fields)
                    .ThenInclude(f => f.Validations)
                .Include(s => s.StepConditions)
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task AddAsync(FormStep step)
        {
            await _context.FormSteps.AddAsync(step);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(FormStep step)
        {
            _context.FormSteps.Update(step);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var step = await _context.FormSteps.FindAsync(id);
            if (step != null)
            {
                _context.FormSteps.Remove(step);
                await _context.SaveChangesAsync();
            }
        }
    }
}
