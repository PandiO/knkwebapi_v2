using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using knkwebapi_v2.Dtos;
using knkwebapi_v2.Services.Interfaces;
using Microsoft.Extensions.Caching.Memory;

namespace knkwebapi_v2.Services;

/// <summary>
/// Caches dependency resolution results to avoid repeat work for identical requests.
/// </summary>
public class CachedDependencyResolutionService : IDependencyResolutionService
{
    private static readonly TimeSpan CacheTtl = TimeSpan.FromMinutes(5);
    private readonly IMemoryCache _cache;
    private readonly IDependencyResolutionService _innerService;

    public CachedDependencyResolutionService(
        IMemoryCache cache,
        IDependencyResolutionService innerService)
    {
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        _innerService = innerService ?? throw new ArgumentNullException(nameof(innerService));
    }

    public async Task<DependencyResolutionResponse> ResolveDependenciesAsync(DependencyResolutionRequest request)
    {
        if (request == null)
        {
            return new DependencyResolutionResponse();
        }

        var cacheKey = BuildCacheKey(request);
        if (_cache.TryGetValue(cacheKey, out DependencyResolutionResponse? cached)
            && cached != null)
        {
            return cached;
        }

        var result = await _innerService.ResolveDependenciesAsync(request);
        _cache.Set(cacheKey, result, CacheTtl);
        return result;
    }

    public Task<ValidationIssueDto[]> CheckConfigurationHealthAsync(int formConfigurationId)
    {
        return _innerService.CheckConfigurationHealthAsync(formConfigurationId);
    }

    private static string BuildCacheKey(DependencyResolutionRequest request)
    {
        var fieldIds = request.FieldIds ?? Array.Empty<int>();
        var fieldKey = string.Join(',', fieldIds.OrderBy(id => id));
        var contextHash = ComputeContextHash(request.FormContextSnapshot);
        var configId = request.FormConfigurationId?.ToString() ?? "none";
        return $"dependency-resolution:{configId}:{fieldKey}:{contextHash}";
    }

    private static string ComputeContextHash(Dictionary<string, object?>? context)
    {
        if (context == null || context.Count == 0)
        {
            return "empty";
        }

        var normalized = NormalizeContext(context);
        var json = JsonSerializer.Serialize(normalized);
        using var sha = SHA256.Create();
        var hash = sha.ComputeHash(Encoding.UTF8.GetBytes(json));
        return Convert.ToHexString(hash);
    }

    private static SortedDictionary<string, object?> NormalizeContext(Dictionary<string, object?> context)
    {
        var normalized = new SortedDictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
        foreach (var kvp in context)
        {
            normalized[kvp.Key] = NormalizeValue(kvp.Value);
        }

        return normalized;
    }

    private static object? NormalizeValue(object? value)
    {
        if (value == null)
        {
            return null;
        }

        if (value is JsonElement element)
        {
            return ConvertJsonElement(element);
        }

        if (value is Dictionary<string, object?> dict)
        {
            return NormalizeContext(dict);
        }

        if (value is IDictionary<string, object?> genericDict)
        {
            return NormalizeContext(new Dictionary<string, object?>(genericDict));
        }

        if (value is IEnumerable<object?> list)
        {
            return list.Select(NormalizeValue).ToList();
        }

        if (value is System.Collections.IEnumerable enumerable && value is not string)
        {
            var normalized = new List<object?>();
            foreach (var item in enumerable)
            {
                normalized.Add(NormalizeValue(item));
            }

            return normalized;
        }

        return value;
    }

    private static object? ConvertJsonElement(JsonElement element)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.Object:
                var obj = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
                foreach (var prop in element.EnumerateObject())
                {
                    obj[prop.Name] = ConvertJsonElement(prop.Value);
                }
                return obj;
            case JsonValueKind.Array:
                var list = new List<object?>();
                foreach (var item in element.EnumerateArray())
                {
                    list.Add(ConvertJsonElement(item));
                }
                return list;
            case JsonValueKind.String:
                return element.GetString();
            case JsonValueKind.Number:
                if (element.TryGetInt64(out var longValue))
                {
                    return longValue;
                }
                if (element.TryGetDouble(out var doubleValue))
                {
                    return doubleValue;
                }
                return null;
            case JsonValueKind.True:
            case JsonValueKind.False:
                return element.GetBoolean();
            case JsonValueKind.Null:
            case JsonValueKind.Undefined:
                return null;
            default:
                return null;
        }
    }
}
