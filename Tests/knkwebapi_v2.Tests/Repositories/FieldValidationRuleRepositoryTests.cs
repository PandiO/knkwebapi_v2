using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using FluentAssertions;
using Moq;
using Microsoft.EntityFrameworkCore;
using knkwebapi_v2.Models;
using knkwebapi_v2.Repositories;
using knkwebapi_v2.Properties;

namespace knkwebapi_v2.Tests.Repositories
{
    /// <summary>
    /// Integration tests for FieldValidationRuleRepository CRUD operations
    /// and circular dependency detection logic.
    /// </summary>
    public class FieldValidationRuleRepositoryTests
    {
        private readonly KnKDbContext _context;
        private readonly FieldValidationRuleRepository _repository;

        public FieldValidationRuleRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<KnKDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new KnKDbContext(options);
            _repository = new FieldValidationRuleRepository(_context);
        }

        private void SeedTestData()
        {
            var config = new FormConfiguration { Id = 1, Name = "Test Form", EntityTypeName = "Town" };
            var step = new FormStep { Id = 1, FormConfigurationId = 1, StepName = "Test Step" };
            var field1 = new FormField { Id = 1, FieldName = "Field1", Label = "Field 1", FormStepId = 1 };
            var field2 = new FormField { Id = 2, FieldName = "Field2", Label = "Field 2", FormStepId = 1 };
            var field3 = new FormField { Id = 3, FieldName = "Field3", Label = "Field 3", FormStepId = 1 };

            _context.FormConfigurations.Add(config);
            _context.FormSteps.Add(step);
            _context.FormFields.Add(field1);
            _context.FormFields.Add(field2);
            _context.FormFields.Add(field3);
            _context.SaveChanges();
        }

        [Fact]
        public async Task GetByIdAsync_WithValidId_ReturnsRule()
        {
            // Arrange
            SeedTestData();
            var rule = new FieldValidationRule
            {
                FormFieldId = 1,
                ValidationType = "ConditionalRequired",
                ErrorMessage = "Test error",
                IsBlocking = true
            };
            _context.FieldValidationRules.Add(rule);
            _context.SaveChanges();

            // Act
            var result = await _repository.GetByIdAsync(rule.Id);

            // Assert
            result.Should().NotBeNull();
            result!.ValidationType.Should().Be("ConditionalRequired");
            result.ErrorMessage.Should().Be("Test error");
        }

        [Fact]
        public async Task GetByIdAsync_WithInvalidId_ReturnsNull()
        {
            // Arrange
            SeedTestData();

            // Act
            var result = await _repository.GetByIdAsync(999);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetByFormFieldIdAsync_ReturnsAllRulesForField()
        {
            // Arrange
            SeedTestData();
            var rule1 = new FieldValidationRule
            {
                FormFieldId = 1,
                ValidationType = "ConditionalRequired"
            };
            var rule2 = new FieldValidationRule
            {
                FormFieldId = 1,
                ValidationType = "LocationInsideRegion"
            };
            var rule3 = new FieldValidationRule
            {
                FormFieldId = 2,
                ValidationType = "ConditionalRequired"
            };

            _context.FieldValidationRules.Add(rule1);
            _context.FieldValidationRules.Add(rule2);
            _context.FieldValidationRules.Add(rule3);
            _context.SaveChanges();

            // Act
            var result = await _repository.GetByFormFieldIdAsync(1);

            // Assert
            result.Should().HaveCount(2);
            result.Should().Contain(r => r.ValidationType == "ConditionalRequired");
            result.Should().Contain(r => r.ValidationType == "LocationInsideRegion");
            result.Should().NotContain(r => r.FormFieldId == 2);
        }

        [Fact]
        public async Task GetByFormFieldIdAsync_WithNoRules_ReturnsEmptyList()
        {
            // Arrange
            SeedTestData();

            // Act
            var result = await _repository.GetByFormFieldIdAsync(1);

            // Assert
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetByFormConfigurationIdAsync_ReturnsAllRulesInConfiguration()
        {
            // Arrange
            SeedTestData();
            var rule1 = new FieldValidationRule
            {
                FormFieldId = 1,
                ValidationType = "ConditionalRequired"
            };
            var rule2 = new FieldValidationRule
            {
                FormFieldId = 2,
                ValidationType = "LocationInsideRegion"
            };

            _context.FieldValidationRules.Add(rule1);
            _context.FieldValidationRules.Add(rule2);
            _context.SaveChanges();

            // Act
            var result = await _repository.GetByFormConfigurationIdAsync(1);

            // Assert
            result.Should().HaveCount(2);
        }

        [Fact]
        public async Task CreateAsync_AddsNewRule()
        {
            // Arrange
            SeedTestData();
            var newRule = new FieldValidationRule
            {
                FormFieldId = 1,
                ValidationType = "ConditionalRequired",
                ErrorMessage = "New error",
                IsBlocking = true,
                CreatedAt = DateTime.UtcNow
            };

            // Act
            var created = await _repository.CreateAsync(newRule);

            // Assert
            created.Id.Should().BeGreaterThan(0);
            var retrieved = await _repository.GetByIdAsync(created.Id);
            retrieved.Should().NotBeNull();
            retrieved!.ErrorMessage.Should().Be("New error");
        }

        [Fact]
        public async Task UpdateAsync_ModifiesExistingRule()
        {
            // Arrange
            SeedTestData();
            var rule = new FieldValidationRule
            {
                FormFieldId = 1,
                ValidationType = "ConditionalRequired",
                ErrorMessage = "Original error"
            };
            _context.FieldValidationRules.Add(rule);
            _context.SaveChanges();

            var ruleId = rule.Id;

            // Act
            var retrieved = await _repository.GetByIdAsync(ruleId);
            retrieved!.ErrorMessage = "Updated error";
            await _repository.UpdateAsync(retrieved);

            // Assert
            var updated = await _repository.GetByIdAsync(ruleId);
            updated!.ErrorMessage.Should().Be("Updated error");
        }

        [Fact]
        public async Task DeleteAsync_RemovesRule()
        {
            // Arrange
            SeedTestData();
            var rule = new FieldValidationRule
            {
                FormFieldId = 1,
                ValidationType = "ConditionalRequired"
            };
            _context.FieldValidationRules.Add(rule);
            _context.SaveChanges();
            var ruleId = rule.Id;

            // Act
            await _repository.DeleteAsync(ruleId);

            // Assert
            var retrieved = await _repository.GetByIdAsync(ruleId);
            retrieved.Should().BeNull();
        }

        [Fact]
        public async Task GetRulesDependingOnFieldAsync_ReturnsRulesWithDependency()
        {
            // Arrange
            SeedTestData();
            var rule1 = new FieldValidationRule
            {
                FormFieldId = 1,
                ValidationType = "ConditionalRequired",
                DependsOnFieldId = 2
            };
            var rule2 = new FieldValidationRule
            {
                FormFieldId = 2,
                ValidationType = "ConditionalRequired",
                DependsOnFieldId = 2
            };
            var rule3 = new FieldValidationRule
            {
                FormFieldId = 3,
                ValidationType = "ConditionalRequired",
                DependsOnFieldId = 1
            };

            _context.FieldValidationRules.Add(rule1);
            _context.FieldValidationRules.Add(rule2);
            _context.FieldValidationRules.Add(rule3);
            _context.SaveChanges();

            // Act
            var result = await _repository.GetRulesDependingOnFieldAsync(2);

            // Assert
            result.Should().HaveCount(2);
            result.Should().Contain(r => r.FormFieldId == 1);
            result.Should().Contain(r => r.FormFieldId == 2);
        }

        [Fact]
        public async Task HasCircularDependencyAsync_WithDirectCircle_ReturnsTrue()
        {
            // Arrange
            SeedTestData();
            var rule = new FieldValidationRule
            {
                FormFieldId = 1,
                ValidationType = "ConditionalRequired",
                DependsOnFieldId = 2
            };
            _context.FieldValidationRules.Add(rule);
            _context.SaveChanges();

            // Act - Try to create a circular dependency: Field 2 depends on Field 1
            var hasCircular = await _repository.HasCircularDependencyAsync(2, 1);

            // Assert
            hasCircular.Should().BeTrue();
        }

        [Fact]
        public async Task HasCircularDependencyAsync_WithoutDependency_ReturnsFalse()
        {
            // Arrange
            SeedTestData();

            // Act - No existing dependencies
            var hasCircular = await _repository.HasCircularDependencyAsync(1, 2);

            // Assert
            hasCircular.Should().BeFalse();
        }

        [Fact]
        public async Task HasCircularDependencyAsync_WithIndirectCircle_ReturnsTrue()
        {
            // Arrange
            SeedTestData();
            // Create chain: Field 1 -> Field 2 -> Field 3
            var rule1 = new FieldValidationRule
            {
                FormFieldId = 1,
                ValidationType = "ConditionalRequired",
                DependsOnFieldId = 2
            };
            var rule2 = new FieldValidationRule
            {
                FormFieldId = 2,
                ValidationType = "ConditionalRequired",
                DependsOnFieldId = 3
            };
            _context.FieldValidationRules.Add(rule1);
            _context.FieldValidationRules.Add(rule2);
            _context.SaveChanges();

            // Act - Try to create circular by making Field 3 depend on Field 1
            var hasCircular = await _repository.HasCircularDependencyAsync(3, 1);

            // Assert
            hasCircular.Should().BeTrue();
        }

        [Fact]
        public async Task HasCircularDependencyAsync_WithComplexNonCircularDependencies_ReturnsFalse()
        {
            // Arrange
            SeedTestData();
            // Create structure: Field 1 -> Field 2, Field 1 -> Field 3
            var rule1 = new FieldValidationRule
            {
                FormFieldId = 1,
                ValidationType = "ConditionalRequired",
                DependsOnFieldId = 2
            };
            var rule2 = new FieldValidationRule
            {
                FormFieldId = 1,
                ValidationType = "LocationInsideRegion",
                DependsOnFieldId = 3
            };
            _context.FieldValidationRules.Add(rule1);
            _context.FieldValidationRules.Add(rule2);
            _context.SaveChanges();

            // Act - Make Field 2 depend on Field 3 (no circle)
            var hasCircular = await _repository.HasCircularDependencyAsync(2, 3);

            // Assert
            hasCircular.Should().BeFalse();
        }

        [Fact]
        public async Task IncludesNavigationProperties_LoadsFormFieldAndDependsOnField()
        {
            // Arrange
            SeedTestData();
            var rule = new FieldValidationRule
            {
                FormFieldId = 1,
                ValidationType = "ConditionalRequired",
                DependsOnFieldId = 2
            };
            _context.FieldValidationRules.Add(rule);
            _context.SaveChanges();

            // Act
            var retrieved = await _repository.GetByIdAsync(rule.Id);

            // Assert
            retrieved!.FormField.Should().NotBeNull();
            retrieved.FormField.Label.Should().Be("Field 1");
            retrieved.DependsOnField.Should().NotBeNull();
            retrieved.DependsOnField.Label.Should().Be("Field 2");
        }
    }
}
