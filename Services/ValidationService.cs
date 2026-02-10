using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using knkwebapi_v2.Dtos;
using knkwebapi_v2.Models;
using knkwebapi_v2.Repositories;
using knkwebapi_v2.Services.Interfaces;

namespace knkwebapi_v2.Services
{
    /// <summary>
    /// Service implementation for field validation operations.
    /// Coordinates validation rule execution and result formatting.
    /// </summary>
    public class ValidationService : IValidationService
    {
        private readonly IFieldValidationRuleRepository _ruleRepository;
        private readonly IFormFieldRepository _fieldRepository;
        private readonly IFormConfigurationRepository _configRepository;
        private readonly IEnumerable<IValidationMethod> _validationMethods;
        private readonly IDependencyResolutionService _dependencyResolver;
        private readonly IMapper _mapper;

        public ValidationService(
            IFieldValidationRuleRepository ruleRepository,
            IFormFieldRepository fieldRepository,
            IFormConfigurationRepository configRepository,
            IEnumerable<IValidationMethod> validationMethods,
            IDependencyResolutionService dependencyResolver,
            IMapper mapper)
        {
            _ruleRepository = ruleRepository;
            _fieldRepository = fieldRepository;
            _configRepository = configRepository;
            _validationMethods = validationMethods;
            _dependencyResolver = dependencyResolver;
            _mapper = mapper;
        }

        public async Task<FieldValidationRuleDto?> GetByIdAsync(int id)
        {
            if (id <= 0) return null;
            var entity = await _ruleRepository.GetByIdAsync(id);
            return entity == null ? null : _mapper.Map<FieldValidationRuleDto>(entity);
        }

        public async Task<IEnumerable<FieldValidationRuleDto>> GetByFormFieldIdAsync(int fieldId)
        {
            var list = await _ruleRepository.GetByFormFieldIdAsync(fieldId);
            return _mapper.Map<IEnumerable<FieldValidationRuleDto>>(list);
        }

        public async Task<IEnumerable<FieldValidationRuleDto>> GetByFormConfigurationIdAsync(int formConfigurationId)
        {
            var list = await _ruleRepository.GetByFormConfigurationIdAsync(formConfigurationId);
            return _mapper.Map<IEnumerable<FieldValidationRuleDto>>(list);
        }

        /// <summary>
        /// Get validation rules for a form field with dependency path information.
        /// Returns rules as configured; dependency resolution happens on the frontend.
        /// 
        /// DEPENDENCY RESOLUTION FLOW:
        /// 1. Backend returns rules with dependencyPath property (e.g., "Town.WgRegionId")
        /// 2. Frontend's WorldBoundFieldRenderer receives these rules
        /// 3. Frontend uses resolveDependencyPath() utility to resolve based on form context
        /// 4. Resolved values are included in validationContext sent to plugin
        /// 
        /// The dependencyPath property enables multi-layer resolution:
        /// - Layer 0: Direct field value ("WgRegionId")
        /// - Layer 1: Single navigation ("Town.WgRegionId")
        /// - Layer 2+: Multi-level navigation ("District.Town.WgRegionId")
        /// </summary>
        public async Task<IEnumerable<FieldValidationRuleDto>> GetByFormFieldIdWithDependenciesAsync(
            int fieldId,
            Dictionary<string, object>? formContext = null)
        {
            var rules = await _ruleRepository.GetByFormFieldIdAsync(fieldId);
            return _mapper.Map<IEnumerable<FieldValidationRuleDto>>(rules);
        }

        public async Task<FieldValidationRuleDto> CreateAsync(CreateFieldValidationRuleDto dto)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            var field = await _fieldRepository.GetByIdAsync(dto.FormFieldId);
            if (field == null) throw new ArgumentException($"FormField {dto.FormFieldId} not found", nameof(dto.FormFieldId));

            if (dto.DependsOnFieldId.HasValue)
            {
                var dependsOn = await _fieldRepository.GetByIdAsync(dto.DependsOnFieldId.Value);
                if (dependsOn == null) throw new ArgumentException($"DependsOnField {dto.DependsOnFieldId.Value} not found", nameof(dto.DependsOnFieldId));

                var hasCircular = await _ruleRepository.HasCircularDependencyAsync(dto.FormFieldId, dto.DependsOnFieldId.Value);
                if (hasCircular) throw new ArgumentException("Circular dependency detected between fields.");
            }

            var entity = _mapper.Map<FieldValidationRule>(dto);
            var created = await _ruleRepository.CreateAsync(entity);
            return _mapper.Map<FieldValidationRuleDto>(created);
        }

        public async Task UpdateAsync(int id, UpdateFieldValidationRuleDto dto)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));
            var existing = await _ruleRepository.GetByIdAsync(id) ?? throw new KeyNotFoundException();

            if (dto.DependsOnFieldId.HasValue)
            {
                var dependsOn = await _fieldRepository.GetByIdAsync(dto.DependsOnFieldId.Value);
                if (dependsOn == null) throw new ArgumentException($"DependsOnField {dto.DependsOnFieldId.Value} not found", nameof(dto.DependsOnFieldId));

                var hasCircular = await _ruleRepository.HasCircularDependencyAsync(existing.FormFieldId, dto.DependsOnFieldId.Value);
                if (hasCircular) throw new ArgumentException("Circular dependency detected between fields.");
            }

            _mapper.Map(dto, existing);
            await _ruleRepository.UpdateAsync(existing);
        }

        public async Task DeleteAsync(int id)
        {
            var existing = await _ruleRepository.GetByIdAsync(id) ?? throw new KeyNotFoundException();
            await _ruleRepository.DeleteAsync(existing.Id);
        }

        public async Task<ValidationResultDto> ValidateFieldAsync(
            int fieldId,
            object? fieldValue,
            object? dependencyValue,
            Dictionary<string, object>? formContextData)
        {
            // Get all validation rules for this field
            var rules = await _ruleRepository.GetByFormFieldIdAsync(fieldId);
            
            if (!rules.Any())
            {
                // No rules = validation passes
                return new ValidationResultDto
                {
                    IsValid = true,
                    IsBlocking = false,
                    Message = "No validation rules configured"
                };
            }

            // Execute each rule and collect results
            var results = new List<ValidationResultDto>();
            
            foreach (var rule in rules)
            {
                var result = await ExecuteValidationRuleAsync(rule, fieldValue, dependencyValue, formContextData);
                results.Add(result);
            }

            // If any blocking rule failed, return the first failure
            var blockingFailure = results.FirstOrDefault(r => !r.IsValid && r.IsBlocking);
            if (blockingFailure != null)
            {
                return blockingFailure;
            }

            // If any non-blocking rule failed, return the first warning
            var nonBlockingFailure = results.FirstOrDefault(r => !r.IsValid && !r.IsBlocking);
            if (nonBlockingFailure != null)
            {
                return nonBlockingFailure;
            }

            // All rules passed
            var successMessages = results.Where(r => r.IsValid && !string.IsNullOrEmpty(r.Message)).Select(r => r.Message).ToList();
            return new ValidationResultDto
            {
                IsValid = true,
                IsBlocking = false,
                Message = successMessages.Any() ? string.Join("; ", successMessages) : "Validation passed"
            };
        }

        public async Task<ValidationResultDto> ValidateFieldAsync(ValidateFieldRequestDto request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            return await ValidateFieldAsync(
                request.FieldId,
                request.FieldValue,
                request.DependencyValue,
                request.FormContextData);
        }

        public async Task<Dictionary<int, ValidationResultDto>> ValidateMultipleFieldsAsync(
            IEnumerable<int> fieldIds,
            Dictionary<int, object?> fieldValues,
            Dictionary<string, object>? formContextData)
        {
            var results = new Dictionary<int, ValidationResultDto>();

            foreach (var fieldId in fieldIds)
            {
                fieldValues.TryGetValue(fieldId, out var fieldValue);
                
                // For multi-field validation, we need to determine dependency values
                // This is a simplified implementation - may need enhancement
                var result = await ValidateFieldAsync(fieldId, fieldValue, null, formContextData);
                results[fieldId] = result;
            }

            return results;
        }

        public async Task<IEnumerable<ValidationIssueDto>> PerformConfigurationHealthCheckAsync(int formConfigurationId)
        {
            var issues = new List<ValidationIssueDto>();

            // Get all validation rules in this configuration
            var rules = await _ruleRepository.GetByFormConfigurationIdAsync(formConfigurationId);
            
            // Get the form configuration with all steps and fields
            var config = await _configRepository.GetByIdAsync(formConfigurationId);
            if (config == null)
            {
                issues.Add(new ValidationIssueDto
                {
                    Severity = "Error",
                    Message = $"Form configuration with ID {formConfigurationId} not found"
                });
                return issues;
            }

            // Build a field order map (stepIndex, fieldIndex in step)
            // CRITICAL: Use FieldOrderJson to get the correct visual order, not database order
            var fieldOrderMap = new Dictionary<int, (int stepIndex, int fieldIndex)>();
            for (int stepIdx = 0; stepIdx < config.Steps.Count; stepIdx++)
            {
                var step = config.Steps[stepIdx];
                
                // Get the ordered fields based on FieldOrderJson
                var orderedFields = GetOrderedFields(step);
                
                for (int fieldIdx = 0; fieldIdx < orderedFields.Count; fieldIdx++)
                {
                    var field = orderedFields[fieldIdx];
                    fieldOrderMap[field.Id] = (stepIdx, fieldIdx);
                }
            }

            foreach (var rule in rules)
            {
                // Check 1: Dependency field exists
                if (rule.DependsOnFieldId.HasValue)
                {
                    if (!fieldOrderMap.ContainsKey(rule.DependsOnFieldId.Value))
                    {
                        issues.Add(new ValidationIssueDto
                        {
                            Severity = "Error",
                            Message = $"Validation rule {rule.Id} references non-existent dependency field ID {rule.DependsOnFieldId.Value}",
                            FieldId = rule.FormFieldId,
                            RuleId = rule.Id
                        });
                        continue;
                    }

                    // Check 2: Dependency field comes before dependent field
                    var dependentFieldOrder = fieldOrderMap[rule.FormFieldId];
                    var dependencyFieldOrder = fieldOrderMap[rule.DependsOnFieldId.Value];

                    if (dependencyFieldOrder.stepIndex > dependentFieldOrder.stepIndex)
                    {
                        issues.Add(new ValidationIssueDto
                        {
                            Severity = "Warning",
                            Message = $"Dependency field (ID {rule.DependsOnFieldId.Value}) appears AFTER dependent field (ID {rule.FormFieldId}). Consider reordering fields.",
                            FieldId = rule.FormFieldId,
                            RuleId = rule.Id
                        });
                    }
                    else if (dependencyFieldOrder.stepIndex == dependentFieldOrder.stepIndex 
                             && dependencyFieldOrder.fieldIndex > dependentFieldOrder.fieldIndex)
                    {
                        issues.Add(new ValidationIssueDto
                        {
                            Severity = "Warning",
                            Message = $"Dependency field appears after dependent field in the same step. Users may need to fill fields in reverse order.",
                            FieldId = rule.FormFieldId,
                            RuleId = rule.Id
                        });
                    }

                    // Check 3: Circular dependency
                    var hasCircular = await _ruleRepository.HasCircularDependencyAsync(
                        rule.FormFieldId, 
                        rule.DependsOnFieldId.Value);
                    
                    if (hasCircular)
                    {
                        issues.Add(new ValidationIssueDto
                        {
                            Severity = "Error",
                            Message = $"Circular dependency detected: Field {rule.FormFieldId} → {rule.DependsOnFieldId.Value}",
                            FieldId = rule.FormFieldId,
                            RuleId = rule.Id
                        });
                    }
                }

                // Check 4: Validation method exists
                var validationMethod = _validationMethods.FirstOrDefault(m => m.ValidationType == rule.ValidationType);
                if (validationMethod == null)
                {
                    issues.Add(new ValidationIssueDto
                    {
                        Severity = "Error",
                        Message = $"Unknown validation type: {rule.ValidationType}",
                        FieldId = rule.FormFieldId,
                        RuleId = rule.Id
                    });
                }
            }

            return issues;
        }

        public async Task<IEnumerable<ValidationIssueDto>> ValidateConfigurationHealthAsync(int formConfigurationId)
        {
            return await PerformConfigurationHealthCheckAsync(formConfigurationId);
        }

        public async Task<IEnumerable<int>> GetDependentFieldIdsAsync(int fieldId)
        {
            var rules = await _ruleRepository.GetRulesDependingOnFieldAsync(fieldId);
            return rules.Select(r => r.FormFieldId).Distinct();
        }

        public async Task<IEnumerable<ValidationIssueDto>> ValidateDraftConfigurationAsync(FormConfigurationDto configDto)
        {
            var issues = new List<ValidationIssueDto>();

            if (configDto.Steps == null || !configDto.Steps.Any())
            {
                return issues;
            }

            // For draft validation, we focus on field ordering issues based on stored validation rules
            // We can't validate dependency rules from FieldValidationDto since they don't include dependency info
            // Instead, we check if any saved rules exist for fields and whether ordering is correct

            // If config has ID, load its validation rules from database
            if (!string.IsNullOrEmpty(configDto.Id) && int.TryParse(configDto.Id, out var configId))
            {
                // Load validation rules for this configuration
                var rules = await _ruleRepository.GetByFormConfigurationIdAsync(configId);
                
                // Build field position map from draft config
                var fieldPositionMap = new Dictionary<int, (int stepIndex, int fieldIndex)>();
                
                for (int stepIdx = 0; stepIdx < configDto.Steps.Count; stepIdx++)
                {
                    var step = configDto.Steps[stepIdx];
                    var orderedFieldDtos = GetOrderedFieldDtos(step);
                    
                    for (int fieldIdx = 0; fieldIdx < orderedFieldDtos.Count; fieldIdx++)
                    {
                        var fieldDto = orderedFieldDtos[fieldIdx];
                        if (!string.IsNullOrEmpty(fieldDto.Id) && int.TryParse(fieldDto.Id, out var fieldId))
                        {
                            fieldPositionMap[fieldId] = (stepIdx, fieldIdx);
                        }
                    }
                }

                // Check each rule's dependency ordering
                foreach (var rule in rules)
                {
                    if (!rule.DependsOnFieldId.HasValue)
                        continue;

                    if (!fieldPositionMap.ContainsKey(rule.FormFieldId))
                    {
                        issues.Add(new ValidationIssueDto
                        {
                            Severity = "Error",
                            Message = $"Validation rule {rule.Id} references field ID {rule.FormFieldId} which is not in the current configuration",
                                FieldId = rule.FormFieldId,
                            RuleId = rule.Id
                        });
                        continue;
                    }

                    if (!fieldPositionMap.ContainsKey(rule.DependsOnFieldId.Value))
                    {
                        issues.Add(new ValidationIssueDto
                        {
                            Severity = "Error",
                            Message = $"Validation rule {rule.Id} references non-existent dependency field ID {rule.DependsOnFieldId.Value}",
                                FieldId = rule.FormFieldId,
                            RuleId = rule.Id
                        });
                        continue;
                    }

                    // Check dependency field ordering
                    var dependentFieldOrder = fieldPositionMap[rule.FormFieldId];
                    var dependencyFieldOrder = fieldPositionMap[rule.DependsOnFieldId.Value];

                    if (dependencyFieldOrder.stepIndex > dependentFieldOrder.stepIndex)
                    {
                        issues.Add(new ValidationIssueDto
                        {
                            Severity = "Warning",
                            Message = $"Dependency field (ID {rule.DependsOnFieldId.Value}) appears AFTER dependent field (ID {rule.FormFieldId}). Consider reordering fields.",
                                FieldId = rule.FormFieldId,
                            RuleId = rule.Id
                        });
                    }
                    else if (dependencyFieldOrder.stepIndex == dependentFieldOrder.stepIndex && 
                             dependencyFieldOrder.fieldIndex >= dependentFieldOrder.fieldIndex)
                    {
                        issues.Add(new ValidationIssueDto
                        {
                            Severity = "Warning",
                            Message = $"Dependency field (ID {rule.DependsOnFieldId.Value}) appears AFTER or at same position as dependent field (ID {rule.FormFieldId}) in step {dependentFieldOrder.stepIndex + 1}. Reorder fields.",
                            FieldId = rule.FormFieldId,
                            RuleId = rule.Id
                        });
                    }
                }
            }

            return issues;
        }

        /// <summary>
        /// Get fields in the correct visual order based on FieldOrderJson for DTOs.
        /// </summary>
        private List<FormFieldDto> GetOrderedFieldDtos(FormStepDto step)
        {
            if (string.IsNullOrWhiteSpace(step.FieldOrderJson) || step.Fields == null)
            {
                return step.Fields?.ToList() ?? new List<FormFieldDto>();
            }

            try
            {
                var guidOrder = System.Text.Json.JsonSerializer.Deserialize<List<Guid>>(step.FieldOrderJson);
                if (guidOrder == null || guidOrder.Count == 0)
                {
                    return step.Fields.ToList();
                }

                // Create a map of fieldGuid -> field
                    // Parse string GUIDs to Guid for comparison
                    var fieldMap = new Dictionary<Guid, FormFieldDto>();
                    foreach (var field in step.Fields)
                    {
                        if (!string.IsNullOrEmpty(field.FieldGuid) && Guid.TryParse(field.FieldGuid, out var parsedGuid))
                        {
                            fieldMap[parsedGuid] = field;
                        }
                    }

                // Reorder fields based on the GUID order
                var reordered = new List<FormFieldDto>();
                foreach (var guid in guidOrder)
                {
                    if (fieldMap.TryGetValue(guid, out var field))
                    {
                        reordered.Add(field);
                    }
                }

                // Add any fields that weren't in the order array
                foreach (var field in step.Fields)
                {
                    if (!reordered.Contains(field))
                    {
                        reordered.Add(field);
                    }
                }

                return reordered;
            }
            catch
            {
                return step.Fields.ToList();
            }
        }

        /// <summary>
        /// Execute a single validation rule.
        /// </summary>
        private async Task<ValidationResultDto> ExecuteValidationRuleAsync(
            FieldValidationRule rule,
            object? fieldValue,
            object? dependencyValue,
            Dictionary<string, object>? formContextData)
        {
            // Check if dependency is required but not filled
            if (rule.DependsOnFieldId.HasValue && dependencyValue == null && !rule.RequiresDependencyFilled)
            {
                return new ValidationResultDto
                {
                    IsValid = true,
                    IsBlocking = false,
                    Message = "Validation pending: dependency field not filled",
                    Metadata = new ValidationMetadataDto
                    {
                        ValidationType = rule.ValidationType,
                        ExecutedAt = DateTime.UtcNow.ToString("o"),
                        DependencyFieldName = rule.DependsOnField?.FieldName
                    }
                };
            }

            // Find the validation method implementation
            var validationMethod = _validationMethods.FirstOrDefault(m => m.ValidationType == rule.ValidationType);
            if (validationMethod == null)
            {
                return new ValidationResultDto
                {
                    IsValid = false,
                    IsBlocking = true,
                    Message = $"Validation method not found: {rule.ValidationType}",
                    Metadata = new ValidationMetadataDto
                    {
                        ValidationType = rule.ValidationType,
                        ExecutedAt = DateTime.UtcNow.ToString("o")
                    }
                };
            }

            // Execute the validation
            try
            {
                var result = await validationMethod.ValidateAsync(
                    fieldValue,
                    dependencyValue,
                    rule.ConfigJson,
                    formContextData);

                return new ValidationResultDto
                {
                    IsValid = result.IsValid,
                    IsBlocking = rule.IsBlocking,
                    Message = result.IsValid 
                        ? (rule.SuccessMessage ?? result.Message) 
                        : (rule.ErrorMessage ?? result.Message),
                    Placeholders = result.Placeholders,
                    Metadata = new ValidationMetadataDto
                    {
                        ValidationType = rule.ValidationType,
                        ExecutedAt = DateTime.UtcNow.ToString("o"),
                        DependencyFieldName = rule.DependsOnField?.FieldName,
                        DependencyValue = dependencyValue
                    }
                };
            }
            catch (Exception ex)
            {
                return new ValidationResultDto
                {
                    IsValid = false,
                    IsBlocking = rule.IsBlocking,
                    Message = $"Validation error: {ex.Message}",
                    Metadata = new ValidationMetadataDto
                    {
                        ValidationType = rule.ValidationType,
                        ExecutedAt = DateTime.UtcNow.ToString("o")
                    }
                };
            }
        }

        /// <summary>
        /// Get fields in the correct visual order based on FieldOrderJson.
        /// Falls back to database order if FieldOrderJson is not available.
        /// </summary>
        private List<FormField> GetOrderedFields(FormStep step)
        {
            if (string.IsNullOrWhiteSpace(step.FieldOrderJson))
            {
                return step.Fields.ToList();
            }

            try
            {
                var guidOrder = System.Text.Json.JsonSerializer.Deserialize<List<Guid>>(step.FieldOrderJson);
                if (guidOrder == null || guidOrder.Count == 0)
                {
                    return step.Fields.ToList();
                }

                // Create a map of fieldGuid -> field
                var fieldMap = step.Fields.ToDictionary(f => f.FieldGuid, f => f);

                // Reorder fields based on the GUID order in FieldOrderJson
                var reordered = new List<FormField>();
                foreach (var guid in guidOrder)
                {
                    if (fieldMap.TryGetValue(guid, out var field))
                    {
                        reordered.Add(field);
                    }
                }

                // Add any fields that weren't in the order array (shouldn't happen, but be safe)
                foreach (var field in step.Fields)
                {
                    if (!reordered.Contains(field))
                    {
                        reordered.Add(field);
                    }
                }

                return reordered;
            }
            catch
            {
                // If parsing fails, return fields as-is
                return step.Fields.ToList();
            }
        }
    }
}
