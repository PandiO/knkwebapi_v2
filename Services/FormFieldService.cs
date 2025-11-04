using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using knkwebapi_v2.Models;
using knkwebapi_v2.Repositories;
using knkwebapi_v2.Enums;

namespace knkwebapi_v2.Services
{
    public class FormFieldService : IFormFieldService
    {
        private readonly IFormFieldRepository _repo;

        public FormFieldService(IFormFieldRepository repo)
        {
            _repo = repo;
        }

        public async Task<IEnumerable<FormField>> GetAllReusableAsync()
        {
            return await _repo.GetAllReusableAsync();
        }

        public async Task<FormField?> GetByIdAsync(int id)
        {
            if (id <= 0) return null;
            return await _repo.GetByIdAsync(id);
        }

        public async Task<FormField> CreateAsync(FormField field)
        {
            if (field == null) throw new ArgumentNullException(nameof(field));
            if (string.IsNullOrWhiteSpace(field.FieldName)) 
                throw new ArgumentException("FieldName is required.", nameof(field));
            if (string.IsNullOrWhiteSpace(field.Label)) 
                throw new ArgumentException("Label is required.", nameof(field));
            
            // Validate Object type has ObjectType specified
            if (field.FieldType == FieldType.Object && string.IsNullOrWhiteSpace(field.ObjectType))
                throw new ArgumentException("ObjectType is required when FieldType is Object.", nameof(field));

            await _repo.AddAsync(field);
            return field;
        }

        public async Task UpdateAsync(int id, FormField field)
        {
            if (field == null) throw new ArgumentNullException(nameof(field));
            if (id <= 0) throw new ArgumentException("Invalid id.", nameof(id));

            var existing = await _repo.GetByIdAsync(id);
            if (existing == null) throw new KeyNotFoundException($"FormField with id {id} not found.");

            existing.FieldName = field.FieldName;
            existing.Label = field.Label;
            existing.Placeholder = field.Placeholder;
            existing.Description = field.Description;
            existing.FieldType = field.FieldType;
            existing.ObjectType = field.ObjectType;
            existing.EnumType = field.EnumType;
            existing.DefaultValue = field.DefaultValue;
            existing.Required = field.Required;
            existing.ReadOnly = field.ReadOnly;
            existing.IsReusable = field.IsReusable;

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
