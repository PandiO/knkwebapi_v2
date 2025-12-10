using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using knkwebapi_v2.Dtos;
using knkwebapi_v2.Enums;
using knkwebapi_v2.Models;
using knkwebapi_v2.Repositories.Interfaces;
using knkwebapi_v2.Services.Interfaces;

namespace knkwebapi_v2.Services
{
    public class DisplaySectionService : IDisplaySectionService
    {
        private readonly IDisplaySectionRepository _repo;
        private readonly IMapper _mapper;

        public DisplaySectionService(
            IDisplaySectionRepository repo,
            IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }

        public async Task<IEnumerable<DisplaySectionDto>> GetAllReusableAsync()
        {
            var sections = await _repo.GetAllReusableAsync();
            return _mapper.Map<IEnumerable<DisplaySectionDto>>(sections);
        }

        public async Task<DisplaySectionDto?> GetByIdAsync(int id)
        {
            if (id <= 0) return null;
            var section = await _repo.GetByIdAsync(id);
            return section == null ? null : _mapper.Map<DisplaySectionDto>(section);
        }

        public async Task<DisplaySectionDto> CreateReusableAsync(DisplaySectionDto dto)
        {
            if (dto == null) 
                throw new ArgumentNullException(nameof(dto));
            if (string.IsNullOrWhiteSpace(dto.SectionName))
                throw new ArgumentException("SectionName is required.", nameof(dto));

            var entity = _mapper.Map<DisplaySection>(dto);
            entity.IsReusable = true;
            entity.DisplayConfigurationId = null; // Reusable sections are not tied to a config
            entity.CreatedAt = DateTime.UtcNow;

            var created = await _repo.CreateAsync(entity);
            return _mapper.Map<DisplaySectionDto>(created);
        }

        public async Task UpdateAsync(int id, DisplaySectionDto dto)
        {
            if (dto == null) 
                throw new ArgumentNullException(nameof(dto));
            if (id <= 0) 
                throw new ArgumentException("Invalid id.", nameof(id));

            var existing = await _repo.GetByIdAsync(id, includeRelated: false);
            if (existing == null)
                throw new KeyNotFoundException($"DisplaySection with id {id} not found.");

            var entity = _mapper.Map<DisplaySection>(dto);
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

        public async Task<DisplaySectionDto> CloneSectionAsync(
            int sourceSectionId, 
            ReuseLinkMode linkMode)
        {
            var sourceSection = await _repo.GetByIdAsync(sourceSectionId);
            if (sourceSection == null)
                throw new KeyNotFoundException($"Source section {sourceSectionId} not found.");

            if (!sourceSection.IsReusable)
                throw new InvalidOperationException(
                    $"Section {sourceSectionId} is not marked as reusable.");

            DisplaySection clonedSection;

            if (linkMode == ReuseLinkMode.Link)
            {
                // LINK mode: Create lightweight reference
                clonedSection = new DisplaySection
                {
                    SectionGuid = Guid.NewGuid(),
                    SectionName = sourceSection.SectionName, // Will be overridden from source at read-time
                    IsReusable = false,
                    SourceSectionId = sourceSectionId,
                    IsLinkedToSource = true,
                    FieldOrderJson = sourceSection.FieldOrderJson, // Can be customized
                    CreatedAt = DateTime.UtcNow
                };
            }
            else
            {
                // COPY mode: Full independent clone
                clonedSection = new DisplaySection
                {
                    SectionGuid = Guid.NewGuid(),
                    SectionName = sourceSection.SectionName,
                    Description = sourceSection.Description,
                    IsReusable = false,
                    SourceSectionId = sourceSectionId, // Track origin
                    IsLinkedToSource = false,
                    FieldOrderJson = sourceSection.FieldOrderJson,
                    RelatedEntityPropertyName = sourceSection.RelatedEntityPropertyName,
                    RelatedEntityTypeName = sourceSection.RelatedEntityTypeName,
                    IsCollection = sourceSection.IsCollection,
                    ActionButtonsConfigJson = sourceSection.ActionButtonsConfigJson,
                    CreatedAt = DateTime.UtcNow
                };

                // Clone fields
                clonedSection.Fields = sourceSection.Fields.Select(f => new DisplayField
                {
                    FieldGuid = Guid.NewGuid(),
                    FieldName = f.FieldName,
                    Label = f.Label,
                    Description = f.Description,
                    TemplateText = f.TemplateText,
                    FieldType = f.FieldType,
                    IsReusable = false,
                    SourceFieldId = f.Id,
                    IsLinkedToSource = false,
                    CreatedAt = DateTime.UtcNow
                }).ToList();

                // Clone subsections recursively
                clonedSection.SubSections = sourceSection.SubSections.Select(ss => CloneSubSection(ss)).ToList();
            }

            var created = await _repo.CreateAsync(clonedSection);
            return _mapper.Map<DisplaySectionDto>(created);
        }

        private DisplaySection CloneSubSection(DisplaySection source)
        {
            var cloned = new DisplaySection
            {
                SectionGuid = Guid.NewGuid(),
                SectionName = source.SectionName,
                Description = source.Description,
                IsReusable = false,
                SourceSectionId = source.Id,
                IsLinkedToSource = false,
                FieldOrderJson = source.FieldOrderJson,
                RelatedEntityPropertyName = source.RelatedEntityPropertyName,
                RelatedEntityTypeName = source.RelatedEntityTypeName,
                IsCollection = source.IsCollection,
                ActionButtonsConfigJson = source.ActionButtonsConfigJson,
                CreatedAt = DateTime.UtcNow
            };

            // Clone fields
            cloned.Fields = source.Fields.Select(f => new DisplayField
            {
                FieldGuid = Guid.NewGuid(),
                FieldName = f.FieldName,
                Label = f.Label,
                Description = f.Description,
                TemplateText = f.TemplateText,
                FieldType = f.FieldType,
                IsReusable = false,
                SourceFieldId = f.Id,
                IsLinkedToSource = false,
                CreatedAt = DateTime.UtcNow
            }).ToList();

            // Recursively clone subsections
            cloned.SubSections = source.SubSections.Select(ss => CloneSubSection(ss)).ToList();

            return cloned;
        }
    }
}
