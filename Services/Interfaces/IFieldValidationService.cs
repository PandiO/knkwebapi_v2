using System.Collections.Generic;
using System.Threading.Tasks;
using knkwebapi_v2.Dtos;
using knkwebapi_v2.Models;

namespace knkwebapi_v2.Services.Interfaces;

/// <summary>
/// Service interface for executing field validations with placeholder resolution.
/// 
/// RESPONSIBILITY:
/// This service orchestrates validation execution and placeholder resolution.
/// It differs from IValidationService which handles CRUD operations for validation rules.
/// 
/// VALIDATION FLOW:
/// 1. Receive validation request (rule, field value, dependencies, context)
/// 2. Extract placeholder paths from rule's ErrorMessage/SuccessMessage
/// 3. Build PlaceholderResolutionRequest with Layer 0 values from context
/// 4. Call PlaceholderResolutionService to resolve Layers 1-3
/// 5. Execute validation logic (dispatch to type-specific validator)
/// 6. Return ValidationResultDto with message template + placeholders
/// 7. Frontend interpolates final message for display
/// 
/// </summary>
public interface IFieldValidationService
{
    /// <summary>
    /// Execute validation for a field value against a specific validation rule.
    /// 
    /// FLOW:
    /// 1. Resolve placeholders for rule's error/success messages
    /// 2. Determine validation type from rule.ValidationType
    /// 3. Dispatch to type-specific validation method
    /// 4. Return ValidationResultDto with:
    ///    - Message template (unreplaced)
    ///    - Placeholders dictionary (all layers resolved)
    ///    - IsValid flag
    ///    - IsBlocking from rule
    /// 
    /// IMPORTANT: Message is NOT interpolated here - that happens in frontend/plugin.
    /// 
    /// </summary>
    /// <param name="rule">The validation rule to execute</param>
    /// <param name="fieldValue">The current value of the field being validated</param>
    /// <param name="dependencyFieldValue">Value of dependency field (if required by rule)</param>
    /// <param name="currentEntityPlaceholders">Layer 0 placeholders from frontend (current form data)</param>
    /// <param name="entityId">The ID of the entity being validated (for navigation placeholders)</param>
    /// <returns>Validation result with message template and placeholders</returns>
    Task<ValidationResultDto> ValidateFieldAsync(
        FieldValidationRule rule,
        object? fieldValue,
        object? dependencyFieldValue = null,
        Dictionary<string, string>? currentEntityPlaceholders = null,
        int? entityId = null);

    /// <summary>
    /// Resolve placeholders for a specific validation rule.
    /// 
    /// This is a helper method that can be called independently to resolve placeholders
    /// without executing validation logic. Useful for preview/debugging.
    /// 
    /// </summary>
    /// <param name="rule">The validation rule containing ErrorMessage/SuccessMessage templates</param>
    /// <param name="entityId">The ID of the entity (for navigation placeholders)</param>
    /// <param name="currentEntityPlaceholders">Layer 0 placeholders from frontend</param>
    /// <returns>Placeholder resolution response with all resolved values</returns>
    Task<PlaceholderResolutionResponse> ResolvePlaceholdersForRuleAsync(
        FieldValidationRule rule,
        int? entityId = null,
        Dictionary<string, string>? currentEntityPlaceholders = null);

    /// <summary>
    /// Validate that a Location is inside a specified region.
    /// 
    /// VALIDATION TYPE: LocationInsideRegion
    /// 
    /// ALGORITHM:
    /// 1. Parse ConfigJson for regionPropertyPath (e.g., "TownId")
    /// 2. Extract region ID from dependencyFieldValue using property path
    /// 3. Parse fieldValue as Location coordinates (X, Y, Z)
    /// 4. Call IRegionService to check if coordinates are inside region
    /// 5. Add computed placeholders:
    ///    - {coordinates} → Formatted as "(X, Y, Z)"
    ///    - {regionName} → From region ID
    /// 6. Merge with existing placeholders
    /// 7. Return ValidationResultDto
    /// 
    /// DEPENDENCIES:
    /// - IRegionService: To check WorldGuard region containment
    /// - IPlaceholderResolutionService: To resolve navigation placeholders (e.g., {Town.Name})
    /// 
    /// </summary>
    /// <param name="rule">The validation rule</param>
    /// <param name="fieldValue">Location object with X, Y, Z coordinates</param>
    /// <param name="dependencyFieldValue">Entity containing the region ID</param>
    /// <param name="placeholders">Pre-resolved placeholders from all layers</param>
    /// <returns>Validation result</returns>
    Task<ValidationResultDto> ValidateLocationInsideRegionAsync(
        FieldValidationRule rule,
        object? fieldValue,
        object? dependencyFieldValue,
        Dictionary<string, string> placeholders);

    /// <summary>
    /// Validate that a child region is fully contained within parent region.
    /// 
    /// VALIDATION TYPE: RegionContainment
    /// 
    /// ALGORITHM:
    /// 1. Parse ConfigJson for parentRegionPath (e.g., "Town.RegionId")
    /// 2. Extract child region ID from fieldValue
    /// 3. Extract parent region ID from dependencyFieldValue using property path
    /// 4. Call IRegionService to check if child region is inside parent
    /// 5. Add computed placeholders:
    ///    - {violationCount} → Number of out-of-bounds points (if validation fails)
    ///    - {regionName} → Child region ID
    /// 6. Return ValidationResultDto
    /// 
    /// NOTE: Implementation is placeholder for now (Phase 2 focus is placeholder infrastructure).
    /// Full implementation will be added in later phase.
    /// 
    /// </summary>
    /// <param name="rule">The validation rule</param>
    /// <param name="fieldValue">Child region ID</param>
    /// <param name="dependencyFieldValue">Entity containing parent region ID</param>
    /// <param name="placeholders">Pre-resolved placeholders</param>
    /// <returns>Validation result</returns>
    Task<ValidationResultDto> ValidateRegionContainmentAsync(
        FieldValidationRule rule,
        object? fieldValue,
        object? dependencyFieldValue,
        Dictionary<string, string> placeholders);

    /// <summary>
    /// Validate conditional required field logic.
    /// 
    /// VALIDATION TYPE: ConditionalRequired
    /// 
    /// ALGORITHM:
    /// 1. Parse ConfigJson for condition (operator, value)
    /// 2. Evaluate dependency field value against condition
    /// 3. If condition is met:
    ///    - Check if field value is empty
    ///    - If empty → validation fails (field is required)
    ///    - If filled → validation passes
    /// 4. If condition is NOT met:
    ///    - Validation passes (field is not required)
    /// 5. Return ValidationResultDto
    /// 
    /// EXAMPLE CONFIG:
    /// {
    ///   "operator": "Equals",
    ///   "value": "Custom",
    ///   "errorMessage": "{fieldName} is required when {dependencyFieldName} is '{dependencyValue}'"
    /// }
    /// 
    /// </summary>
    /// <param name="rule">The validation rule</param>
    /// <param name="fieldValue">Current field value to check for emptiness</param>
    /// <param name="dependencyFieldValue">Dependency field value to evaluate condition against</param>
    /// <param name="placeholders">Pre-resolved placeholders</param>
    /// <returns>Validation result</returns>
    Task<ValidationResultDto> ValidateConditionalRequiredAsync(
        FieldValidationRule rule,
        object? fieldValue,
        object? dependencyFieldValue,
        Dictionary<string, string> placeholders);
}
