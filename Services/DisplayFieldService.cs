using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using knkwebapi_v2.Dtos;
using knkwebapi_v2.Enums;
using knkwebapi_v2.Models;
using knkwebapi_v2.Repositories.Interfaces;
using knkwebapi_v2.Services.Interfaces;

namespace knkwebapi_v2.Services
{
    public class DisplayFieldService : IDisplayFieldService
    {
        private readonly IDisplayFieldRepository _repo;
        private readonly IMapper _mapper;

        public DisplayFieldService(
            IDisplayFieldRepository repo,
            IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }

        public async Task<IEnumerable<DisplayFieldDto>> GetAllReusableAsync()
        {
            var fields = await _repo.GetAllReusableAsync();
            return _mapper.Map<IEnumerable<DisplayFieldDto>>(fields);
        }

        public async Task<DisplayFieldDto?> GetByIdAsync(int id)
        {
            if (id <= 0) return null;
            var field = await _repo.GetByIdAsync(id);
            return field == null ? null : _mapper.Map<DisplayFieldDto>(field);
        }

        public async Task<DisplayFieldDto> CreateReusableAsync(DisplayFieldDto dto)
        {
            if (dto == null) 
                throw new ArgumentNullException(nameof(dto));
            if (string.IsNullOrWhiteSpace(dto.Label))
                throw new ArgumentException("Label is required.", nameof(dto));

            var entity = _mapper.Map<DisplayField>(dto);
            entity.IsReusable = true;
            entity.DisplaySectionId = null; // Reusable fields are not tied to a section
            entity.CreatedAt = DateTime.UtcNow;

            var created = await _repo.CreateAsync(entity);
            return _mapper.Map<DisplayFieldDto>(created);
        }

        public async Task UpdateAsync(int id, DisplayFieldDto dto)
        {
            if (dto == null) 
                throw new ArgumentNullException(nameof(dto));
            if (id <= 0) 
                throw new ArgumentException("Invalid id.", nameof(id));

            var existing = await _repo.GetByIdAsync(id);
            if (existing == null)
                throw new KeyNotFoundException($"DisplayField with id {id} not found.");

            var entity = _mapper.Map<DisplayField>(dto);
            entity.Id = id;
            entity.CreatedAt = existing.CreatedAt;
            entity.UpdatedAt = DateTime.UtcNow;

            await _repo.UpdateAsync(entity);
        }

        public async Task DeleteAsync(int id)
        {
            if (id <= 0)
                throw new ArgumentException("Invalid id.", nameof(id));

            await _repo.DeleteAsync(id);
        }

        public async Task<DisplayFieldDto> CloneFieldAsync(
            int sourceFieldId, 
            ReuseLinkMode linkMode)
        {
            var sourceField = await _repo.GetByIdAsync(sourceFieldId);
            if (sourceField == null)
                throw new KeyNotFoundException($"Source field {sourceFieldId} not found.");

            if (!sourceField.IsReusable)
                throw new InvalidOperationException(
                    $"Field {sourceFieldId} is not marked as reusable.");

            DisplayField clonedField;

            if (linkMode == ReuseLinkMode.Link)
            {
                // LINK mode: Create lightweight reference
                clonedField = new DisplayField
                {
                    FieldGuid = Guid.NewGuid(),
                    Label = sourceField.Label, // Will be overridden from source at read-time
                    IsReusable = false,
                    SourceFieldId = sourceFieldId,
                    IsLinkedToSource = true,
                    CreatedAt = DateTime.UtcNow
                };
            }
            else
            {
                // COPY mode: Full independent clone
                clonedField = new DisplayField
                {
                    FieldGuid = Guid.NewGuid(),
                    FieldName = sourceField.FieldName,
                    Label = sourceField.Label,
                    Description = sourceField.Description,
                    TemplateText = sourceField.TemplateText,
                    FieldType = sourceField.FieldType,
                    IsReusable = false,
                    SourceFieldId = sourceFieldId, // Track origin
                    IsLinkedToSource = false,
                    CreatedAt = DateTime.UtcNow
                };
            }

            var created = await _repo.CreateAsync(clonedField);
            return _mapper.Map<DisplayFieldDto>(created);
        }
    }
}
