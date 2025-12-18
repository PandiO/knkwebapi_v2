using knkwebapi_v2.Models;
using knkwebapi_v2.Properties;
using Microsoft.EntityFrameworkCore;

namespace knkwebapi_v2.Repositories
{
    public class WorkflowRepository : Interfaces.IWorkflowRepository
    {
        private readonly KnKDbContext _context;

        public WorkflowRepository(KnKDbContext context)
        {
            _context = context;
        }

        public async Task<WorkflowSession?> GetSessionByIdAsync(int id)
        {
            return await _context.WorkflowSessions
                .Include(s => s.Steps)
                .Include(s => s.WorldTasks)
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<WorkflowSession?> GetSessionByGuidAsync(Guid guid)
        {
            return await _context.WorkflowSessions
                .Include(s => s.Steps)
                .Include(s => s.WorldTasks)
                .FirstOrDefaultAsync(s => s.SessionGuid == guid);
        }

        public async Task AddSessionAsync(WorkflowSession session)
        {
            await _context.WorkflowSessions.AddAsync(session);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateSessionAsync(WorkflowSession session)
        {
            _context.WorkflowSessions.Update(session);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteSessionAsync(int id)
        {
            var session = await _context.WorkflowSessions.FindAsync(id);
            if (session != null)
            {
                _context.WorkflowSessions.Remove(session);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<StepProgress?> GetStepAsync(int sessionId, string stepKey)
        {
            return await _context.StepProgresses
                .FirstOrDefaultAsync(s => s.WorkflowSessionId == sessionId && s.StepKey == stepKey);
        }

        public async Task<List<StepProgress>> GetStepsAsync(int sessionId)
        {
            return await _context.StepProgresses
                .Where(s => s.WorkflowSessionId == sessionId)
                .OrderBy(s => s.StepIndex)
                .ToListAsync();
        }

        public async Task AddStepAsync(StepProgress step)
        {
            await _context.StepProgresses.AddAsync(step);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateStepAsync(StepProgress step)
        {
            _context.StepProgresses.Update(step);
            await _context.SaveChangesAsync();
        }
    }
}
