using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace knkwebapi_v2.Dtos;

/// <summary>
/// Request DTO for resolving placeholders in validation messages.
/// 
/// FLOW:
/// 1. Frontend extracts current form field values (Layer 0 placeholders)
/// 2. Frontend creates this request with the rule ID and layer 0 placeholders
/// 3. Backend resolves all layers (1-3) using database queries
/// 4. Backend returns PlaceholderResolutionResponse with fully resolved values
/// 5. Frontend interpolates messages using the resolved values
/// 
/// </summary>
public class PlaceholderResolutionRequest
{
    /// <summary>
    /// Optional: The FieldValidationRule ID.
    /// If provided, placeholders are extracted from the rule's ErrorMessage and SuccessMessage.
    /// If omitted, explicit placeholder path list must be provided.
    /// </summary>
    [JsonPropertyName("fieldValidationRuleId")]
    public int? FieldValidationRuleId { get; set; }

    /// <summary>
    /// Optional: Explicit list of placeholder paths to resolve.
    /// If FieldValidationRuleId is provided, this can be omitted (will be extracted from rule).
    /// 
    /// Examples:
    /// - ["Town.Name", "Town.Districts.Count"]
    /// - ["coordinates", "regionName"]
    /// </summary>
    [JsonPropertyName("placeholderPaths")]
    public List<string>? PlaceholderPaths { get; set; }

    /// <summary>
    /// Layer 0 placeholders: Direct form field values from current form.
    /// 
    /// KEY/VALUE PAIRS:
    /// - Key: Placeholder name without braces (e.g., "Name", "Description")
    /// - Value: The actual form field value (e.g., "York", "A historic district")
    /// 
    /// IMPORTANT: Layer 0 placeholders MUST NOT contain dots (no navigation).
    /// Navigation is handled by backend in Layers 1-3.
    /// 
    /// Examples:
    /// - { "Name": "York", "Description": "Historic City" }
    /// - { "coordinates": "(125.5, 64.0, -350.2)" }
    /// </summary>
    [JsonPropertyName("currentEntityPlaceholders")]
    public Dictionary<string, string>? CurrentEntityPlaceholders { get; set; }

    /// <summary>
    /// Optional: The ID of the entity instance being validated.
    /// Required when resolving Layers 1-3 (navigations to related entities).
    /// 
    /// This is used to:
    /// 1. Fetch related entities via foreign keys
    /// 2. Extract navigation properties
    /// 3. Resolve multi-level paths
    /// </summary>
    [JsonPropertyName("entityId")]
    public int? EntityId { get; set; }

    /// <summary>
    /// Optional: The entity type being validated (e.g., "District", "Structure").
    /// If not provided, will be inferred from context (if possible).
    /// Helps identify correct entity type for DB queries.
    /// </summary>
    [JsonPropertyName("entityTypeName")]
    public string? EntityTypeName { get; set; }

    /// <summary>
    /// Optional: Additional context data (key-value pairs) for resolution.
    /// Useful for passing computed values or external data.
    /// 
    /// Examples:
    /// - { "currentLocation": "(125.5, 64.0, -350.2)" }
    /// - { "playerWorldName": "world" }
    /// </summary>
    [JsonPropertyName("contextData")]
    public Dictionary<string, string>? ContextData { get; set; }
}

/// <summary>
/// Response DTO containing all resolved placeholder values.
/// 
/// STRUCTURE:
/// - ResolvedPlaceholders: Dictionary of successfully resolved placeholders
/// - ResolutionErrors: List of any placeholders that failed to resolve
/// 
/// CONSUMER: Frontend FormWizard component calls interpolatePlaceholders()
/// using ResolvedPlaceholders to replace placeholders in error/success messages.
/// </summary>
public class PlaceholderResolutionResponse
{
    /// <summary>
    /// Successfully resolved placeholders.
    /// 
    /// KEY/VALUE PAIRS:
    /// - Key: Full placeholder path (e.g., "Town.Name", "Town.Districts.Count")
    /// - Value: Resolved value as string (e.g., "Springfield", "5")
    /// 
    /// Usage Example:
    /// - Message template: "Location is outside {Town.Name}'s boundaries."
    /// - Resolved: { "Town.Name": "Springfield" }
    /// - Final: "Location is outside Springfield's boundaries."
    /// </summary>
    [JsonPropertyName("resolvedPlaceholders")]
    public Dictionary<string, string> ResolvedPlaceholders { get; set; } = new();

    /// <summary>
    /// Errors that occurred during resolution (for debugging/logging).
    /// 
    /// If a placeholder cannot be resolved:
    /// 1. Error is added to this list
    /// 2. Placeholder remains unreplaced in frontend message (shown with braces)
    /// 3. Exception is NOT thrown (fail-open design)
    /// 
    /// This allows partial resolution: some placeholders work, others degrade gracefully.
    /// </summary>
    [JsonPropertyName("resolutionErrors")]
    public List<PlaceholderResolutionError> ResolutionErrors { get; set; } = new();

    /// <summary>
    /// Total number of placeholders that were requested to be resolved.
    /// Useful for monitoring: if ResolutionErrors.Count == 0, all succeeded.
    /// </summary>
    [JsonPropertyName("totalPlaceholdersRequested")]
    public int TotalPlaceholdersRequested { get; set; }

    /// <summary>
    /// True if all placeholders were successfully resolved.
    /// False if any resolution error occurred.
    /// </summary>
    [JsonPropertyName("isSuccessful")]
    public bool IsSuccessful => ResolutionErrors.Count == 0;
}

/// <summary>
/// Represents a single placeholder resolution error.
/// 
/// WHY ERRORS DON'T BLOCK:
/// Some placeholders might fail to resolve due to:
/// - Dependency field not filled (ForeignKey not set)
/// - Related entity not found in database
/// - Navigation chain broken (null intermediate value)
/// - Collection empty (for First/Last aggregates)
/// 
/// Rather than block the entire validation, we:
/// 1. Return the error details for logging
/// 2. Leave the placeholder unreplaced (shown with braces to user)
/// 3. Allow validation to proceed (fail-open design)
/// 
/// This provides better UX: user sees "Location is outside {Town.Name}'s boundaries"
/// instead of getting a 500 error.
/// </summary>
public class PlaceholderResolutionError
{
    /// <summary>
    /// The placeholder path that failed to resolve (e.g., "Town.Name")
    /// </summary>
    [JsonPropertyName("placeholderPath")]
    public string PlaceholderPath { get; set; } = string.Empty;

    /// <summary>
    /// Error code identifying the type of failure:
    /// - "DependencyNotFilled": Dependency field has no value
    /// - "EntityNotFound": Related entity not found in database
    /// - "NavigationFailed": Navigation chain broken (null intermediate)
    /// - "AggregateEmpty": Collection is empty (for First/Last)
    /// - "InvalidPath": Placeholder path structure is invalid
    /// - "ResolutionTimeout": Query execution timed out
    /// - "Exception": Unexpected exception during resolution
    /// </summary>
    [JsonPropertyName("errorCode")]
    public string ErrorCode { get; set; } = string.Empty;

    /// <summary>
    /// Human-readable error message for logging and debugging.
    /// </summary>
    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Optional: Stack trace for debugging unexpected exceptions.
    /// Only populated if errorCode == "Exception".
    /// Should NOT be returned to frontend (keep in logs only).
    /// </summary>
    [JsonPropertyName("stackTrace")]
    public string? StackTrace { get; set; }

    /// <summary>
    /// Optional: Additional details about the error.
    /// Examples:
    /// - "DependencyFieldId not set" (for DependencyNotFilled)
    /// - "Town with ID 5 not found" (for EntityNotFound)
    /// - "Navigation property 'Town' returned null" (for NavigationFailed)
    /// </summary>
    [JsonPropertyName("details")]
    public string? Details { get; set; }
}
