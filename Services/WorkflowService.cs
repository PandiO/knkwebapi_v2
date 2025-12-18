using AutoMapper;
using knkwebapi_v2.Dtos;
using knkwebapi_v2.Models;
using knkwebapi_v2.Repositories.Interfaces;
using knkwebapi_v2.Services.Interfaces;

namespace knkwebapi_v2.Services
{
    public class WorkflowService : IWorkflowService
    {
        private readonly IWorkflowRepository _workflowRepo;
        private readonly IMapper _mapper;

        public WorkflowService(IWorkflowRepository workflowRepo, IMapper mapper)
        {
            _workflowRepo = workflowRepo;
            _mapper = mapper;
        }

        public async Task<WorkflowSessionReadDto> StartSessionAsync(WorkflowSessionCreateDto dto)
        {
            var entity = _mapper.Map<WorkflowSession>(dto);
            entity.CreatedAt = DateTime.UtcNow;
            await _workflowRepo.AddSessionAsync(entity);
            return _mapper.Map<WorkflowSessionReadDto>(entity);
        }

        public async Task<WorkflowSessionReadDto?> GetSessionAsync(int id)
        {
            var sess = await _workflowRepo.GetSessionByIdAsync(id);
            return sess == null ? null : _mapper.Map<WorkflowSessionReadDto>(sess);
        }

        public async Task<WorkflowSessionReadDto?> GetSessionByGuidAsync(Guid guid)
        {
            var sess = await _workflowRepo.GetSessionByGuidAsync(guid);
            return sess == null ? null : _mapper.Map<WorkflowSessionReadDto>(sess);
        }

        public async Task<WorkflowSessionReadDto?> ResumeSessionAsync(int id)
        {
            var sess = await _workflowRepo.GetSessionByIdAsync(id);
            if (sess == null) return null;
            if (sess.Status == "Paused")
            {
                sess.Status = "InProgress";
                sess.UpdatedAt = DateTime.UtcNow;
                await _workflowRepo.UpdateSessionAsync(sess);
            }
            return _mapper.Map<WorkflowSessionReadDto>(sess);
        }

        public async Task<List<StepProgressReadDto>> GetProgressAsync(int sessionId)
        {
            var steps = await _workflowRepo.GetStepsAsync(sessionId);
            return steps.Select(_mapper.Map<StepProgressReadDto>).ToList();
        }

        public async Task<StepProgressReadDto> SetStepCompletedAsync(int sessionId, string stepKey, int? stepIndex = null)
        {
            if (string.IsNullOrWhiteSpace(stepKey)) throw new ArgumentException("stepKey is required", nameof(stepKey));
            var step = await _workflowRepo.GetStepAsync(sessionId, stepKey);
            if (step == null)
            {
                step = new StepProgress
                {
                    WorkflowSessionId = sessionId,
                    StepKey = stepKey,
                    StepIndex = stepIndex ?? 0,
                    Status = "Pending",
                    CreatedAt = DateTime.UtcNow
                };
                await _workflowRepo.AddStepAsync(step);
            }
            step.Status = "Completed";
            step.UpdatedAt = DateTime.UtcNow;
            step.CompletedAt = DateTime.UtcNow;
            await _workflowRepo.UpdateStepAsync(step);
            return _mapper.Map<StepProgressReadDto>(step);
        }

        public async Task DeleteSessionAsync(int id)
        {
            await _workflowRepo.DeleteSessionAsync(id);
        }
    }
}
