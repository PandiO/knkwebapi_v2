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
        /// Deletes the deepest descendants first so parent/child hierarchies remain valid
        /// even when ParentProgressId is configured with Restrict delete behavior.
        /// Returns the count of deleted records.
        /// </summary>
        public async Task<int> DeleteCompletedOlderThanAsync(System.DateTime beforeDate)
        {
            var expiredRootIds = await _context.FormSubmissionProgresses
                .AsNoTracking()
                .Where(p => p.Status == "Completed" && p.CompletedAt.HasValue && p.CompletedAt < beforeDate)
                .Select(p => p.Id)
                .ToListAsync();

            if (expiredRootIds.Count == 0)
            {
                return 0;
            }

            var staleBranchIds = new HashSet<int>(expiredRootIds);
            var queue = new Queue<int>(expiredRootIds);

            while (queue.Count > 0)
            {
                var currentParentId = queue.Dequeue();

                var childIds = await _context.FormSubmissionProgresses
                    .AsNoTracking()
                    .Where(p => p.ParentProgressId == currentParentId)
                    .Select(p => p.Id)
                    .ToListAsync();

                foreach (var childId in childIds)
                {
                    if (staleBranchIds.Add(childId))
                    {
                        queue.Enqueue(childId);
                    }
                }
            }

            var descendantsByParent = await _context.FormSubmissionProgresses
                .AsNoTracking()
                .Where(p => p.ParentProgressId.HasValue && staleBranchIds.Contains(p.ParentProgressId.Value))
                .Select(p => new { ParentId = p.ParentProgressId!.Value, ChildId = p.Id })
                .GroupBy(x => x.ParentId)
                .ToDictionaryAsync(g => g.Key, g => g.Select(x => x.ChildId).ToHashSet());

            var deleteOrder = new List<int>();
            var remaining = new HashSet<int>(staleBranchIds);

            while (remaining.Count > 0)
            {
                var leafIds = remaining
                    .Where(id => !descendantsByParent.TryGetValue(id, out var childIds) || childIds.All(childId => !remaining.Contains(childId)))
                    .ToList();

                if (leafIds.Count == 0)
                {
                    leafIds = remaining.ToList();
                }

                foreach (var id in leafIds)
                {
                    remaining.Remove(id);
                    deleteOrder.Add(id);
                }
            }

            var rowsToDelete = await _context.FormSubmissionProgresses
                .Where(p => deleteOrder.Contains(p.Id))
                .ToListAsync();

            if (rowsToDelete.Count == 0)
            {
                return 0;
            }

            _context.FormSubmissionProgresses.RemoveRange(rowsToDelete);
            await _context.SaveChangesAsync();

            return rowsToDelete.Count;
        }
    }
}
