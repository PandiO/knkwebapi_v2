using System.Collections.Generic;

namespace knkwebapi_v2.Dtos;

/// <summary>
/// Represents a validation rule to be executed during WorldTask processing.
/// Embedded in WorldTask.InputJson for plugin-side validation.
/// </summary>
public class WorldTaskValidationRuleDto
{
    /// <summary>
    /// Type of validation to perform.
    /// Supported: "LocationInsideRegion", "RegionContainment"
    /// </summary>
    public string ValidationType { get; set; } = string.Empty;
    
    /// <summary>
    /// Type-specific configuration as JSON string.
    /// Example: {"regionPropertyPath": "WgRegionId", "allowBoundary": false}
    /// </summary>
    public string ConfigJson { get; set; } = "{}";
    
    /// <summary>
    /// Error message to display when validation fails.
    /// Supports placeholders: {regionName}, {coordinates}, {violations}, {entityName}
    /// </summary>
    public string ErrorMessage { get; set; } = string.Empty;
    
    /// <summary>
    /// If true, validation failure blocks task completion.
    /// If false, validation failure is a warning only.
    /// </summary>
    public bool IsBlocking { get; set; } = true;
    
    /// <summary>
    /// Resolved value of the dependency field (e.g., parent Town entity).
    /// Already resolved by web app before creating WorldTask.
    /// Can be an object (full entity) or primitive value (ID, region name).
    /// </summary>
    public object? DependencyFieldValue { get; set; }
}

/// <summary>
/// Validation context embedded in WorldTask.InputJson.
/// Contains all validation rules and form data needed for validation.
/// </summary>
public class WorldTaskValidationContextDto
{
    /// <summary>
    /// List of validation rules to execute for this WorldTask.
    /// Plugin iterates through these and validates captured data.
    /// </summary>
    public List<WorldTaskValidationRuleDto> ValidationRules { get; set; } = new();
    
    /// <summary>
    /// Complete form context (all field values collected so far).
    /// Used for additional validation scenarios beyond simple dependency resolution.
    /// </summary>
    public Dictionary<string, object?> FormContext { get; set; } = new();
}

/// <summary>
/// Result of validation execution.
/// Returned by validation service and embedded in API responses.
/// </summary>
public class WorldTaskValidationResultDto
{
    /// <summary>
    /// Whether validation passed.
    /// </summary>
    public bool IsValid { get; set; }
    
    /// <summary>
    /// Validation result message (error or success message).
    /// Placeholders should be replaced with actual values.
    /// </summary>
    public string? Message { get; set; }
    
    /// <summary>
    /// Whether this validation blocks task completion.
    /// </summary>
    public bool IsBlocking { get; set; }
    
    /// <summary>
    /// Placeholder replacement values for error message.
    /// Example: {"regionName": "town_springfield", "coordinates": "(100.5, 64.0, 200.3)"}
    /// </summary>
    public Dictionary<string, string>? Placeholders { get; set; }
    
    /// <summary>
    /// Additional metadata about the validation execution.
    /// </summary>
    public WorldTaskValidationMetadataDto? Metadata { get; set; }
}

/// <summary>
/// Metadata about validation execution.
/// </summary>
public class WorldTaskValidationMetadataDto
{
    /// <summary>
    /// Type of validation that was executed.
    /// </summary>
    public string ValidationType { get; set; } = string.Empty;
    
    /// <summary>
    /// When the validation was executed.
    /// </summary>
    public DateTime ExecutedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Name of the dependency field (if applicable).
    /// </summary>
    public string? DependencyFieldName { get; set; }
    
    /// <summary>
    /// Value of the dependency field (if applicable).
    /// </summary>
    public object? DependencyFieldValue { get; set; }
}
