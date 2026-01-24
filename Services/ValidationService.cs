using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

        public ValidationService(
            IFieldValidationRuleRepository ruleRepository,
            IFormFieldRepository fieldRepository,
            IFormConfigurationRepository configRepository,
            IEnumerable<IValidationMethod> validationMethods)
        {
            _ruleRepository = ruleRepository;
            _fieldRepository = fieldRepository;
            _configRepository = configRepository;
            _validationMethods = validationMethods;
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
            var fieldOrderMap = new Dictionary<int, (int stepIndex, int fieldIndex)>();
            for (int stepIdx = 0; stepIdx < config.Steps.Count; stepIdx++)
            {
                var step = config.Steps[stepIdx];
                for (int fieldIdx = 0; fieldIdx < step.Fields.Count; fieldIdx++)
                {
                    var field = step.Fields[fieldIdx];
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

        public async Task<IEnumerable<int>> GetDependentFieldIdsAsync(int fieldId)
        {
            var rules = await _ruleRepository.GetRulesDependingOnFieldAsync(fieldId);
            return rules.Select(r => r.FormFieldId).Distinct();
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
    }
}
