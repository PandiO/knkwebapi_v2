using System.Collections.Generic;
using System.Threading.Tasks;
using knkwebapi_v2.Models;

namespace knkwebapi_v2.Repositories
{
    /// <summary>
    /// Repository interface for FieldValidationRule data access operations.
    /// Provides CRUD operations and dependency analysis methods.
    /// </summary>
    public interface IFieldValidationRuleRepository
    {
        /// <summary>
        /// Get a validation rule by its unique identifier.
        /// </summary>
        /// <param name="id">The validation rule ID</param>
        /// <returns>The validation rule or null if not found</returns>
        Task<FieldValidationRule?> GetByIdAsync(int id);
        
        /// <summary>
        /// Get all validation rules for a specific form field.
        /// Includes navigation properties (FormField, DependsOnField).
        /// </summary>
        /// <param name="formFieldId">The form field ID</param>
        /// <returns>Collection of validation rules for the field</returns>
        Task<IEnumerable<FieldValidationRule>> GetByFormFieldIdAsync(int formFieldId);
        
        /// <summary>
        /// Get all validation rules for all fields within a form configuration.
        /// Useful for configuration health checks and bulk validation.
        /// </summary>
        /// <param name="formConfigurationId">The form configuration ID</param>
        /// <returns>Collection of all validation rules in the configuration</returns>
        Task<IEnumerable<FieldValidationRule>> GetByFormConfigurationIdAsync(int formConfigurationId);
        
        /// <summary>
        /// Get all validation rules that depend on a specific field.
        /// Used to trigger re-validation when a dependency field value changes.
        /// </summary>
        /// <param name="fieldId">The dependency field ID</param>
        /// <returns>Collection of rules that depend on the specified field</returns>
        Task<IEnumerable<FieldValidationRule>> GetRulesDependingOnFieldAsync(int fieldId);
        
        /// <summary>
        /// Check if creating a dependency between two fields would create a circular reference.
        /// Prevents infinite validation loops.
        /// 
        /// EXAMPLE CIRCULAR DEPENDENCY:
        /// - Field A depends on Field B
        /// - Field B depends on Field C
        /// - Field C depends on Field A (CIRCULAR!)
        /// </summary>
        /// <param name="fieldId">The field being validated</param>
        /// <param name="dependsOnFieldId">The field it would depend on</param>
        /// <returns>True if adding this dependency would create a circular reference</returns>
        Task<bool> HasCircularDependencyAsync(int fieldId, int dependsOnFieldId);
        
        /// <summary>
        /// Create a new validation rule.
        /// </summary>
        /// <param name="rule">The validation rule to create</param>
        /// <returns>The created validation rule with assigned ID</returns>
        Task<FieldValidationRule> CreateAsync(FieldValidationRule rule);
        
        /// <summary>
        /// Update an existing validation rule.
        /// </summary>
        /// <param name="rule">The validation rule with updated values</param>
        /// <returns>Task representing the async operation</returns>
        Task UpdateAsync(FieldValidationRule rule);
        
        /// <summary>
        /// Delete a validation rule by ID.
        /// </summary>
        /// <param name="id">The validation rule ID to delete</param>
        /// <returns>Task representing the async operation</returns>
        Task DeleteAsync(int id);
    }
}
