using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using knkwebapi_v2.Dtos;
using knkwebapi_v2.Models;
using knkwebapi_v2.Repositories;
using knkwebapi_v2.Services;
using knkwebapi_v2.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace knkwebapi_v2.Tests.Services;

public class DependencyResolutionServiceTests
{
    private readonly Mock<IPathResolutionService> _pathService;
    private readonly Mock<IFieldValidationRuleRepository> _ruleRepository;
    private readonly Mock<IFormFieldRepository> _fieldRepository;
    private readonly Mock<IFormConfigurationService> _formConfigService;
    private readonly Mock<IMetadataService> _metadataService;
    private readonly Mock<ILogger<DependencyResolutionService>> _logger;
    private readonly DependencyResolutionService _service;

    public DependencyResolutionServiceTests()
    {
        _pathService = new Mock<IPathResolutionService>();
        _ruleRepository = new Mock<IFieldValidationRuleRepository>();
        _fieldRepository = new Mock<IFormFieldRepository>();
        _formConfigService = new Mock<IFormConfigurationService>();
        _metadataService = new Mock<IMetadataService>();
        _logger = new Mock<ILogger<DependencyResolutionService>>();

        _service = new DependencyResolutionService(
            _pathService.Object,
            _ruleRepository.Object,
            _fieldRepository.Object,
            _formConfigService.Object,
            _metadataService.Object,
            _logger.Object);
    }

    [Fact]
    public async Task ResolveDependenciesAsync_WithValidPath_ReturnsSuccess()
    {
        var rule = new FieldValidationRule
        {
            Id = 1,
            FormFieldId = 10,
            DependsOnFieldId = 20,
            DependencyPath = "Town.WgRegionId"
        };

        _ruleRepository.Setup(r => r.GetByFieldIdsAsync(It.IsAny<IEnumerable<int>>()))
            .ReturnsAsync(new[] { rule });

        _pathService.Setup(p => p.ValidatePathAsync("Town", "WgRegionId"))
            .ReturnsAsync(new PathValidationResult { IsValid = true });

        _pathService.Setup(p => p.ResolvePathAsync("Town", "WgRegionId", It.IsAny<object?>()))
            .ReturnsAsync("town_1");

        var request = new DependencyResolutionRequest
        {
            FieldIds = new[] { 10 },
            FormContextSnapshot = new Dictionary<string, object?>
            {
                ["Town"] = new Dictionary<string, object?>
                {
                    ["WgRegionId"] = "town_1"
                }
            }
        };

        var result = await _service.ResolveDependenciesAsync(request);

        result.Resolved.Should().ContainKey(1);
        result.Resolved[1].Status.Should().Be("success");
        result.Resolved[1].ResolvedValue.Should().Be("town_1");
    }

    [Fact]
    public async Task ResolveDependenciesAsync_WithMissingRoot_ReturnsPending()
    {
        var rule = new FieldValidationRule
        {
            Id = 2,
            FormFieldId = 11,
            DependsOnFieldId = 21,
            DependencyPath = "Town.WgRegionId"
        };

        _ruleRepository.Setup(r => r.GetByFieldIdsAsync(It.IsAny<IEnumerable<int>>()))
            .ReturnsAsync(new[] { rule });

        var request = new DependencyResolutionRequest
        {
            FieldIds = new[] { 11 },
            FormContextSnapshot = new Dictionary<string, object?>()
        };

        var result = await _service.ResolveDependenciesAsync(request);

        result.Resolved.Should().ContainKey(2);
        result.Resolved[2].Status.Should().Be("pending");
    }

    [Fact]
    public async Task ResolveDependenciesAsync_WithInvalidPath_ReturnsError()
    {
        var rule = new FieldValidationRule
        {
            Id = 3,
            FormFieldId = 12,
            DependsOnFieldId = 22,
            DependencyPath = "Town.InvalidProperty"
        };

        _ruleRepository.Setup(r => r.GetByFieldIdsAsync(It.IsAny<IEnumerable<int>>()))
            .ReturnsAsync(new[] { rule });

        _pathService.Setup(p => p.ValidatePathAsync("Town", "InvalidProperty"))
            .ReturnsAsync(new PathValidationResult
            {
                IsValid = false,
                ErrorMessage = "Property not found",
                Suggestion = "Use a valid property"
            });

        var request = new DependencyResolutionRequest
        {
            FieldIds = new[] { 12 },
            FormContextSnapshot = new Dictionary<string, object?>
            {
                ["Town"] = new Dictionary<string, object?>
                {
                    ["Name"] = "Cinix"
                }
            }
        };

        var result = await _service.ResolveDependenciesAsync(request);

        result.Resolved.Should().ContainKey(3);
        result.Resolved[3].Status.Should().Be("error");
        result.Resolved[3].Message.Should().Be("Property not found");
        result.Resolved[3].ErrorDetail.Should().Be("Use a valid property");
    }
}
