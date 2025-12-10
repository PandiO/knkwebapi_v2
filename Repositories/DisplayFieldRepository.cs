using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using knkwebapi_v2.Models;
using knkwebapi_v2.Properties;
using knkwebapi_v2.Repositories.Interfaces;

namespace knkwebapi_v2.Repositories
{
    public class DisplayFieldRepository : IDisplayFieldRepository
    {
        private readonly KnKDbContext _context;

        public DisplayFieldRepository(KnKDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<DisplayField>> GetAllReusableAsync()
        {
            return await _context.DisplayFields
                .Where(f => f.IsReusable)
                .ToListAsync();
        }

        public async Task<DisplayField?> GetByIdAsync(int id)
        {
            return await _context.DisplayFields
                .FirstOrDefaultAsync(f => f.Id == id);
        }

        public async Task<DisplayField> CreateAsync(DisplayField field)
        {
            await _context.DisplayFields.AddAsync(field);
            await _context.SaveChangesAsync();
            return field;
        }

        public async Task UpdateAsync(DisplayField field)
        {
            _context.DisplayFields.Update(field);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var field = await _context.DisplayFields.FindAsync(id);
            if (field != null)
            {
                _context.DisplayFields.Remove(field);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<DisplayField?> GetSourceFieldAsync(int sourceFieldId)
        {
            return await _context.DisplayFields
                .FirstOrDefaultAsync(f => f.Id == sourceFieldId);
        }
    }
}
