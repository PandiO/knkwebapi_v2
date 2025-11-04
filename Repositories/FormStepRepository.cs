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
