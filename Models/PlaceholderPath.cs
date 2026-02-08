using System;
using System.Collections.Generic;
using System.Linq;

namespace knkwebapi_v2.Models;

/// <summary>
/// Parses and represents placeholder paths for variable resolution in validation error/success messages.
/// 
/// PLACEHOLDERS LAYERS:
///
/// Layer 0: Direct properties from current form data
/// - Pattern: {PropertyName}
/// - Example: {Name} → resolves from form field value
/// - Depth: 0
/// 
/// Layer 1: Single-level navigation with DB query
/// - Pattern: {NavigationProperty.PropertyName}
/// - Example: {Town.Name} → fetch Town entity, get Name
/// - Depth: 1
/// - Requires: Foreign key to related entity
/// 
/// Layer 2: Multi-level navigation with dynamic Include chains
/// - Pattern: {Navigation1.Navigation2.PropertyName}
/// - Example: {District.Town.Name} → fetch with Include, navigate chain
/// - Depth: 2+
/// - Requires: Multiple navigations in entity model
/// 
/// Layer 3: Aggregate operations on collections
/// - Pattern: {Navigation.Collection.AggregateOp}
/// - Example: {Town.Districts.Count} → Load collection, execute aggregate
/// - Depth: 2+ with collection endpoint
/// - Requires: Collection navigation and LINQ aggregate
/// 
/// </summary>
public class PlaceholderPath
{
    /// <summary>
    /// Full placeholder path without braces (e.g., "Town.Districts.Count")
    /// </summary>
    public string FullPath { get; set; } = string.Empty;

    /// <summary>
    /// Segments of the path split by dots (e.g., ["Town", "Districts", "Count"])
    /// </summary>
    public string[] Segments { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Depth of the placeholder (number of navigation levels).
    /// 0 = Layer 0 (direct property)
    /// 1 = Layer 1 (single navigation)
    /// 2+ = Layer 2 (multi-level navigation)
    /// </summary>
    public int Depth { get; set; }

    /// <summary>
    /// The final property or operation name (e.g., "Name", "Count")
    /// </summary>
    public string FinalSegment { get; set; } = string.Empty;

    /// <summary>
    /// Navigation path excluding the final segment (e.g., "Town" for "Town.Name")
    /// </summary>
    public string NavigationPath { get; set; } = string.Empty;

    /// <summary>
    /// True if the final segment is an aggregate operation (Count, First, Last, Any, Sum, etc.)
    /// </summary>
    public bool IsAggregateOperation { get; set; }

    /// <summary>
    /// Contains the aggregate operation name if IsAggregateOperation is true
    /// </summary>
    public string? AggregateOperationName { get; set; }

    /// <summary>
    /// Parses a placeholder string (with or without braces) into a PlaceholderPath.
    /// 
    /// EXAMPLES:
    /// - Input: "Name" → Output: PlaceholderPath(FullPath="Name", Depth=0)
    /// - Input: "{Town.Name}" → Output: PlaceholderPath(FullPath="Town.Name", Depth=1)
    /// - Input: "District.Town.Name" → Output: PlaceholderPath(FullPath="District.Town.Name", Depth=2)
    /// - Input: "Town.Districts.Count" → Output: PlaceholderPath(FullPath="Town.Districts.Count", Depth=2, IsAggregateOperation=true)
    /// 
    /// </summary>
    /// <param name="placeholder">The placeholder string (with or without braces)</param>
    /// <returns>Parsed PlaceholderPath object</returns>
    /// <exception cref="ArgumentException">If placeholder format is invalid</exception>
    public static PlaceholderPath Parse(string placeholder)
    {
        if (string.IsNullOrWhiteSpace(placeholder))
        {
            throw new ArgumentException("Placeholder cannot be null or empty", nameof(placeholder));
        }

        // Remove braces if present
        var path = placeholder.Trim();
        if (path.StartsWith("{") && path.EndsWith("}"))
        {
            path = path.Substring(1, path.Length - 2);
        }

        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ArgumentException("Placeholder path cannot be empty", nameof(placeholder));
        }

        // Validate characters (alphanumeric, dots, underscores)
        if (!System.Text.RegularExpressions.Regex.IsMatch(path, @"^[a-zA-Z_][a-zA-Z0-9_.]*$"))
        {
            throw new ArgumentException($"Invalid placeholder path format: {placeholder}", nameof(placeholder));
        }

        var segments = path.Split('.', StringSplitOptions.RemoveEmptyEntries);
        if (segments.Length == 0)
        {
            throw new ArgumentException($"Placeholder path has no segments: {placeholder}", nameof(placeholder));
        }

        var result = new PlaceholderPath
        {
            FullPath = path,
            Segments = segments,
            Depth = segments.Length - 1,
            FinalSegment = segments[^1]
        };

        // Check if final segment is an aggregate operation
        var aggregateOps = new[] { "Count", "First", "Last", "Any", "Sum", "Average", "Max", "Min" };
        if (aggregateOps.Contains(result.FinalSegment, StringComparer.OrdinalIgnoreCase))
        {
            result.IsAggregateOperation = true;
            result.AggregateOperationName = result.FinalSegment;
        }

        // Build navigation path (all segments except the last one)
        if (segments.Length > 1)
        {
            result.NavigationPath = string.Join(".", segments.SkipLast(1));
        }

        return result;
    }

    /// <summary>
    /// Gets the Include paths needed for EF Core to load this placeholder.
    /// 
    /// EXAMPLES:
    /// - "Town.Name" → ["Town"]
    /// - "District.Town.Name" → ["District", "District.Town"]
    /// - "Town.Districts.Count" → ["Town", "Town.Districts"]
    /// 
    /// </summary>
    public IEnumerable<string> GetIncludePaths()
    {
        if (Depth == 0)
        {
            return Enumerable.Empty<string>();
        }

        var paths = new List<string>();
        for (int i = 1; i <= Depth; i++)
        {
            paths.Add(string.Join(".", Segments.Take(i)));
        }

        return paths;
    }

    /// <summary>
    /// Gets the navigation chain (all segments except the final one) as a list.
    /// </summary>
    public string[] GetNavigationChain()
    {
        if (Depth == 0)
        {
            return Array.Empty<string>();
        }

        return Segments.SkipLast(1).ToArray();
    }

    public override string ToString() => FullPath;
}
