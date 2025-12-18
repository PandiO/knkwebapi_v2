using AutoMapper;
using knkwebapi_v2.Dtos;
using knkwebapi_v2.Models;
using knkwebapi_v2.Repositories.Interfaces;
using knkwebapi_v2.Services.Interfaces;

namespace knkwebapi_v2.Services
{
    public class WorldTaskService : IWorldTaskService
    {
        private readonly IWorldTaskRepository _taskRepo;
        private readonly IWorkflowRepository _workflowRepo;
        private readonly IMapper _mapper;

        public WorldTaskService(IWorldTaskRepository taskRepo, IWorkflowRepository workflowRepo, IMapper mapper)
        {
            _taskRepo = taskRepo;
            _workflowRepo = workflowRepo;
            _mapper = mapper;
        }

        public async Task<WorldTaskReadDto> CreateAsync(WorldTaskCreateDto dto)
        {
            var entity = _mapper.Map<WorldTask>(dto);
            entity.CreatedAt = DateTime.UtcNow;
            await _taskRepo.AddAsync(entity);
            return _mapper.Map<WorldTaskReadDto>(entity);
        }

        public async Task<WorldTaskReadDto?> GetByIdAsync(int id)
        {
            var item = await _taskRepo.GetByIdAsync(id);
            return item == null ? null : _mapper.Map<WorldTaskReadDto>(item);
        }

        public async Task<PagedResultDto<WorldTaskReadDto>> SearchAsync(PagedQueryDto queryDto)
        {
            var q = _mapper.Map<PagedQuery>(queryDto);
            var result = await _taskRepo.SearchAsync(q);
            return new PagedResultDto<WorldTaskReadDto>
            {
                Items = result.Items.Select(_mapper.Map<WorldTaskReadDto>).ToList(),
                TotalCount = result.TotalCount,
                PageNumber = result.PageNumber,
                PageSize = result.PageSize
            };
        }

        public async Task<List<WorldTaskReadDto>> GetBySessionAsync(int sessionId)
        {
            var items = await _taskRepo.GetBySessionAsync(sessionId);
            return items.Select(_mapper.Map<WorldTaskReadDto>).ToList();
        }

        public async Task<PagedResultDto<WorldTaskReadDto>> GetByUserAsync(int userId, PagedQueryDto queryDto)
        {
            var q = _mapper.Map<PagedQuery>(queryDto);
            var result = await _taskRepo.GetByUserAsync(userId, q);
            return new PagedResultDto<WorldTaskReadDto>
            {
                Items = result.Items.Select(_mapper.Map<WorldTaskReadDto>).ToList(),
                TotalCount = result.TotalCount,
                PageNumber = result.PageNumber,
                PageSize = result.PageSize
            };
        }

        public async Task<WorldTaskReadDto> UpdateStatusAsync(int id, string status, string? payloadJson = null)
        {
            var entity = await _taskRepo.GetByIdAsync(id) ?? throw new KeyNotFoundException();
            entity.Status = status;
            if (payloadJson != null)
            {
                entity.PayloadJson = payloadJson;
            }
            entity.UpdatedAt = DateTime.UtcNow;
            if (string.Equals(status, "Completed", StringComparison.OrdinalIgnoreCase))
            {
                entity.CompletedAt = DateTime.UtcNow;

                // Link to StepProgress if configured
                if (!string.IsNullOrWhiteSpace(entity.StepKey))
                {
                    var step = await _workflowRepo.GetStepAsync(entity.WorkflowSessionId, entity.StepKey!);
                    if (step == null)
                    {
                        step = new StepProgress
                        {
                            WorkflowSessionId = entity.WorkflowSessionId,
                            StepKey = entity.StepKey!,
                            StepIndex = 0,
                            Status = "Pending",
                            CreatedAt = DateTime.UtcNow
                        };
                        await _workflowRepo.AddStepAsync(step);
                    }
                    step.Status = "Completed";
                    step.UpdatedAt = DateTime.UtcNow;
                    step.CompletedAt = DateTime.UtcNow;
                    await _workflowRepo.UpdateStepAsync(step);
                }
            }

            await _taskRepo.UpdateAsync(entity);
            return _mapper.Map<WorldTaskReadDto>(entity);
        }

        public async Task DeleteAsync(int id)
        {
            await _taskRepo.DeleteAsync(id);
        }
    }
}
