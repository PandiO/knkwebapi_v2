using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using knkwebapi_v2.Models;
using knkwebapi_v2.Repositories;
using knkwebapi_v2.Dtos;
using AutoMapper;

namespace knkwebapi_v2.Services
{
    public class FormStepService : IFormStepService
    {
        private readonly IFormStepRepository _repo;
        private readonly IMapper _mapper;

        public FormStepService(IFormStepRepository repo, IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }

        public async Task<IEnumerable<FormStepDto>> GetAllReusableAsync()
        {
            var list = await _repo.GetAllReusableAsync();
            return _mapper.Map<IEnumerable<FormStepDto>>(list);
        }

        public async Task<FormStepDto?> GetByIdAsync(int id)
        {
            if (id <= 0) return null;
            var entity = await _repo.GetByIdAsync(id);
            return entity == null ? null : _mapper.Map<FormStepDto>(entity);
        }

        public async Task<FormStepDto> CreateAsync(FormStepDto step)
        {
            if (step == null) throw new ArgumentNullException(nameof(step));
            if (string.IsNullOrWhiteSpace(step.StepName))
                throw new ArgumentException("StepName is required.", nameof(step));

            var entity = _mapper.Map<FormStep>(step);
            await _repo.AddAsync(entity);
            return _mapper.Map<FormStepDto>(entity);
        }

        public async Task UpdateAsync(int id, FormStepDto step)
        {
            if (step == null) throw new ArgumentNullException(nameof(step));
            if (id <= 0) throw new ArgumentException("Invalid id.", nameof(id));

            var existing = await _repo.GetByIdAsync(id);
            if (existing == null) throw new KeyNotFoundException($"FormStep with id {id} not found.");

            var incoming = _mapper.Map<FormStep>(step);
            existing.StepName = incoming.StepName;
            existing.Description = incoming.Description;
            existing.IsReusable = incoming.IsReusable;
            existing.FieldOrderJson = incoming.FieldOrderJson;

            await _repo.UpdateAsync(existing);
        }

        public async Task DeleteAsync(int id)
        {
            if (id <= 0) throw new ArgumentException("Invalid id.", nameof(id));
            var existing = await _repo.GetByIdAsync(id);
            if (existing == null) throw new KeyNotFoundException($"FormStep with id {id} not found.");

            await _repo.DeleteAsync(id);
        }
    }
}
