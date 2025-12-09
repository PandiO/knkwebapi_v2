using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using knkwebapi_v2.Dtos;
using knkwebapi_v2.Enums;
using knkwebapi_v2.Models;

namespace knkwebapi_v2.Services
{
    /// <summary>
    /// Service for validating FormStep and FormField compatibility against EntityMetadataDto.
    /// Ensures reused templates are compatible with target entities before saving.
    /// </summary>
    public interface IFormTemplateValidationService
    {
        /// <summary>
        /// Validate a single FormField against entity metadata.
        /// </summary>
        TemplateFieldValidationResult ValidateField(FormField field, EntityMetadataDto? entityMetadata);

        /// <summary>
        /// Validate a FormStep and all its fields against entity metadata.
        /// </summary>
        TemplateStepValidationResult ValidateStep(FormStep step, EntityMetadataDto? entityMetadata);

        /// <summary>
        /// Validate an entire FormConfiguration against entity metadata.
        /// Returns false if any field is incompatible.
        /// </summary>
        Task<FormConfigurationValidationResult> ValidateConfigurationAsync(
            FormConfiguration config,
            IMetadataService metadataService);
    }

    public class FormTemplateValidationService : IFormTemplateValidationService
    {
        public TemplateFieldValidationResult ValidateField(FormField field, EntityMetadataDto? entityMetadata)
        {
            var result = new TemplateFieldValidationResult
            {
                FormFieldId = field.Id,
                FieldName = field.FieldName,
                IsCompatible = true,
                Issues = new List<string>()
            };

            if (entityMetadata == null)
            {
                result.IsCompatible = false;
                result.Issues.Add("Entity metadata not found.");
                return result;
            }

            // Try to find matching field in entity metadata
            var metadataField = entityMetadata.Fields?.FirstOrDefault(f =>
                f.FieldName.Equals(field.FieldName, StringComparison.OrdinalIgnoreCase));

            if (metadataField == null)
            {
                result.IsCompatible = false;
                result.Issues.Add($"Field '{field.FieldName}' does not exist on entity '{entityMetadata.EntityName}'.");
                return result;
            }

            // Validate type compatibility
            ValidateFieldTypeCompatibility(field, metadataField, result);

            // Validate nullability/required
            if (!metadataField.IsNullable && !field.Required)
            {
                result.Issues.Add($"Field '{field.FieldName}' is non-nullable on entity but marked as not required in form.");
            }

            // If there are issues, mark as incompatible
            if (result.Issues.Count > 0)
            {
                result.IsCompatible = false;
            }

            return result;
        }

        public TemplateStepValidationResult ValidateStep(FormStep step, EntityMetadataDto? entityMetadata)
        {
            var result = new TemplateStepValidationResult
            {
                FormStepId = step.Id,
                StepName = step.StepName,
                IsCompatible = true,
                FieldResults = new List<TemplateFieldValidationResult>()
            };

            if (entityMetadata == null)
            {
                result.IsCompatible = false;
                return result;
            }

            // Validate all fields in the step
            foreach (var field in step.Fields)
            {
                var fieldResult = ValidateField(field, entityMetadata);
                result.FieldResults.Add(fieldResult);

                if (!fieldResult.IsCompatible)
                {
                    result.IsCompatible = false;
                }
            }

            return result;
        }

        public async Task<FormConfigurationValidationResult> ValidateConfigurationAsync(
            FormConfiguration config,
            IMetadataService metadataService)
        {
            return await Task.Run(() =>
            {
                var result = new FormConfigurationValidationResult
                {
                    FormConfigurationId = config.Id,
                    EntityTypeName = config.EntityTypeName,
                    IsValid = true,
                    StepResults = new List<TemplateStepValidationResult>()
                };

                // Get entity metadata
                var entityMetadata = metadataService.GetEntityMetadata(config.EntityTypeName);
                if (entityMetadata == null)
                {
                    result.IsValid = false;
                    result.Summary = $"Entity metadata not found for '{config.EntityTypeName}'.";
                    return result;
                }

                // Validate all steps
                foreach (var step in config.Steps)
                {
                    var stepResult = ValidateStep(step, entityMetadata);
                    result.StepResults.Add(stepResult);

                    if (!stepResult.IsCompatible)
                    {
                        result.IsValid = false;
                    }
                }

                // Generate summary
                var incompatibleCount = result.StepResults
                    .SelectMany(s => s.FieldResults)
                    .Count(f => !f.IsCompatible);

                if (incompatibleCount > 0)
                {
                    result.Summary = $"FormConfiguration has {incompatibleCount} incompatible field(s) for entity '{config.EntityTypeName}'.";
                }
                else
                {
                    result.Summary = "All fields are compatible with entity metadata.";
                }

                return result;
            });
        }

        private void ValidateFieldTypeCompatibility(
            FormField field,
            FieldMetadataDto metadataField,
            TemplateFieldValidationResult result)
        {
            switch (field.FieldType)
            {
                case FieldType.String:
                case FieldType.Integer:
                case FieldType.Boolean:
                case FieldType.DateTime:
                case FieldType.Decimal:
                    // Simple type: verify metadata field type matches
                    ValidateSimpleTypeCompatibility(field, metadataField, result);
                    break;

                case FieldType.Object:
                    // Object type: verify it's marked as related entity
                    ValidateObjectTypeCompatibility(field, metadataField, result);
                    break;

                case FieldType.Enum:
                    // Enum type: basic compatibility
                    if (!metadataField.FieldType.Equals("Enum", StringComparison.OrdinalIgnoreCase))
                    {
                        result.Issues.Add($"Field '{field.FieldName}' is Enum in form but '{metadataField.FieldType}' on entity.");
                    }
                    break;

                case FieldType.List:
                    // List type: verify element type
                    ValidateListTypeCompatibility(field, metadataField, result);
                    break;

                case FieldType.HybridMinecraftMaterialRefPicker:
                    // Special type for Minecraft materials, no validation needed
                    break;

                default:
                    result.Issues.Add($"Unknown field type: {field.FieldType}");
                    break;
            }
        }

        private void ValidateSimpleTypeCompatibility(
            FormField field,
            FieldMetadataDto metadataField,
            TemplateFieldValidationResult result)
        {
            var expectedType = field.FieldType switch
            {
                FieldType.String => "String",
                FieldType.Integer => "Int32",
                FieldType.Boolean => "Boolean",
                FieldType.DateTime => "DateTime",
                FieldType.Decimal => "Decimal",
                _ => ""
            };

            if (!metadataField.FieldType.Equals(expectedType, StringComparison.OrdinalIgnoreCase))
            {
                // Be lenient: nullable versions (e.g., "Int32?" vs "Int32") should still match
                if (!metadataField.FieldType.Contains(expectedType, StringComparison.OrdinalIgnoreCase))
                {
                    result.Issues.Add($"Field '{field.FieldName}' type mismatch: expected '{expectedType}', got '{metadataField.FieldType}'.");
                }
            }
        }

        private void ValidateObjectTypeCompatibility(
            FormField field,
            FieldMetadataDto metadataField,
            TemplateFieldValidationResult result)
        {
            if (!metadataField.IsRelatedEntity)
            {
                result.Issues.Add($"Field '{field.FieldName}' is object-type but entity metadata does not mark it as related entity.");
                return;
            }

            // If ObjectType is specified, verify it matches
            if (!string.IsNullOrWhiteSpace(field.ObjectType))
            {
                if (!field.ObjectType.Equals(metadataField.RelatedEntityType, StringComparison.OrdinalIgnoreCase))
                {
                    result.Issues.Add($"Field '{field.FieldName}' references '{field.ObjectType}' but entity field is related to '{metadataField.RelatedEntityType}'.");
                }
            }
        }

        private void ValidateListTypeCompatibility(
            FormField field,
            FieldMetadataDto metadataField,
            TemplateFieldValidationResult result)
        {
            // List compatibility is more flexible; just verify it's a collection type if needed
            // Details depend on ElementType, which can vary
            // For now, basic validation only
            if (!metadataField.FieldType.Contains("List", StringComparison.OrdinalIgnoreCase) &&
                !metadataField.FieldType.Contains("Collection", StringComparison.OrdinalIgnoreCase) &&
                !metadataField.FieldType.Contains("IEnumerable", StringComparison.OrdinalIgnoreCase))
            {
                // Could be a related entity marked for list rendering
                if (!metadataField.IsRelatedEntity)
                {
                    result.Issues.Add($"Field '{field.FieldName}' is list-type but entity metadata does not mark it as a collection.");
                }
            }
        }
    }
}
