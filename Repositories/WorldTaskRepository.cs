using knkwebapi_v2.Models;
using knkwebapi_v2.Properties;
using Microsoft.EntityFrameworkCore;

namespace knkwebapi_v2.Repositories
{
    public class WorldTaskRepository : Interfaces.IWorldTaskRepository
    {
        private readonly KnKDbContext _context;

        public WorldTaskRepository(KnKDbContext context)
        {
            _context = context;
        }

        public async Task<WorldTask?> GetByIdAsync(int id)
        {
            return await _context.WorldTasks
                .Include(t => t.WorkflowSession)
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task AddAsync(WorldTask task)
        {
            await _context.WorldTasks.AddAsync(task);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(WorldTask task)
        {
            _context.WorldTasks.Update(task);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var entity = await _context.WorldTasks.FindAsync(id);
            if (entity != null)
            {
                _context.WorldTasks.Remove(entity);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<PagedResult<WorldTask>> SearchAsync(PagedQuery query)
        {
            var q = _context.WorldTasks.AsQueryable();

            if (!string.IsNullOrWhiteSpace(query.SearchTerm))
            {
                var term = query.SearchTerm.ToLower();
                q = q.Where(t => t.TaskType.ToLower().Contains(term) || (t.Status.ToLower().Contains(term)));
            }

            if (query.Filters != null)
            {
                if (query.Filters.TryGetValue("sessionId", out var sid) && int.TryParse(sid, out var sessionId))
                {
                    q = q.Where(t => t.WorkflowSessionId == sessionId);
                }
                if (query.Filters.TryGetValue("status", out var status) && !string.IsNullOrWhiteSpace(status))
                {
                    q = q.Where(t => t.Status == status);
                }
                if (query.Filters.TryGetValue("assignedUserId", out var auid) && int.TryParse(auid, out var assignedUserId))
                {
                    q = q.Where(t => t.AssignedUserId == assignedUserId);
                }
            }

            q = ApplySorting(q, query.SortBy, query.SortDescending);

            var total = await q.CountAsync();
            var items = await q
                .OrderByDescending(t => t.CreatedAt)
                .Skip((query.PageNumber - 1) * query.PageSize)
                .Take(query.PageSize)
                .ToListAsync();

            return new PagedResult<WorldTask>
            {
                Items = items,
                TotalCount = total,
                PageNumber = query.PageNumber,
                PageSize = query.PageSize
            };
        }

        public async Task<List<WorldTask>> GetBySessionAsync(int sessionId)
        {
            return await _context.WorldTasks
                .Where(t => t.WorkflowSessionId == sessionId)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
        }

        public async Task<PagedResult<WorldTask>> GetByUserAsync(int userId, PagedQuery query)
        {
            query.Filters ??= new Dictionary<string, string>();
            query.Filters["assignedUserId"] = userId.ToString();
            return await SearchAsync(query);
        }

        private static IQueryable<WorldTask> ApplySorting(IQueryable<WorldTask> q, string? sortBy, bool desc)
        {
            if (string.IsNullOrWhiteSpace(sortBy)) return desc ? q.OrderByDescending(t => t.CreatedAt) : q.OrderBy(t => t.CreatedAt);
            return sortBy.ToLower() switch
            {
                "status" => desc ? q.OrderByDescending(t => t.Status) : q.OrderBy(t => t.Status),
                "tasktype" => desc ? q.OrderByDescending(t => t.TaskType) : q.OrderBy(t => t.TaskType),
                _ => desc ? q.OrderByDescending(t => t.CreatedAt) : q.OrderBy(t => t.CreatedAt)
            };
        }
    }
}
