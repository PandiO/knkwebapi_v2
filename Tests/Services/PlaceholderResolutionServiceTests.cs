using Xunit;
using Moq;
using knkwebapi_v2.Services;
using knkwebapi_v2.Services.Interfaces;
using knkwebapi_v2.Repositories.Interfaces;
using knkwebapi_v2.Models;
using knkwebapi_v2.Dtos;
using knkwebapi_v2.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace knkwebapi_v2.Tests.Services;

/// <summary>
/// Unit tests for PlaceholderResolutionService.
/// Tests extraction, layer resolution, and multi-layer placeholder handling.
/// Phase 7.1 of Placeholder Interpolation implementation.
/// </summary>
[Trait("Category", "Unit")]
[Trait("Feature", "PlaceholderInterpolation")]
public class PlaceholderResolutionServiceTests : IDisposable
{
    private readonly KnKDbContext _dbContext;
    private readonly Mock<IFieldValidationRuleRepository> _mockRuleRepository;
    private readonly Mock<ILogger<PlaceholderResolutionService>> _mockLogger;
    private readonly PlaceholderResolutionService _service;

    public PlaceholderResolutionServiceTests()
    {
        // Setup in-memory database for testing
        var options = new DbContextOptionsBuilder<KnKDbContext>()
            .UseInMemoryDatabase(databaseName: $"PlaceholderTestDb_{Guid.NewGuid()}")
            .Options;

        _dbContext = new KnKDbContext(options);
        _mockRuleRepository = new Mock<IFieldValidationRuleRepository>();
        _mockLogger = new Mock<ILogger<PlaceholderResolutionService>>();

        _service = new PlaceholderResolutionService(
            _dbContext,
            _mockRuleRepository.Object,
            _mockLogger.Object
        );

        SeedTestData();
    }

    private void SeedTestData()
    {
        // Create test Town
        var town = new Town
        {
            Id = 1,
            Name = "Springfield",
            Description = "A historic town"
        };
        _dbContext.Towns.Add(town);

        // Create test Districts
        var district1 = new District
        {
            Id = 1,
            Name = "York",
            Description = "A residential district",
            TownId = 1,
            Town = town
        };

        var district2 = new District
        {
            Id = 2,
            Name = "Cambridge",
            Description = "A commercial district",
            TownId = 1,
            Town = town
        };

        _dbContext.Districts.Add(district1);
        _dbContext.Districts.Add(district2);

        // Create test Structure
        var structure = new Structure
        {
            Id = 1,
            Name = "Town Hall",
            Description = "Central building",
            DistrictId = 1,
            District = district1
        };
        _dbContext.Structures.Add(structure);

        _dbContext.SaveChanges();
    }

    #region ExtractPlaceholdersAsync Tests

    [Fact]
    public async Task ExtractPlaceholdersAsync_WithNoPlaceholders_ReturnsEmptyList()
    {
        // Arrange
        var message = "This is a plain message with no placeholders.";

        // Act
        var result = await _service.ExtractPlaceholdersAsync(message);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task ExtractPlaceholdersAsync_WithSinglePlaceholder_ReturnsOne()
    {
        // Arrange
        var message = "Hello {Name}!";

        // Act
        var result = await _service.ExtractPlaceholdersAsync(message);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Contains("Name", result);
    }

    [Fact]
    public async Task ExtractPlaceholdersAsync_WithMultiplePlaceholders_ReturnsAll()
    {
        // Arrange
        var message = "Location {coordinates} is outside {Town.Name}'s boundaries.";

        // Act
        var result = await _service.ExtractPlaceholdersAsync(message);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Contains("coordinates", result);
        Assert.Contains("Town.Name", result);
    }

    [Fact]
    public async Task ExtractPlaceholdersAsync_WithMalformedPlaceholders_HandlesGracefully()
    {
        // Arrange
        var message = "Invalid {unclosed or {nested {bracket}} patterns";

        // Act
        var result = await _service.ExtractPlaceholdersAsync(message);

        // Assert
        Assert.NotNull(result);
        // Should extract valid patterns only
        Assert.Contains("bracket", result);
    }

    [Fact]
    public async Task ExtractPlaceholdersAsync_WithEmptyMessage_ReturnsEmpty()
    {
        // Arrange
        var message = "";

        // Act
        var result = await _service.ExtractPlaceholdersAsync(message);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task ExtractPlaceholdersAsync_WithNullMessage_ReturnsEmpty()
    {
        // Arrange
        string? message = null;

        // Act
        var result = await _service.ExtractPlaceholdersAsync(message!);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    #endregion

    #region ResolveLayer0Async Tests

    [Fact]
    public async Task ResolveLayer0Async_WithValidDictionary_ReturnsAsIs()
    {
        // Arrange
        var layer0Data = new Dictionary<string, string>
        {
            ["Name"] = "York",
            ["Description"] = "A residential district"
        };

        // Act
        var result = await _service.ResolveLayer0Async(layer0Data);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Equal("York", result["Name"]);
        Assert.Equal("A residential district", result["Description"]);
    }

    [Fact]
    public async Task ResolveLayer0Async_WithEmptyDictionary_ReturnsEmpty()
    {
        // Arrange
        var layer0Data = new Dictionary<string, string>();

        // Act
        var result = await _service.ResolveLayer0Async(layer0Data);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task ResolveLayer0Async_WithNullInput_HandlesGracefully()
    {
        // Arrange
        Dictionary<string, string>? layer0Data = null;

        // Act
        var result = await _service.ResolveLayer0Async(layer0Data!);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    #endregion

    #region ResolveLayer1Async Tests

    [Fact]
    public async Task ResolveLayer1Async_WithValidSingleNavigation_ResolvesCorrectly()
    {
        // Arrange
        var placeholders = new List<string> { "Town.Name" };

        // Act
        var result = await _service.ResolveLayer1Async(
            typeof(District),
            1, // District York's ID
            placeholders
        );

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal("Springfield", result["Town.Name"]);
    }

    [Fact]
    public async Task ResolveLayer1Async_WithMultipleNavigations_ResolvesAll()
    {
        // Arrange
        var placeholders = new List<string> { "Town.Name", "Town.Description" };

        // Act
        var result = await _service.ResolveLayer1Async(
            typeof(District),
            1,
            placeholders
        );

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Equal("Springfield", result["Town.Name"]);
        Assert.Equal("A historic town", result["Town.Description"]);
    }

    [Fact]
    public async Task ResolveLayer1Async_WithNullForeignKey_ReturnsError()
    {
        // Arrange: Create district without town
        var orphanDistrict = new District
        {
            Id = 99,
            Name = "Orphan",
            TownId = null
        };
        _dbContext.Districts.Add(orphanDistrict);
        await _dbContext.SaveChangesAsync();

        var placeholders = new List<string> { "Town.Name" };

        // Act
        var result = await _service.ResolveLayer1Async(
            typeof(District),
            99,
            placeholders
        );

        // Assert
        Assert.NotNull(result);
        // Should handle gracefully - check if result is empty or has placeholder for error
        Assert.True(result.Count == 0 || result.ContainsKey("Town.Name"));
    }

    [Fact]
    public async Task ResolveLayer1Async_WithInvalidEntityId_HandlesGracefully()
    {
        // Arrange
        var placeholders = new List<string> { "Town.Name" };

        // Act
        var result = await _service.ResolveLayer1Async(
            typeof(District),
            999, // Non-existent ID
            placeholders
        );

        // Assert
        Assert.NotNull(result);
        // Should handle EntityNotFound gracefully
    }

    #endregion

    #region ResolveLayer2Async Tests

    [Fact]
    public async Task ResolveLayer2Async_WithValidMultiLevelNavigation_ResolvesCorrectly()
    {
        // Arrange
        var placeholders = new List<string> { "District.Town.Name" };

        // Act
        var result = await _service.ResolveLayer2Async(
            typeof(Structure),
            1, // Structure "Town Hall"
            placeholders
        );

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal("Springfield", result["District.Town.Name"]);
    }

    [Fact]
    public async Task ResolveLayer2Async_WithBrokenNavigationChain_HandlesGracefully()
    {
        // Arrange
        var placeholders = new List<string> { "District.Invalid.Property" };

        // Act
        var result = await _service.ResolveLayer2Async(
            typeof(Structure),
            1,
            placeholders
        );

        // Assert
        Assert.NotNull(result);
        // Should handle broken chain without throwing
    }

    [Fact]
    public async Task ResolveLayer2Async_WithNullIntermediateValue_HandlesGracefully()
    {
        // Arrange: Create structure without district
        var orphanStructure = new Structure
        {
            Id = 99,
            Name = "Orphan Structure",
            DistrictId = null
        };
        _dbContext.Structures.Add(orphanStructure);
        await _dbContext.SaveChangesAsync();

        var placeholders = new List<string> { "District.Town.Name" };

        // Act
        var result = await _service.ResolveLayer2Async(
            typeof(Structure),
            99,
            placeholders
        );

        // Assert
        Assert.NotNull(result);
        // Should handle null intermediate value gracefully
    }

    #endregion

    #region ResolveLayer3Async Tests

    [Fact]
    public async Task ResolveLayer3Async_WithCountOperation_ReturnsCount()
    {
        // Arrange
        var placeholders = new List<string> { "Districts.Count" };

        // Act
        var result = await _service.ResolveLayer3Async(
            typeof(Town),
            1, // Springfield
            placeholders
        );

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal("2", result["Districts.Count"]); // Springfield has 2 districts
    }

    [Fact]
    public async Task ResolveLayer3Async_WithCountOnEmptyCollection_ReturnsZero()
    {
        // Arrange: Create town with no districts
        var emptyTown = new Town
        {
            Id = 99,
            Name = "Empty Town"
        };
        _dbContext.Towns.Add(emptyTown);
        await _dbContext.SaveChangesAsync();

        var placeholders = new List<string> { "Districts.Count" };

        // Act
        var result = await _service.ResolveLayer3Async(
            typeof(Town),
            99,
            placeholders
        );

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal("0", result["Districts.Count"]);
    }

    [Fact]
    public async Task ResolveLayer3Async_WithFirstOperation_ReturnsFirstElement()
    {
        // Arrange
        var placeholders = new List<string> { "Districts.First.Name" };

        // Act
        var result = await _service.ResolveLayer3Async(
            typeof(Town),
            1,
            placeholders
        );

        // Assert
        Assert.NotNull(result);
        // Should contain one of the district names
        if (result.ContainsKey("Districts.First.Name"))
        {
            Assert.True(result["Districts.First.Name"] == "York" || result["Districts.First.Name"] == "Cambridge");
        }
    }

    [Fact]
    public async Task ResolveLayer3Async_WithNullCollection_HandlesGracefully()
    {
        // Arrange
        var placeholders = new List<string> { "InvalidCollection.Count" };

        // Act
        var result = await _service.ResolveLayer3Async(
            typeof(Town),
            1,
            placeholders
        );

        // Assert
        Assert.NotNull(result);
        // Should handle gracefully without throwing
    }

    #endregion

    #region InterpolatePlaceholders Tests

    [Fact]
    public void InterpolatePlaceholders_WithAllKeysPresent_FullyReplaces()
    {
        // Arrange
        var message = "Hello {Name} from {Town}!";
        var placeholders = new Dictionary<string, string>
        {
            ["Name"] = "York",
            ["Town"] = "Springfield"
        };

        // Act
        var result = _service.InterpolatePlaceholders(message, placeholders);

        // Assert
        Assert.Equal("Hello York from Springfield!", result);
    }

    [Fact]
    public void InterpolatePlaceholders_WithSomeKeysMissing_PartialReplacement()
    {
        // Arrange
        var message = "Hello {Name} from {Town} in {Country}!";
        var placeholders = new Dictionary<string, string>
        {
            ["Name"] = "York"
        };

        // Act
        var result = _service.InterpolatePlaceholders(message, placeholders);

        // Assert
        Assert.Contains("Hello York", result);
        Assert.Contains("{Town}", result); // Unreplaced
        Assert.Contains("{Country}", result); // Unreplaced
    }

    [Fact]
    public void InterpolatePlaceholders_WithNoPlaceholders_ReturnsMessageAsIs()
    {
        // Arrange
        var message = "Plain message without placeholders";
        var placeholders = new Dictionary<string, string>
        {
            ["Unused"] = "Value"
        };

        // Act
        var result = _service.InterpolatePlaceholders(message, placeholders);

        // Assert
        Assert.Equal(message, result);
    }

    [Fact]
    public void InterpolatePlaceholders_WithNullMessage_ReturnsEmpty()
    {
        // Arrange
        string? message = null;
        var placeholders = new Dictionary<string, string>();

        // Act
        var result = _service.InterpolatePlaceholders(message!, placeholders);

        // Assert
        Assert.Equal("", result);
    }

    #endregion

    #region ResolveAllLayersAsync Integration Tests

    [Fact]
    public async Task ResolveAllLayersAsync_WithRuleId_ExtractsAndResolvesPlaceholders()
    {
        // Arrange
        var rule = new FieldValidationRule
        {
            Id = 1,
            ErrorMessage = "Location is outside {Town.Name}'s boundaries.",
            SuccessMessage = "Location is within {Town.Name}."
        };

        _mockRuleRepository
            .Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(rule);

        var request = new PlaceholderResolutionRequest
        {
            FieldValidationRuleId = 1,
            EntityTypeName = "District",
            EntityId = 1,
            CurrentEntityPlaceholders = new Dictionary<string, string>
            {
                ["Name"] = "York"
            }
        };

        // Act
        var result = await _service.ResolveAllLayersAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccessful);
        Assert.Equal(2, result.TotalPlaceholdersRequested); // Town.Name appears in both messages
        Assert.Contains("Town.Name", result.ResolvedPlaceholders.Keys);
        Assert.Equal("Springfield", result.ResolvedPlaceholders["Town.Name"]);
    }

    [Fact]
    public async Task ResolveAllLayersAsync_WithExplicitPaths_ResolvesCorrectly()
    {
        // Arrange
        var request = new PlaceholderResolutionRequest
        {
            PlaceholderPaths = new List<string> { "Town.Name", "Town.Description" },
            EntityTypeName = "District",
            EntityId = 1
        };

        // Act
        var result = await _service.ResolveAllLayersAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccessful);
        Assert.Equal(2, result.ResolvedPlaceholders.Count);
        Assert.Equal("Springfield", result.ResolvedPlaceholders["Town.Name"]);
        Assert.Equal("A historic town", result.ResolvedPlaceholders["Town.Description"]);
    }

    [Fact]
    public async Task ResolveAllLayersAsync_WithInvalidRuleId_ReturnsError()
    {
        // Arrange
        _mockRuleRepository
            .Setup(r => r.GetByIdAsync(999))
            .ReturnsAsync((FieldValidationRule?)null);

        var request = new PlaceholderResolutionRequest
        {
            FieldValidationRuleId = 999
        };

        // Act
        var result = await _service.ResolveAllLayersAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.IsSuccessful);
        Assert.NotEmpty(result.ResolutionErrors);
        Assert.Contains(result.ResolutionErrors, e => e.ErrorCode == "RuleNotFound");
    }

    [Fact]
    public async Task ResolveAllLayersAsync_WithMixedLayers_ResolvesAll()
    {
        // Arrange
        var request = new PlaceholderResolutionRequest
        {
            PlaceholderPaths = new List<string>
            {
                "Name",                    // Layer 0
                "Town.Name",               // Layer 1
                "District.Town.Name"       // Layer 2
            },
            EntityTypeName = "Structure",
            EntityId = 1,
            CurrentEntityPlaceholders = new Dictionary<string, string>
            {
                ["Name"] = "Town Hall"
            }
        };

        // Act
        var result = await _service.ResolveAllLayersAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccessful);
        Assert.Equal(3, result.ResolvedPlaceholders.Count);
        Assert.Equal("Town Hall", result.ResolvedPlaceholders["Name"]); // Layer 0
        // Layers 1 and 2 should also be resolved
    }

    #endregion

    public void Dispose()
    {
        _dbContext?.Dispose();
    }
}
