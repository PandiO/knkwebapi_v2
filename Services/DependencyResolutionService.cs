using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using knkwebapi_v2.Dtos;
using knkwebapi_v2.Models;
using knkwebapi_v2.Repositories;
using knkwebapi_v2.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace knkwebapi_v2.Services;

/// <summary>
/// Service for resolving multi-layer dependencies when building validation contexts.
/// Handles resolving dependency paths against a form context snapshot.
/// </summary>
public class DependencyResolutionService : IDependencyResolutionService
{
    private readonly IPathResolutionService _pathResolutionService;
    private readonly IFieldValidationRuleRepository _ruleRepository;
    private readonly IFormFieldRepository _fieldRepository;
    private readonly IFormConfigurationRepository _formConfigurationRepository;
    private readonly ILogger<DependencyResolutionService> _logger;

    public DependencyResolutionService(
        IPathResolutionService pathResolutionService,
        IFieldValidationRuleRepository ruleRepository,
        IFormFieldRepository fieldRepository,
        IFormConfigurationRepository formConfigurationRepository,
        ILogger<DependencyResolutionService> logger)
    {
        _pathResolutionService = pathResolutionService ?? throw new ArgumentNullException(nameof(pathResolutionService));
        _ruleRepository = ruleRepository ?? throw new ArgumentNullException(nameof(ruleRepository));
        _fieldRepository = fieldRepository ?? throw new ArgumentNullException(nameof(fieldRepository));
        _formConfigurationRepository = formConfigurationRepository ?? throw new ArgumentNullException(nameof(formConfigurationRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<DependencyResolutionResponse> ResolveDependenciesAsync(DependencyResolutionRequest request)
    {
        var response = new DependencyResolutionResponse { ResolvedAt = DateTime.UtcNow };

        if (request?.FieldIds == null || request.FieldIds.Length == 0)
        {
            _logger.LogDebug("Dependency resolution skipped: no field IDs provided.");
            return response;
        }

        var rules = await _ruleRepository.GetByFieldIdsAsync(request.FieldIds);
        var formContext = NormalizeFormContext(request.FormContextSnapshot);
        var dependencyFieldNames = new Dictionary<int, string>();

        _logger.LogDebug("Resolving dependencies for {RuleCount} rules", rules.Count());

        foreach (var rule in rules.Where(r => r.DependsOnFieldId.HasValue))
        {
            var dependencyPath = GetDependencyPath(rule);
            if (string.IsNullOrWhiteSpace(dependencyPath))
            {
                _logger.LogWarning("Dependency path missing for rule {RuleId}", rule.Id);
                response.Resolved[rule.Id] = new ResolvedDependency
                {
                    RuleId = rule.Id,
                    Status = "error",
                    Message = "No dependency path configured",
                    ErrorDetail = "DependencyPath is required for multi-layer resolution",
                    ResolvedAt = DateTime.UtcNow
                };
                continue;
            }

            var dependencyFieldName = await GetDependencyFieldNameAsync(rule, dependencyFieldNames);
            var resolved = await ResolveDependencyForRuleAsync(
                rule,
                dependencyPath,
                dependencyFieldName,
                formContext);

            response.Resolved[rule.Id] = resolved;
        }

        return response;
    }

    public async Task<ValidationIssueDto[]> CheckConfigurationHealthAsync(int formConfigurationId)
    {
        var config = await _formConfigurationRepository.GetByIdAsync(formConfigurationId);
        if (config == null)
        {
            return new[]
            {
                new ValidationIssueDto
                {
                    Severity = "Error",
                    Message = $"Form configuration with ID {formConfigurationId} not found"
                }
            };
        }

        // TODO: Phase 3 will add detailed dependency health checks.
        return Array.Empty<ValidationIssueDto>();
    }

    private static string? GetDependencyPath(FieldValidationRule rule)
    {
        if (!string.IsNullOrWhiteSpace(rule.DependencyPath))
        {
            return rule.DependencyPath;
        }

        return ExtractPathFromConfigJson(rule.ConfigJson);
    }

    private static string? ExtractPathFromConfigJson(string? configJson)
    {
        if (string.IsNullOrWhiteSpace(configJson))
        {
            return null;
        }

        try
        {
            using var doc = JsonDocument.Parse(configJson);
            if (doc.RootElement.ValueKind != JsonValueKind.Object)
            {
                return null;
            }

            if (doc.RootElement.TryGetProperty("dependencyPath", out var dependencyPath)
                && dependencyPath.ValueKind == JsonValueKind.String)
            {
                return dependencyPath.GetString();
            }
        }
        catch (JsonException)
        {
            // Ignore invalid JSON and fall back to null.
        }

        return null;
    }

    private async Task<string?> GetDependencyFieldNameAsync(
        FieldValidationRule rule,
        Dictionary<int, string> cache)
    {
        if (!rule.DependsOnFieldId.HasValue)
        {
            return null;
        }

        var fieldId = rule.DependsOnFieldId.Value;

        if (rule.DependsOnField?.FieldName != null)
        {
            cache[fieldId] = rule.DependsOnField.FieldName;
            return rule.DependsOnField.FieldName;
        }

        if (cache.TryGetValue(fieldId, out var cached))
        {
            return cached;
        }

        var field = await _fieldRepository.GetByIdAsync(fieldId);
        if (field?.FieldName == null)
        {
            return null;
        }

        cache[fieldId] = field.FieldName;
        return field.FieldName;
    }

    private async Task<ResolvedDependency> ResolveDependencyForRuleAsync(
        FieldValidationRule rule,
        string dependencyPath,
        string? dependencyFieldName,
        Dictionary<string, object?> formContext)
    {
        var resolved = new ResolvedDependency
        {
            RuleId = rule.Id,
            DependencyPath = dependencyPath,
            ResolvedAt = DateTime.UtcNow
        };

        var (rootKey, relativePath) = ParseDependencyPath(dependencyPath, dependencyFieldName);
        if (string.IsNullOrWhiteSpace(rootKey))
        {
            _logger.LogWarning("Dependency path missing root segment for rule {RuleId}", rule.Id);
            resolved.Status = "error";
            resolved.Message = "Dependency path could not be resolved.";
            resolved.ErrorDetail = "Missing root entity or field name.";
            return resolved;
        }

        var rootValue = TryGetContextValue(formContext, rootKey);
        if (rootValue == null && !string.IsNullOrWhiteSpace(dependencyFieldName)
            && !rootKey.Equals(dependencyFieldName, StringComparison.OrdinalIgnoreCase))
        {
            rootValue = TryGetContextValue(formContext, dependencyFieldName);
        }

        if (rootValue == null)
        {
            _logger.LogDebug("Dependency root value not found for rule {RuleId} and key {RootKey}",
                rule.Id, rootKey);
            resolved.Status = "pending";
            resolved.Message = $"Dependency field '{rootKey}' not yet filled.";
            return resolved;
        }

        if (string.IsNullOrWhiteSpace(relativePath))
        {
            resolved.Status = "success";
            resolved.ResolvedValue = rootValue;
            return resolved;
        }

        var validation = await _pathResolutionService.ValidatePathAsync(rootKey, relativePath);
        if (!validation.IsValid)
        {
            _logger.LogWarning("Dependency path validation failed for rule {RuleId}: {Message}",
                rule.Id, validation.ErrorMessage);
            resolved.Status = "error";
            resolved.Message = validation.ErrorMessage ?? "Dependency path is invalid.";
            resolved.ErrorDetail = validation.Suggestion;
            return resolved;
        }

        var resolvedValue = await _pathResolutionService.ResolvePathAsync(rootKey, relativePath, rootValue);
        if (resolvedValue == null)
        {
            _logger.LogDebug("Dependency path resolved to null for rule {RuleId}", rule.Id);
            resolved.Status = "pending";
            resolved.Message = $"Dependency path '{dependencyPath}' resolved to null.";
            return resolved;
        }

        resolved.Status = "success";
        resolved.ResolvedValue = resolvedValue;
        return resolved;
    }

    private static (string? rootKey, string? relativePath) ParseDependencyPath(
        string dependencyPath,
        string? dependencyFieldName)
    {
        var segments = dependencyPath
            .Split('.', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        if (segments.Length == 0)
        {
            return (dependencyFieldName, string.Empty);
        }

        if (segments.Length == 1)
        {
            if (!string.IsNullOrWhiteSpace(dependencyFieldName))
            {
                return (dependencyFieldName, segments[0]);
            }

            return (segments[0], string.Empty);
        }

        return (segments[0], string.Join('.', segments.Skip(1)));
    }

    private static object? TryGetContextValue(
        Dictionary<string, object?> formContext,
        string key)
    {
        if (formContext.TryGetValue(key, out var value))
        {
            return value;
        }

        var match = formContext.Keys.FirstOrDefault(k =>
            k.Equals(key, StringComparison.OrdinalIgnoreCase));

        return match != null ? formContext[match] : null;
    }

    private static Dictionary<string, object?> NormalizeFormContext(
        Dictionary<string, object?>? formContextSnapshot)
    {
        var normalized = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
        if (formContextSnapshot == null)
        {
            return normalized;
        }

        foreach (var kvp in formContextSnapshot)
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
            return NormalizeDictionary(dict);
        }

        if (value is IDictionary nonGeneric)
        {
            return NormalizeDictionary(nonGeneric);
        }

        if (value is IEnumerable enumerable && value is not string)
        {
            var list = new List<object?>();
            foreach (var item in enumerable)
            {
                list.Add(NormalizeValue(item));
            }

            return list;
        }

        return value;
    }

    private static Dictionary<string, object?> NormalizeDictionary(Dictionary<string, object?> dict)
    {
        var normalized = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
        foreach (var kvp in dict)
        {
            normalized[kvp.Key] = NormalizeValue(kvp.Value);
        }

        return normalized;
    }

    private static Dictionary<string, object?> NormalizeDictionary(IDictionary dict)
    {
        var normalized = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
        foreach (DictionaryEntry entry in dict)
        {
            var key = entry.Key?.ToString();
            if (string.IsNullOrWhiteSpace(key))
            {
                continue;
            }

            normalized[key] = NormalizeValue(entry.Value);
        }

        return normalized;
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
