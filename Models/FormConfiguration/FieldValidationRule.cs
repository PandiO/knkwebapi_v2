using System;

namespace knkwebapi_v2.Models;

/// <summary>
/// Represents a validation rule attached to a FormField.
/// 
/// SCENARIO:
/// - Field: District's SpawnLocationId
/// - Rule: Location must be inside the District's Town's WorldGuard region
/// - Config: depends on district TownId field; fetches Town's WgRegionId; validates Location inside region
/// 
/// EXECUTION FLOW (Frontend):
/// 1. User fills District.TownId field (selects a Town)
/// 2. System stores selected Town entity (at least {id, WgRegionId})
/// 3. User fills District.SpawnLocationId (selects or creates Location)
/// 4. On location selection/creation:
///    a. Check if dependency field (TownId) is filled → if not, show warning "Cannot validate until Town selected"
///    b. If filled, fetch Town data from form context
///    c. Invoke validation API (regions.validateLocationInside) with {townWgRegionId, locationCoords}
///    d. Show result: ✅ success or ❌ failure with error message
/// 5. If IsBlocking = true and validation failed, prevent step progression
/// </summary>
public class FieldValidationRule
{
    public int Id { get; set; }

    /// <summary>
    /// Foreign key to the FormField this rule is attached to.
    /// </summary>
    public int FormFieldId { get; set; }
    public FormField FormField { get; set; } = null!;

    /// <summary>
    /// Type of validation to perform.
    /// 
    /// Supported types (v1):
    /// - "LocationInsideRegion": Validates Location coordinates are inside a WorldGuard region
    /// - "RegionContainment": Validates child region is fully contained within parent region
    /// 
    /// Future types:
    /// - "UniqueInEntity": Field value is unique within entity scope
    /// - "CustomApiCall": Invoke arbitrary validation API endpoint
    /// </summary>
    public string ValidationType { get; set; } = string.Empty;

    /// <summary>
    /// Foreign key to the FormField this rule depends on (for data retrieval).
    /// 
    /// EXAMPLE:
    /// - This rule is on LocationId field
    /// - DependsOnFieldId = TownId field
    /// - At validation time, system fetches the TownId value from form context
    /// - Uses TownId to fetch Town entity
    /// - Extracts Town.WgRegionId for validation
    /// 
    /// If NULL, rule does not depend on another field (e.g., "check email is unique")
    /// </summary>
    public int? DependsOnFieldId { get; set; }
    public FormField? DependsOnField { get; set; }

    /// <summary>
    /// Generic JSON configuration for this validation rule.
    /// Structure varies by ValidationType.
    /// 
    /// EXAMPLE ConfigJson for "LocationInsideRegion":
    /// {
    ///   "validationApiMethod": "regions.validateLocationInside",
    ///   "dependencyPath": "TownId",           // Path to fetch dependency (field name or nav property)
    ///   "parentEntityRegionProperty": "WgRegionId",  // Which property on parent entity holds region ID
    ///   "childCoordinatesFromField": "SpawnLocationId",  // Field containing location data
    ///   "successMessage": "Location is within town boundaries.",
    ///   "failMessage": "Location is outside town boundaries. Select a location within {parentEntityName}."
    /// }
    /// 
    /// EXAMPLE ConfigJson for "RegionContainment":
    /// {
    ///   "validationApiMethod": "regions.validateRegionContained",
    ///   "dependencyPath": "TownId",
    ///   "parentEntityRegionProperty": "WgRegionId",
    ///   "childRegionIdFromField": "WgRegionId",  // Field on current entity containing child region
    ///   "successMessage": "District region is fully contained within town region.",
    ///   "failMessage": "District region extends outside town region. Adjust your selection."
    /// }
    /// </summary>
    public string ConfigJson { get; set; } = "{}";

    /// <summary>
    /// Error message displayed to user if validation fails.
    /// Supports placeholders: {parentEntityName}, {regionName}, {fieldLabel}
    /// Backend validation API returns placeholder values.
    /// </summary>
    public string ErrorMessage { get; set; } = string.Empty;

    /// <summary>
    /// Success message displayed if validation passes.
    /// Optional; if empty, just clears error state.
    /// </summary>
    public string? SuccessMessage { get; set; }

    /// <summary>
    /// If true, validation failure blocks field completion and step progression.
    /// If false, validation is informational only (warning badge shown but no blocking).
    /// </summary>
    public bool IsBlocking { get; set; } = true;

    /// <summary>
    /// If false, validation is skipped if the dependency field is not yet filled.
    /// If true, validation failure message shown even if dependency not filled.
    /// Default: false (more forgiving UX).
    /// </summary>
    public bool RequiresDependencyFilled { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
