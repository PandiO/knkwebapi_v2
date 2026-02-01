using AutoMapper;
using knkwebapi_v2.Dtos;
using knkwebapi_v2.Models;
using knkwebapi_v2.Repositories.Interfaces;
using knkwebapi_v2.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using MySqlConnector;
using System.Text.Json;

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
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            // Validate that critical routing fields are provided
            if (dto.WorkflowSessionId <= 0)
                throw new ArgumentException("WorkflowSessionId must be greater than 0", nameof(dto.WorkflowSessionId));

            if (dto.StepNumber < 0)
                throw new ArgumentException("StepNumber must be >= 0", nameof(dto.StepNumber));

            dto.StepKey = dto.StepKey?.Trim();
            if (string.IsNullOrWhiteSpace(dto.StepKey))
                throw new ArgumentException("StepKey is required for workflow context", nameof(dto.StepKey));

            var payloadJson = dto.InputJson ?? dto.PayloadJson;
            var normalizedFieldName = string.IsNullOrWhiteSpace(dto.FieldName) ? null : dto.FieldName.Trim();
            if (normalizedFieldName == null && !string.IsNullOrWhiteSpace(payloadJson))
            {
                normalizedFieldName = ExtractFieldNameFromPayload(payloadJson);
            }

            if (string.IsNullOrWhiteSpace(normalizedFieldName))
                throw new ArgumentException("FieldName is required for plugin field routing", nameof(dto.FieldName));

            dto.FieldName = normalizedFieldName;

            if (string.IsNullOrWhiteSpace(dto.TaskType))
                throw new ArgumentException("TaskType is required", nameof(dto.TaskType));

            var entity = _mapper.Map<WorldTask>(dto);
            entity.CreatedAt = DateTime.UtcNow;
            entity.FieldName = normalizedFieldName;
            if (payloadJson != null)
            {
                entity.InputJson ??= payloadJson;
                entity.PayloadJson ??= payloadJson;
            }
            
            // Generate unique LinkCode (6-digit random code)
            entity.LinkCode = GenerateLinkCode();
            
            await _taskRepo.AddAsync(entity);
            return _mapper.Map<WorldTaskReadDto>(entity);
        }

        public async Task<WorldTaskReadDto?> GetByIdAsync(int id)
        {
            var item = await _taskRepo.GetByIdAsync(id);
            return item == null ? null : _mapper.Map<WorldTaskReadDto>(item);
        }

        public async Task<WorldTaskReadDto?> GetByLinkCodeAsync(string linkCode)
        {
            var item = await _taskRepo.GetByLinkCodeAsync(linkCode);
            return item == null ? null : _mapper.Map<WorldTaskReadDto>(item);
        }

        public async Task<List<WorldTaskReadDto>> ListByStatusAsync(string status, string? serverId = null)
        {
            var items = await _taskRepo.ListByStatusAsync(status, serverId);
            return items.Select(_mapper.Map<WorldTaskReadDto>).ToList();
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
                var now = DateTime.UtcNow;
                entity.CompletedAt = now;

                // Link to StepProgress if configured (idempotent, handles duplicate keys)
                if (!string.IsNullOrWhiteSpace(entity.StepKey))
                {
                    var step = await GetOrCreateStepAsync(entity.WorkflowSessionId, entity.StepKey!, entity.StepNumber ?? 0);
                    if (step.Status != "Completed" || step.CompletedAt == null)
                    {
                        step.Status = "Completed";
                        step.UpdatedAt = now;
                        step.CompletedAt = now;
                        await _workflowRepo.UpdateStepAsync(step);
                    }
                }
            }

            await _taskRepo.UpdateAsync(entity);
            return _mapper.Map<WorldTaskReadDto>(entity);
        }

        public async Task<WorldTaskReadDto> ClaimAsync(int id, ClaimTaskDto dto)
        {
            var entity = await _taskRepo.GetByIdAsync(id) ?? throw new KeyNotFoundException();
            
            // Idempotent check: if already InProgress, return existing
            if (entity.Status == "InProgress")
            {
                return _mapper.Map<WorldTaskReadDto>(entity);
            }

            if (entity.Status != "Pending")
            {
                throw new InvalidOperationException($"Task cannot be claimed in status: {entity.Status}");
            }

            entity.Status = "InProgress";
            entity.ClaimedAt = DateTime.UtcNow;
            entity.UpdatedAt = DateTime.UtcNow;
            entity.ClaimedByServerId = dto.ClaimedByServerId;
            entity.ClaimedByMinecraftUsername = dto.ClaimedByMinecraftUsername;

            await _taskRepo.UpdateAsync(entity);
            return _mapper.Map<WorldTaskReadDto>(entity);
        }

        public async Task<WorldTaskReadDto> CompleteAsync(int id, CompleteTaskDto dto)
        {
            var entity = await _taskRepo.GetByIdAsync(id) ?? throw new KeyNotFoundException();
            
            // Idempotent check: if already Completed, return existing
            if (entity.Status == "Completed")
            {
                return _mapper.Map<WorldTaskReadDto>(entity);
            }

            entity.Status = "Completed";
            entity.OutputJson = dto.OutputJson;
            entity.CompletedAt = DateTime.UtcNow;
            entity.UpdatedAt = DateTime.UtcNow;

            // Link to StepProgress if configured
            if (!string.IsNullOrWhiteSpace(entity.StepKey))
            {
                var step = await GetOrCreateStepAsync(entity.WorkflowSessionId, entity.StepKey!, entity.StepNumber ?? 0);
                var now = DateTime.UtcNow;
                if (step.Status != "Completed" || step.CompletedAt == null)
                {
                    step.Status = "Completed";
                    step.UpdatedAt = now;
                    step.CompletedAt = now;
                    await _workflowRepo.UpdateStepAsync(step);
                }
            }

            await _taskRepo.UpdateAsync(entity);
            return _mapper.Map<WorldTaskReadDto>(entity);
        }

        public async Task<WorldTaskReadDto> FailAsync(int id, FailTaskDto dto)
        {
            var entity = await _taskRepo.GetByIdAsync(id) ?? throw new KeyNotFoundException();
            
            entity.Status = "Failed";
            entity.ErrorMessage = dto.ErrorMessage;
            entity.UpdatedAt = DateTime.UtcNow;

            await _taskRepo.UpdateAsync(entity);
            return _mapper.Map<WorldTaskReadDto>(entity);
        }

        public async Task DeleteAsync(int id)
        {
            await _taskRepo.DeleteAsync(id);
        }
        /// <summary>
        /// Creates a world task from FormField context with guaranteed step/field information.
        /// </summary>
        public async Task<WorldTaskReadDto> CreateFromFormFieldAsync(
            int workflowSessionId,
            int stepNumber,
            string stepKey,
            string fieldName,
            string taskType,
            string? inputJson = null,
            int? assignedUserId = null)
        {
            var dto = new WorldTaskCreateDto
            {
                WorkflowSessionId = workflowSessionId,
                StepNumber = stepNumber,
                StepKey = stepKey,
                FieldName = fieldName,
                TaskType = taskType,
                InputJson = inputJson,
                AssignedUserId = assignedUserId
            };
            return await CreateAsync(dto);
        }

        private static string GenerateLinkCode()
        {
            // Generate a 6-digit random alphanumeric code
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, 6)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        private async Task<StepProgress> GetOrCreateStepAsync(int sessionId, string stepKey, int stepIndex)
        {
            var normalizedStepKey = stepKey?.Trim();
            if (string.IsNullOrWhiteSpace(normalizedStepKey)) throw new ArgumentException("stepKey is required", nameof(stepKey));

            var existing = await _workflowRepo.GetStepAsync(sessionId, normalizedStepKey);
            if (existing != null) return existing;

            var now = DateTime.UtcNow;
            var step = new StepProgress
            {
                WorkflowSessionId = sessionId,
                StepKey = normalizedStepKey,
                StepIndex = stepIndex,
                Status = "Pending",
                CreatedAt = now,
                UpdatedAt = now
            };

            try
            {
                await _workflowRepo.AddStepAsync(step);
                return step;
            }
            catch (DbUpdateException ex) when (IsDuplicateStepKeyException(ex))
            {
                return await _workflowRepo.GetStepAsync(sessionId, normalizedStepKey)
                    ?? throw new InvalidOperationException($"Step '{normalizedStepKey}' not found after duplicate key exception.");
            }
        }

        private static bool IsDuplicateStepKeyException(DbUpdateException ex) =>
            ex.InnerException is MySqlException { Number: 1062 };

        private static string? ExtractFieldNameFromPayload(string payloadJson)
        {
            if (string.IsNullOrWhiteSpace(payloadJson)) return null;

            // Try raw payload first, then a JSON-unescaped version in case the payload arrived double-encoded.
            foreach (var candidate in EnumeratePayloadCandidates(payloadJson))
            {
                var value = TryParseFieldName(candidate);
                if (!string.IsNullOrWhiteSpace(value)) return value;
            }

            return null;
        }

        private static IEnumerable<string> EnumeratePayloadCandidates(string payload)
        {
            yield return payload;

            // If payload looks escaped (contains \" or starts/ends with quotes), attempt to unescape once.
            var looksEscaped = payload.Contains("\\\"") || (payload.StartsWith("\"") && payload.EndsWith("\""));
            if (looksEscaped)
            {
                string? unescaped = null;
                try
                {
                    unescaped = JsonSerializer.Deserialize<string>(payload);
                }
                catch (JsonException)
                {
                    // Ignore; the raw candidate may still work.
                }

                if (!string.IsNullOrWhiteSpace(unescaped)) yield return unescaped;
            }
        }

        private static string? TryParseFieldName(string json)
        {
            try
            {
                using var doc = JsonDocument.Parse(json);
                if (doc.RootElement.ValueKind != JsonValueKind.Object) return null;

                foreach (var prop in doc.RootElement.EnumerateObject())
                {
                    if (string.Equals(prop.Name, "fieldName", StringComparison.OrdinalIgnoreCase))
                    {
                        var value = prop.Value.GetString();
                        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
                    }
                }
            }
            catch (JsonException)
            {
                // Ignore malformed payloads; caller will handle missing field names.
            }

            return null;
        }
    }
}
