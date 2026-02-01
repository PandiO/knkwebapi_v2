using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using knkwebapi_v2.Models;
using knkwebapi_v2.Properties;
using Microsoft.EntityFrameworkCore;

namespace knkwebapi_v2.Repositories
{
    public class FormSubmissionProgressRepository : IFormSubmissionProgressRepository
    {
        private readonly KnKDbContext _context;

        public FormSubmissionProgressRepository(KnKDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<FormSubmissionProgress>> GetByEntityTypeNameAsync(string entityTypeName, int? userId)
        {
            var query = _context.FormSubmissionProgresses
                .Include(p => p.FormConfiguration)
                .Include(p => p.ParentProgress)
                .Where(p => p.FormConfiguration.EntityTypeName == entityTypeName);

            if (userId.HasValue)
            {
                query = query.Where(p => p.UserId == userId.Value);
            }

            return await query.ToListAsync();
        }
        
        public async Task<IEnumerable<FormSubmissionProgress>> GetByUserIdAsync(int userId)
        {
            return await _context.FormSubmissionProgresses
                .Where(p => p.UserId == userId)
                .Include(p => p.FormConfiguration)
                .Include(p => p.ParentProgress)
                .ToListAsync();
        }

        public async Task<FormSubmissionProgress?> GetByIdAsync(int id)
        {
            return await _context.FormSubmissionProgresses
                .Include(p => p.FormConfiguration)
                    .ThenInclude(fc => fc.Steps)
                        .ThenInclude(s => s.Fields)
                .Include(p => p.ParentProgress)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task AddAsync(FormSubmissionProgress progress)
        {
            await _context.FormSubmissionProgresses.AddAsync(progress);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(FormSubmissionProgress progress)
        {
            progress.UpdatedAt = System.DateTime.UtcNow;
            _context.FormSubmissionProgresses.Update(progress);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var progress = await _context.FormSubmissionProgresses.FindAsync(id);
            if (progress != null)
            {
                _context.FormSubmissionProgresses.Remove(progress);
                await _context.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Get all completed form submissions that are older than the specified date.
        /// Used for retention policy cleanup.
        /// </summary>
        public async Task<IEnumerable<FormSubmissionProgress>> GetCompletedOlderThanAsync(System.DateTime beforeDate)
        {
            return await _context.FormSubmissionProgresses
                .Where(p => p.Status == "Completed" && p.CompletedAt.HasValue && p.CompletedAt < beforeDate)
                .ToListAsync();
        }

        /// <summary>
        /// Delete all completed form submissions older than the specified date.
        /// Returns the count of deleted records.
        /// </summary>
        public async Task<int> DeleteCompletedOlderThanAsync(System.DateTime beforeDate)
        {
            var count = await _context.FormSubmissionProgresses
                .Where(p => p.Status == "Completed" && p.CompletedAt.HasValue && p.CompletedAt < beforeDate)
                .ExecuteDeleteAsync();
            return count;
        }
    }
}
