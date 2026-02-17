using System.Collections.Generic;
using System.Threading.Tasks;
using knkwebapi_v2.Dtos;
using knkwebapi_v2.Models;

namespace knkwebapi_v2.Services.Interfaces
{
    /// <summary>
    /// Service interface for field validation rule CRUD and management operations.
    /// Handles rule creation, updates, deletion, and configuration health validation.
    /// Complements IValidationService which handles rule execution.
    /// </summary>
    public interface IFieldValidationRuleService
    {
        // CRUD Operations
        /// <summary>
        /// Get a validation rule by ID.
        /// </summary>
        /// <param name="id">The validation rule ID</param>
        /// <returns>The validation rule DTO or null if not found</returns>
        Task<FieldValidationRuleDto?> GetByIdAsync(int id);

        /// <summary>
        /// Get all validation rules for a specific form field.
        /// </summary>
        /// <param name="fieldId">The form field ID</param>
        /// <returns>Collection of validation rules for the field</returns>
        Task<IEnumerable<FieldValidationRuleDto>> GetByFormFieldIdAsync(int fieldId);

        /// <summary>
        /// Get all validation rules for a form configuration.
        /// </summary>
        /// <param name="formConfigurationId">The form configuration ID</param>
        /// <returns>Collection of validation rules in the configuration</returns>
        Task<IEnumerable<FieldValidationRuleDto>> GetByFormConfigurationIdAsync(int formConfigurationId);

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
        /// <param name="fieldId">The form field ID</param>
        /// <param name="formContext">Optional form context for dependency resolution</param>
        /// <returns>Collection of validation rules with dependency info</returns>
        Task<IEnumerable<FieldValidationRuleDto>> GetByFormFieldIdWithDependenciesAsync(
            int fieldId,
            Dictionary<string, object>? formContext = null);

        /// <summary>
        /// Create a new validation rule.
        /// </summary>
        /// <param name="dto">The create DTO</param>
        /// <returns>The created validation rule DTO</returns>
        Task<FieldValidationRuleDto> CreateAsync(CreateFieldValidationRuleDto dto);

        /// <summary>
        /// Update an existing validation rule.
        /// </summary>
        /// <param name="id">The validation rule ID</param>
        /// <param name="dto">The update DTO</param>
        Task UpdateAsync(int id, UpdateFieldValidationRuleDto dto);

        /// <summary>
        /// Delete a validation rule.
        /// </summary>
        /// <param name="id">The validation rule ID</param>
        Task DeleteAsync(int id);

        // Health Check Operations
        /// <summary>
        /// Perform a health check on a form configuration's validation rules.
        /// Detects broken dependencies, circular references, and incorrect field ordering.
        /// </summary>
        /// <param name="formConfigurationId">The form configuration ID to check</param>
        /// <returns>Collection of validation issues (errors, warnings, info)</returns>
        Task<IEnumerable<ValidationIssueDto>> ValidateConfigurationHealthAsync(int formConfigurationId);

        /// <summary>
        /// Validate a draft form configuration that hasn't been saved yet.
        /// Performs the same health checks as ValidateConfigurationHealthAsync but on in-memory config.
        /// </summary>
        /// <param name="configDto">The draft form configuration DTO</param>
        /// <returns>Collection of validation issues (errors, warnings, info)</returns>
        Task<IEnumerable<ValidationIssueDto>> ValidateDraftConfigurationAsync(FormConfigurationDto configDto);

        // Dependency Analysis
        /// <summary>
        /// Get all fields that depend on a specific field (for re-validation when dependency changes).
        /// </summary>
        /// <param name="fieldId">The dependency field ID</param>
        /// <returns>Collection of field IDs that depend on the specified field</returns>
        Task<IEnumerable<int>> GetDependentFieldIdsAsync(int fieldId);
    }
}
