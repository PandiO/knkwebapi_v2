using knkwebapi_v2.Dtos;

namespace knkwebapi_v2.Services.Interfaces
{
    public interface IWorkflowService
    {
        Task<WorkflowSessionReadDto> StartSessionAsync(WorkflowSessionCreateDto dto);
        Task<WorkflowSessionReadDto?> GetSessionAsync(int id);
        Task<WorkflowSessionReadDto?> GetSessionByGuidAsync(Guid guid);
        Task<WorkflowSessionReadDto?> ResumeSessionAsync(int id);
        Task<List<StepProgressReadDto>> GetProgressAsync(int sessionId);
        Task<StepProgressReadDto> SetStepCompletedAsync(int sessionId, string stepKey, int? stepIndex = null);
        Task DeleteSessionAsync(int id);
    }
}
