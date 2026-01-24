using System.Collections.Generic;
using System.Threading.Tasks;
using knkwebapi_v2.Dtos;
using knkwebapi_v2.Models;

namespace knkwebapi_v2.Services.Interfaces
{
    /// <summary>
    /// Service interface for field validation operations.
    /// Orchestrates validation rule execution and result formatting.
    /// </summary>
    public interface IValidationService
    {
        /// <summary>
        /// Validate a field value against its configured validation rules.
        /// </summary>
        /// <param name="fieldId">The form field ID to validate</param>
        /// <param name="fieldValue">The current value of the field</param>
        /// <param name="dependencyValue">The value of the dependency field (if applicable)</param>
        /// <param name="formContextData">Additional context data (entity IDs, relationships, etc.)</param>
        /// <returns>Validation result with success/failure, messages, and metadata</returns>
        Task<ValidationResultDto> ValidateFieldAsync(
            int fieldId,
            object? fieldValue,
            object? dependencyValue,
            Dictionary<string, object>? formContextData
        );
        
        /// <summary>
        /// Validate multiple fields at once (e.g., before step progression).
        /// </summary>
        /// <param name="fieldIds">Collection of field IDs to validate</param>
        /// <param name="fieldValues">Dictionary of field IDs to their current values</param>
        /// <param name="formContextData">Additional context data</param>
        /// <returns>Dictionary of field IDs to their validation results</returns>
        Task<Dictionary<int, ValidationResultDto>> ValidateMultipleFieldsAsync(
            IEnumerable<int> fieldIds,
            Dictionary<int, object?> fieldValues,
            Dictionary<string, object>? formContextData
        );
        
        /// <summary>
        /// Perform a health check on a form configuration's validation rules.
        /// Detects broken dependencies, circular references, and incorrect field ordering.
        /// </summary>
        /// <param name="formConfigurationId">The form configuration ID to check</param>
        /// <returns>Collection of validation issues (errors, warnings, info)</returns>
        Task<IEnumerable<ValidationIssueDto>> PerformConfigurationHealthCheckAsync(int formConfigurationId);
        
        /// <summary>
        /// Get all fields that depend on a specific field (for re-validation when dependency changes).
        /// </summary>
        /// <param name="fieldId">The dependency field ID</param>
        /// <returns>Collection of field IDs that depend on the specified field</returns>
        Task<IEnumerable<int>> GetDependentFieldIdsAsync(int fieldId);
    }
}
