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

/// <summary>
/// Phase 3 tests: Enhanced Configuration Health Checks
/// Tests all 6 validation check implementations
/// </summary>
public class DependencyResolutionHealthCheckTests
{
    private readonly Mock<IPathResolutionService> _pathService;
    private readonly Mock<IFieldValidationRuleRepository> _ruleRepository;
    private readonly Mock<IFormFieldRepository> _fieldRepository;
    private readonly Mock<IFormConfigurationService> _formConfigService;
    private readonly Mock<IMetadataService> _metadataService;
    private readonly Mock<ILogger<DependencyResolutionService>> _logger;
    private readonly DependencyResolutionService _service;

    public DependencyResolutionHealthCheckTests()
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

    #region Health Check 1: Field-Entity Alignment

    [Fact]
    public async Task CheckHealth_WithMissingConfig_ReturnsError()
    {
        _formConfigService.Setup(r => r.GetByIdAsync(999))
            .ReturnsAsync((FormConfigurationDto?)null);

        var result = await _service.CheckConfigurationHealthAsync(999);

        result.Should().HaveCount(1);
        result[0].Severity.Should().Be("Error");
        result[0].Message.Should().Contain("not found");
    }

    [Fact]
    public async Task CheckHealth_WithMissingEntityType_ReturnsError()
    {
        var config = CreateTestConfig();
        config.EntityTypeName = "";

        _formConfigService.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(config);

        var result = await _service.CheckConfigurationHealthAsync(1);

        result.Should().Contain(i => i.Severity == "Error" && i.Message.Contains("missing entity type name"));
    }

    [Fact]
    public async Task CheckHealth_WithInvalidEntityMetadata_ReturnsError()
    {
        var config = CreateTestConfig();

        _formConfigService.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(config);

        _metadataService.Setup(m => m.GetEntityMetadata("Town"))
            .Returns((EntityMetadataDto?)null);

        var result = await _service.CheckConfigurationHealthAsync(1);

        result.Should().Contain(i => i.Severity == "Error" && i.Message.Contains("not found in system metadata"));
    }

    #endregion

    #region Health Check 2: Property Existence

    [Fact]
    public async Task CheckHealth_WithInvalidDependencyPath_ReturnsError()
    {
        var config = CreateTestConfig();
        var rules = new List<FieldValidationRule>
        {
            new FieldValidationRule
            {
                Id = 1,
                FormFieldId = 60,
                DependencyPath = "Town.InvalidProperty"
            }
        };

        _formConfigService.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(config);

        _metadataService.Setup(m => m.GetEntityMetadata("Town"))
            .Returns(CreateMockMetadata());

        _ruleRepository.Setup(r => r.GetByFormConfigurationIdAsync(1))
            .ReturnsAsync(rules);

        _fieldRepository.Setup(f => f.GetByIdAsync(60))
            .ReturnsAsync(new FormField { Id = 60, Label = "Test Field" });

        _pathService.Setup(p => p.ValidatePathAsync("Town", "Town.InvalidProperty"))
            .ReturnsAsync(new PathValidationResult
            {
                IsValid = false,
                ErrorMessage = "Property 'InvalidProperty' not found"
            });

        var result = await _service.CheckConfigurationHealthAsync(1);

        result.Should().Contain(i =>
            i.Severity == "Error" &&
            i.Message.Contains("invalid dependency path") &&
            i.FieldId == 60);
    }

    [Fact]
    public async Task CheckHealth_WithValidDependencyPath_NoError()
    {
        var config = CreateTestConfig();
        var rules = new List<FieldValidationRule>
        {
            new FieldValidationRule
            {
                Id = 1,
                FormFieldId = 60,
                DependencyPath = "Town.WgRegionId"
            }
        };

        _formConfigService.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(config);

        _metadataService.Setup(m => m.GetEntityMetadata("Town"))
            .Returns(CreateMockMetadata());

        _ruleRepository.Setup(r => r.GetByFormConfigurationIdAsync(1))
            .ReturnsAsync(rules);

        _pathService.Setup(p => p.ValidatePathAsync("Town", "Town.WgRegionId"))
            .ReturnsAsync(new PathValidationResult { IsValid = true });

        var result = await _service.CheckConfigurationHealthAsync(1);

        result.Should().NotContain(i => i.Message.Contains("invalid dependency path"));
    }

    #endregion

    #region Health Check 3: Required Field Completeness

    [Fact]
    public async Task CheckHealth_WithMissingRequiredField_ReturnsWarning()
    {
        var config = CreateTestConfig();
        config.Steps = new List<FormStepDto>
        {
            new FormStepDto
            {
                StepName = "Step 1",
                Fields = new List<FormFieldDto>
                {
                    new FormFieldDto { Id = "1", FieldName = "Name" }
                    // Missing "Description" which is required
                }
            }
        };

        var metadata = new EntityMetadataDto
        {
            EntityName = "Town",
            Fields = new List<FieldMetadataDto>
            {
                new FieldMetadataDto { FieldName = "Name", IsNullable = false, HasDefaultValue = false },
                new FieldMetadataDto { FieldName = "Description", IsNullable = false, HasDefaultValue = false },
                new FieldMetadataDto { FieldName = "Id", IsNullable = false, HasDefaultValue = true }
            }
        };

        _formConfigService.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(config);

        _metadataService.Setup(m => m.GetEntityMetadata("Town"))
            .Returns(metadata);

        _ruleRepository.Setup(r => r.GetByFormConfigurationIdAsync(1))
            .ReturnsAsync(new List<FieldValidationRule>());

        var result = await _service.CheckConfigurationHealthAsync(1);

        result.Should().Contain(i =>
            i.Severity == "Warning" &&
            i.Message.Contains("Description") &&
            i.Message.Contains("not in the form configuration"));
    }

    [Fact]
    public async Task CheckHealth_WithOptionalRequiredField_ReturnsWarning()
    {
        var config = CreateTestConfig();
        config.Steps = new List<FormStepDto>
        {
            new FormStepDto
            {
                StepName = "Step 1",
                Fields = new List<FormFieldDto>
                {
                    new FormFieldDto
                    {
                        Id = "1",
                        FieldName = "Name",
                        IsRequired = false // Marked as optional but entity requires it
                    }
                }
            }
        };

        var metadata = new EntityMetadataDto
        {
            EntityName = "Town",
            Fields = new List<FieldMetadataDto>
            {
                new FieldMetadataDto { FieldName = "Name", IsNullable = false, HasDefaultValue = false }
            }
        };

        _formConfigService.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(config);

        _metadataService.Setup(m => m.GetEntityMetadata("Town"))
            .Returns(metadata);

        _ruleRepository.Setup(r => r.GetByFormConfigurationIdAsync(1))
            .ReturnsAsync(new List<FieldValidationRule>());

        var result = await _service.CheckConfigurationHealthAsync(1);

        result.Should().Contain(i =>
            i.Severity == "Warning" &&
            i.Message.Contains("required by entity") &&
            i.Message.Contains("marked as optional") &&
            i.FieldId == 1);
    }

    [Fact]
    public async Task CheckHealth_WithFieldHavingDefault_NoWarning()
    {
        var config = CreateTestConfig();
        config.Steps = new List<FormStepDto>
        {
            new FormStepDto
            {
                StepName = "Step 1",
                Fields = new List<FormFieldDto>
                {
                    new FormFieldDto { Id = "1", FieldName = "Name" }
                    // Not including "AllowEntry" but it has a default
                }
            }
        };

        var metadata = new EntityMetadataDto
        {
            EntityName = "Town",
            Fields = new List<FieldMetadataDto>
            {
                new FieldMetadataDto { FieldName = "Name", IsNullable = false, HasDefaultValue = false },
                new FieldMetadataDto { FieldName = "AllowEntry", IsNullable = false, HasDefaultValue = true }
            }
        };

        _formConfigService.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(config);

        _metadataService.Setup(m => m.GetEntityMetadata("Town"))
            .Returns(metadata);

        _ruleRepository.Setup(r => r.GetByFormConfigurationIdAsync(1))
            .ReturnsAsync(new List<FieldValidationRule>());

        var result = await _service.CheckConfigurationHealthAsync(1);

        result.Should().NotContain(i => i.Message.Contains("AllowEntry"));
    }

    #endregion

    #region Health Check 4: Collection Warning (v1)

    [Fact]
    public async Task CheckHealth_WithCollectionPath_ReturnsWarning()
    {
        var config = CreateTestConfig();
        var rules = new List<FieldValidationRule>
        {
            new FieldValidationRule
            {
                Id = 1,
                FormFieldId = 60,
                DependencyPath = "Town.Streets[0].Name"
            }
        };

        _formConfigService.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(config);

        _metadataService.Setup(m => m.GetEntityMetadata("Town"))
            .Returns(CreateMockMetadata());

        _ruleRepository.Setup(r => r.GetByFormConfigurationIdAsync(1))
            .ReturnsAsync(rules);

        _fieldRepository.Setup(f => f.GetByIdAsync(60))
            .ReturnsAsync(new FormField { Id = 60, Label = "Test Field" });

        _pathService.Setup(p => p.ValidatePathAsync("Town", "Town.Streets[0].Name"))
            .ReturnsAsync(new PathValidationResult
            {
                IsValid = false,
                ErrorMessage = "Path contains collection [0] which is not supported",
                IsCollectionNavigation = true
            });

        var result = await _service.CheckConfigurationHealthAsync(1);

        result.Should().Contain(i =>
            i.Severity == "Warning" &&
            i.Message.Contains("collection") &&
            i.Message.Contains("v2") &&
            i.FieldId == 60);
    }

    #endregion

    #region Health Check 5: Circular Dependency Detection

    [Fact]
    public async Task CheckHealth_WithCircularDependency_ReturnsError()
    {
        var config = CreateTestConfig();
        config.Steps = new List<FormStepDto>
        {
            new FormStepDto
            {
                StepName = "Step 1",
                Fields = new List<FormFieldDto>
                {
                    new FormFieldDto { Id = "10", Label = "Field A" },
                    new FormFieldDto { Id = "20", Label = "Field B" }
                }
            }
        };

        var rules = new List<FieldValidationRule>
        {
            new FieldValidationRule
            {
                Id = 1,
                FormFieldId = 10,
                DependsOnFieldId = 20 // A depends on B
            },
            new FieldValidationRule
            {
                Id = 2,
                FormFieldId = 20,
                DependsOnFieldId = 10 // B depends on A -> Circular!
            }
        };

        _formConfigService.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(config);

        _metadataService.Setup(m => m.GetEntityMetadata("Town"))
            .Returns(CreateMockMetadata());

        _ruleRepository.Setup(r => r.GetByFormConfigurationIdAsync(1))
            .ReturnsAsync(rules);

        var result = await _service.CheckConfigurationHealthAsync(1);

        result.Should().Contain(i =>
            i.Severity == "Error" &&
            i.Message.Contains("Circular dependency"));
    }

    [Fact]
    public async Task CheckHealth_WithThreeWayCircularDependency_ReturnsError()
    {
        var config = CreateTestConfig();
        config.Steps = new List<FormStepDto>
        {
            new FormStepDto
            {
                StepName = "Step 1",
                Fields = new List<FormFieldDto>
                {
                    new FormFieldDto { Id = "10", Label = "Field A" },
                    new FormFieldDto { Id = "20", Label = "Field B" },
                    new FormFieldDto { Id = "30", Label = "Field C" }
                }
            }
        };

        var rules = new List<FieldValidationRule>
        {
            new FieldValidationRule { Id = 1, FormFieldId = 10, DependsOnFieldId = 20 }, // A -> B
            new FieldValidationRule { Id = 2, FormFieldId = 20, DependsOnFieldId = 30 }, // B -> C
            new FieldValidationRule { Id = 3, FormFieldId = 30, DependsOnFieldId = 10 }  // C -> A (cycle!)
        };

        _formConfigService.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(config);

        _metadataService.Setup(m => m.GetEntityMetadata("Town"))
            .Returns(CreateMockMetadata());

        _ruleRepository.Setup(r => r.GetByFormConfigurationIdAsync(1))
            .ReturnsAsync(rules);

        var result = await _service.CheckConfigurationHealthAsync(1);

        result.Should().Contain(i =>
            i.Severity == "Error" &&
            i.Message.Contains("Circular dependency"));
    }

    [Fact]
    public async Task CheckHealth_WithoutCircularDependency_NoError()
    {
        var config = CreateTestConfig();
        config.Steps = new List<FormStepDto>
        {
            new FormStepDto
            {
                StepName = "Step 1",
                Fields = new List<FormFieldDto>
                {
                    new FormFieldDto { Id = "10", Label = "Field A" },
                    new FormFieldDto { Id = "20", Label = "Field B" },
                    new FormFieldDto { Id = "30", Label = "Field C" }
                }
            }
        };

        var rules = new List<FieldValidationRule>
        {
            new FieldValidationRule { Id = 1, FormFieldId = 10, DependsOnFieldId = 20 }, // A -> B
            new FieldValidationRule { Id = 2, FormFieldId = 20, DependsOnFieldId = 30 }  // B -> C (no cycle)
        };

        _formConfigService.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(config);

        _metadataService.Setup(m => m.GetEntityMetadata("Town"))
            .Returns(CreateMockMetadata());

        _ruleRepository.Setup(r => r.GetByFormConfigurationIdAsync(1))
            .ReturnsAsync(rules);

        var result = await _service.CheckConfigurationHealthAsync(1);

        result.Should().NotContain(i => i.Message.Contains("Circular dependency"));
    }

    #endregion

    #region Health Check 6: Field Ordering Validation

    [Fact]
    public async Task CheckHealth_WithWrongFieldOrdering_ReturnsWarning()
    {
        var config = CreateTestConfig();
        config.Steps = new List<FormStepDto>
        {
            new FormStepDto
            {
                StepName = "Step 1",
                Fields = new List<FormFieldDto>
                {
                    new FormFieldDto { Id = "10", Label = "Dependent Field" }, // First
                    new FormFieldDto { Id = "20", Label = "Dependency Field" }  // Second
                }
            }
        };

        var rules = new List<FieldValidationRule>
        {
            new FieldValidationRule
            {
                Id = 1,
                FormFieldId = 10, // Dependent field comes first (wrong!)
                DependsOnFieldId = 20 // But depends on field that comes later
            }
        };

        _formConfigService.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(config);

        _metadataService.Setup(m => m.GetEntityMetadata("Town"))
            .Returns(CreateMockMetadata());

        _ruleRepository.Setup(r => r.GetByFormConfigurationIdAsync(1))
            .ReturnsAsync(rules);

        var result = await _service.CheckConfigurationHealthAsync(1);

        result.Should().Contain(i =>
            i.Severity == "Warning" &&
            i.Message.Contains("comes after it") &&
            i.Message.Contains("Reorder fields"));
    }

    [Fact]
    public async Task CheckHealth_WithCorrectFieldOrdering_NoWarning()
    {
        var config = CreateTestConfig();
        config.Steps = new List<FormStepDto>
        {
            new FormStepDto
            {
                StepName = "Step 1",
                Fields = new List<FormFieldDto>
                {
                    new FormFieldDto { Id = "20", Label = "Dependency Field" },  // First (correct)
                    new FormFieldDto { Id = "10", Label = "Dependent Field" }  // Second
                }
            }
        };

        var rules = new List<FieldValidationRule>
        {
            new FieldValidationRule
            {
                Id = 1,
                FormFieldId = 10,
                DependsOnFieldId = 20 // Dependency comes before dependent (correct)
            }
        };

        _formConfigService.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(config);

        _metadataService.Setup(m => m.GetEntityMetadata("Town"))
            .Returns(CreateMockMetadata());

        _ruleRepository.Setup(r => r.GetByFormConfigurationIdAsync(1))
            .ReturnsAsync(rules);

        var result = await _service.CheckConfigurationHealthAsync(1);

        result.Should().NotContain(i => i.Message.Contains("comes after it"));
    }

    [Fact]
    public async Task CheckHealth_WithOrderingAcrossSteps_ReturnsWarning()
    {
        var config = CreateTestConfig();
        config.Steps = new List<FormStepDto>
        {
            new FormStepDto
            {
                StepName = "Step 1",
                Fields = new List<FormFieldDto>
                {
                    new FormFieldDto { Id = "10", Label = "Dependent Field" }
                }
            },
            new FormStepDto
            {
                StepName = "Step 2",
                Fields = new List<FormFieldDto>
                {
                    new FormFieldDto { Id = "20", Label = "Dependency Field" }
                }
            }
        };

        var rules = new List<FieldValidationRule>
        {
            new FieldValidationRule
            {
                Id = 1,
                FormFieldId = 10, // In step 1
                DependsOnFieldId = 20 // In step 2 (comes later - wrong!)
            }
        };

        _formConfigService.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(config);

        _metadataService.Setup(m => m.GetEntityMetadata("Town"))
            .Returns(CreateMockMetadata());

        _ruleRepository.Setup(r => r.GetByFormConfigurationIdAsync(1))
            .ReturnsAsync(rules);

        var result = await _service.CheckConfigurationHealthAsync(1);

        result.Should().Contain(i =>
            i.Severity == "Warning" &&
            i.Message.Contains("comes after it"));
    }

    #endregion

    #region Comprehensive Health Check

    [Fact]
    public async Task CheckHealth_WithHealthyConfig_ReturnsEmpty()
    {
        var config = CreateTestConfig();
        config.Steps = new List<FormStepDto>
        {
            new FormStepDto
            {
                StepName = "Step 1",
                Fields = new List<FormFieldDto>
                {
                    new FormFieldDto { Id = "1", FieldName = "Name", IsRequired = true }
                }
            }
        };

        var metadata = new EntityMetadataDto
        {
            EntityName = "Town",
            Fields = new List<FieldMetadataDto>
            {
                new FieldMetadataDto { FieldName = "Name", IsNullable = false, HasDefaultValue = false }
            }
        };

        _formConfigService.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(config);

        _metadataService.Setup(m => m.GetEntityMetadata("Town"))
            .Returns(metadata);

        _ruleRepository.Setup(r => r.GetByFormConfigurationIdAsync(1))
            .ReturnsAsync(new List<FieldValidationRule>());

        var result = await _service.CheckConfigurationHealthAsync(1);

        result.Should().BeEmpty();
    }

    #endregion

    #region Helper Methods

    private FormConfigurationDto CreateTestConfig()
    {
        return new FormConfigurationDto
        {
            Id = "1",
            EntityTypeName = "Town",
            ConfigurationName = "Test Form",
            Steps = new List<FormStepDto>()
        };
    }

    private EntityMetadataDto CreateMockMetadata()
    {
        return new EntityMetadataDto
        {
            EntityName = "Town",
            Fields = new List<FieldMetadataDto>
            {
                new FieldMetadataDto { FieldName = "Name", FieldType = "string" },
                new FieldMetadataDto { FieldName = "WgRegionId", FieldType = "string" }
            }
        };
    }

    #endregion
}

