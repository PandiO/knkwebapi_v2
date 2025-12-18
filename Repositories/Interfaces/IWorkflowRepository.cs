using knkwebapi_v2.Models;

namespace knkwebapi_v2.Repositories.Interfaces
{
    public interface IWorkflowRepository
    {
        Task<WorkflowSession?> GetSessionByIdAsync(int id);
        Task<WorkflowSession?> GetSessionByGuidAsync(Guid guid);
        Task AddSessionAsync(WorkflowSession session);
        Task UpdateSessionAsync(WorkflowSession session);
        Task DeleteSessionAsync(int id);

        Task<StepProgress?> GetStepAsync(int sessionId, string stepKey);
        Task<List<StepProgress>> GetStepsAsync(int sessionId);
        Task AddStepAsync(StepProgress step);
        Task UpdateStepAsync(StepProgress step);
    }
}
