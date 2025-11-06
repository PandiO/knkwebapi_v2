using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using knkwebapi_v2.Models;
using knkwebapi_v2.Repositories;
using knkwebapi_v2.Dtos;
using AutoMapper;

namespace knkwebapi_v2.Services
{
    public class FormSubmissionProgressService : IFormSubmissionProgressService
    {
        private readonly IFormSubmissionProgressRepository _repo;
        private readonly IMapper _mapper;

        public FormSubmissionProgressService(IFormSubmissionProgressRepository repo, IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }

        public async Task<IEnumerable<FormSubmissionProgressDto>> GetByUserIdAsync(int userId)
        {
            if (userId <= 0) return new List<FormSubmissionProgressDto>();
            var list = await _repo.GetByUserIdAsync(userId);
            return _mapper.Map<IEnumerable<FormSubmissionProgressDto>>(list);
        }

        public async Task<FormSubmissionProgressDto?> GetByIdAsync(int id)
        {
            if (id <= 0) return null;
            var entity = await _repo.GetByIdAsync(id);
            return entity == null ? null : _mapper.Map<FormSubmissionProgressDto>(entity);
        }

        public async Task<FormSubmissionProgressDto> SaveProgressAsync(FormSubmissionProgressDto progress)
        {
            if (progress == null) throw new ArgumentNullException(nameof(progress));
            if (string.IsNullOrWhiteSpace(progress.UserId)) throw new ArgumentException("UserId is required.", nameof(progress));
            if (string.IsNullOrWhiteSpace(progress.FormConfigurationId)) throw new ArgumentException("FormConfigurationId is required.", nameof(progress));

            // Validate UserId can be parsed to int
            if (!int.TryParse(progress.UserId, out var userId) || userId <= 0)
                throw new ArgumentException("UserId must be a valid positive integer.", nameof(progress));
            
            // Validate FormConfigurationId can be parsed to int
            if (!int.TryParse(progress.FormConfigurationId, out var configId) || configId <= 0)
                throw new ArgumentException("FormConfigurationId must be a valid positive integer.", nameof(progress));

            var entity = _mapper.Map<FormSubmissionProgress>(progress);
            await _repo.AddAsync(entity);
            return _mapper.Map<FormSubmissionProgressDto>(entity);
        }

        public async Task<FormSubmissionProgressDto> UpdateProgressAsync(int id, FormSubmissionProgressDto progress)
        {
            if (progress == null) throw new ArgumentNullException(nameof(progress));
            if (id <= 0) throw new ArgumentException("Invalid id.", nameof(id));

            var existing = await _repo.GetByIdAsync(id);
            if (existing == null) throw new KeyNotFoundException($"FormSubmissionProgress with id {id} not found.");

            var incoming = _mapper.Map<FormSubmissionProgress>(progress);
            existing.CurrentStepIndex = incoming.CurrentStepIndex;
            existing.CurrentStepDataJson = incoming.CurrentStepDataJson;
            existing.AllStepsDataJson = incoming.AllStepsDataJson;
            existing.Status = incoming.Status;
            if (string.Equals(incoming.Status, "Completed", StringComparison.OrdinalIgnoreCase))
            {
                existing.CompletedAt = DateTime.UtcNow;
            }

            await _repo.UpdateAsync(existing);
            return _mapper.Map<FormSubmissionProgressDto>(existing);
        }

        public async Task DeleteAsync(int id)
        {
            if (id <= 0) throw new ArgumentException("Invalid id.", nameof(id));
            var existing = await _repo.GetByIdAsync(id);
            if (existing == null) throw new KeyNotFoundException($"FormSubmissionProgress with id {id} not found.");

            await _repo.DeleteAsync(id);
        }
    }
}
