using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using knkwebapi_v2.Models;
using knkwebapi_v2.Properties;
using Microsoft.EntityFrameworkCore;

namespace knkwebapi_v2.Repositories
{
    public class FormFieldRepository : IFormFieldRepository
    {
        private readonly KnKDbContext _context;

        public FormFieldRepository(KnKDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<FormField>> GetAllReusableAsync()
        {
            return await _context.FormFields
                .Where(f => f.IsReusable)
                .Include(f => f.Validations)
                .ToListAsync();
        }

        public async Task<FormField?> GetByIdAsync(int id)
        {
            return await _context.FormFields
                .Include(f => f.Validations)
                .FirstOrDefaultAsync(f => f.Id == id);
        }

        public async Task AddAsync(FormField field)
        {
            await _context.FormFields.AddAsync(field);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(FormField field)
        {
            _context.FormFields.Update(field);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var field = await _context.FormFields.FindAsync(id);
            if (field != null)
            {
                _context.FormFields.Remove(field);
                await _context.SaveChangesAsync();
            }
        }
    }
}
