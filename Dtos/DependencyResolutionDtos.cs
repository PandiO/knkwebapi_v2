using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace knkwebapi_v2.Dtos;

/// <summary>
/// Request to pre-resolve dependencies for validation rules on specified fields.
/// </summary>
public class DependencyResolutionRequest
{
    /// <summary>
    /// IDs of form fields that have validation rules depending on other fields.
    /// </summary>
    [JsonPropertyName("fieldIds")]
    public int[] FieldIds { get; set; } = Array.Empty<int>();

    /// <summary>
    /// Current snapshot of form context data.
    /// Example: { "Town": { "id": 4, "name": "Cinix", "wgRegionId": "town_1" }, "WgRegionId": null }
    /// </summary>
    [JsonPropertyName("formContextSnapshot")]
    public Dictionary<string, object?> FormContextSnapshot { get; set; } = new();

    /// <summary>
    /// Optional: Form configuration ID for field name resolution.
    /// </summary>
    [JsonPropertyName("formConfigurationId")]
    public int? FormConfigurationId { get; set; }
}

/// <summary>
/// Resolution result for a single validation rule dependency.
/// </summary>
public class ResolvedDependency
{
    /// <summary>
    /// ID of the validation rule being resolved.
    /// </summary>
    [JsonPropertyName("ruleId")]
    public int RuleId { get; set; }

    /// <summary>
    /// Status of resolution: "success", "pending", "error".
    /// </summary>
    [JsonPropertyName("status")]
    public string Status { get; set; } = "pending";

    /// <summary>
    /// The extracted value to be used in validation.
    /// </summary>
    [JsonPropertyName("resolvedValue")]
    public object? ResolvedValue { get; set; }

    /// <summary>
    /// The full path that was resolved.
    /// </summary>
    [JsonPropertyName("dependencyPath")]
    public string DependencyPath { get; set; } = string.Empty;

    /// <summary>
    /// Timestamp of resolution.
    /// </summary>
    [JsonPropertyName("resolvedAt")]
    public DateTime ResolvedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Human-readable reason if status is not "success".
    /// </summary>
    [JsonPropertyName("message")]
    public string? Message { get; set; }

    /// <summary>
    /// For error status: Details on what went wrong.
    /// </summary>
    [JsonPropertyName("errorDetail")]
    public string? ErrorDetail { get; set; }
}

/// <summary>
/// Response containing resolved dependencies for all requested rules.
/// </summary>
public class DependencyResolutionResponse
{
    /// <summary>
    /// Map of RuleId -> ResolvedDependency.
    /// </summary>
    [JsonPropertyName("resolved")]
    public Dictionary<int, ResolvedDependency> Resolved { get; set; } = new();

    /// <summary>
    /// Timestamp of resolution.
    /// </summary>
    [JsonPropertyName("resolvedAt")]
    public DateTime ResolvedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Optional validation issues detected during resolution.
    /// </summary>
    [JsonPropertyName("issues")]
    public ValidationIssueDto[]? Issues { get; set; }
}

/// <summary>
/// Request payload for validating a dependency path.
/// </summary>
public class ValidatePathRequest
{
    [JsonPropertyName("path")]
    public string Path { get; set; } = string.Empty;

    [JsonPropertyName("entityTypeName")]
    public string EntityTypeName { get; set; } = string.Empty;
}
