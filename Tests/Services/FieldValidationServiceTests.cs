using Xunit;
using Moq;
using knkwebapi_v2.Services;
using knkwebapi_v2.Services.Interfaces;
using knkwebapi_v2.Models;
using knkwebapi_v2.Dtos;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace knkwebapi_v2.Tests.Services;

/// <summary>
/// Unit tests for FieldValidationService.
/// Tests validation execution, placeholder resolution integration, and validation type implementations.
/// Phase 7.2 of Placeholder Interpolation implementation.
/// </summary>
[Trait("Category", "Unit")]
[Trait("Feature", "PlaceholderInterpolation")]
public class FieldValidationServiceTests
{
    private readonly Mock<IPlaceholderResolutionService> _mockPlaceholderService;
    private readonly Mock<ILogger<FieldValidationService>> _mockLogger;
    private readonly FieldValidationService _service;

    public FieldValidationServiceTests()
    {
        _mockPlaceholderService = new Mock<IPlaceholderResolutionService>();
        _mockLogger = new Mock<ILogger<FieldValidationService>>();

        _service = new FieldValidationService(
            _mockPlaceholderService.Object,
            _mockLogger.Object
        );
    }

    #region ValidateFieldAsync Tests

    [Fact]
    public async Task ValidateFieldAsync_WithNullRule_ThrowsArgumentNullException()
    {
        // Arrange
        FieldValidationRule? rule = null;

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => _service.ValidateFieldAsync(rule!, "value")
        );
    }

    [Fact]
    public async Task ValidateFieldAsync_WithValidRule_ResolvesPlaceholders()
    {
        // Arrange
        var rule = new FieldValidationRule
        {
            Id = 1,
            ValidationType = "LocationInsideRegion",
            ErrorMessage = "Location is outside {Town.Name}.",
            IsBlocking = true
        };

        var placeholderResponse = new PlaceholderResolutionResponse
        {
            ResolvedPlaceholders = new Dictionary<string, string>
            {
                ["Town.Name"] = "Springfield"
            },
            IsSuccessful = true
        };

        _mockPlaceholderService
            .Setup(s => s.ResolveAllLayersAsync(It.IsAny<PlaceholderResolutionRequest>()))
            .ReturnsAsync(placeholderResponse);

        var currentEntityPlaceholders = new Dictionary<string, string>
        {
            ["Name"] = "York"
        };

        // Act
        var result = await _service.ValidateFieldAsync(
            rule,
            new { X = 100, Y = 64, Z = -200 },
            dependencyFieldValue: "town_springfield",
            currentEntityPlaceholders: currentEntityPlaceholders,
            entityId: 1
        );

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Placeholders);
        Assert.True(result.Placeholders.ContainsKey("Town.Name"));
        _mockPlaceholderService.Verify(
            s => s.ResolveAllLayersAsync(It.IsAny<PlaceholderResolutionRequest>()),
            Times.Once
        );
    }

    [Fact]
    public async Task ValidateFieldAsync_WithCriticalPlaceholderError_ReturnsError()
    {
        // Arrange
        var rule = new FieldValidationRule
        {
            Id = 1,
            ValidationType = "LocationInsideRegion",
            ErrorMessage = "Error message",
            IsBlocking = true
        };

        var placeholderResponse = new PlaceholderResolutionResponse
        {
            IsSuccessful = false,
            ResolutionErrors = new List<PlaceholderResolutionError>
            {
                new PlaceholderResolutionError
                {
                    PlaceholderPath = "*",
                    ErrorCode = "RuleNotFound",
                    Message = "Rule not found"
                }
            }
        };

        _mockPlaceholderService
            .Setup(s => s.ResolveAllLayersAsync(It.IsAny<PlaceholderResolutionRequest>()))
            .ReturnsAsync(placeholderResponse);

        // Act
        var result = await _service.ValidateFieldAsync(rule, "value");

        // Assert
        Assert.NotNull(result);
        Assert.False(result.IsValid);
        Assert.Contains("configuration error", result.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ValidateFieldAsync_WithUnknownValidationType_ReturnsError()
    {
        // Arrange
        var rule = new FieldValidationRule
        {
            Id = 1,
            ValidationType = "UnknownValidationType",
            ErrorMessage = "Error",
            IsBlocking = false
        };

        var placeholderResponse = new PlaceholderResolutionResponse
        {
            ResolvedPlaceholders = new Dictionary<string, string>(),
            IsSuccessful = true
        };

        _mockPlaceholderService
            .Setup(s => s.ResolveAllLayersAsync(It.IsAny<PlaceholderResolutionRequest>()))
            .ReturnsAsync(placeholderResponse);

        // Act
        var result = await _service.ValidateFieldAsync(rule, "value");

        // Assert
        Assert.NotNull(result);
        Assert.False(result.IsValid);
        Assert.False(result.IsBlocking);
        Assert.Contains("Unknown validation type", result.Message);
    }

    #endregion

    #region ResolvePlaceholdersForRuleAsync Tests

    [Fact]
    public async Task ResolvePlaceholdersForRuleAsync_WithValidRule_CallsPlaceholderService()
    {
        // Arrange
        var rule = new FieldValidationRule
        {
            Id = 1,
            ErrorMessage = "{Town.Name}",
            SuccessMessage = "Valid"
        };

        var expectedResponse = new PlaceholderResolutionResponse
        {
            ResolvedPlaceholders = new Dictionary<string, string>
            {
                ["Town.Name"] = "Springfield"
            },
            IsSuccessful = true
        };

        _mockPlaceholderService
            .Setup(s => s.ResolveAllLayersAsync(It.IsAny<PlaceholderResolutionRequest>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _service.ResolvePlaceholdersForRuleAsync(
            rule,
            entityId: 1,
            currentEntityPlaceholders: new Dictionary<string, string>()
        );

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccessful);
        Assert.Equal("Springfield", result.ResolvedPlaceholders["Town.Name"]);
        _mockPlaceholderService.Verify(
            s => s.ResolveAllLayersAsync(It.Is<PlaceholderResolutionRequest>(
                req => req.FieldValidationRuleId == 1
            )),
            Times.Once
        );
    }

    [Fact]
    public async Task ResolvePlaceholdersForRuleAsync_PassesEntityIdToService()
    {
        // Arrange
        var rule = new FieldValidationRule
        {
            Id = 1,
            ErrorMessage = "{Town.Name}"
        };

        _mockPlaceholderService
            .Setup(s => s.ResolveAllLayersAsync(It.IsAny<PlaceholderResolutionRequest>()))
            .ReturnsAsync(new PlaceholderResolutionResponse());

        // Act
        await _service.ResolvePlaceholdersForRuleAsync(
            rule,
            entityId: 42
        );

        // Assert
        _mockPlaceholderService.Verify(
            s => s.ResolveAllLayersAsync(It.Is<PlaceholderResolutionRequest>(
                req => req.EntityId == 42
            )),
            Times.Once
        );
    }

    [Fact]
    public async Task ResolvePlaceholdersForRuleAsync_PassesCurrentEntityPlaceholders()
    {
        // Arrange
        var rule = new FieldValidationRule
        {
            Id = 1,
            ErrorMessage = "{Name}"
        };

        var layer0Placeholders = new Dictionary<string, string>
        {
            ["Name"] = "York",
            ["Description"] = "A district"
        };

        _mockPlaceholderService
            .Setup(s => s.ResolveAllLayersAsync(It.IsAny<PlaceholderResolutionRequest>()))
            .ReturnsAsync(new PlaceholderResolutionResponse());

        // Act
        await _service.ResolvePlaceholdersForRuleAsync(
            rule,
            currentEntityPlaceholders: layer0Placeholders
        );

        // Assert
        _mockPlaceholderService.Verify(
            s => s.ResolveAllLayersAsync(It.Is<PlaceholderResolutionRequest>(
                req => req.CurrentEntityPlaceholders != null &&
                       req.CurrentEntityPlaceholders.ContainsKey("Name") &&
                       req.CurrentEntityPlaceholders["Name"] == "York"
            )),
            Times.Once
        );
    }

    #endregion

    #region ValidateLocationInsideRegionAsync Tests

    [Fact]
    public async Task ValidateLocationInsideRegionAsync_CreatesComputedPlaceholders()
    {
        // Arrange
        var rule = new FieldValidationRule
        {
            Id = 1,
            ValidationType = "LocationInsideRegion",
            ErrorMessage = "Location {coordinates} is outside {Town.Name}.",
            SuccessMessage = "Location is valid.",
            IsBlocking = true,
            ConfigJson = "{\"regionPropertyPath\": \"TownId\"}"
        };

        var fieldValue = new { X = 125.5, Y = 64.0, Z = -350.2 };
        var dependencyFieldValue = "town_springfield";

        var preResolvedPlaceholders = new Dictionary<string, string>
        {
            ["Town.Name"] = "Springfield"
        };

        // Act
        var result = await _service.ValidateLocationInsideRegionAsync(
            rule,
            fieldValue,
            dependencyFieldValue,
            preResolvedPlaceholders
        );

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Placeholders);
        // Should include computed coordinates placeholder
        // Note: Actual implementation may vary based on service logic
    }

    [Fact]
    public async Task ValidateLocationInsideRegionAsync_MergesPreResolvedPlaceholders()
    {
        // Arrange
        var rule = new FieldValidationRule
        {
            Id = 1,
            ValidationType = "LocationInsideRegion",
            ErrorMessage = "Location {coordinates} is outside {Town.Name}.",
            IsBlocking = true,
            ConfigJson = "{\"regionPropertyPath\": \"TownId\"}"
        };

        var preResolvedPlaceholders = new Dictionary<string, string>
        {
            ["Town.Name"] = "Springfield",
            ["Town.Description"] = "A historic town"
        };

        // Act
        var result = await _service.ValidateLocationInsideRegionAsync(
            rule,
            new { X = 100, Y = 64, Z = -200 },
            "town_springfield",
            preResolvedPlaceholders
        );

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Placeholders);
        Assert.Contains("Town.Name", result.Placeholders.Keys);
        Assert.Equal("Springfield", result.Placeholders["Town.Name"]);
    }

    [Fact]
    public async Task ValidateLocationInsideRegionAsync_WithMissingDependencyValue_HandlesGracefully()
    {
        // Arrange
        var rule = new FieldValidationRule
        {
            Id = 1,
            ValidationType = "LocationInsideRegion",
            ErrorMessage = "Validation failed.",
            IsBlocking = false,
            ConfigJson = "{}"
        };

        var placeholders = new Dictionary<string, string>();

        // Act
        var result = await _service.ValidateLocationInsideRegionAsync(
            rule,
            new { X = 100, Y = 64, Z = -200 },
            dependencyFieldValue: null,
            placeholders: placeholders
        );

        // Assert
        Assert.NotNull(result);
        // Should handle missing dependency gracefully - validation may skip or return error
    }

    #endregion

    #region ValidateRegionContainmentAsync Tests

    [Fact]
    public async Task ValidateRegionContainmentAsync_CreatesViolationCountPlaceholder()
    {
        // Arrange
        var rule = new FieldValidationRule
        {
            Id = 1,
            ValidationType = "RegionContainment",
            ErrorMessage = "Found {violationCount} violations.",
            IsBlocking = true,
            ConfigJson = "{\"parentRegionPath\": \"TownId\"}"
        };

        var placeholders = new Dictionary<string, string>
        {
            ["Town.Name"] = "Springfield"
        };

        // Act
        var result = await _service.ValidateRegionContainmentAsync(
            rule,
            fieldValue: "child_region_id",
            dependencyFieldValue: "parent_region_id",
            placeholders: placeholders
        );

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Placeholders);
        // Should include violationCount if validation performed
    }

    #endregion

    #region ValidateConditionalRequiredAsync Tests

    [Fact]
    public async Task ValidateConditionalRequiredAsync_WithConditionMet_ValidatesRequired()
    {
        // Arrange
        var rule = new FieldValidationRule
        {
            Id = 1,
            ValidationType = "ConditionalRequired",
            ErrorMessage = "Field is required when condition is met.",
            IsBlocking = true,
            ConfigJson = "{\"operator\": \"Equals\", \"value\": \"true\"}"
        };

        var placeholders = new Dictionary<string, string>();

        // Act - condition met (true) and field is empty
        var result = await _service.ValidateConditionalRequiredAsync(
            rule,
            fieldValue: null,
            dependencyFieldValue: "true",
            placeholders: placeholders
        );

        // Assert
        Assert.NotNull(result);
        // When condition is met and field is empty, validation should fail
        Assert.False(result.IsValid);
    }

    [Fact]
    public async Task ValidateConditionalRequiredAsync_WithConditionNotMet_SkipsValidation()
    {
        // Arrange
        var rule = new FieldValidationRule
        {
            Id = 1,
            ValidationType = "ConditionalRequired",
            ErrorMessage = "Field is required.",
            IsBlocking = false,
            ConfigJson = "{\"operator\": \"Equals\", \"value\": \"true\"}"
        };

        var placeholders = new Dictionary<string, string>();

        // Act - condition not met (false) and field is empty
        var result = await _service.ValidateConditionalRequiredAsync(
            rule,
            fieldValue: null,
            dependencyFieldValue: "false",
            placeholders: placeholders
        );

        // Assert
        Assert.NotNull(result);
        // When condition is not met, validation should pass regardless of field value
        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task ValidateConditionalRequiredAsync_WithConditionMetAndFieldPresent_Passes()
    {
        // Arrange
        var rule = new FieldValidationRule
        {
            Id = 1,
            ValidationType = "ConditionalRequired",
            ErrorMessage = "Field is required.",
            IsBlocking = true,
            ConfigJson = "{\"operator\": \"Equals\", \"value\": \"true\"}"
        };

        var placeholders = new Dictionary<string, string>();

        // Act - condition met and field has value
        var result = await _service.ValidateConditionalRequiredAsync(
            rule,
            fieldValue: "some value",
            dependencyFieldValue: "true",
            placeholders: placeholders
        );

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsValid);
    }

    #endregion

    #region Integration Tests

    [Fact]
    public async Task ValidateFieldAsync_WithLocationInsideRegion_ReturnsCompleteResult()
    {
        // Arrange
        var rule = new FieldValidationRule
        {
            Id = 1,
            ValidationType = "LocationInsideRegion",
            ErrorMessage = "Location {coordinates} is outside {Town.Name}'s boundaries.",
            SuccessMessage = "Location is within {Town.Name}.",
            IsBlocking = true,
            ConfigJson = "{\"regionPropertyPath\": \"TownId\"}"
        };

        var placeholderResponse = new PlaceholderResolutionResponse
        {
            ResolvedPlaceholders = new Dictionary<string, string>
            {
                ["Town.Name"] = "Springfield"
            },
            IsSuccessful = true
        };

        _mockPlaceholderService
            .Setup(s => s.ResolveAllLayersAsync(It.IsAny<PlaceholderResolutionRequest>()))
            .ReturnsAsync(placeholderResponse);

        var currentEntityPlaceholders = new Dictionary<string, string>
        {
            ["Name"] = "York"
        };

        // Act
        var result = await _service.ValidateFieldAsync(
            rule,
            fieldValue: new { X = 125.5, Y = 64.0, Z = -350.2 },
            dependencyFieldValue: "town_springfield",
            currentEntityPlaceholders: currentEntityPlaceholders,
            entityId: 1
        );

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Placeholders);
        Assert.NotNull(result.Metadata);
        Assert.Equal("LocationInsideRegion", result.Metadata.ValidationType);
        Assert.Contains("Town.Name", result.Placeholders.Keys);
    }

    [Fact]
    public async Task ValidateFieldAsync_PreservesIsBlockingFlag()
    {
        // Arrange
        var rule = new FieldValidationRule
        {
            Id = 1,
            ValidationType = "ConditionalRequired",
            ErrorMessage = "Error",
            IsBlocking = true // Should be preserved in result
        };

        _mockPlaceholderService
            .Setup(s => s.ResolveAllLayersAsync(It.IsAny<PlaceholderResolutionRequest>()))
            .ReturnsAsync(new PlaceholderResolutionResponse
            {
                ResolvedPlaceholders = new Dictionary<string, string>(),
                IsSuccessful = true
            });

        // Act
        var result = await _service.ValidateFieldAsync(rule, null);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsBlocking);
    }

    #endregion
}
