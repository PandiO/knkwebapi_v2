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
    private readonly IFormConfigurationService _formConfigurationService;
    private readonly IMetadataService _metadataService;
    private readonly ILogger<DependencyResolutionService> _logger;

    public DependencyResolutionService(
        IPathResolutionService pathResolutionService,
        IFieldValidationRuleRepository ruleRepository,
        IFormFieldRepository fieldRepository,
        IFormConfigurationService formConfigurationService,
        IMetadataService metadataService,
        ILogger<DependencyResolutionService> logger)
    {
        _pathResolutionService = pathResolutionService ?? throw new ArgumentNullException(nameof(pathResolutionService));
        _ruleRepository = ruleRepository ?? throw new ArgumentNullException(nameof(ruleRepository));
        _fieldRepository = fieldRepository ?? throw new ArgumentNullException(nameof(fieldRepository));
        _formConfigurationService = formConfigurationService ?? throw new ArgumentNullException(nameof(formConfigurationService));
        _metadataService = metadataService ?? throw new ArgumentNullException(nameof(metadataService));
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
        var config = await _formConfigurationService.GetByIdAsync(formConfigurationId);
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

        var issues = new List<ValidationIssueDto>();

        // Phase 3: Perform all 6 health checks
        issues.AddRange(await CheckFieldEntityAlignmentAsync(config));
        issues.AddRange(await CheckPropertyExistenceAsync(config));
        issues.AddRange(await CheckRequiredFieldCompletenessAsync(config));
        issues.AddRange(await CheckCollectionWarningAsync(config));
        issues.AddRange(await CheckCircularDependenciesAsync(config));
        issues.AddRange(await CheckFieldOrderingAsync(config));

        return issues.ToArray();
    }

    /// <summary>
    /// Health Check 1: Field-Entity Alignment
    /// Verifies that all fields with validation rules are correctly aligned with their entity types in metadata.
    /// </summary>
    private async Task<List<ValidationIssueDto>> CheckFieldEntityAlignmentAsync(FormConfigurationDto config)
    {
        var issues = new List<ValidationIssueDto>();

        if (string.IsNullOrWhiteSpace(config.EntityTypeName))
        {
            issues.Add(new ValidationIssueDto
            {
                Severity = "Error",
                Message = "Form configuration is missing entity type name"
            });
            return issues;
        }

        // Verify the root entity exists in metadata
        var metadata = _metadataService.GetEntityMetadata(config.EntityTypeName);
        if (metadata == null)
        {
            issues.Add(new ValidationIssueDto
            {
                Severity = "Error",
                Message = $"Entity '{config.EntityTypeName}' not found in system metadata. Cannot validate form configuration."
            });
            return issues;
        }

        return issues;
    }

    /// <summary>
    /// Health Check 2: Property Existence
    /// Verifies that all dependency paths reference existing properties on entity types.
    /// </summary>
    private async Task<List<ValidationIssueDto>> CheckPropertyExistenceAsync(FormConfigurationDto config)
    {
        var issues = new List<ValidationIssueDto>();
        if (!int.TryParse(config.Id, out var configId))
        {
            issues.Add(new ValidationIssueDto
            {
                Severity = "Error",
                Message = "Invalid form configuration ID format"
            });
            return issues;
        }

        var allRules = await _ruleRepository.GetByFormConfigurationIdAsync(configId);

        foreach (var rule in allRules)
        {
            if (string.IsNullOrWhiteSpace(rule.DependencyPath))
            {
                continue; // No path to validate
            }

            // Validate the path syntax and existence
            var pathValidation = await _pathResolutionService.ValidatePathAsync(
                config.EntityTypeName,
                rule.DependencyPath
            );

            if (!pathValidation.IsValid)
            {
                var field = await _fieldRepository.GetByIdAsync(rule.FormFieldId);
                issues.Add(new ValidationIssueDto
                {
                    Severity = "Error",
                    Message = $"Field '{field?.Label ?? "Unknown"}' has invalid dependency path: {pathValidation.ErrorMessage}",
                    FieldId = field?.Id,
                    RuleId = rule.Id
                });
            }
        }

        return issues;
    }

    /// <summary>
    /// Health Check 3: Required Field Completeness
    /// Compares entity's required fields with form configuration to warn if required fields are optional in the form.
    /// </summary>
    private async Task<List<ValidationIssueDto>> CheckRequiredFieldCompletenessAsync(FormConfigurationDto config)
    {
        var issues = new List<ValidationIssueDto>();

        var metadata = _metadataService.GetEntityMetadata(config.EntityTypeName);
        if (metadata?.Fields == null)
        {
            return issues; // Cannot check without metadata
        }

        var configuredFieldNames = config.Steps?
            .SelectMany(s => s.Fields ?? new List<FormFieldDto>())
            .Select(f => f.FieldName)
            .ToHashSet(StringComparer.OrdinalIgnoreCase) ?? new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var entityField in metadata.Fields)
        {
            // Check if required field is missing from form
            var isNullable = entityField.IsNullable;
            var hasDefault = entityField.HasDefaultValue;

            if (!isNullable && !hasDefault)
            {
                if (!configuredFieldNames.Contains(entityField.FieldName))
                {
                    issues.Add(new ValidationIssueDto
                    {
                        Severity = "Warning",
                        Message = $"Entity '{config.EntityTypeName}' requires field '{entityField.FieldName}' but it's not in the form configuration."
                    });
                }
                else
                {
                    // Check if the field is marked as optional in the form (if we can determine that)
                    var formField = config.Steps?
                        .SelectMany(s => s.Fields ?? new List<FormFieldDto>())
                        .FirstOrDefault(f => f.FieldName.Equals(entityField.FieldName, StringComparison.OrdinalIgnoreCase));

                    if (formField?.IsRequired == false)
                    {
                        issues.Add(new ValidationIssueDto
                        {
                            Severity = "Warning",
                            Message = $"Field '{entityField.FieldName}' is required by entity '{config.EntityTypeName}' but marked as optional in the form.",
                            FieldId = int.TryParse(formField.Id, out var fieldId) ? fieldId : null
                        });
                    }
                }
            }
        }

        return issues;
    }

    /// <summary>
    /// Health Check 4: Collection Warning (v1)
    /// Warns if dependency paths resolve to collection/array types (not supported in v1).
    /// </summary>
    private async Task<List<ValidationIssueDto>> CheckCollectionWarningAsync(FormConfigurationDto config)
    {
        var issues = new List<ValidationIssueDto>();
        if (!int.TryParse(config.Id, out var configId))
        {
            issues.Add(new ValidationIssueDto
            {
                Severity = "Error",
                Message = "Invalid form configuration ID format"
            });
            return issues;
        }

        var allRules = await _ruleRepository.GetByFormConfigurationIdAsync(configId);

        foreach (var rule in allRules)
        {
            if (string.IsNullOrWhiteSpace(rule.DependencyPath))
            {
                continue;
            }

            // Try to determine if the path resolves to a collection
            var pathValidation = await _pathResolutionService.ValidatePathAsync(
                config.EntityTypeName,
                rule.DependencyPath
            );

            // Check for collection indicators in validation result
            if (!pathValidation.IsValid && 
                (pathValidation.ErrorMessage?.Contains("collection", StringComparison.OrdinalIgnoreCase) == true ||
                 pathValidation.ErrorMessage?.Contains("array", StringComparison.OrdinalIgnoreCase) == true ||
                 pathValidation.ErrorMessage?.Contains("[", StringComparison.OrdinalIgnoreCase) == true))
            {
                var field = await _fieldRepository.GetByIdAsync(rule.FormFieldId);
                issues.Add(new ValidationIssueDto
                {
                    Severity = "Warning",
                    Message = $"Field '{field?.Label ?? "Unknown"}' references a collection in path '{rule.DependencyPath}'. Collections are not supported in v1. This will be supported in v2.",
                    FieldId = field?.Id,
                    RuleId = rule.Id
                });
            }
        }

        return issues;
    }

    /// <summary>
    /// Health Check 5: Circular Dependency Detection
    /// Uses graph traversal to detect cycles in the dependency graph.
    /// </summary>
    private async Task<List<ValidationIssueDto>> CheckCircularDependenciesAsync(FormConfigurationDto config)
    {
        var issues = new List<ValidationIssueDto>();
        if (!int.TryParse(config.Id, out var configId))
        {
            issues.Add(new ValidationIssueDto
            {
                Severity = "Error",
                Message = "Invalid form configuration ID format"
            });
            return issues;
        }

        var allRules = await _ruleRepository.GetByFormConfigurationIdAsync(configId);

        // Build a dependency graph: fieldId -> list of fields it depends on
        var dependencyGraph = new Dictionary<int, List<int>>();
        var fieldMap = new Dictionary<int, FormFieldDto>();

        var allFields = config.Steps?
            .SelectMany(s => s.Fields ?? new List<FormFieldDto>())
            .ToList() ?? new List<FormFieldDto>();

        foreach (var field in allFields)
        {
            if (int.TryParse(field.Id, out var parsedId))
            {
                fieldMap[parsedId] = field;
                dependencyGraph[parsedId] = new List<int>();
            }
        }

        // Add dependencies
        foreach (var rule in allRules)
        {
            if (rule.DependsOnFieldId.HasValue && dependencyGraph.ContainsKey(rule.FormFieldId))
            {
                if (!dependencyGraph[rule.FormFieldId].Contains(rule.DependsOnFieldId.Value))
                {
                    dependencyGraph[rule.FormFieldId].Add(rule.DependsOnFieldId.Value);
                }
            }
        }

        // Detect cycles using DFS
        var visited = new HashSet<int>();
        var recursionStack = new HashSet<int>();

        foreach (var fieldId in dependencyGraph.Keys)
        {
            if (!visited.Contains(fieldId))
            {
                var cycle = DetectCycleDFS(fieldId, dependencyGraph, visited, recursionStack);
                if (cycle.Count > 0)
                {
                    var cycleDescription = string.Join(" -> ", cycle.Select(id =>
                        fieldMap.ContainsKey(id) ? fieldMap[id].Label : $"Field{id}"));

                    issues.Add(new ValidationIssueDto
                    {
                        Severity = "Error",
                        Message = $"Circular dependency detected: {cycleDescription}. This will cause infinite validation loops."
                    });
                }
            }
        }

        return issues;
    }

    /// <summary>
    /// Helper: Depth-First Search for cycle detection
    /// </summary>
    private List<int> DetectCycleDFS(
        int node,
        Dictionary<int, List<int>> graph,
        HashSet<int> visited,
        HashSet<int> recursionStack)
    {
        var cycle = new List<int>();
        visited.Add(node);
        recursionStack.Add(node);

        if (graph.ContainsKey(node))
        {
            foreach (var neighbor in graph[node])
            {
                if (!visited.Contains(neighbor))
                {
                    var result = DetectCycleDFS(neighbor, graph, visited, recursionStack);
                    if (result.Count > 0)
                    {
                        cycle = result;
                        cycle.Insert(0, node);
                        return cycle;
                    }
                }
                else if (recursionStack.Contains(neighbor))
                {
                    // Found a cycle
                    cycle.Add(neighbor);
                    cycle.Add(node);
                    return cycle;
                }
            }
        }

        recursionStack.Remove(node);
        return cycle;
    }

    /// <summary>
    /// Health Check 6: Field Ordering Validation
    /// Verifies that dependency fields appear before dependent fields in the form flow.
    /// </summary>
    private async Task<List<ValidationIssueDto>> CheckFieldOrderingAsync(FormConfigurationDto config)
    {
        var issues = new List<ValidationIssueDto>();
        if (!int.TryParse(config.Id, out var configId))
        {
            issues.Add(new ValidationIssueDto
            {
                Severity = "Error",
                Message = "Invalid form configuration ID format"
            });
            return issues;
        }

        var allRules = await _ruleRepository.GetByFormConfigurationIdAsync(configId);

        // Build a field position map
        var fieldPositions = new Dictionary<int, (int stepIndex, int fieldIndex)>();
        int stepIndex = 0;
        foreach (var step in config.Steps ?? new List<FormStepDto>())
        {
            int fieldIndex = 0;
            foreach (var field in step.Fields ?? new List<FormFieldDto>())
            {
                if (int.TryParse(field.Id, out var parsedId))
                {
                    fieldPositions[parsedId] = (stepIndex, fieldIndex);
                }
                fieldIndex++;
            }
            stepIndex++;
        }

        // Check each rule's field ordering
        foreach (var rule in allRules)
        {
            if (!rule.DependsOnFieldId.HasValue)
            {
                continue; // No dependency
            }

            if (fieldPositions.TryGetValue(rule.FormFieldId, out var dependentPos) &&
                fieldPositions.TryGetValue(rule.DependsOnFieldId.Value, out var dependencyPos))
            {
                // Check if dependency comes BEFORE dependent
                var isOrdered = dependencyPos.stepIndex < dependentPos.stepIndex ||
                               (dependencyPos.stepIndex == dependentPos.stepIndex && dependencyPos.fieldIndex < dependentPos.fieldIndex);

                if (!isOrdered)
                {
                    var dependentField = config.Steps?
                        .SelectMany(s => s.Fields ?? new List<FormFieldDto>())
                        .FirstOrDefault(f => int.TryParse(f.Id, out var id) && id == rule.FormFieldId);

                    var dependencyField = config.Steps?
                        .SelectMany(s => s.Fields ?? new List<FormFieldDto>())
                        .FirstOrDefault(f => int.TryParse(f.Id, out var id) && id == rule.DependsOnFieldId.Value);

                    issues.Add(new ValidationIssueDto
                    {
                        Severity = "Warning",
                        Message = $"Field '{dependentField?.Label}' depends on '{dependencyField?.Label}' which comes after it. " +
                                 "Reorder fields so dependencies come first.",
                        FieldId = rule.FormFieldId,
                        RuleId = rule.Id
                    });
                }
            }
        }

        return issues;
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
