using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using knkwebapi_v2.Properties;
using knkwebapi_v2.Services.Interfaces;

namespace knkwebapi_v2.Services;

/// <summary>
/// Service for resolving multi-layer dependencies when building validation contexts.
/// Handles fetching related entities and extracting property values.
/// </summary>
public class DependencyResolutionService : IDependencyResolutionService
{
    private readonly KnKDbContext _context;

    public DependencyResolutionService(KnKDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Resolve a dependency field value with multi-layer support.
    /// 
    /// Example: To validate that a Location is inside a Town's region:
    /// - dependencyFieldId: 61 (the WgRegionId field)
    /// - dependencyPath: "Town.WgRegionId" (navigate to Town, get WgRegionId)
    /// - currentFieldValues: { "Town": townEntity }
    /// 
    /// Returns: "town_1" (the WgRegionId value)
    /// </summary>
    public object? ResolveDependency(
        object? fieldValue,
        string? dependencyPath)
    {
        // If field value is null, nothing to resolve from
        if (fieldValue == null)
        {
            return null;
        }

        // If no path specified, return the field value itself (Layer 0)
        if (string.IsNullOrWhiteSpace(dependencyPath))
        {
            return fieldValue;
        }

        // Resolve the path through the object
        return DependencyPathResolver.ResolvePath(fieldValue, dependencyPath);
    }

    /// <summary>
    /// Resolve a dependency field value from a form context dictionary.
    /// Useful when working with deserialized JSON where dependency data is in a dictionary.
    /// </summary>
    public object? ResolveDependencyFromContext(
        Dictionary<string, object>? formContext,
        string dependencyFieldName,
        string? dependencyPath)
    {
        if (formContext == null || !formContext.TryGetValue(dependencyFieldName, out var fieldValue))
        {
            return null;
        }

        // If no path specified, return the dependency field value itself (Layer 0)
        if (string.IsNullOrWhiteSpace(dependencyPath))
        {
            return fieldValue;
        }

        // Resolve the path through the object/dictionary
        return DependencyPathResolver.ResolvePathFromContext(formContext, dependencyFieldName, dependencyPath);
    }

    /// <summary>
    /// Resolve a dependency field value using Entity Framework with string-based entity type and ID.
    /// Note: This simplified version focuses on in-memory resolution.
    /// For database queries, use the generic ResolveDependencyForEntityAsync&lt;TEntity&gt;.
    /// </summary>
    public async Task<object?> ResolveDependencyForEntityAsync(
        string dependencyEntityType,
        object dependencyId,
        string? dependencyPath)
    {
        // For now, this method is not implemented with full database support
        // The primary use case (frontend-based dependency resolution) works via resolveDependencyPath() utility
        // Full database support would require knowing entity types at compile time or using reflection extensively
        // 
        // If needed in future, this can be extended with specific entity type handling
        // or using a factory pattern for dynamic entity type resolution
        
        return await Task.FromResult<object?>(null);
    }

    /// <summary>
    /// Batch resolve multiple dependencies in a single operation.
    /// More efficient than calling ResolveDependencyForEntityAsync multiple times.
    /// </summary>
    public async Task<Dictionary<string, object?>> ResolveDependenciesAsync(
        List<(string entityType, object entityId, string? dependencyPath)> dependencies)
    {
        var results = new Dictionary<string, object?>();

        foreach (var (entityType, entityId, dependencyPath) in dependencies)
        {
            var key = $"{entityType}:{entityId}:{dependencyPath}";
            var resolved = await ResolveDependencyForEntityAsync(entityType, entityId, dependencyPath);
            results[key] = resolved;
        }

        return results;
    }

    /// <summary>
    /// Resolve a dependency field value for a specific entity instance with multi-layer support (generic version).
    /// Used when you have an entity ID and need to fetch related data.
    /// Type-safe version when you know the entity type at compile time.
    ///
    /// Example: For a Structure entity, resolve "District.Town.WgRegionId"
    /// </summary>
    public async Task<object?> ResolveDependencyForEntityAsync<TEntity>(
        int entityId,
        string dependencyPath)
        where TEntity : class
    {
        if (string.IsNullOrWhiteSpace(dependencyPath))
        {
            return null;
        }

        var (segments, depth) = DependencyPathResolver.ParsePath(dependencyPath);

        if (segments.Length == 0)
        {
            return null;
        }

        // For Layer 0 (direct property, no navigation)
        if (depth == 0)
        {
            var entity = await _context.Set<TEntity>().FindAsync(entityId);
            if (entity == null)
            {
                return null;
            }

            return DependencyPathResolver.ResolvePath(entity, dependencyPath);
        }

        // For Layer 1+ (requires Include chains)
        // This is more complex and requires dynamic Include building
        return await ResolveDependencyWithIncludesAsync<TEntity>(entityId, segments);
    }

    /// <summary>
    /// Build an Include chain for a query to fetch related entities.
    /// Handles arbitrary depth of navigation.
    /// </summary>
    private async Task<object?> ResolveDependencyWithIncludesAsync<TEntity>(
        int entityId,
        string[] segments)
        where TEntity : class
    {
        if (segments.Length == 0)
        {
            return null;
        }

        // Start with the base query
        var query = _context.Set<TEntity>().AsQueryable();

        // Build Include chains for all segments except the last
        // (the last segment is the property we're extracting, not a navigation)
        for (int i = 0; i < segments.Length - 1; i++)
        {
            var navigationPath = string.Join(".", segments.Take(i + 1));
            
            // This is a simplified approach - for complex scenarios,
            // you might need to use Include() in a loop or use reflection
            query = IncludeNavigation(query, navigationPath);
        }

        // Fetch the entity with all includes
        var entity = await query.FirstOrDefaultAsync(e =>
            EF.Property<int>(e, "Id") == entityId);

        if (entity == null)
        {
            return null;
        }

        // Now extract the final value using the full path
        return DependencyPathResolver.ResolvePath(entity, string.Join(".", segments));
    }

    /// <summary>
    /// Helper method to apply Include to a query.
    /// In a real implementation, this would use reflection or dynamic LINQ.
    /// For now, it demonstrates the concept.
    /// </summary>
    private IQueryable<TEntity> IncludeNavigation<TEntity>(
        IQueryable<TEntity> query,
        string navigationPath)
        where TEntity : class
    {
        // EF Core's Include method expects a string path
        // This is a simplified version - a production implementation would be more robust
        return query.Include(navigationPath);
    }

    /// <summary>
    /// Resolve multiple dependency paths at once for efficiency.
    /// Useful when building validation context with multiple rules.
    /// </summary>
    public Dictionary<string, object?> ResolveDependencies(
        Dictionary<string, object?> fieldValues,
        List<(string fieldName, string? dependencyPath)> dependencies)
    {
        var results = new Dictionary<string, object?>();

        foreach (var (fieldName, dependencyPath) in dependencies)
        {
            if (fieldValues.TryGetValue(fieldName, out var fieldValue))
            {
                var resolved = DependencyPathResolver.ResolvePathFromContext(
                    fieldValues.ToDictionary(kvp => kvp.Key, kvp => kvp.Value),
                    fieldName,
                    dependencyPath);
                
                results[fieldName] = resolved;
            }
        }

        return results;
    }
}
