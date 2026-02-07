using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using knkwebapi_v2.Properties;
using knkwebapi_v2.Dtos;
using knkwebapi_v2.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace knkwebapi_v2.Services
{
    /// <summary>
    /// Service implementation for resolving placeholder variables in validation messages.
    /// Handles multi-layer navigation through entity relationships.
    /// </summary>
    public class PlaceholderResolutionService : IPlaceholderResolutionService
    {
        private readonly KnKDbContext _dbContext;
        private readonly ILogger<PlaceholderResolutionService> _logger;

        public PlaceholderResolutionService(
            KnKDbContext dbContext,
            ILogger<PlaceholderResolutionService> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task<ResolvePlaceholdersResponseDto> ResolvePlaceholdersAsync(ResolvePlaceholdersRequestDto request)
        {
            var response = new ResolvePlaceholdersResponseDto();

            // Start with Layer 0 placeholders (already provided from frontend)
            if (request.CurrentEntityPlaceholders != null)
            {
                foreach (var kvp in request.CurrentEntityPlaceholders)
                {
                    response.ResolvedPlaceholders[kvp.Key] = kvp.Value;
                }
            }

            // Parse and categorize placeholder paths
            var layer1Paths = new List<string>();
            var layer2Paths = new List<string>();
            var layer3Paths = new List<string>();

            foreach (var path in request.PlaceholderPaths)
            {
                if (string.IsNullOrWhiteSpace(path)) continue;

                // Skip if already resolved in Layer 0
                if (response.ResolvedPlaceholders.ContainsKey(path)) continue;

                var dotCount = path.Count(c => c == '.');
                
                if (dotCount == 0)
                {
                    // Layer 0 - should already be resolved from request
                    if (!response.ResolvedPlaceholders.ContainsKey(path))
                    {
                        response.UnresolvedPlaceholders.Add(path);
                    }
                }
                else if (path.EndsWith(".Count", StringComparison.OrdinalIgnoreCase) ||
                         path.EndsWith(".First", StringComparison.OrdinalIgnoreCase) ||
                         path.EndsWith(".Last", StringComparison.OrdinalIgnoreCase))
                {
                    // Layer 3 - Aggregate operations
                    layer3Paths.Add(path);
                }
                else if (dotCount == 1)
                {
                    // Layer 1 - Single navigation
                    layer1Paths.Add(path);
                }
                else
                {
                    // Layer 2 - Multi-level navigation
                    layer2Paths.Add(path);
                }
            }

            // Resolve Layer 1 (single navigation)
            if (layer1Paths.Any())
            {
                await ResolveSingleNavigationAsync(
                    request.CurrentEntityType,
                    request.CurrentEntityId,
                    layer1Paths,
                    response);
            }

            // Resolve Layer 2 (multi-level navigation)
            if (layer2Paths.Any())
            {
                await ResolveMultiNavigationAsync(
                    request.CurrentEntityType,
                    request.CurrentEntityId,
                    layer2Paths,
                    response);
            }

            // Resolve Layer 3 (aggregates)
            if (layer3Paths.Any())
            {
                await ResolveAggregatesAsync(
                    request.CurrentEntityType,
                    request.CurrentEntityId,
                    layer3Paths,
                    response);
            }

            return response;
        }

        /// <summary>
        /// Resolve Layer 1: Single-level navigation (e.g., Town.Name)
        /// </summary>
        private async Task ResolveSingleNavigationAsync(
            string entityTypeName,
            int? entityId,
            List<string> paths,
            ResolvePlaceholdersResponseDto response)
        {
            if (!entityId.HasValue)
            {
                foreach (var path in paths)
                {
                    response.UnresolvedPlaceholders.Add(path);
                    response.ResolutionErrors.Add($"Cannot resolve {path}: No entity ID provided");
                }
                return;
            }

            try
            {
                var entityType = GetEntityType(entityTypeName);
                if (entityType == null)
                {
                    foreach (var path in paths)
                    {
                        response.UnresolvedPlaceholders.Add(path);
                        response.ResolutionErrors.Add($"Cannot resolve {path}: Entity type {entityTypeName} not found");
                    }
                    return;
                }

                // Group paths by navigation property
                var grouped = paths
                    .Select(p => p.Split('.'))
                    .Where(parts => parts.Length == 2)
                    .GroupBy(parts => parts[0])
                    .ToDictionary(g => g.Key, g => g.Select(parts => parts[1]).ToList());

                foreach (var (navProperty, properties) in grouped)
                {
                    try
                    {
                        var navProp = entityType.GetProperty(navProperty);
                        if (navProp == null)
                        {
                            foreach (var prop in properties)
                            {
                                var fullPath = $"{navProperty}.{prop}";
                                response.UnresolvedPlaceholders.Add(fullPath);
                                response.ResolutionErrors.Add($"Navigation property {navProperty} not found on {entityTypeName}");
                            }
                            continue;
                        }

                        // Get foreign key property (e.g., TownId for Town navigation)
                        var fkPropName = $"{navProperty}Id";
                        var fkProp = entityType.GetProperty(fkPropName);
                        
                        if (fkProp == null)
                        {
                            foreach (var prop in properties)
                            {
                                var fullPath = $"{navProperty}.{prop}";
                                response.UnresolvedPlaceholders.Add(fullPath);
                                response.ResolutionErrors.Add($"Foreign key {fkPropName} not found on {entityTypeName}");
                            }
                            continue;
                        }

                        // Fetch the current entity to get FK value using reflection
                        var setMethod = typeof(DbContext).GetMethod(nameof(DbContext.Set), Array.Empty<Type>())!.MakeGenericMethod(entityType);
                        var dbSet = setMethod.Invoke(_dbContext, null);
                        
                        var firstOrDefaultMethod = typeof(System.Linq.Queryable)
                            .GetMethods()
                            .First(m => m.Name == "FirstOrDefault" && m.GetParameters().Length == 2)
                            .MakeGenericMethod(entityType);

                        var parameter = System.Linq.Expressions.Expression.Parameter(entityType, "e");
                        var idProperty = System.Linq.Expressions.Expression.Property(parameter, "Id");
                        var constant = System.Linq.Expressions.Expression.Constant(entityId.Value);
                        var equals = System.Linq.Expressions.Expression.Equal(idProperty, constant);
                        var lambda = System.Linq.Expressions.Expression.Lambda(equals, parameter);

                        var entityTask = (Task)firstOrDefaultMethod.Invoke(null, new[] { dbSet, lambda })!;
                        await entityTask.ConfigureAwait(false);
                        var entity = ((dynamic)entityTask).Result;

                        if (entity == null)
                        {
                            foreach (var prop in properties)
                            {
                                var fullPath = $"{navProperty}.{prop}";
                                response.UnresolvedPlaceholders.Add(fullPath);
                                response.ResolutionErrors.Add($"Entity {entityTypeName} with ID {entityId.Value} not found");
                            }
                            continue;
                        }

                        var fkValue = fkProp.GetValue(entity);
                        if (fkValue == null)
                        {
                            foreach (var prop in properties)
                            {
                                var fullPath = $"{navProperty}.{prop}";
                                response.UnresolvedPlaceholders.Add(fullPath);
                                response.ResolutionErrors.Add($"Navigation property {navProperty} is null");
                            }
                            continue;
                        }

                        // Fetch related entity using reflection
                        var relatedEntityType = navProp.PropertyType;
                        var relatedSetMethod = typeof(DbContext).GetMethod(nameof(DbContext.Set), Array.Empty<Type>())!.MakeGenericMethod(relatedEntityType);
                        var relatedDbSet = relatedSetMethod.Invoke(_dbContext, null);
                        
                        var relatedFirstOrDefaultMethod = typeof(System.Linq.Queryable)
                            .GetMethods()
                            .First(m => m.Name == "FirstOrDefault" && m.GetParameters().Length == 2)
                            .MakeGenericMethod(relatedEntityType);

                        var relatedParameter = System.Linq.Expressions.Expression.Parameter(relatedEntityType, "e");
                        var relatedIdProperty = System.Linq.Expressions.Expression.Property(relatedParameter, "Id");
                        var relatedConstant = System.Linq.Expressions.Expression.Constant((int)fkValue);
                        var relatedEquals = System.Linq.Expressions.Expression.Equal(relatedIdProperty, relatedConstant);
                        var relatedLambda = System.Linq.Expressions.Expression.Lambda(relatedEquals, relatedParameter);

                        var relatedEntityTask = (Task)relatedFirstOrDefaultMethod.Invoke(null, new[] { relatedDbSet, relatedLambda })!;
                        await relatedEntityTask.ConfigureAwait(false);
                        var relatedEntity = ((dynamic)relatedEntityTask).Result;

                        if (relatedEntity == null)
                        {
                            foreach (var prop in properties)
                            {
                                var fullPath = $"{navProperty}.{prop}";
                                response.UnresolvedPlaceholders.Add(fullPath);
                            }
                            continue;
                        }

                        // Extract properties
                        foreach (var propName in properties)
                        {
                            var prop = relatedEntityType.GetProperty(propName);
                            var fullPath = $"{navProperty}.{propName}";
                            
                            if (prop != null)
                            {
                                var value = prop.GetValue(relatedEntity);
                                response.ResolvedPlaceholders[fullPath] = value?.ToString() ?? "";
                            }
                            else
                            {
                                response.UnresolvedPlaceholders.Add(fullPath);
                                response.ResolutionErrors.Add($"Property {propName} not found on {relatedEntityType.Name}");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning($"Error resolving navigation {navProperty}: {ex.Message}");
                        foreach (var prop in properties)
                        {
                            var fullPath = $"{navProperty}.{prop}";
                            response.UnresolvedPlaceholders.Add(fullPath);
                            response.ResolutionErrors.Add($"Error resolving {fullPath}: {ex.Message}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in ResolveSingleNavigationAsync: {ex.Message}");
                foreach (var path in paths)
                {
                    response.UnresolvedPlaceholders.Add(path);
                    response.ResolutionErrors.Add($"Error resolving {path}: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Resolve Layer 2: Multi-level navigation (e.g., District.Town.Name)
        /// </summary>
        private async Task ResolveMultiNavigationAsync(
            string entityTypeName,
            int? entityId,
            List<string> paths,
            ResolvePlaceholdersResponseDto response)
        {
            if (!entityId.HasValue)
            {
                foreach (var path in paths)
                {
                    response.UnresolvedPlaceholders.Add(path);
                    response.ResolutionErrors.Add($"Cannot resolve {path}: No entity ID provided");
                }
                return;
            }

            try
            {
                var entityType = GetEntityType(entityTypeName);
                if (entityType == null)
                {
                    foreach (var path in paths)
                    {
                        response.UnresolvedPlaceholders.Add(path);
                        response.ResolutionErrors.Add($"Entity type {entityTypeName} not found");
                    }
                    return;
                }

                foreach (var path in paths)
                {
                    try
                    {
                        var segments = path.Split('.');
                        if (segments.Length < 2)
                        {
                            response.UnresolvedPlaceholders.Add(path);
                            continue;
                        }

                        // Build Include expressions dynamically
                        var includeExpressions = new List<string>();
                        var currentPath = "";
                        for (int i = 0; i < segments.Length - 1; i++)
                        {
                            currentPath = string.IsNullOrEmpty(currentPath) 
                                ? segments[i] 
                                : $"{currentPath}.{segments[i]}";
                            includeExpressions.Add(currentPath);
                        }

                        // Fetch entity with all includes using reflection
                        var setMethod = typeof(DbContext).GetMethod(nameof(DbContext.Set), Array.Empty<Type>())!.MakeGenericMethod(entityType);
                        var dbSet = setMethod.Invoke(_dbContext, null);
                        
                        // Build query with includes
                        var query = (IQueryable)dbSet!;
                        foreach (var include in includeExpressions)
                        {
                            var includeMethod = typeof(EntityFrameworkQueryableExtensions)
                                .GetMethods()
                                .First(m => m.Name == "Include" && m.GetParameters().Length == 2 && m.GetParameters()[1].ParameterType == typeof(string))
                                .MakeGenericMethod(entityType);
                            query = (IQueryable)includeMethod.Invoke(null, new object[] { query, include })!;
                        }

                        // Execute FirstOrDefaultAsync
                        var firstOrDefaultMethod = typeof(EntityFrameworkQueryableExtensions)
                            .GetMethods()
                            .First(m => m.Name == "FirstOrDefaultAsync" && m.GetParameters().Length == 3) // query, predicate, cancellationToken
                            .MakeGenericMethod(entityType);

                        var parameter = Expression.Parameter(entityType, "e");
                        var idProperty = Expression.Property(parameter, "Id");
                        var constant = Expression.Constant(entityId.Value);
                        var equals = Expression.Equal(idProperty, constant);
                        var lambda = Expression.Lambda(equals, parameter);

                        var entityTask = (Task)firstOrDefaultMethod.Invoke(null, new object[] { query, lambda, default(System.Threading.CancellationToken) })!;
                        await entityTask.ConfigureAwait(false);
                        var entity = ((dynamic)entityTask).Result;
                        
                        if (entity == null)
                        {
                            response.UnresolvedPlaceholders.Add(path);
                            response.ResolutionErrors.Add($"Entity {entityTypeName} with ID {entityId.Value} not found");
                            continue;
                        }

                        // Navigate through property chain
                        object? current = entity;
                        for (int i = 0; i < segments.Length; i++)
                        {
                            if (current == null)
                            {
                                response.UnresolvedPlaceholders.Add(path);
                                response.ResolutionErrors.Add($"Null navigation at {segments[i]} in {path}");
                                break;
                            }

                            var prop = current.GetType().GetProperty(segments[i]);
                            if (prop == null)
                            {
                                response.UnresolvedPlaceholders.Add(path);
                                response.ResolutionErrors.Add($"Property {segments[i]} not found in {path}");
                                break;
                            }

                            current = prop.GetValue(current);
                        }

                        if (current != null)
                        {
                            response.ResolvedPlaceholders[path] = current.ToString() ?? "";
                        }
                        else if (!response.UnresolvedPlaceholders.Contains(path))
                        {
                            response.UnresolvedPlaceholders.Add(path);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning($"Error resolving multi-navigation {path}: {ex.Message}");
                        response.UnresolvedPlaceholders.Add(path);
                        response.ResolutionErrors.Add($"Error resolving {path}: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in ResolveMultiNavigationAsync: {ex.Message}");
                foreach (var path in paths)
                {
                    response.UnresolvedPlaceholders.Add(path);
                    response.ResolutionErrors.Add($"Error resolving {path}: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Resolve Layer 3: Aggregate operations (e.g., Town.Districts.Count)
        /// </summary>
        private async Task ResolveAggregatesAsync(
            string entityTypeName,
            int? entityId,
            List<string> paths,
            ResolvePlaceholdersResponseDto response)
        {
            if (!entityId.HasValue)
            {
                foreach (var path in paths)
                {
                    response.UnresolvedPlaceholders.Add(path);
                    response.ResolutionErrors.Add($"Cannot resolve {path}: No entity ID provided");
                }
                return;
            }

            try
            {
                var entityType = GetEntityType(entityTypeName);
                if (entityType == null)
                {
                    foreach (var path in paths)
                    {
                        response.UnresolvedPlaceholders.Add(path);
                        response.ResolutionErrors.Add($"Entity type {entityTypeName} not found");
                    }
                    return;
                }

                foreach (var path in paths)
                {
                    try
                    {
                        var segments = path.Split('.');
                        var operation = segments[^1]; // Last segment is the operation
                        var navigationPath = string.Join(".", segments.Take(segments.Length - 1));

                        // Build Include for navigation path using reflection
                        var setMethod = typeof(DbContext).GetMethod(nameof(DbContext.Set), Array.Empty<Type>())!.MakeGenericMethod(entityType);
                        var dbSet = setMethod.Invoke(_dbContext, null);
                        
                        var includeMethod = typeof(EntityFrameworkQueryableExtensions)
                            .GetMethods()
                            .First(m => m.Name == "Include" && m.GetParameters().Length == 2 && m.GetParameters()[1].ParameterType == typeof(string))
                            .MakeGenericMethod(entityType);
                        var query = (IQueryable)includeMethod.Invoke(null, new object[] { dbSet!, navigationPath })!;

                        // Execute FirstOrDefaultAsync
                        var firstOrDefaultMethod = typeof(System.Linq.Queryable)
                            .GetMethods()
                            .First(m => m.Name == "FirstOrDefault" && m.GetParameters().Length == 2)
                            .MakeGenericMethod(entityType);

                        var parameter = Expression.Parameter(entityType, "e");
                        var idProperty = Expression.Property(parameter, "Id");
                        var constant = Expression.Constant(entityId.Value);
                        var equals = Expression.Equal(idProperty, constant);
                        var lambda = Expression.Lambda(equals, parameter);

                        var entityTask = (Task)firstOrDefaultMethod.Invoke(null, new object[] { query, lambda })!;
                        await entityTask.ConfigureAwait(false);
                        var entity = ((dynamic)entityTask).Result;
                        
                        if (entity == null)
                        {
                            response.UnresolvedPlaceholders.Add(path);
                            continue;
                        }

                        // Navigate to collection
                        object? current = entity;
                        for (int i = 0; i < segments.Length - 1; i++)
                        {
                            if (current == null) break;
                            var prop = current.GetType().GetProperty(segments[i]);
                            if (prop == null) break;
                            current = prop.GetValue(current);
                        }

                        if (current == null)
                        {
                            response.UnresolvedPlaceholders.Add(path);
                            response.ResolutionErrors.Add($"Navigation failed for {path}");
                            continue;
                        }

                        // Apply operation
                        var result = operation.ToLower() switch
                        {
                            "count" => (current as System.Collections.IEnumerable)?.Cast<object>().Count().ToString() ?? "0",
                            "first" => (current as System.Collections.IEnumerable)?.Cast<object>().FirstOrDefault()?.ToString() ?? "",
                            "last" => (current as System.Collections.IEnumerable)?.Cast<object>().LastOrDefault()?.ToString() ?? "",
                            _ => null
                        };

                        if (result != null)
                        {
                            response.ResolvedPlaceholders[path] = result;
                        }
                        else
                        {
                            response.UnresolvedPlaceholders.Add(path);
                            response.ResolutionErrors.Add($"Unsupported operation: {operation}");
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning($"Error resolving aggregate {path}: {ex.Message}");
                        response.UnresolvedPlaceholders.Add(path);
                        response.ResolutionErrors.Add($"Error resolving {path}: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in ResolveAggregatesAsync: {ex.Message}");
                foreach (var path in paths)
                {
                    response.UnresolvedPlaceholders.Add(path);
                    response.ResolutionErrors.Add($"Error resolving {path}: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Get entity Type by name using reflection on DbContext properties
        /// </summary>
        private Type? GetEntityType(string entityTypeName)
        {
            try
            {
                // Find DbSet<T> property matching the entity name
                var dbSetProperties = _dbContext.GetType()
                    .GetProperties()
                    .Where(p => p.PropertyType.IsGenericType &&
                               p.PropertyType.GetGenericTypeDefinition() == typeof(DbSet<>));

                foreach (var prop in dbSetProperties)
                {
                    var entityType = prop.PropertyType.GetGenericArguments()[0];
                    if (entityType.Name.Equals(entityTypeName, StringComparison.OrdinalIgnoreCase))
                    {
                        return entityType;
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting entity type for {entityTypeName}: {ex.Message}");
                return null;
            }
        }
    }
}
