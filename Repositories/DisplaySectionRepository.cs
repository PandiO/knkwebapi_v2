using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using knkwebapi_v2.Models;
using knkwebapi_v2.Properties;
using knkwebapi_v2.Repositories.Interfaces;

namespace knkwebapi_v2.Repositories
{
    public class DisplaySectionRepository : IDisplaySectionRepository
    {
        private readonly KnKDbContext _context;

        public DisplaySectionRepository(KnKDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<DisplaySection>> GetAllReusableAsync()
        {
            return await _context.DisplaySections
                .Where(s => s.IsReusable)
                .Include(s => s.Fields)
                .Include(s => s.SubSections)
                    .ThenInclude(ss => ss.Fields)
                .ToListAsync();
        }

        public async Task<DisplaySection?> GetByIdAsync(int id, bool includeRelated = true)
        {
            if (!includeRelated)
            {
                return await _context.DisplaySections
                    .FirstOrDefaultAsync(s => s.Id == id);
            }

            return await _context.DisplaySections
                .Include(s => s.Fields)
                .Include(s => s.SubSections)
                    .ThenInclude(ss => ss.Fields)
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<DisplaySection> CreateAsync(DisplaySection section)
        {
            await _context.DisplaySections.AddAsync(section);
            await _context.SaveChangesAsync();
            return section;
        }

        public async Task UpdateAsync(DisplaySection section)
        {
            _context.DisplaySections.Update(section);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var section = await _context.DisplaySections.FindAsync(id);
            if (section != null)
            {
                _context.DisplaySections.Remove(section);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<DisplaySection?> GetSourceSectionAsync(int sourceSectionId)
        {
            return await _context.DisplaySections
                .Include(s => s.Fields)
                .Include(s => s.SubSections)
                    .ThenInclude(ss => ss.Fields)
                .FirstOrDefaultAsync(s => s.Id == sourceSectionId);
        }
    }
}
