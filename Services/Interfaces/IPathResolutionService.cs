using System.Collections.Generic;
using System.Threading.Tasks;

namespace knkwebapi_v2.Services.Interfaces
{
    /// <summary>
    /// Shared service for resolving entity relationship paths used by both
    /// dependency resolution and placeholder interpolation systems.
    /// 
    /// DESIGN DECISION (Feb 2026):
    /// Both multi-layer dependency resolution (v2.0) and placeholder interpolation
    /// share this single service to avoid code duplication and ensure consistent
    /// path navigation behavior.
    /// 
    /// SCOPE:
    /// - v1: Single-hop paths only ("Entity.Property")
    /// - v2: Multi-hop paths with collection operators ("Entity.Relation[first].Property")
    /// </summary>
    public interface IPathResolutionService
    {
        /// <summary>
        /// Navigate and resolve a path on a specific entity to extract a value.
        /// 
        /// USAGE:
        /// - Dependency Resolution: Resolve "Town.wgRegionId" → Gets dependency field value
        /// - Placeholder Interpolation: Resolve "Town.Name" → Gets value for {Town.Name} placeholder
        /// 
        /// EXAMPLES:
        /// - entityTypeName: "Town"
        /// - path: "wgRegionId" (single property)
        /// - currentValue: { id: 4, name: "Springfield", wgRegionId: "town_1" }
        /// - Returns: "town_1"
        /// 
        /// - entityTypeName: "District"
        /// - path: "Town.wgRegionId" (v1 single-hop navigation)
        /// - currentValue: { id: 1, townId: 4, town: { id: 4, wgRegionId: "town_1" } }
        /// - Returns: "town_1"
        /// </summary>
        /// <param name="entityTypeName">Type name of the entity (e.g., "Town", "District")</param>
        /// <param name="path">Dot-notation path to navigate (e.g., "wgRegionId" or "Town.wgRegionId")</param>
        /// <param name="currentValue">Current entity value (object or dictionary)</param>
        /// <returns>Resolved value at the end of the path, or null if not found</returns>
        Task<object?> ResolvePathAsync(
            string entityTypeName,
            string path,
            object? currentValue
        );

        /// <summary>
        /// Validate a path is syntactically correct and references exist in metadata.
        /// 
        /// VALIDATION CHECKS:
        /// - v1: Exactly 1 dot for entity.property pattern, or 0 dots for direct property
        /// - Entity exists in system metadata
        /// - Property exists on entity
        /// - v1: No collection navigation attempts
        /// - v2: Valid collection operator syntax
        /// 
        /// USAGE:
        /// - On Save: Immediate validation when admin creates/updates validation rule
        /// - Health Panel: Comprehensive validation during form configuration review
        /// </summary>
        /// <param name="entityTypeName">Entity type to validate against</param>
        /// <param name="path">Path to validate</param>
        /// <returns>Validation result with detailed error messages and suggestions</returns>
        Task<PathValidationResult> ValidatePathAsync(
            string entityTypeName,
            string path
        );

        /// <summary>
        /// Get EF Core Include paths needed for optimizing database queries.
        /// 
        /// EXAMPLES:
        /// - Input path: "Town.wgRegionId"
        /// - Returns: ["Town"]
        /// 
        /// - Input path: "District.Town.wgRegionId" (v2)
        /// - Returns: ["District", "District.Town"]
        /// 
        /// Used by placeholder interpolation Layers 1-3 to optimize database queries
        /// with proper eager loading of navigation properties.
        /// </summary>
        /// <param name="path">Dot-notation path</param>
        /// <returns>Array of Include paths for EF Core</returns>
        string[] GetIncludePathsForNavigation(string path);

        /// <summary>
        /// Get property suggestions for an entity type.
        /// Used by PathBuilder component dropdown to show available properties.
        /// 
        /// v1 BEHAVIOR: Returns ALL properties of the entity
        /// - Scalars (id, name, wgRegionId)
        /// - Navigation properties (Town, District)
        /// - Computed properties if applicable
        /// 
        /// v2 ENHANCEMENT: Smart filtering based on validation type
        /// - Show only properties compatible with specific validation types
        /// - Mark properties as "recommended" vs "advanced"
        /// </summary>
        /// <param name="entityTypeName">Entity type name</param>
        /// <returns>List of property suggestions with metadata</returns>
        Task<List<EntityPropertySuggestion>> GetEntityPropertiesAsync(string entityTypeName);
    }

    /// <summary>
    /// Result of path validation with detailed error information.
    /// </summary>
    public class PathValidationResult
    {
        /// <summary>
        /// Whether the path is valid and can be resolved.
        /// </summary>
        public bool IsValid { get; set; }

        /// <summary>
        /// Error message if validation failed.
        /// Examples:
        /// - "Path 'Town.District.wgRegionId' has multiple dots; v1 only supports single-hop paths."
        /// - "Property 'invalidProp' not found on entity 'Town'"
        /// - "Entity 'InvalidEntity' not found in system metadata"
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// Suggestion for fixing the error.
        /// Examples:
        /// - "Use 'Town.wgRegionId' instead of 'Town.District.wgRegionId'"
        /// - "Available properties: id, name, wgRegionId, description"
        /// </summary>
        public string? Suggestion { get; set; }

        /// <summary>
        /// List of properties that were referenced but don't exist.
        /// Used for detailed error reporting in health checks.
        /// </summary>
        public List<string> MissingProperties { get; set; } = new();

        /// <summary>
        /// Whether this path attempts to navigate a collection (v1 unsupported).
        /// Used to provide specific guidance for v2 upgrade path.
        /// </summary>
        public bool IsCollectionNavigation { get; set; }
    }

    /// <summary>
    /// Property suggestion for PathBuilder dropdown.
    /// </summary>
    public class EntityPropertySuggestion
    {
        /// <summary>
        /// Property name (e.g., "wgRegionId", "name").
        /// </summary>
        public string PropertyName { get; set; } = string.Empty;

        /// <summary>
        /// Property type (e.g., "string", "int", "Town", "District").
        /// </summary>
        public string PropertyType { get; set; } = string.Empty;

        /// <summary>
        /// Whether this property is a navigation property to another entity.
        /// </summary>
        public bool IsNavigationProperty { get; set; }

        /// <summary>
        /// Whether this property is a collection (List, IEnumerable, etc.).
        /// Used to warn about v1 limitations.
        /// </summary>
        public bool IsCollection { get; set; }

        /// <summary>
        /// Optional description of the property.
        /// Extracted from XML documentation or entity metadata.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Whether this property is compatible with specific validation types.
        /// v2 feature: Enables smart filtering in PathBuilder.
        /// </summary>
        public Dictionary<string, bool>? ValidationTypeCompatibility { get; set; }
    }
}
