using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using knkwebapi_v2.Models;
using knkwebapi_v2.Repositories;
using knkwebapi_v2.Enums;
using knkwebapi_v2.Dtos;
using AutoMapper;

namespace knkwebapi_v2.Services
{
    public class FormFieldService : IFormFieldService
    {
        private readonly IFormFieldRepository _repo;
        private readonly IMapper _mapper;

        public FormFieldService(IFormFieldRepository repo, IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }

        public async Task<IEnumerable<FormFieldDto>> GetAllReusableAsync()
        {
            var list = await _repo.GetAllReusableAsync();
            return _mapper.Map<IEnumerable<FormFieldDto>>(list);
        }

        public async Task<FormFieldDto?> GetByIdAsync(int id)
        {
            if (id <= 0) return null;
            var entity = await _repo.GetByIdAsync(id);
            return entity == null ? null : _mapper.Map<FormFieldDto>(entity);
        }

        public async Task<FormFieldDto> CreateAsync(FormFieldDto field)
        {
            if (field == null) throw new ArgumentNullException(nameof(field));
            if (string.IsNullOrWhiteSpace(field.FieldName))
                throw new ArgumentException("FieldName is required.", nameof(field));
            if (string.IsNullOrWhiteSpace(field.Label))
                throw new ArgumentException("Label is required.", nameof(field));

            if (field.FieldType == FieldType.Object && string.IsNullOrWhiteSpace(field.ObjectType))
                throw new ArgumentException("ObjectType is required when FieldType is Object.", nameof(field));

            var entity = _mapper.Map<FormField>(field);
            await _repo.AddAsync(entity);
            return _mapper.Map<FormFieldDto>(entity);
        }

        public async Task UpdateAsync(int id, FormFieldDto field)
        {
            if (field == null) throw new ArgumentNullException(nameof(field));
            if (id <= 0) throw new ArgumentException("Invalid id.", nameof(id));

            var existing = await _repo.GetByIdAsync(id);
            if (existing == null) throw new KeyNotFoundException($"FormField with id {id} not found.");

            var incoming = _mapper.Map<FormField>(field);
            existing.FieldName = incoming.FieldName;
            existing.Label = incoming.Label;
            existing.Placeholder = incoming.Placeholder;
            existing.Description = incoming.Description;
            existing.FieldType = incoming.FieldType;
            existing.ObjectType = incoming.ObjectType;
            existing.EnumType = incoming.EnumType;
            existing.DefaultValue = incoming.DefaultValue;
            existing.Required = incoming.Required;
            existing.ReadOnly = incoming.ReadOnly;
            existing.IsReusable = incoming.IsReusable;

            await _repo.UpdateAsync(existing);
        }

        public async Task DeleteAsync(int id)
        {
            if (id <= 0) throw new ArgumentException("Invalid id.", nameof(id));
            var existing = await _repo.GetByIdAsync(id);
            if (existing == null) throw new KeyNotFoundException($"FormField with id {id} not found.");

            await _repo.DeleteAsync(id);
        }
    }
}
