using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.EntityFrameworkCore;

namespace knkwebapi_v2.Services;

/// <summary>
/// Resolves multi-layer dependency paths through entity relationships.
///
/// Supports navigating through related entities to extract property values.
/// Examples:
/// - "WgRegionId" (Layer 0): Direct property
/// - "Town.WgRegionId" (Layer 1): Navigate to Town, get WgRegionId
/// - "District.Town.WgRegionId" (Layer 2): Navigate through multiple relations
/// </summary>
public class DependencyPathResolver
{
    /// <summary>
    /// Resolve a dependency path from a root object through its related entities.
    /// 
    /// Example: Given a Structure object and path "District.Town.WgRegionId"
    /// Returns: The WgRegionId value of the Town related to the District
    /// </summary>
    public static object? ResolvePath(object? rootObject, string? path)
    {
        if (rootObject == null || string.IsNullOrWhiteSpace(path))
        {
            return rootObject;
        }

        var segments = path.Split('.');
        var currentObject = rootObject;

        foreach (var segment in segments)
        {
            if (currentObject == null)
            {
                return null;
            }

            currentObject = GetPropertyValue(currentObject, segment);
        }

        return currentObject;
    }

    /// <summary>
    /// Get a property value from an object, handling both simple properties and navigation properties.
    /// </summary>
    private static object? GetPropertyValue(object obj, string propertyName)
    {
        if (obj == null)
        {
            return null;
        }

        var type = obj.GetType();
        var property = type.GetProperty(propertyName, BindingFlags.IgnoreCase | BindingFlags.Public);

        if (property == null)
        {
            // Property not found - fail gracefully
            return null;
        }

        try
        {
            return property.GetValue(obj);
        }
        catch
        {
            // If property access fails, return null
            return null;
        }
    }

    /// <summary>
    /// Resolve a dependency path from form context data (JavaScript objects/dictionaries).
    /// Used on the frontend or when working with JSON data.
    /// </summary>
    public static object? ResolvePathFromContext(
        Dictionary<string, object?>? context,
        string? dependencyFieldName,
        string? dependencyPath)
    {
        if (context == null || string.IsNullOrWhiteSpace(dependencyFieldName))
        {
            return null;
        }

        // First, get the dependency field value from context
        if (!context.TryGetValue(dependencyFieldName, out var fieldValue))
        {
            return null;
        }

        // If no path specified, return the field value itself
        if (string.IsNullOrWhiteSpace(dependencyPath))
        {
            return fieldValue;
        }

        // If the field value is a dictionary, navigate through it
        if (fieldValue is Dictionary<string, object?> dict)
        {
            return ResolvePathFromDictionary(dict, dependencyPath);
        }

        // If the field value is an object, navigate through its properties
        if (fieldValue != null)
        {
            return ResolvePath(fieldValue, dependencyPath);
        }

        return null;
    }

    /// <summary>
    /// Resolve a path from a dictionary (JSON object).
    /// Handles the case where formContext contains serialized entity data.
    /// </summary>
    private static object? ResolvePathFromDictionary(Dictionary<string, object?> dict, string path)
    {
        var segments = path.Split('.');
        var current = (object?)dict;

        foreach (var segment in segments)
        {
            if (current == null)
            {
                return null;
            }

            if (current is Dictionary<string, object?> currentDict)
            {
                // Try case-insensitive lookup
                var key = currentDict.Keys.FirstOrDefault(k =>
                    k.Equals(segment, StringComparison.OrdinalIgnoreCase));

                if (key != null)
                {
                    current = currentDict[key];
                }
                else
                {
                    return null;
                }
            }
            else
            {
                // Not a dictionary, try property access
                current = GetPropertyValue(current, segment);
            }
        }

        return current;
    }

    /// <summary>
    /// Parse a dependency path to understand the navigation layers.
    /// Returns the path segments and the depth of navigation.
    /// </summary>
    public static (string[] segments, int depth) ParsePath(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return (Array.Empty<string>(), 0);
        }

        var segments = path.Split('.').Where(s => !string.IsNullOrWhiteSpace(s)).ToArray();
        return (segments, segments.Length - 1); // depth = number of navigations (n segments = n-1 navigations)
    }
}
