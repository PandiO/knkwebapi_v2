using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using knkwebapi_v2.Models;
using knkwebapi_v2.Repositories;
using knkwebapi_v2.Services;
using knkwebapi_v2.Services.Interfaces;
using knkwebapi_v2.Services.ValidationMethods;
using knkwebapi_v2.Properties;
using knkwebapi_v2.Dtos;
using AutoMapper;
using Microsoft.Extensions.Logging;
using Moq;

namespace knkwebapi_v2.Tests.Integration
{
    /// <summary>
    /// End-to-end integration tests for multi-layer dependency resolution.
    /// These tests cover complete workflows from rule creation through validation execution.
    /// 
    /// Test Coverage:
    /// - Path creation and validation workflows
    /// - Circular dependency detection
    /// - Field ordering requirements
    /// - Multi-step form flows
    /// - Error recovery
    /// - Plugin execution with resolved dependencies
    /// </summary>
    public class DependencyResolutionE2ETests : IDisposable
    {
        private readonly KnKDbContext _context;
        private readonly IPathResolutionService _pathResolutionService;
        private readonly IDependencyResolutionService _dependencyResolutionService;
        private readonly IValidationService _validationService;
        private readonly IFieldValidationRuleRepository _ruleRepository;
        private readonly IFormFieldRepository _fieldRepository;
        private readonly IFormConfigurationRepository _configRepository;
        private readonly IFormStepRepository _formStepRepository;
        private readonly IMapper _mapper;

        public DependencyResolutionE2ETests()
        {
            var services = new ServiceCollection();

            // Setup DbContext
            var options = new DbContextOptionsBuilder<KnKDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new KnKDbContext(options);

            // Setup AutoMapper
            var mapperConfig = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<FieldValidationRule, FieldValidationRuleDto>().ReverseMap();
                cfg.CreateMap<FormField, FormFieldDto>().ReverseMap();
                cfg.CreateMap<FormConfiguration, FormConfigurationDto>().ReverseMap();
                cfg.CreateMap<FormStep, FormStepDto>().ReverseMap();
            });
            _mapper = mapperConfig.CreateMapper();

            // Setup repositories
            _ruleRepository = new FieldValidationRuleRepository(_context);
            _fieldRepository = new FormFieldRepository(_context);
            _configRepository = new FormConfigurationRepository(_context);
            _formStepRepository = new FormStepRepository(_context);

            // Setup mock logger
            var mockLogger = new Mock<ILogger<PathResolutionService>>();
            var mockMetadataService = new Mock<IMetadataService>();

            // Setup path resolution service
            _pathResolutionService = new PathResolutionService(mockLogger.Object, mockMetadataService.Object);

            // Setup dependency resolution service
            _dependencyResolutionService = new DependencyResolutionService(
                _ruleRepository,
                _fieldRepository,
                _configRepository,
                _pathResolutionService,
                mockLogger.Object
            );

            // Setup validation service
            var conditionalValidator = new ConditionalRequiredValidator();
            var validationMethods = new List<IValidationMethod> { conditionalValidator };
            
            _validationService = new ValidationService(
                _ruleRepository,
                _fieldRepository,
                _configRepository,
                validationMethods,
                _mapper
            );
        }

        public void Dispose()
        {
            _context?.Dispose();
        }

        #region Test Data Helpers

        private FormConfiguration SeedDistrictCreationForm()
        {
            var config = new FormConfiguration
            {
                Id = 1,
                Name = "District Creation Form",
                Description = "Complete flow for creating a new district with location validation"
            };

            var step1 = new FormStep
            {
                Id = 1,
                FormConfigurationId = 1,
                Title = "Step 1: Select Town",
                Order = 0,
                Fields = new List<FormField>
                {
                    new()
                    {
                        Id = 1,
                        Label = "Town",
                        FormStepId = 1,
                        FieldType = "select",
                        Order = 0,
                        Required = true,
                        EntityType = "Town"
                    }
                }
            };

            var step2 = new FormStep
            {
                Id = 2,
                FormConfigurationId = 1,
                Title = "Step 2: Basic Info",
                Order = 1,
                Fields = new List<FormField>
                {
                    new()
                    {
                        Id = 2,
                        Label = "District Name",
                        FormStepId = 2,
                        FieldType = "text",
                        Order = 0,
                        Required = true
                    }
                }
            };

            var step3 = new FormStep
            {
                Id = 3,
                FormConfigurationId = 1,
                Title = "Step 3: Location",
                Order = 2,
                Fields = new List<FormField>
                {
                    new()
                    {
                        Id = 3,
                        Label = "Location",
                        FormStepId = 3,
                        FieldType = "location",
                        Order = 0,
                        Required = true,
                        DependsOnField = 1 // Depends on Town selection
                    }
                }
            };

            _context.FormConfigurations.Add(config);
            _context.FormSteps.AddRange(step1, step2, step3);
            _context.SaveChanges();

            return config;
        }

        private void SeedValidationRules(int configId)
        {
            var rules = new List<FieldValidationRule>
            {
                new()
                {
                    Id = 1,
                    FormConfigurationId = configId,
                    FormFieldId = 3, // Location field
                    ValidationType = "LocationInsideRegion",
                    DependencyPath = "Town.wgRegionId", // Depends on Town selection
                    IsActive = true,
                    ErrorMessage = "Location {coordinates} is outside region {regionName}",
                    ConfigJson = "{\"checkRadius\": 100}",
                    CreatedAt = DateTime.UtcNow
                }
            };

            _context.FieldValidationRules.AddRange(rules);
            _context.SaveChanges();
        }

        #endregion

        #region E2E Workflow Tests

        [Fact]
        public async Task E2E_DistrictCreation_WithLocationValidation_HappyPath()
        {
            // Arrange
            var formConfig = SeedDistrictCreationForm();
            SeedValidationRules(formConfig.Id);

            var formContext = new Dictionary<string, object>
            {
                { "Town", new { Id = 1, WgRegionId = "town_1", Name = "Springfield" } },
                { "Location", new { X = 100, Y = 64, Z = -200 } }
            };

            // Act - Resolve dependencies
            var resolutionRequest = new DependencyResolutionRequest
            {
                FieldIds = new List<int> { 3 }, // Location field
                FormConfigurationId = formConfig.Id
            };

            var resolutionResponse = await _dependencyResolutionService.ResolveDependenciesAsync(resolutionRequest, formContext);

            // Assert
            resolutionResponse.Should().NotBeNull();
            resolutionResponse.ResolvedDependencies.Should().HaveCount(1);
            
            var resolvedDep = resolutionResponse.ResolvedDependencies.First();
            resolvedDep.RuleId.Should().Be(1);
            resolvedDep.Status.Should().Be("resolved");
            resolvedDep.DependencyFieldValue.Should().Be("town_1");
        }

        [Fact]
        public async Task E2E_CircularDependencyDetection_BlocksConfiguration()
        {
            // Arrange
            var config = SeedDistrictCreationForm();
            
            // Create circular dependency: A depends on B, B depends on A
            var circularRule1 = new FieldValidationRule
            {
                Id = 10,
                FormConfigurationId = config.Id,
                FormFieldId = 2, // District Name
                ValidationType = "ConditionalRequired",
                DependencyPath = "Location.someProperty", // Depends on Location
                IsActive = true,
                ConfigJson = "{}"
            };

            var circularRule2 = new FieldValidationRule
            {
                Id = 11,
                FormConfigurationId = config.Id,
                FormFieldId = 3, // Location
                ValidationType = "LocationInsideRegion",
                DependencyPath = "DistrictName.someProperty", // Circular: depends on District Name
                IsActive = true,
                ConfigJson = "{}"
            };

            _context.FieldValidationRules.AddRange(circularRule1, circularRule2);
            _context.SaveChanges();

            // Act
            var validationRequest = new DependencyResolutionRequest
            {
                FieldIds = new List<int> { 2, 3 },
                FormConfigurationId = config.Id
            };

            var formContext = new Dictionary<string, object>();

            // Assert - Should detect circular dependency
            var result = await _dependencyResolutionService.ResolveDependenciesAsync(validationRequest, formContext);
            result.HasErrors.Should().BeTrue();
            result.ErrorSummary.Should().Contain("circular", StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task E2E_FieldOrderingValidation_WarnsDependencyAfterDependent()
        {
            // Arrange
            var config = SeedDistrictCreationForm();
            
            // Location field (ID 3, Step 3) depends on Town field (ID 1, Step 1)
            // This is correct order, should NOT warn
            var rule = new FieldValidationRule
            {
                Id = 1,
                FormConfigurationId = config.Id,
                FormFieldId = 3, // Location (Step 3)
                ValidationType = "LocationInsideRegion",
                DependencyPath = "Town.wgRegionId", // Depends on Town (Step 1)
                IsActive = true,
                ConfigJson = "{}"
            };

            _context.FieldValidationRules.Add(rule);
            _context.SaveChanges();

            // Act
            var healthCheck = await _dependencyResolutionService.ValidateFormConfigurationAsync(config.Id);

            // Assert
            healthCheck.FieldOrderingIssues.Should().HaveCount(0);
        }

        [Fact]
        public async Task E2E_FieldOrderingValidation_ErrorsWhenDependencyIsAfter()
        {
            // Arrange
            var config = SeedDistrictCreationForm();
            
            // Reverse the field order: Step 2 depends on Step 3 (incorrect)
            var rule = new FieldValidationRule
            {
                Id = 1,
                FormConfigurationId = config.Id,
                FormFieldId = 2, // District Name (Step 2)
                ValidationType = "LocationInsideRegion",
                DependencyPath = "Location.someProperty", // Depends on Location (Step 3)
                IsActive = true,
                ConfigJson = "{}"
            };

            _context.FieldValidationRules.Add(rule);
            _context.SaveChanges();

            // Act
            var healthCheck = await _dependencyResolutionService.ValidateFormConfigurationAsync(config.Id);

            // Assert
            healthCheck.FieldOrderingIssues.Should().HaveCount(1);
            healthCheck.FieldOrderingIssues.First().Should().Contain("Step 3");
        }

        [Fact]
        public async Task E2E_MultiStepFormFlow_WithMultipleDependencies()
        {
            // Arrange
            var config = SeedDistrictCreationForm();
            
            // Create multiple interdependent validation rules
            var rule1 = new FieldValidationRule
            {
                Id = 1,
                FormConfigurationId = config.Id,
                FormFieldId = 3, // Location
                ValidationType = "LocationInsideRegion",
                DependencyPath = "Town.wgRegionId",
                IsActive = true,
                ErrorMessage = "Location outside region {regionName}",
                ConfigJson = "{}"
            };

            var rule2 = new FieldValidationRule
            {
                Id = 2,
                FormConfigurationId = config.Id,
                FormFieldId = 2, // District Name
                ValidationType = "ConditionalRequired",
                DependencyPath = "Town.wgRegionId",
                IsActive = true,
                ErrorMessage = "District name required for region {regionName}",
                ConfigJson = "{}"
            };

            _context.FieldValidationRules.AddRange(rule1, rule2);
            _context.SaveChanges();

            var formContext = new Dictionary<string, object>
            {
                { "Town", new { Id = 1, WgRegionId = "town_1", Name = "Springfield" } }
            };

            // Act - Validate multiple fields
            var validationRequest = new DependencyResolutionRequest
            {
                FieldIds = new List<int> { 2, 3 },
                FormConfigurationId = config.Id
            };

            var response = await _dependencyResolutionService.ResolveDependenciesAsync(validationRequest, formContext);

            // Assert - Both dependencies should be resolved
            response.ResolvedDependencies.Should().HaveCount(2);
            response.ResolvedDependencies.Should().AllSatisfy(d => 
                d.Status.Should().Be("resolved")
            );
        }

        [Fact]
        public async Task E2E_ErrorRecovery_FromInvalidDependencyPath()
        {
            // Arrange
            var config = SeedDistrictCreationForm();
            
            var invalidRule = new FieldValidationRule
            {
                Id = 1,
                FormConfigurationId = config.Id,
                FormFieldId = 3,
                ValidationType = "LocationInsideRegion",
                DependencyPath = "Town.nonExistentProperty", // Invalid path
                IsActive = true,
                ConfigJson = "{}"
            };

            _context.FieldValidationRules.Add(invalidRule);
            _context.SaveChanges();

            var formContext = new Dictionary<string, object>
            {
                { "Town", new { Id = 1 } }
            };

            // Act
            var validationRequest = new DependencyResolutionRequest
            {
                FieldIds = new List<int> { 3 },
                FormConfigurationId = config.Id
            };

            var response = await _dependencyResolutionService.ResolveDependenciesAsync(validationRequest, formContext);

            // Assert - Should return error status (not throw)
            response.ResolvedDependencies.Should().HaveCount(1);
            var dependency = response.ResolvedDependencies.First();
            dependency.Status.Should().Be("error");
            dependency.ErrorDetail.Should().Contain("nonExistentProperty", StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task E2E_ErrorRecovery_FromNullDependencyValue()
        {
            // Arrange
            var config = SeedDistrictCreationForm();
            
            var rule = new FieldValidationRule
            {
                Id = 1,
                FormConfigurationId = config.Id,
                FormFieldId = 3,
                ValidationType = "LocationInsideRegion",
                DependencyPath = "Town.wgRegionId",
                IsActive = true,
                ConfigJson = "{}"
            };

            _context.FieldValidationRules.Add(rule);
            _context.SaveChanges();

            var formContext = new Dictionary<string, object>
            {
                { "Town", null } // Null dependency value
            };

            // Act
            var validationRequest = new DependencyResolutionRequest
            {
                FieldIds = new List<int> { 3 },
                FormConfigurationId = config.Id
            };

            var response = await _dependencyResolutionService.ResolveDependenciesAsync(validationRequest, formContext);

            // Assert - Should handle gracefully
            response.ResolvedDependencies.Should().HaveCount(1);
            var dependency = response.ResolvedDependencies.First();
            dependency.Status.Should().Be("pending"); // Pending user input
        }

        [Fact]
        public async Task E2E_WorldTaskIntegration_IncludesResolvedDependencies()
        {
            // Arrange
            var config = SeedDistrictCreationForm();
            SeedValidationRules(config.Id);

            var formContext = new Dictionary<string, object>
            {
                { "Town", new { Id = 1, WgRegionId = "town_1", Name = "Springfield" } },
                { "Location", new { X = 100, Y = 64, Z = -200 } }
            };

            // Act - Simulate WorldTask creation
            var validationRequest = new DependencyResolutionRequest
            {
                FieldIds = new List<int> { 3 },
                FormConfigurationId = config.Id
            };

            var resolutionResponse = await _dependencyResolutionService.ResolveDependenciesAsync(validationRequest, formContext);

            // Create WorldTask input with resolved dependencies
            var worldTaskInput = new
            {
                TaskType = "CreateDistrict",
                FieldValues = new
                {
                    TownId = 1,
                    DistrictName = "New District",
                    Location = new { X = 100, Y = 64, Z = -200 }
                },
                ValidationContext = new
                {
                    ResolvedDependencies = resolutionResponse.ResolvedDependencies.ToDictionary(
                        r => r.RuleId,
                        r => new { r.DependencyFieldValue, r.Status }
                    )
                }
            };

            // Assert
            worldTaskInput.Should().NotBeNull();
            worldTaskInput.ValidationContext.Should().NotBeNull();
            ((Dictionary<int, dynamic>)worldTaskInput.ValidationContext.ResolvedDependencies)
                .Should().HaveCount(1);
        }

        #endregion

        #region Configuration Health Check Tests

        [Fact]
        public async Task ConfigurationHealthCheck_AllValidationsPass()
        {
            // Arrange
            var config = SeedDistrictCreationForm();
            SeedValidationRules(config.Id);

            // Act
            var healthCheck = await _dependencyResolutionService.ValidateFormConfigurationAsync(config.Id);

            // Assert
            healthCheck.IsHealthy.Should().BeTrue();
            healthCheck.PropertyExistenceIssues.Should().HaveCount(0);
            healthCheck.FieldOrderingIssues.Should().HaveCount(0);
            healthCheck.CircularDependencyIssues.Should().HaveCount(0);
        }

        [Fact]
        public async Task ConfigurationHealthCheck_ReturnsDetailedIssueInfo()
        {
            // Arrange
            var config = SeedDistrictCreationForm();
            
            var invalidRule = new FieldValidationRule
            {
                Id = 1,
                FormConfigurationId = config.Id,
                FormFieldId = 3,
                ValidationType = "LocationInsideRegion",
                DependencyPath = "InvalidEntity.invalidProperty",
                IsActive = true,
                ConfigJson = "{}"
            };

            _context.FieldValidationRules.Add(invalidRule);
            _context.SaveChanges();

            // Act
            var healthCheck = await _dependencyResolutionService.ValidateFormConfigurationAsync(config.Id);

            // Assert
            healthCheck.IsHealthy.Should().BeFalse();
            healthCheck.PropertyExistenceIssues.Should().NotBeEmpty();
            var issue = healthCheck.PropertyExistenceIssues.First();
            issue.Should().Contain("InvalidEntity", StringComparison.OrdinalIgnoreCase);
        }

        #endregion

        #region Region Containment Validation Tests (Phase 8)

        /// <summary>
        /// Tests the complete District creation flow with region containment validation.
        /// This test replicates the exact 11-step manual scenario that was failing:
        /// 
        /// Scenario:
        /// 1. Admin opens District creation form (FormConfig ID 2)
        /// 2. Proceeds to Step 5 (FormStep ID 5)
        /// 3. Step 5 has three fields: Town (ID 11), WgRegionId (ID 9), Location (ID 10)
        /// 4. Field 9 (WgRegionId) has validation rules requiring region containment
        /// 5. Initially all fields empty - validation pending
        /// 6. Admin fills Town field with existing town (has WgRegionId)
        /// 7. Admin completes WorldTask for WgRegionId - fills with child region
        /// 8. Validation should verify child region is inside parent (Town.WgRegionId)
        /// 9. Admin fills Location field
        /// 10. All validations should still pass (dependency value must be preserved)
        /// 11. Admin clicks Next - form should progress to next step
        /// 
        /// Bug Reproduced: After step 9, validation would fail with null dependencyValue
        /// despite formContextData containing the Town object.
        /// 
        /// Fix: ValidationService now auto-extracts dependency values from formContextData.
        /// </summary>
        [Fact]
        public async Task DistrictCreation_WithRegionContainment_ValidatesDependencyConsistently()
        {
            // Arrange - Seed database with form configuration
            var config = SeedDistrictFormWithRegionValidation();
            var townField = config.Steps.First(s => s.Id == 5).Fields.First(f => f.Id == 11);
            var wgRegionField = config.Steps.First(s => s.Id == 5).Fields.First(f => f.Id == 9);
            var locationField = config.Steps.First(s => s.Id == 5).Fields.First(f => f.Id == 10);

            // Create mock validation method for region containment
            var mockRegionValidator = new Mock<IValidationMethod>();
            mockRegionValidator.Setup(v => v.ValidationType).Returns("RegionContainment");
            mockRegionValidator.Setup(v => v.ValidateAsync(
                It.IsAny<object?>(),
                It.IsAny<object?>(),
                It.IsAny<Dictionary<string, object>?>(),
                It.IsAny<string?>()
            )).ReturnsAsync((object? fieldValue, object? dependencyValue, Dictionary<string, object>? context, string? config) =>
            {
                // Simulate region containment check
                if (dependencyValue == null) return new ValidationResultDto { IsValid = false, Message = "Parent region not specified" };
                if (fieldValue == null) return new ValidationResultDto { IsValid = false, Message = "Child region not specified" };
                
                // Extract parent region ID from Town object
                var depDict = dependencyValue as Dictionary<string, object>;
                var parentRegionId = depDict?["wgRegionId"] as string;
                var childRegionId = fieldValue as string;

                if (string.IsNullOrEmpty(parentRegionId)) 
                    return new ValidationResultDto { IsValid = false, Message = "Parent region ID not found" };
                
                // Simulate successful containment check (in reality, calls Minecraft plugin HTTP API)
                return new ValidationResultDto { IsValid = true, Message = "Region containment validated" };
            });

            var validationService = new ValidationService(
                _ruleRepository,
                _fieldRepository,
                _configRepository,
                new List<IValidationMethod> { mockRegionValidator.Object },
                _mapper
            );

            // Step 1-5: Navigate to Step 5
            var formContextData = new Dictionary<string, object>();

            // Step 6: Fill Town field (ID 11) with existing Town
            var townData = new Dictionary<string, object>
            {
                { "id", 3 },
                { "name", "Cinix" },
                { "description", "Test Town" },
                { "wgRegionId", "tempregion_worldtask_17" }
            };
            formContextData["Town"] = townData;

            // Validation 1: WgRegionId still empty, dependencyValue should be Town object
            var validation1 = await validationService.ValidateFieldAsync(
                wgRegionField.Id,
                fieldValue: null,
                dependencyValue: null, // Frontend doesn't send this
                formContextData: formContextData
            );

            // Should fail because WgRegionId is null
            validation1.Should().NotBeNull();
            validation1.IsValid.Should().BeFalse();
            validation1.Message.Should().Contain("Child region not specified");

            // Step 7: Admin completes WorldTask for WgRegionId field
            var childRegionId = "tempregion_worldtask_74";
            formContextData["WgRegionId"] = childRegionId;

            // Validation 2: WgRegionId filled, dependencyValue should be extracted from formContextData
            var validation2 = await validationService.ValidateFieldAsync(
                wgRegionField.Id,
                fieldValue: childRegionId,
                dependencyValue: null, // Frontend doesn't send this - backend must extract it
                formContextData: formContextData
            );

            // Should pass - region containment validated
            validation2.Should().NotBeNull();
            validation2.IsValid.Should().BeTrue();
            validation2.Message.Should().Contain("Region containment validated");

            // Step 9: Admin fills Location field
            formContextData["Location"] = new Dictionary<string, object>
            {
                { "x", 100 },
                { "y", 64 },
                { "z", 200 },
                { "world", "world" }
            };

            // Validation 3: CRITICAL TEST - WgRegionId validation should still pass
            // This was failing with "dependencyValue: null" despite Town being in formContextData
            var validation3 = await validationService.ValidateFieldAsync(
                wgRegionField.Id,
                fieldValue: childRegionId,
                dependencyValue: null, // Frontend doesn't send this - backend must extract it
                formContextData: formContextData
            );

            // Assert - All validations after filling Location should still pass
            validation3.Should().NotBeNull();
            validation3.IsValid.Should().BeTrue("Dependency value should be auto-extracted from formContextData");
            validation3.Message.Should().Contain("Region containment validated");

            // Verify backend extracted dependency correctly
            mockRegionValidator.Verify(v => v.ValidateAsync(
                It.Is<object?>(fv => fv != null && fv.ToString() == childRegionId),
                It.Is<object?>(dv => dv != null), // Dependency value must not be null
                It.IsAny<Dictionary<string, object>?>(),
                It.IsAny<string?>()
            ), Times.AtLeastOnce);
        }

        /// <summary>
        /// Tests case-insensitive property extraction in dependency path resolution.
        /// 
        /// Bug: RegionContainmentValidator used exact string matching for property names,
        /// failing when JSON used camelCase ("wgRegionId") but config specified PascalCase ("WgRegionId").
        /// 
        /// Fix: Use StringComparison.OrdinalIgnoreCase for all property lookups.
        /// </summary>
        [Fact]
        public async Task DependencyExtraction_WithCaseInsensitiveProperties_ExtractsCorrectly()
        {
            // Arrange
            var formContextData = new Dictionary<string, object>
            {
                {
                    "Town", new Dictionary<string, object>
                    {
                        { "id", 3 },
                        { "name", "Cinix" },
                        { "wgRegionId", "tempregion_worldtask_17" } // camelCase in JSON
                    }
                }
            };

            var config = SeedDistrictFormWithRegionValidation();
            var wgRegionField = config.Steps.First(s => s.Id == 5).Fields.First(f => f.Id == 9);

            // Create validation rule with PascalCase property path
            var rule = new FieldValidationRule
            {
                Id = 100,
                FormFieldId = wgRegionField.Id,
                FormConfigurationId = config.Id,
                ValidationType = "RegionContainment",
                DependsOnFieldId = 11, // Town field
                DependencyPath = "Town.WgRegionId", // PascalCase in config
                RequiresDependencyFilled = true,
                IsActive = true,
                IsBlocking = true,
                ConfigJson = "{}"
            };

            _context.FieldValidationRules.Add(rule);
            await _context.SaveChangesAsync();

            // Mock validator that checks dependency value extraction
            var mockValidator = new Mock<IValidationMethod>();
            mockValidator.Setup(v => v.ValidationType).Returns("RegionContainment");
            mockValidator.Setup(v => v.ValidateAsync(
                It.IsAny<object?>(),
                It.IsAny<object?>(),
                It.IsAny<Dictionary<string, object>?>(),
                It.IsAny<string?>()
            )).ReturnsAsync((object? fv, object? dv, Dictionary<string, object>? ctx, string? cfg) =>
            {
                // Verify dependency value was extracted despite case mismatch
                if (dv == null) return new ValidationResultDto { IsValid = false, Message = "Dependency not extracted" };
                
                var depDict = dv as Dictionary<string, object>;
                var regionId = depDict?["wgRegionId"] as string;
                
                if (regionId != "tempregion_worldtask_17")
                    return new ValidationResultDto { IsValid = false, Message = $"Wrong region extracted: {regionId}" };
                
                return new ValidationResultDto { IsValid = true, Message = "Case-insensitive extraction successful" };
            });

            var validationService = new ValidationService(
                _ruleRepository,
                _fieldRepository,
                _configRepository,
                new List<IValidationMethod> { mockValidator.Object },
                _mapper
            );

            // Act - Validate with camelCase JSON data and PascalCase config path
            var result = await validationService.ValidateFieldAsync(
                wgRegionField.Id,
                fieldValue: "tempregion_worldtask_74",
                dependencyValue: null,
                formContextData: formContextData
            );

            // Assert - Extraction should work despite case mismatch
            result.Should().NotBeNull();
            result.IsValid.Should().BeTrue("Case-insensitive property extraction should succeed");
            result.Message.Should().Contain("Case-insensitive extraction successful");
        }

        private FormConfiguration SeedDistrictFormWithRegionValidation()
        {
            var config = new FormConfiguration
            {
                Id = 2,
                Name = "District Management",
                Description = "Create and manage districts with region containment validation"
            };

            var step5 = new FormStep
            {
                Id = 5,
                FormConfigurationId = 2,
                Title = "Step 5: Location & Region",
                Order = 4,
                Fields = new List<FormField>()
            };

            var townField = new FormField
            {
                Id = 11,
                FormStepId = 5,
                FieldName = "Town",
                Label = "Parent Town",
                FieldType = "select",
                EntityType = "Town",
                Required = true,
                Order = 0
            };

            var wgRegionField = new FormField
            {
                Id = 9,
                FormStepId = 5,
                FieldName = "WgRegionId",
                Label = "WorldGuard Region",
                FieldType = "worldtask",
                WorldTaskType = "wg-region-id",
                Required = true,
                Order = 1
            };

            var locationField = new FormField
            {
                Id = 10,
                FormStepId = 5,
                FieldName = "Location",
                Label = "District Center",
                FieldType = "worldtask",
                WorldTaskType = "location",
                Required = true,
                Order = 2
            };

            step5.Fields.Add(townField);
            step5.Fields.Add(wgRegionField);
            step5.Fields.Add(locationField);
            config.Steps = new List<FormStep> { step5 };

            _context.FormConfigurations.Add(config);
            _context.FormSteps.Add(step5);
            _context.FormFields.AddRange(townField, wgRegionField, locationField);

            // Add validation rule for region containment
            var rule = new FieldValidationRule
            {
                Id = 99,
                FormFieldId = 9,
                FormConfigurationId = 2,
                ValidationType = "RegionContainment",
                DependsOnFieldId = 11,
                DependencyPath = "Town.WgRegionId",
                RequiresDependencyFilled = true,
                IsActive = true,
                IsBlocking = true,
                ConfigJson = "{\"requireFullContainment\":true}"
            };

            _context.FieldValidationRules.Add(rule);
            _context.SaveChanges();

            return config;
        }

        #endregion
    }
}
