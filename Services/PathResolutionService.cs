using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using knkwebapi_v2.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace knkwebapi_v2.Services
{
    /// <summary>
    /// Implementation of path resolution service for navigating entity relationships.
    /// 
    /// SHARED SERVICE:
    /// Used by both dependency resolution and placeholder interpolation to ensure
    /// consistent path navigation behavior across the system.
    /// 
    /// SCOPE:
    /// - v1: Single-hop paths only ("Entity.Property")
    /// - v2: Multi-hop paths with collection operators (future)
    /// </summary>
    public class PathResolutionService : IPathResolutionService
    {
        private readonly ILogger<PathResolutionService> _logger;
        private readonly IMetadataService _metadataService;

        public PathResolutionService(
            ILogger<PathResolutionService> logger,
            IMetadataService metadataService)
        {
            _logger = logger;
            _metadataService = metadataService;
        }

        /// <inheritdoc />
        public async Task<object?> ResolvePathAsync(
            string entityTypeName,
            string path,
            object? currentValue)
        {
            if (currentValue == null)
            {
                _logger.LogDebug("Cannot resolve path '{Path}' on null value for entity type '{EntityType}'",
                    path, entityTypeName);
                return null;
            }

            if (string.IsNullOrWhiteSpace(path))
            {
                _logger.LogDebug("No path specified, returning current value for entity type '{EntityType}'",
                    entityTypeName);
                return currentValue;
            }

            try
            {
                // Split path into segments (e.g., "Town.wgRegionId" → ["Town", "wgRegionId"])
                var segments = path.Split('.');

                // v1 VALIDATION: Maximum 1 dot allowed (Entity.Property pattern)
                if (segments.Length > 2)
                {
                    _logger.LogWarning(
                        "Multi-hop path '{Path}' attempted on entity '{EntityType}'; v1 only supports single-hop paths",
                        path, entityTypeName);
                    return null;
                }

                var currentObject = currentValue;

                // Navigate through each segment
                foreach (var segment in segments)
                {
                    if (currentObject == null)
                    {
                        _logger.LogDebug("Null value encountered while navigating segment '{Segment}' in path '{Path}'",
                            segment, path);
                        return null;
                    }

                    // Try to get property value
                    currentObject = GetPropertyValue(currentObject, segment);
                }

                _logger.LogDebug("Successfully resolved path '{Path}' for entity '{EntityType}' to value: {Value}",
                    path, entityTypeName, currentObject);

                return currentObject;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error resolving path '{Path}' for entity type '{EntityType}'",
                    path, entityTypeName);
                return null;
            }
        }

        /// <inheritdoc />
        public async Task<PathValidationResult> ValidatePathAsync(
            string entityTypeName,
            string path)
        {
            var result = new PathValidationResult { IsValid = true };

            // Validate path is not empty
            if (string.IsNullOrWhiteSpace(path))
            {
                result.IsValid = false;
                result.ErrorMessage = "Path cannot be empty.";
                result.Suggestion = "Specify a property path like 'wgRegionId' or 'Town.wgRegionId'";
                return result;
            }

            // Validate path syntax (no leading/trailing dots, no consecutive dots, no spaces)
            if (path.StartsWith(".") || path.EndsWith(".") || path.Contains("..") || path.Contains(" "))
            {
                result.IsValid = false;
                result.ErrorMessage = $"Path '{path}' has invalid syntax.";
                result.Suggestion = "Remove leading/trailing dots, consecutive dots, and spaces. Example: 'Town.wgRegionId'";
                return result;
            }

            var segments = path.Split('.');

            // v1 CONSTRAINT: Maximum 2 segments (Entity.Property)
            if (segments.Length > 2)
            {
                result.IsValid = false;
                result.ErrorMessage = $"Path '{path}' has multiple levels (dots); v1 only supports single-hop paths.";
                result.Suggestion = "Use a path with at most one dot, like 'Town.wgRegionId'. Multi-hop paths planned for v2.";
                return result;
            }

            // Check if path attempts collection navigation (brackets)
            if (path.Contains("[") || path.Contains("]"))
            {
                result.IsValid = false;
                result.IsCollectionNavigation = true;
                result.ErrorMessage = $"Path '{path}' attempts collection navigation; not supported in v1.";
                result.Suggestion = "Collection operators like '[first]' are planned for v2. Select a single-entity relationship.";
                return result;
            }

            // Validate entity exists in metadata
            var entityMetadata = _metadataService.GetEntityMetadata(entityTypeName);
            if (entityMetadata == null)
            {
                result.IsValid = false;
                result.ErrorMessage = $"Entity '{entityTypeName}' not found in system metadata.";
                result.Suggestion = "Check entity name spelling or ensure entity is registered in metadata service.";
                return result;
            }

            // Validate each segment is a valid property
            Type? currentType = GetEntityType(entityTypeName);
            if (currentType == null)
            {
                result.IsValid = false;
                result.ErrorMessage = $"Could not resolve CLR type for entity '{entityTypeName}'.";
                result.Suggestion = "Ensure entity is properly defined in the Models namespace.";
                return result;
            }

            foreach (var segment in segments)
            {
                var property = currentType.GetProperty(segment,
                    BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

                if (property == null)
                {
                    result.IsValid = false;
                    result.ErrorMessage = $"Property '{segment}' not found on entity '{currentType.Name}'.";
                    result.MissingProperties.Add(segment);

                    // Suggest available properties
                    var availableProps = currentType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                        .Select(p => p.Name)
                        .Take(10)
                        .ToList();
                    result.Suggestion = $"Available properties: {string.Join(", ", availableProps)}";

                    return result;
                }

                // Check if property is a collection (v1 unsupported)
                if (IsCollectionType(property.PropertyType))
                {
                    result.IsValid = false;
                    result.IsCollectionNavigation = true;
                    result.ErrorMessage = $"Property '{segment}' is a collection; v1 does not support collection navigation.";
                    result.Suggestion = "Select a single-entity navigation property instead. Collection support planned for v2.";
                    return result;
                }

                // Update current type for next segment validation
                currentType = property.PropertyType;
            }

            _logger.LogDebug("Path '{Path}' validated successfully for entity '{EntityType}'",
                path, entityTypeName);

            return result;
        }

        /// <inheritdoc />
        public string[] GetIncludePathsForNavigation(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return Array.Empty<string>();
            }

            var segments = path.Split('.');

            // For single property (no dot), no includes needed
            if (segments.Length == 1)
            {
                return Array.Empty<string>();
            }

            // For "Entity.Property", include "Entity"
            // For "Entity.Relation.Property" (v2), include "Entity" and "Entity.Relation"
            var includePaths = new List<string>();
            var currentPath = string.Empty;

            // Include all segments except the last one (which is the final property)
            for (int i = 0; i < segments.Length - 1; i++)
            {
                currentPath = string.IsNullOrEmpty(currentPath)
                    ? segments[i]
                    : $"{currentPath}.{segments[i]}";

                includePaths.Add(currentPath);
            }

            _logger.LogDebug("Generated include paths for '{Path}': {IncludePaths}",
                path, string.Join(", ", includePaths));

            return includePaths.ToArray();
        }

        /// <inheritdoc />
        public async Task<List<EntityPropertySuggestion>> GetEntityPropertiesAsync(string entityTypeName)
        {
            var suggestions = new List<EntityPropertySuggestion>();

            try
            {
                // Get entity type via reflection
                Type? entityType = GetEntityType(entityTypeName);
                if (entityType == null)
                {
                    _logger.LogWarning("Could not find entity type '{EntityType}' for property suggestions",
                        entityTypeName);
                    return suggestions;
                }

                // Get all public instance properties
                var properties = entityType.GetProperties(BindingFlags.Public | BindingFlags.Instance);

                foreach (var property in properties)
                {
                    var suggestion = new EntityPropertySuggestion
                    {
                        PropertyName = property.Name,
                        PropertyType = GetFriendlyTypeName(property.PropertyType),
                        IsNavigationProperty = IsNavigationProperty(property.PropertyType),
                        IsCollection = IsCollectionType(property.PropertyType)
                    };

                    // v1: Show all properties
                    // v2: Add smart filtering based on validation type compatibility
                    suggestions.Add(suggestion);
                }

                _logger.LogDebug("Retrieved {Count} property suggestions for entity '{EntityType}'",
                    suggestions.Count, entityTypeName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error retrieving property suggestions for entity '{EntityType}'",
                    entityTypeName);
            }

            return suggestions;
        }

        #region Private Helper Methods

        /// <summary>
        /// Get property value from an object using reflection.
        /// Handles both POCO objects and dictionaries.
        /// </summary>
        private object? GetPropertyValue(object obj, string propertyName)
        {
            if (obj == null)
            {
                return null;
            }

            // Handle Dictionary<string, object>
            if (obj is Dictionary<string, object?> dict)
            {
                // Case-insensitive lookup
                var key = dict.Keys.FirstOrDefault(k =>
                    k.Equals(propertyName, StringComparison.OrdinalIgnoreCase));

                return key != null ? dict[key] : null;
            }

            // Handle POCO objects via reflection
            var type = obj.GetType();
            var property = type.GetProperty(propertyName,
                BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

            if (property == null)
            {
                _logger.LogDebug("Property '{PropertyName}' not found on type '{TypeName}'",
                    propertyName, type.Name);
                return null;
            }

            try
            {
                return property.GetValue(obj);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex,
                    "Error getting value of property '{PropertyName}' from object of type '{TypeName}'",
                    propertyName, type.Name);
                return null;
            }
        }

        /// <summary>
        /// Get entity CLR type by name via reflection.
        /// </summary>
        private Type? GetEntityType(string entityTypeName)
        {
            try
            {
                // Try to find type in knkwebapi_v2.Models namespace
                var assembly = Assembly.GetExecutingAssembly();
                var modelTypes = assembly.GetTypes()
                    .Where(t => t.Namespace?.StartsWith("knkwebapi_v2.Models") == true)
                    .ToList();

                var entityType = modelTypes.FirstOrDefault(t =>
                    t.Name.Equals(entityTypeName, StringComparison.OrdinalIgnoreCase));

                return entityType;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resolving entity type '{EntityTypeName}'", entityTypeName);
                return null;
            }
        }

        /// <summary>
        /// Check if a type represents a navigation property (class type, not primitive).
        /// </summary>
        private bool IsNavigationProperty(Type type)
        {
            // Primitive types, strings, and value types are not navigation properties
            if (type.IsPrimitive || type == typeof(string) || type.IsValueType)
            {
                return false;
            }

            // Collections are navigation properties (but handled separately)
            if (IsCollectionType(type))
            {
                return true;
            }

            // Class types (except string) are navigation properties
            return type.IsClass;
        }

        /// <summary>
        /// Check if a type represents a collection.
        /// </summary>
        private bool IsCollectionType(Type type)
        {
            // Arrays
            if (type.IsArray)
            {
                return true;
            }

            // IEnumerable<T>, ICollection<T>, List<T>, etc.
            if (type.IsGenericType)
            {
                var genericTypeDef = type.GetGenericTypeDefinition();
                if (genericTypeDef == typeof(IEnumerable<>) ||
                    genericTypeDef == typeof(ICollection<>) ||
                    genericTypeDef == typeof(List<>) ||
                    genericTypeDef == typeof(IList<>))
                {
                    return true;
                }
            }

            // IEnumerable (non-generic)
            if (typeof(System.Collections.IEnumerable).IsAssignableFrom(type) &&
                type != typeof(string)) // String is IEnumerable but not a collection
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Get a friendly type name for display.
        /// </summary>
        private string GetFriendlyTypeName(Type type)
        {
            if (type == typeof(string))
            {
                return "string";
            }

            if (type == typeof(int))
            {
                return "int";
            }

            if (type == typeof(bool))
            {
                return "bool";
            }

            if (type == typeof(DateTime))
            {
                return "DateTime";
            }

            if (type == typeof(decimal))
            {
                return "decimal";
            }

            if (type == typeof(double))
            {
                return "double";
            }

            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                var underlyingType = Nullable.GetUnderlyingType(type);
                return $"{GetFriendlyTypeName(underlyingType!)}?";
            }

            if (IsCollectionType(type))
            {
                if (type.IsGenericType)
                {
                    var elementType = type.GetGenericArguments()[0];
                    return $"List<{GetFriendlyTypeName(elementType)}>";
                }
                return "Collection";
            }

            // For entity types, return just the class name
            return type.Name;
        }

        #endregion
    }
}
