using System.Collections.Generic;
using System.Threading.Tasks;

namespace knkwebapi_v2.Services.Interfaces
{
    /// <summary>
    /// Service for resolving multi-layer dependency field values.
    /// 
    /// Supports navigating through entity relationships to resolve final property values.
    /// Used when a validation rule depends on another field's value, and that value may need
    /// to be transformed by navigating through related entities.
    /// </summary>
    public interface IDependencyResolutionService
    {
        /// <summary>
        /// Resolve a dependency field value using direct path navigation.
        /// Used for in-memory objects where the dependency object is already loaded.
        /// 
        /// LAYERS SUPPORTED:
        /// - Layer 0: Direct property (e.g., "WgRegionId")
        /// - Layer 1: Single navigation (e.g., "Town.WgRegionId") 
        /// - Layer 2+: Multi-level navigation (e.g., "District.Town.WgRegionId")
        /// </summary>
        /// <param name="dependencyValue">The dependency field's current value (from form context)</param>
        /// <param name="dependencyPath">Path to navigate (e.g., "Town.WgRegionId" for Layer 1)</param>
        /// <returns>Resolved value after navigating the path, or null if not found</returns>
        object? ResolveDependency(object? dependencyValue, string? dependencyPath);

        /// <summary>
        /// Resolve a dependency field value from a form context dictionary.
        /// Useful when working with deserialized JSON where dependency data is in a dictionary.
        /// </summary>
        /// <param name="formContext">Dictionary containing form field values and related entities</param>
        /// <param name="dependencyFieldName">Name of the dependency field in the context</param>
        /// <param name="dependencyPath">Path to navigate from the dependency value</param>
        /// <returns>Resolved value after navigating the path</returns>
        object? ResolveDependencyFromContext(Dictionary<string, object>? formContext, string dependencyFieldName, string? dependencyPath);

        /// <summary>
        /// Resolve a dependency field value using Entity Framework.
        /// Performs database queries with Include chains to eagerly load related entities.
        /// Used for complex dependencies that require database lookups.
        /// </summary>
        /// <param name="dependencyEntityType">Type of the dependency entity (e.g., "Town")</param>
        /// <param name="dependencyId">Primary key of the dependency entity</param>
        /// <param name="dependencyPath">Path to navigate from the dependency entity</param>
        /// <returns>Resolved value after navigating the path</returns>
        Task<object?> ResolveDependencyForEntityAsync(string dependencyEntityType, object dependencyId, string? dependencyPath);

        /// <summary>
        /// Batch resolve multiple dependencies in a single operation.
        /// More efficient than calling ResolveDependencyForEntityAsync multiple times.
        /// </summary>
        /// <param name="dependencies">List of (entityType, entityId, dependencyPath) tuples to resolve</param>
        /// <returns>Dictionary mapping dependency specifications to resolved values</returns>
        Task<Dictionary<string, object?>> ResolveDependenciesAsync(
            List<(string entityType, object entityId, string? dependencyPath)> dependencies);
    }
}
