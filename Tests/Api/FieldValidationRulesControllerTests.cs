using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;
using KnKWebAPI.Controllers;
using knkwebapi_v2.Dtos;
using knkwebapi_v2.Models;
using knkwebapi_v2.Repositories;
using knkwebapi_v2.Services.Interfaces;

namespace knkwebapi_v2.Tests.Api;

/// <summary>
/// API endpoint tests for FieldValidationRulesController.
/// Focuses on placeholder resolution and rule-based validation endpoints.
/// </summary>
[Trait("Category", "API")]
public class FieldValidationRulesControllerTests
{
    private readonly Mock<IValidationService> _mockValidationService;
    private readonly Mock<IPlaceholderResolutionService> _mockPlaceholderService;
    private readonly Mock<IFieldValidationService> _mockFieldValidationService;
    private readonly Mock<IFieldValidationRuleRepository> _mockRuleRepository;
    private readonly Mock<IDependencyResolutionService> _mockDependencyService;
    private readonly Mock<IPathResolutionService> _mockPathService;
    private readonly FieldValidationRulesController _controller;

    public FieldValidationRulesControllerTests()
    {
        _mockValidationService = new Mock<IValidationService>();
        _mockPlaceholderService = new Mock<IPlaceholderResolutionService>();
        _mockFieldValidationService = new Mock<IFieldValidationService>();
        _mockRuleRepository = new Mock<IFieldValidationRuleRepository>();
        _mockDependencyService = new Mock<IDependencyResolutionService>();
        _mockPathService = new Mock<IPathResolutionService>();

        _controller = new FieldValidationRulesController(
            _mockValidationService.Object,
            _mockPlaceholderService.Object,
            _mockFieldValidationService.Object,
            _mockRuleRepository.Object,
            _mockDependencyService.Object,
            _mockPathService.Object);
    }

    [Fact]
    public async Task ResolvePlaceholders_WithNullRequest_ReturnsBadRequest()
    {
        var result = await _controller.ResolvePlaceholders(null!);

        Assert.IsType<BadRequestResult>(result);
    }

    [Fact]
    public async Task ResolvePlaceholders_WithMissingRuleAndPaths_ReturnsBadRequest()
    {
        var request = new PlaceholderResolutionRequest();

        var result = await _controller.ResolvePlaceholders(request);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task ResolvePlaceholders_WithRuleNotFound_ReturnsNotFound()
    {
        var request = new PlaceholderResolutionRequest { FieldValidationRuleId = 10 };

        _mockValidationService
            .Setup(s => s.GetByIdAsync(10))
            .ReturnsAsync((FieldValidationRuleDto?)null);

        var result = await _controller.ResolvePlaceholders(request);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task ResolvePlaceholders_WithValidRequest_ReturnsOk()
    {
        var request = new PlaceholderResolutionRequest { FieldValidationRuleId = 10 };
        var response = new PlaceholderResolutionResponse
        {
            ResolvedPlaceholders = new Dictionary<string, string> { ["Town.Name"] = "Springfield" }
        };

        _mockValidationService
            .Setup(s => s.GetByIdAsync(10))
            .ReturnsAsync(new FieldValidationRuleDto { Id = 10, ErrorMessage = "Location outside {Town.Name}." });

        _mockPlaceholderService
            .Setup(s => s.ResolveAllLayersAsync(request))
            .ReturnsAsync(response);

        var result = await _controller.ResolvePlaceholders(request);

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task ValidateFieldRule_WithNullRequest_ReturnsBadRequest()
    {
        var result = await _controller.ValidateFieldRule(null!);

        Assert.IsType<BadRequestResult>(result);
    }

    [Fact]
    public async Task ValidateFieldRule_WithMissingRuleId_ReturnsBadRequest()
    {
        var request = new ValidateFieldRuleRequestDto();

        var result = await _controller.ValidateFieldRule(request);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task ValidateFieldRule_WithRuleNotFound_ReturnsNotFound()
    {
        var request = new ValidateFieldRuleRequestDto { FieldValidationRuleId = 12 };

        _mockRuleRepository
            .Setup(r => r.GetByIdAsync(12))
            .ReturnsAsync((FieldValidationRule?)null);

        var result = await _controller.ValidateFieldRule(request);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task ValidateFieldRule_WithValidRequest_ReturnsOk()
    {
        var request = new ValidateFieldRuleRequestDto
        {
            FieldValidationRuleId = 12,
            FieldValue = new { x = 10, y = 64, z = -5 }
        };

        var rule = new FieldValidationRule
        {
            Id = 12,
            ValidationType = "LocationInsideRegion",
            ErrorMessage = "Invalid location",
            IsBlocking = true
        };

        _mockRuleRepository
            .Setup(r => r.GetByIdAsync(12))
            .ReturnsAsync(rule);

        _mockFieldValidationService
            .Setup(s => s.ValidateFieldAsync(rule, request.FieldValue, null, null, null))
            .ReturnsAsync(new ValidationResultDto
            {
                IsValid = true,
                IsBlocking = false,
                Message = "Location is valid"
            });

        var result = await _controller.ValidateFieldRule(request);

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task GetPlaceholdersByRule_WithRuleNotFound_ReturnsNotFound()
    {
        _mockValidationService
            .Setup(s => s.GetByIdAsync(5))
            .ReturnsAsync((FieldValidationRuleDto?)null);

        var result = await _controller.GetPlaceholdersByRule(5);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task GetPlaceholdersByRule_WithValidRule_ReturnsOk()
    {
        var ruleDto = new FieldValidationRuleDto
        {
            Id = 5,
            ErrorMessage = "Location outside {Town.Name}.",
            SuccessMessage = "Location within {Town.Name}."
        };

        _mockValidationService
            .Setup(s => s.GetByIdAsync(5))
            .ReturnsAsync(ruleDto);

        _mockPlaceholderService
            .Setup(s => s.ExtractPlaceholdersAsync(ruleDto.ErrorMessage))
            .ReturnsAsync(new List<string> { "Town.Name" });

        _mockPlaceholderService
            .Setup(s => s.ExtractPlaceholdersAsync(ruleDto.SuccessMessage))
            .ReturnsAsync(new List<string> { "Town.Name" });

        var result = await _controller.GetPlaceholdersByRule(5);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var placeholders = Assert.IsType<List<string>>(okResult.Value);
        Assert.Single(placeholders);
        Assert.Contains("Town.Name", placeholders);
    }
}
