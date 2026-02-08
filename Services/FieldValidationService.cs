using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using knkwebapi_v2.Dtos;
using knkwebapi_v2.Models;
using knkwebapi_v2.Repositories.Interfaces;
using knkwebapi_v2.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace knkwebapi_v2.Services;

/// <summary>
/// Service implementation for executing field validations with placeholder resolution.
/// Coordinates validation logic execution and placeholder variable resolution.
/// </summary>
public class FieldValidationService : IFieldValidationService
{
    private readonly IPlaceholderResolutionService _placeholderService;
    private readonly ILogger<FieldValidationService> _logger;

    public FieldValidationService(
        IPlaceholderResolutionService placeholderService,
        ILogger<FieldValidationService> logger)
    {
        _placeholderService = placeholderService ?? throw new ArgumentNullException(nameof(placeholderService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc/>
    public async Task<ValidationResultDto> ValidateFieldAsync(
        FieldValidationRule rule,
        object? fieldValue,
        object? dependencyFieldValue = null,
        Dictionary<string, string>? currentEntityPlaceholders = null,
        int? entityId = null)
    {
        if (rule == null)
        {
            throw new ArgumentNullException(nameof(rule));
        }

        _logger.LogInformation("Executing validation for rule {RuleId} (Type: {ValidationType})",
            rule.Id, rule.ValidationType);

        try
        {
            // Step 1: Resolve placeholders for this rule
            var placeholderResponse = await ResolvePlaceholdersForRuleAsync(rule, entityId, currentEntityPlaceholders);

            // Check if placeholder resolution had critical errors
            if (placeholderResponse.ResolutionErrors.Any(e => e.ErrorCode == "RuleNotFound" || e.ErrorCode == "EntityTypeNotFound"))
            {
                _logger.LogError("Critical placeholder resolution error for rule {RuleId}", rule.Id);
                return new ValidationResultDto
                {
                    IsValid = false,
                    IsBlocking = rule.IsBlocking,
                    Message = "Validation configuration error. Please contact administrator.",
                    Placeholders = new Dictionary<string, string>(),
                    Metadata = new ValidationMetadataDto
                    {
                        ValidationType = rule.ValidationType,
                        ExecutedAt = DateTime.UtcNow.ToString("o")
                    }
                };
            }

            var placeholders = placeholderResponse.ResolvedPlaceholders;

            // Step 2: Dispatch to type-specific validation method
            ValidationResultDto result = rule.ValidationType switch
            {
                "LocationInsideRegion" => await ValidateLocationInsideRegionAsync(rule, fieldValue, dependencyFieldValue, placeholders),
                "RegionContainment" => await ValidateRegionContainmentAsync(rule, fieldValue, dependencyFieldValue, placeholders),
                "ConditionalRequired" => await ValidateConditionalRequiredAsync(rule, fieldValue, dependencyFieldValue, placeholders),
                _ => new ValidationResultDto
                {
                    IsValid = false,
                    IsBlocking = false,
                    Message = $"Unknown validation type: {rule.ValidationType}",
                    Placeholders = placeholders,
                    Metadata = new ValidationMetadataDto
                    {
                        ValidationType = rule.ValidationType,
                        ExecutedAt = DateTime.UtcNow.ToString("o")
                    }
                }
            };

            // Step 3: Add metadata
            result.Metadata = new ValidationMetadataDto
            {
                ValidationType = rule.ValidationType,
                ExecutedAt = DateTime.UtcNow.ToString("o"),
                DependencyValue = dependencyFieldValue
            };

            _logger.LogInformation("Validation complete for rule {RuleId}. Result: {IsValid}",
                rule.Id, result.IsValid);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during validation for rule {RuleId}", rule.Id);
            return new ValidationResultDto
            {
                IsValid = false,
                IsBlocking = rule.IsBlocking,
                Message = "Validation error occurred. Please try again.",
                Placeholders = new Dictionary<string, string>(),
                Metadata = new ValidationMetadataDto
                {
                    ValidationType = rule.ValidationType,
                    ExecutedAt = DateTime.UtcNow.ToString("o")
                }
            };
        }
    }

    /// <inheritdoc/>
    public async Task<PlaceholderResolutionResponse> ResolvePlaceholdersForRuleAsync(
        FieldValidationRule rule,
        int? entityId = null,
        Dictionary<string, string>? currentEntityPlaceholders = null)
    {
        if (rule == null)
        {
            throw new ArgumentNullException(nameof(rule));
        }

        _logger.LogDebug("Resolving placeholders for rule {RuleId}", rule.Id);

        // Build request
        var request = new PlaceholderResolutionRequest
        {
            FieldValidationRuleId = rule.Id,
            EntityId = entityId,
            CurrentEntityPlaceholders = currentEntityPlaceholders ?? new Dictionary<string, string>()
        };

        // Attempt to infer entity type from rule's FormFieldId
        // (This requires FormField -> FormConfiguration -> EntityType lookup)
        // For now, we'll rely on explicit entityTypeName in request or from rule's ConfigJson
        if (!string.IsNullOrWhiteSpace(rule.ConfigJson))
        {
            try
            {
                var config = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(rule.ConfigJson);
                if (config != null && config.TryGetValue("entityTypeName", out var entityTypeValue))
                {
                    request.EntityTypeName = entityTypeValue.GetString();
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to parse entityTypeName from rule {RuleId} ConfigJson", rule.Id);
            }
        }

        // Resolve all layers
        return await _placeholderService.ResolveAllLayersAsync(request);
    }

    /// <inheritdoc/>
    public async Task<ValidationResultDto> ValidateLocationInsideRegionAsync(
        FieldValidationRule rule,
        object? fieldValue,
        object? dependencyFieldValue,
        Dictionary<string, string> placeholders)
    {
        _logger.LogDebug("Executing LocationInsideRegion validation for rule {RuleId}", rule.Id);

        // PLACEHOLDER IMPLEMENTATION FOR PHASE 2
        // Full implementation will be added in later phase when IRegionService is available
        // For now, we demonstrate the placeholder resolution infrastructure

        // Step 1: Add computed placeholders
        var combinedPlaceholders = new Dictionary<string, string>(placeholders);

        // Add coordinates placeholder if Location object is provided
        if (fieldValue is Location location)
        {
            combinedPlaceholders["coordinates"] = $"({location.X:F2}, {location.Y:F2}, {location.Z:F2})";
        }
        else if (fieldValue != null)
        {
            _logger.LogWarning("Expected Location object but got {Type}", fieldValue.GetType().Name);
        }

        // Parse ConfigJson for additional context
        string? regionPropertyPath = null;
        if (!string.IsNullOrWhiteSpace(rule.ConfigJson))
        {
            try
            {
                var config = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(rule.ConfigJson);
                if (config != null && config.TryGetValue("regionPropertyPath", out var pathValue))
                {
                    regionPropertyPath = pathValue.GetString();
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to parse ConfigJson for rule {RuleId}", rule.Id);
            }
        }

        // Step 2: Placeholder validation logic (actual region check would happen here)
        // For now, we'll return a success result to demonstrate the flow
        bool isValid = true; // TODO: Replace with actual IRegionService.CheckLocationInsideRegion() call

        return new ValidationResultDto
        {
            IsValid = isValid,
            IsBlocking = rule.IsBlocking,
            Message = isValid ? (rule.SuccessMessage ?? "Location is valid") : rule.ErrorMessage,
            Placeholders = combinedPlaceholders
        };
    }

    /// <inheritdoc/>
    public async Task<ValidationResultDto> ValidateRegionContainmentAsync(
        FieldValidationRule rule,
        object? fieldValue,
        object? dependencyFieldValue,
        Dictionary<string, string> placeholders)
    {
        _logger.LogDebug("Executing RegionContainment validation for rule {RuleId}", rule.Id);

        // PLACEHOLDER IMPLEMENTATION FOR PHASE 2
        // Full implementation will be added in later phase when IRegionService is available

        var combinedPlaceholders = new Dictionary<string, string>(placeholders);

        // Add regionName placeholder if fieldValue is a string (region ID)
        if (fieldValue is string regionId)
        {
            combinedPlaceholders["regionName"] = regionId;
        }

        // Placeholder validation logic
        bool isValid = true; // TODO: Replace with actual IRegionService.CheckRegionContainment() call
        int violationCount = 0; // TODO: Get actual violation count from region check

        if (!isValid)
        {
            combinedPlaceholders["violationCount"] = violationCount.ToString();
        }

        return new ValidationResultDto
        {
            IsValid = isValid,
            IsBlocking = rule.IsBlocking,
            Message = isValid ? (rule.SuccessMessage ?? "Region is valid") : rule.ErrorMessage,
            Placeholders = combinedPlaceholders
        };
    }

    /// <inheritdoc/>
    public async Task<ValidationResultDto> ValidateConditionalRequiredAsync(
        FieldValidationRule rule,
        object? fieldValue,
        object? dependencyFieldValue,
        Dictionary<string, string> placeholders)
    {
        _logger.LogDebug("Executing ConditionalRequired validation for rule {RuleId}", rule.Id);

        var combinedPlaceholders = new Dictionary<string, string>(placeholders);

        // Parse ConfigJson for condition
        string? operatorValue = null;
        string? conditionValue = null;

        if (!string.IsNullOrWhiteSpace(rule.ConfigJson))
        {
            try
            {
                var config = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(rule.ConfigJson);
                if (config != null)
                {
                    if (config.TryGetValue("operator", out var opValue))
                    {
                        operatorValue = opValue.GetString();
                    }
                    if (config.TryGetValue("value", out var condValue))
                    {
                        conditionValue = condValue.GetString();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to parse ConfigJson for rule {RuleId}", rule.Id);
            }
        }

        // Step 1: Evaluate condition
        bool conditionMet = false;
        string? depValueStr = dependencyFieldValue?.ToString();

        if (!string.IsNullOrWhiteSpace(operatorValue) && conditionValue != null)
        {
            conditionMet = operatorValue.ToLower() switch
            {
                "equals" => string.Equals(depValueStr, conditionValue, StringComparison.OrdinalIgnoreCase),
                "notequals" => !string.Equals(depValueStr, conditionValue, StringComparison.OrdinalIgnoreCase),
                "contains" => depValueStr?.Contains(conditionValue, StringComparison.OrdinalIgnoreCase) ?? false,
                "startswith" => depValueStr?.StartsWith(conditionValue, StringComparison.OrdinalIgnoreCase) ?? false,
                "endswith" => depValueStr?.EndsWith(conditionValue, StringComparison.OrdinalIgnoreCase) ?? false,
                _ => false
            };
        }

        // Step 2: Check if field is empty (if condition is met)
        bool fieldIsEmpty = fieldValue == null ||
                           (fieldValue is string strValue && string.IsNullOrWhiteSpace(strValue));

        bool isValid = !conditionMet || !fieldIsEmpty;

        // Add dependency value to placeholders for error message
        if (!isValid && depValueStr != null)
        {
            combinedPlaceholders["dependencyValue"] = depValueStr;
        }

        return new ValidationResultDto
        {
            IsValid = isValid,
            IsBlocking = rule.IsBlocking,
            Message = isValid ? (rule.SuccessMessage ?? "Field is valid") : rule.ErrorMessage,
            Placeholders = combinedPlaceholders
        };
    }
}
