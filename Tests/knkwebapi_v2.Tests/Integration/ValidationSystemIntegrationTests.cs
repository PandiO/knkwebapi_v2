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
using AutoMapper;

namespace knkwebapi_v2.Tests.Integration
{
    /// <summary>
    /// End-to-end integration tests for the field validation system.
    /// Tests complete workflows from rule creation through validation execution.
    /// </summary>
    public class ValidationSystemIntegrationTests
    {
        private readonly KnKDbContext _context;
        private readonly IValidationService _validationService;
        private readonly IFieldValidationRuleRepository _ruleRepository;
        private readonly IFormFieldRepository _fieldRepository;
        private readonly IFormConfigurationRepository _configRepository;

        public ValidationSystemIntegrationTests()
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
                // Add your mapping profiles here
                cfg.CreateMap<FieldValidationRule, Dtos.FieldValidationRuleDto>().ReverseMap();
            });
            var mapper = mapperConfig.CreateMapper();

            // Register repositories
            _ruleRepository = new FieldValidationRuleRepository(_context);
            _fieldRepository = new FormFieldRepository(_context);
            _configRepository = new FormConfigurationRepository(_context);

            // Register validation methods
            var conditionalValidator = new ConditionalRequiredValidator();
            var validationMethods = new List<IValidationMethod> { conditionalValidator };

            // Create ValidationService
            _validationService = new ValidationService(
                _ruleRepository,
                _fieldRepository,
                _configRepository,
                validationMethods,
                mapper
            );
        }

        private void SeedTestFormConfiguration()
        {
            var config = new FormConfiguration
            {
                Id = 1,
                Name = "District Creation Form",
                Description = "Form for creating a new district"
            };

            var step1 = new FormStep
            {
                Id = 1,
                FormConfigurationId = 1,
                Title = "Basic Info",
                Order = 0,
                Fields = new List<FormField>
                {
                    new() { Id = 1, Label = "District Name", FormStepId = 1, FieldType = "text", Order = 0, Required = true },
                    new() { Id = 2, Label = "Town", FormStepId = 1, FieldType = "number", Order = 1, Required = true },
                    new() { Id = 3, Label = "Location", FormStepId = 1, FieldType = "number", Order = 2, Required = false }
                }
            };

            _context.FormConfigurations.Add(config);
            _context.FormSteps.Add(step1);
            _context.SaveChanges();
        }

        [Fact]
        public async Task CreateValidationRule_ThenValidateField_HappyPath()
        {
            // Arrange
            SeedTestFormConfiguration();
            
            var createRuleDto = new Dtos.CreateFieldValidationRuleDto
            {
                FormFieldId = 3,  // Location field
                ValidationType = "ConditionalRequired",
                DependsOnFieldId = 2,  // Town field
                ConfigJson = "{\"condition\": {\"operator\": \"equals\", \"value\": \"Town\"}}",
                ErrorMessage = "Location is required when Town is selected",
                SuccessMessage = "Location is valid",
                IsBlocking = true,
                RequiresDependencyFilled = true
            };

            // Act - Create the rule
            var createdRule = await _validationService.CreateAsync(createRuleDto);

            // Assert - Rule was created
            createdRule.Should().NotBeNull();
            createdRule.Id.Should().BeGreaterThan(0);
            createdRule.ValidationType.Should().Be("ConditionalRequired");

            // Verify rule exists in database
            var retrievedRule = await _validationService.GetByIdAsync(createdRule.Id);
            retrievedRule.Should().NotBeNull();
        }

        [Fact]
        public async Task ValidationRule_WithCircularDependency_Blocked()
        {
            // Arrange
            SeedTestFormConfiguration();

            // Create first rule: Field 2 depends on Field 3
            var rule1 = new Dtos.CreateFieldValidationRuleDto
            {
                FormFieldId = 2,
                ValidationType = "ConditionalRequired",
                DependsOnFieldId = 3,
                ConfigJson = "{}",
                ErrorMessage = "Error"
            };

            await _validationService.CreateAsync(rule1);

            // Try to create circular dependency: Field 3 depends on Field 2
            var rule2 = new Dtos.CreateFieldValidationRuleDto
            {
                FormFieldId = 3,
                ValidationType = "ConditionalRequired",
                DependsOnFieldId = 2,
                ConfigJson = "{}",
                ErrorMessage = "Error"
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(
                () => _validationService.CreateAsync(rule2)
            );
            exception.Message.Should().Contain("Circular dependency");
        }

        [Fact]
        public async Task GetFormFieldRules_ReturnsAllRulesForField()
        {
            // Arrange
            SeedTestFormConfiguration();

            var rule1 = new Dtos.CreateFieldValidationRuleDto
            {
                FormFieldId = 3,
                ValidationType = "ConditionalRequired",
                DependsOnFieldId = 2,
                ConfigJson = "{}",
                ErrorMessage = "Error 1"
            };

            var rule2 = new Dtos.CreateFieldValidationRuleDto
            {
                FormFieldId = 3,
                ValidationType = "LocationInsideRegion",
                DependsOnFieldId = 2,
                ConfigJson = "{}",
                ErrorMessage = "Error 2"
            };

            // Act
            await _validationService.CreateAsync(rule1);
            await _validationService.CreateAsync(rule2);
            var rulesForField = await _validationService.GetByFormFieldIdAsync(3);

            // Assert
            rulesForField.Should().HaveCount(2);
            rulesForField.Should().Contain(r => r.ValidationType == "ConditionalRequired");
            rulesForField.Should().Contain(r => r.ValidationType == "LocationInsideRegion");
        }

        [Fact]
        public async Task ConfigurationHealthCheck_WithValidConfiguration_ReturnsNoIssues()
        {
            // Arrange
            SeedTestFormConfiguration();

            var rule = new Dtos.CreateFieldValidationRuleDto
            {
                FormFieldId = 3,  // Location
                ValidationType = "ConditionalRequired",
                DependsOnFieldId = 2,  // Town (comes before Location)
                ConfigJson = "{}",
                ErrorMessage = "Error"
            };

            await _validationService.CreateAsync(rule);

            // Act
            var healthIssues = await _validationService.PerformConfigurationHealthCheckAsync(1);

            // Assert
            healthIssues.Should().BeEmpty();
        }

        [Fact]
        public async Task ConfigurationHealthCheck_WithBrokenDependency_ReturnError()
        {
            // Arrange
            SeedTestFormConfiguration();

            // Manually create a rule with non-existent dependency to simulate broken reference
            var invalidRule = new FieldValidationRule
            {
                FormFieldId = 3,
                ValidationType = "ConditionalRequired",
                DependsOnFieldId = 999,  // Non-existent field
                ConfigJson = "{}",
                ErrorMessage = "Error"
            };

            _context.FieldValidationRules.Add(invalidRule);
            _context.SaveChanges();

            // Act
            var healthIssues = await _validationService.PerformConfigurationHealthCheckAsync(1);

            // Assert
            healthIssues.Should().NotBeEmpty();
            healthIssues.Should().Contain(i => i.Severity == "Error");
        }

        [Fact]
        public async Task ConfigurationHealthCheck_WithWrongFieldOrder_ReturnWarning()
        {
            // Arrange
            SeedTestFormConfiguration();

            // Create rule where dependency comes AFTER dependent field
            var invalidRule = new FieldValidationRule
            {
                FormFieldId = 2,  // Town (order 1)
                ValidationType = "ConditionalRequired",
                DependsOnFieldId = 3,  // Location (order 2)
                ConfigJson = "{}",
                ErrorMessage = "Error"
            };

            _context.FieldValidationRules.Add(invalidRule);
            _context.SaveChanges();

            // Act
            var healthIssues = await _validationService.PerformConfigurationHealthCheckAsync(1);

            // Assert
            healthIssues.Should().NotBeEmpty();
            healthIssues.Should().Contain(i => i.Severity == "Warning");
        }

        [Fact]
        public async Task UpdateRule_ThenValidateField_ReflectsChanges()
        {
            // Arrange
            SeedTestFormConfiguration();

            var createDto = new Dtos.CreateFieldValidationRuleDto
            {
                FormFieldId = 3,
                ValidationType = "ConditionalRequired",
                DependsOnFieldId = 2,
                ConfigJson = "{}",
                ErrorMessage = "Original error"
            };

            var created = await _validationService.CreateAsync(createDto);
            var ruleId = created.Id;

            // Act - Update the rule
            var updateDto = new Dtos.UpdateFieldValidationRuleDto
            {
                ValidationType = "LocationInsideRegion",
                ErrorMessage = "Updated error"
            };

            await _validationService.UpdateAsync(ruleId, updateDto);

            // Assert - Changes persisted
            var updated = await _validationService.GetByIdAsync(ruleId);
            updated.Should().NotBeNull();
            updated!.ValidationType.Should().Be("LocationInsideRegion");
            updated.ErrorMessage.Should().Be("Updated error");
        }

        [Fact]
        public async Task DeleteRule_RemovesFromDatabase()
        {
            // Arrange
            SeedTestFormConfiguration();

            var createDto = new Dtos.CreateFieldValidationRuleDto
            {
                FormFieldId = 3,
                ValidationType = "ConditionalRequired",
                DependsOnFieldId = 2,
                ConfigJson = "{}",
                ErrorMessage = "Error"
            };

            var created = await _validationService.CreateAsync(createDto);
            var ruleId = created.Id;

            // Act
            await _validationService.DeleteAsync(ruleId);

            // Assert
            var deleted = await _validationService.GetByIdAsync(ruleId);
            deleted.Should().BeNull();
        }

        [Fact]
        public async Task MultipleFieldValidation_ValidatesAll()
        {
            // Arrange
            SeedTestFormConfiguration();

            var rule1 = new FieldValidationRule
            {
                FormFieldId = 2,
                ValidationType = "ConditionalRequired",
                DependsOnFieldId = 1,
                ConfigJson = "{}",
                ErrorMessage = "Error 1"
            };

            var rule2 = new FieldValidationRule
            {
                FormFieldId = 3,
                ValidationType = "ConditionalRequired",
                DependsOnFieldId = 2,
                ConfigJson = "{}",
                ErrorMessage = "Error 2"
            };

            _context.FieldValidationRules.Add(rule1);
            _context.FieldValidationRules.Add(rule2);
            _context.SaveChanges();

            var fieldIds = new List<int> { 2, 3 };
            var fieldValues = new Dictionary<int, object?> { { 2, "value1" }, { 3, "value2" } };

            // Act
            var results = await _validationService.ValidateMultipleFieldsAsync(fieldIds, fieldValues, null);

            // Assert
            results.Should().HaveCount(2);
            results.Should().ContainKey(2);
            results.Should().ContainKey(3);
        }

        [Fact]
        public async Task GetConfigurationRules_ReturnsAllRulesInConfiguration()
        {
            // Arrange
            SeedTestFormConfiguration();

            var rule1 = new Dtos.CreateFieldValidationRuleDto
            {
                FormFieldId = 1,
                ValidationType = "ConditionalRequired",
                DependsOnFieldId = 2,
                ConfigJson = "{}",
                ErrorMessage = "Error 1"
            };

            var rule2 = new Dtos.CreateFieldValidationRuleDto
            {
                FormFieldId = 2,
                ValidationType = "LocationInsideRegion",
                DependsOnFieldId = 1,
                ConfigJson = "{}",
                ErrorMessage = "Error 2"
            };

            // Act
            await _validationService.CreateAsync(rule1);
            await _validationService.CreateAsync(rule2);
            var configRules = await _validationService.GetByFormConfigurationIdAsync(1);

            // Assert
            configRules.Should().HaveCount(2);
        }
    }
}
