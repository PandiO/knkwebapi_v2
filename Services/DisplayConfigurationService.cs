using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using knkwebapi_v2.Dtos;
using knkwebapi_v2.Models;
using knkwebapi_v2.Repositories.Interfaces;
using knkwebapi_v2.Services.Interfaces;

namespace knkwebapi_v2.Services
{
    public class DisplayConfigurationService : IDisplayConfigurationService
    {
        private readonly IDisplayConfigurationRepository _repo;
        private readonly IMapper _mapper;

        public DisplayConfigurationService(
            IDisplayConfigurationRepository repo,
            IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }

        public async Task<IEnumerable<DisplayConfigurationDto>> GetAllAsync(bool includeDrafts = true)
        {
            var configs = await _repo.GetAllAsync(includeDrafts);
            return _mapper.Map<IEnumerable<DisplayConfigurationDto>>(configs);
        }

        public async Task<DisplayConfigurationDto?> GetByIdAsync(int id)
        {
            if (id <= 0) return null;
            var config = await _repo.GetByIdAsync(id);
            return config == null ? null : _mapper.Map<DisplayConfigurationDto>(config);
        }

        public async Task<DisplayConfigurationDto?> GetDefaultByEntityTypeNameAsync(
            string entityTypeName,
            bool includeDrafts = false)
        {
            var config = await _repo.GetByEntityTypeNameAsync(
                entityTypeName, 
                defaultOnly: true, 
                includeDrafts: includeDrafts);
            return config == null ? null : _mapper.Map<DisplayConfigurationDto>(config);
        }

        public async Task<IEnumerable<DisplayConfigurationDto>> GetAllByEntityTypeNameAsync(
            string entityTypeName,
            bool includeDrafts = true)
        {
            var configs = await _repo.GetAllByEntityTypeNameAsync(entityTypeName, includeDrafts);
            return _mapper.Map<IEnumerable<DisplayConfigurationDto>>(configs);
        }

        public async Task<IEnumerable<string>> GetEntityTypeNamesAsync()
        {
            return await _repo.GetEntityTypeNamesAsync();
        }

        public async Task<DisplayConfigurationDto> CreateAsync(DisplayConfigurationDto dto)
        {
            if (dto == null) 
                throw new ArgumentNullException(nameof(dto));
            if (string.IsNullOrWhiteSpace(dto.EntityTypeName))
                throw new ArgumentException("EntityTypeName is required.", nameof(dto));

            // Check if default already exists
            if (dto.IsDefault)
            {
                var defaultExists = await _repo.IsDefaultExistsAsync(dto.EntityTypeName);
                if (defaultExists)
                {
                    throw new InvalidOperationException(
                        $"A default DisplayConfiguration for '{dto.EntityTypeName}' already exists.");
                }
            }

            var entity = _mapper.Map<DisplayConfiguration>(dto);
            entity.CreatedAt = DateTime.UtcNow;
            entity.IsDraft = true; // Always start as draft

            var created = await _repo.CreateAsync(entity);
            return _mapper.Map<DisplayConfigurationDto>(created);
        }

        public async Task UpdateAsync(int id, DisplayConfigurationDto dto)
        {
            if (dto == null) 
                throw new ArgumentNullException(nameof(dto));
            if (id <= 0) 
                throw new ArgumentException("Invalid id.", nameof(id));

            // Load existing with all related entities
            var existing = await _repo.GetByIdAsync(id, includeRelated: true);
            if (existing == null)
                throw new KeyNotFoundException($"DisplayConfiguration with id {id} not found.");

            // Check if default already exists (excluding current)
            if (dto.IsDefault)
            {
                var defaultExists = await _repo.IsDefaultExistsAsync(
                    dto.EntityTypeName, 
                    excludeId: id);
                if (defaultExists)
                {
                    throw new InvalidOperationException(
                        $"Another default DisplayConfiguration for '{dto.EntityTypeName}' already exists.");
                }
            }

            // Update scalar properties
            existing.Name = dto.Name;
            existing.EntityTypeName = dto.EntityTypeName;
            existing.IsDefault = dto.IsDefault;
            existing.Description = dto.Description;
            existing.SectionOrderJson = dto.SectionOrderJson ?? "[]";
            existing.UpdatedAt = DateTime.UtcNow;
            // Preserve: IsDraft, CreatedAt, ConfigurationGuid

            // Update sections and their fields (cascade)
            UpdateSections(existing, dto.Sections);

            await _repo.UpdateAsync(existing);
        }

        private void UpdateSections(DisplayConfiguration config, List<DisplaySectionDto> sectionDtos)
        {
            // Get incoming section IDs (excluding temporary IDs that start with "temp-")
            var incomingSectionIds = sectionDtos
                .Where(s => !string.IsNullOrEmpty(s.Id) && !s.Id.StartsWith("temp-"))
                .Select(s => int.Parse(s.Id!))
                .ToHashSet();

            // Remove sections that are no longer in the DTO
            var sectionsToRemove = config.Sections
                .Where(s => !incomingSectionIds.Contains(s.Id))
                .ToList();
            
            foreach (var section in sectionsToRemove)
            {
                config.Sections.Remove(section);
            }

            // Update existing sections or add new ones
            foreach (var sectionDto in sectionDtos)
            {
                // Check if this is a new section (null/empty ID or temporary ID)
                bool isNewSection = string.IsNullOrEmpty(sectionDto.Id) || sectionDto.Id.StartsWith("temp-");
                
                if (!isNewSection)
                {
                    // Update existing section
                    var existingSection = config.Sections.FirstOrDefault(s => s.Id == int.Parse(sectionDto.Id!));
                    if (existingSection != null)
                    {
                        UpdateSection(existingSection, sectionDto);
                    }
                }
                else
                {
                    // Add new section
                    var newSection = _mapper.Map<DisplaySection>(sectionDto);
                    newSection.DisplayConfigurationId = config.Id;
                    newSection.CreatedAt = DateTime.UtcNow;
                    config.Sections.Add(newSection);
                }
            }
        }

        private void UpdateSection(DisplaySection section, DisplaySectionDto dto)
        {
            // Update scalar properties
            section.SectionName = dto.SectionName;
            section.Description = dto.Description;
            section.IsReusable = dto.IsReusable;
            section.SourceSectionId = !string.IsNullOrEmpty(dto.SourceSectionId) ? int.Parse(dto.SourceSectionId) : null;
            section.IsLinkedToSource = dto.IsLinkedToSource;
            section.FieldOrderJson = dto.FieldOrderJson ?? "[]";
            section.RelatedEntityPropertyName = dto.RelatedEntityPropertyName;
            section.RelatedEntityTypeName = dto.RelatedEntityTypeName;
            section.IsCollection = dto.IsCollection;
            section.ActionButtonsConfigJson = dto.ActionButtonsConfigJson ?? "{}";
            section.ParentSectionId = !string.IsNullOrEmpty(dto.ParentSectionId) ? int.Parse(dto.ParentSectionId) : null;
            section.UpdatedAt = DateTime.UtcNow;
            // Preserve: SectionGuid, CreatedAt

            // Update fields (cascade)
            UpdateFields(section, dto.Fields);

            // Update subsections (recursive)
            UpdateSubSections(section, dto.SubSections);
        }

        private void UpdateFields(DisplaySection section, List<DisplayFieldDto> fieldDtos)
        {
            // Get incoming field IDs (excluding temporary IDs that start with "temp-")
            var incomingFieldIds = fieldDtos
                .Where(f => !string.IsNullOrEmpty(f.Id) && !f.Id.StartsWith("temp-"))
                .Select(f => int.Parse(f.Id!))
                .ToHashSet();

            // Remove fields that are no longer in the DTO
            var fieldsToRemove = section.Fields
                .Where(f => !incomingFieldIds.Contains(f.Id))
                .ToList();
            
            foreach (var field in fieldsToRemove)
            {
                section.Fields.Remove(field);
            }

            // Update existing fields or add new ones
            foreach (var fieldDto in fieldDtos)
            {
                // Check if this is a new field (null/empty ID or temporary ID)
                bool isNewField = string.IsNullOrEmpty(fieldDto.Id) || fieldDto.Id.StartsWith("temp-");
                
                if (!isNewField)
                {
                    // Update existing field
                    var existingField = section.Fields.FirstOrDefault(f => f.Id == int.Parse(fieldDto.Id!));
                    if (existingField != null)
                    {
                        existingField.FieldName = fieldDto.FieldName;
                        existingField.Label = fieldDto.Label;
                        existingField.Description = fieldDto.Description;
                        existingField.TemplateText = fieldDto.TemplateText;
                        existingField.FieldType = fieldDto.FieldType;
                        // Persist hot edit flag
                        existingField.IsEditableInDisplay = fieldDto.IsEditableInDisplay;
                        // Persist related entity dedication context
                        existingField.RelatedEntityPropertyName = fieldDto.RelatedEntityPropertyName;
                        existingField.RelatedEntityTypeName = fieldDto.RelatedEntityTypeName;
                        existingField.IsReusable = fieldDto.IsReusable;
                        existingField.SourceFieldId = !string.IsNullOrEmpty(fieldDto.SourceFieldId) ? int.Parse(fieldDto.SourceFieldId) : null;
                        existingField.IsLinkedToSource = fieldDto.IsLinkedToSource;
                        existingField.UpdatedAt = DateTime.UtcNow;
                        // Preserve: FieldGuid, CreatedAt
                    }
                }
                else
                {
                    // Add new field
                    var newField = _mapper.Map<DisplayField>(fieldDto);
                    newField.DisplaySectionId = section.Id;
                    newField.CreatedAt = DateTime.UtcNow;
                    section.Fields.Add(newField);
                }
            }
        }

        private void UpdateSubSections(DisplaySection parentSection, List<DisplaySectionDto> subSectionDtos)
        {
            // Get incoming subsection IDs (excluding temporary IDs that start with "temp-")
            var incomingSubSectionIds = subSectionDtos
                .Where(s => !string.IsNullOrEmpty(s.Id) && !s.Id.StartsWith("temp-"))
                .Select(s => int.Parse(s.Id!))
                .ToHashSet();

            // Remove subsections that are no longer in the DTO
            var subSectionsToRemove = parentSection.SubSections
                .Where(s => !incomingSubSectionIds.Contains(s.Id))
                .ToList();
            
            foreach (var subSection in subSectionsToRemove)
            {
                parentSection.SubSections.Remove(subSection);
            }

            // Update existing subsections or add new ones
            foreach (var subSectionDto in subSectionDtos)
            {
                // Check if this is a new subsection (null/empty ID or temporary ID)
                bool isNewSubSection = string.IsNullOrEmpty(subSectionDto.Id) || subSectionDto.Id.StartsWith("temp-");
                
                if (!isNewSubSection)
                {
                    // Update existing subsection (recursive)
                    var existingSubSection = parentSection.SubSections.FirstOrDefault(s => s.Id == int.Parse(subSectionDto.Id!));
                    if (existingSubSection != null)
                    {
                        UpdateSection(existingSubSection, subSectionDto);
                    }
                }
                else
                {
                    // Add new subsection
                    var newSubSection = _mapper.Map<DisplaySection>(subSectionDto);
                    newSubSection.ParentSectionId = parentSection.Id;
                    newSubSection.CreatedAt = DateTime.UtcNow;
                    parentSection.SubSections.Add(newSubSection);
                }
            }
        }

        public async Task DeleteAsync(int id)
        {
            if (id <= 0)
                throw new ArgumentException("Invalid id.", nameof(id));

            await _repo.DeleteAsync(id);
        }

        public async Task<DisplayConfigurationDto> PublishAsync(int id)
        {
            var config = await _repo.GetByIdAsync(id, includeRelated: false);
            if (config == null)
                throw new KeyNotFoundException($"DisplayConfiguration with id {id} not found.");

            if (!config.IsDraft)
            {
                throw new InvalidOperationException(
                    $"DisplayConfiguration {id} is already published.");
            }

            // Basic validation: check if has sections
            if (string.IsNullOrEmpty(config.SectionOrderJson) || config.SectionOrderJson == "[]")
            {
                throw new InvalidOperationException(
                    $"Cannot publish DisplayConfiguration {id}: No sections configured.");
            }

            // Set to published state
            config.IsDraft = false;
            config.UpdatedAt = DateTime.UtcNow;

            await _repo.UpdateAsync(config);
            
            // Reload with relationships
            var published = await _repo.GetByIdAsync(id);
            return _mapper.Map<DisplayConfigurationDto>(published);
        }
    }
}
