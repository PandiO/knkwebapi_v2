using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;
using FluentAssertions;
using Moq;
using knkwebapi_v2.Models;
using knkwebapi_v2.Services.ValidationMethods;
using knkwebapi_v2.Dtos;
using knkwebapi_v2.Repositories;
using knkwebapi_v2.Services.Interfaces;

namespace knkwebapi_v2.Tests.Services.ValidationMethods
{
    /// <summary>
    /// Unit tests for ConditionalRequiredValidator validation method.
    /// Tests validation logic for fields that are required based on dependency conditions.
    /// </summary>
    public class ConditionalRequiredValidatorTests
    {
        private readonly ConditionalRequiredValidator _validator;

        public ConditionalRequiredValidatorTests()
        {
            _validator = new ConditionalRequiredValidator();
        }

        [Fact]
        public void ValidationType_ReturnsCorrectValue()
        {
            // Act
            var validationType = _validator.ValidationType;

            // Assert
            validationType.Should().Be("ConditionalRequired");
        }

        [Fact]
        public async Task ValidateAsync_WithConditionMetAndFieldEmpty_ReturnsInvalid()
        {
            // Arrange
            var rule = new FieldValidationRule
            {
                Id = 1,
                FormFieldId = 10,
                ValidationType = "ConditionalRequired",
                DependsOnFieldId = 11,
                ConfigJson = JsonSerializer.Serialize(new
                {
                    condition = new { op = "equals", value = "Town" }
                }),
                ErrorMessage = "This field is required when a town is selected",
                IsBlocking = true,
                RequiresDependencyFilled = true
            };

            var formContextData = new Dictionary<string, object?> { { "parentFieldId_11", "Town" } };

            // Act
            var result = await _validator.ValidateAsync(rule, null, null, formContextData);

            // Assert
            result.Should().NotBeNull();
            result.IsValid.Should().BeFalse();
            result.IsBlocking.Should().BeTrue();
            result.Message.Should().Contain("required");
        }

        [Fact]
        public async Task ValidateAsync_WithConditionMetAndFieldFilled_ReturnsValid()
        {
            // Arrange
            var rule = new FieldValidationRule
            {
                Id = 1,
                FormFieldId = 10,
                ValidationType = "ConditionalRequired",
                DependsOnFieldId = 11,
                ConfigJson = JsonSerializer.Serialize(new
                {
                    condition = new { op = "equals", value = "Town" }
                }),
                SuccessMessage = "Field is valid",
                IsBlocking = true
            };

            var formContextData = new Dictionary<string, object?> { { "parentFieldId_11", "Town" } };

            // Act
            var result = await _validator.ValidateAsync(rule, "Location A", null, formContextData);

            // Assert
            result.Should().NotBeNull();
            result.IsValid.Should().BeTrue();
            result.IsBlocking.Should().BeFalse();
        }

        [Fact]
        public async Task ValidateAsync_WithConditionNotMet_ReturnsValid()
        {
            // Arrange
            var rule = new FieldValidationRule
            {
                Id = 1,
                FormFieldId = 10,
                ValidationType = "ConditionalRequired",
                DependsOnFieldId = 11,
                ConfigJson = JsonSerializer.Serialize(new
                {
                    condition = new { op = "equals", value = "Town" }
                }),
                ErrorMessage = "Error message",
                IsBlocking = true
            };

            var formContextData = new Dictionary<string, object?> { { "parentFieldId_11", "District" } };

            // Act
            var result = await _validator.ValidateAsync(rule, null, null, formContextData);

            // Assert
            result.Should().NotBeNull();
            result.IsValid.Should().BeTrue();
            result.IsBlocking.Should().BeFalse();
        }

        [Fact]
        public async Task ValidateAsync_WithMissingDependency_ReturnsValid()
        {
            // Arrange
            var rule = new FieldValidationRule
            {
                Id = 1,
                FormFieldId = 10,
                ValidationType = "ConditionalRequired",
                DependsOnFieldId = 11,
                ConfigJson = JsonSerializer.Serialize(new
                {
                    condition = new { op = "equals", value = "Town" }
                }),
                ErrorMessage = "Error message",
                IsBlocking = true,
                RequiresDependencyFilled = false
            };

            var formContextData = new Dictionary<string, object?>();

            // Act
            var result = await _validator.ValidateAsync(rule, null, null, formContextData);

            // Assert
            result.Should().NotBeNull();
            result.IsValid.Should().BeTrue();
        }

        [Fact]
        public async Task ValidateAsync_WithGreaterThanCondition_ReturnsCorrectResult()
        {
            // Arrange
            var rule = new FieldValidationRule
            {
                Id = 1,
                FormFieldId = 10,
                ValidationType = "ConditionalRequired",
                DependsOnFieldId = 11,
                ConfigJson = JsonSerializer.Serialize(new
                {
                    condition = new { op = "greaterThan", value = 5 }
                }),
                ErrorMessage = "Required when value is greater than 5",
                IsBlocking = true
            };

            var formContextData = new Dictionary<string, object?> { { "parentFieldId_11", 10 } };

            // Act
            var result = await _validator.ValidateAsync(rule, null, null, formContextData);

            // Assert
            result.IsValid.Should().BeFalse();
        }

        [Fact]
        public async Task ValidateAsync_WithLessThanCondition_ReturnsCorrectResult()
        {
            // Arrange
            var rule = new FieldValidationRule
            {
                Id = 1,
                FormFieldId = 10,
                ValidationType = "ConditionalRequired",
                DependsOnFieldId = 11,
                ConfigJson = JsonSerializer.Serialize(new
                {
                    condition = new { op = "lessThan", value = 5 }
                }),
                ErrorMessage = "Required when value is less than 5",
                IsBlocking = true
            };

            var formContextData = new Dictionary<string, object?> { { "parentFieldId_11", 3 } };

            // Act
            var result = await _validator.ValidateAsync(rule, "someValue", null, formContextData);

            // Assert
            result.IsValid.Should().BeTrue();
        }

        [Fact]
        public async Task ValidateAsync_WithNotEqualsCondition_ReturnsCorrectResult()
        {
            // Arrange
            var rule = new FieldValidationRule
            {
                Id = 1,
                FormFieldId = 10,
                ValidationType = "ConditionalRequired",
                DependsOnFieldId = 11,
                ConfigJson = JsonSerializer.Serialize(new
                {
                    condition = new { op = "notEquals", value = "None" }
                }),
                ErrorMessage = "Required when type is not None",
                IsBlocking = true
            };

            var formContextData = new Dictionary<string, object?> { { "parentFieldId_11", "Town" } };

            // Act
            var result = await _validator.ValidateAsync(rule, null, null, formContextData);

            // Assert
            result.IsValid.Should().BeFalse();
        }
    }

    /// <summary>
    /// Unit tests for LocationInsideRegionValidator validation method.
    /// Tests validation logic for locations constrained within regions.
    /// </summary>
    public class LocationInsideRegionValidatorTests
    {
        private readonly Mock<ILocationRepository> _mockLocationRepository;
        private readonly Mock<IRegionService> _mockRegionService;
        private readonly Mock<IGenericEntityService> _mockEntityService;
        private readonly LocationInsideRegionValidator _validator;

        public LocationInsideRegionValidatorTests()
        {
            _mockLocationRepository = new Mock<ILocationRepository>();
            _mockRegionService = new Mock<IRegionService>();
            _mockEntityService = new Mock<IGenericEntityService>();

            _validator = new LocationInsideRegionValidator(
                _mockLocationRepository.Object,
                _mockRegionService.Object,
                _mockEntityService.Object
            );
        }

        [Fact]
        public void ValidationType_ReturnsCorrectValue()
        {
            // Act
            var validationType = _validator.ValidationType;

            // Assert
            validationType.Should().Be("LocationInsideRegion");
        }

        [Fact]
        public async Task ValidateAsync_WithLocationInsideRegion_ReturnsValid()
        {
            // Arrange
            var rule = new FieldValidationRule
            {
                Id = 1,
                FormFieldId = 10,
                ValidationType = "LocationInsideRegion",
                DependsOnFieldId = 11,
                ConfigJson = JsonSerializer.Serialize(new
                {
                    regionPropertyPath = "WgRegionId"
                }),
                SuccessMessage = "Location is within the region",
                IsBlocking = true,
                RequiresDependencyFilled = true
            };

            var location = new Location { Id = 1, X = 100, Z = 200 };
            var formContextData = new Dictionary<string, object?> { { "parentFieldId_11", 5 } };

            _mockLocationRepository.Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(location);
            _mockRegionService.Setup(s => s.IsLocationInsideRegion(It.IsAny<string>(), location))
                .ReturnsAsync(true);

            // Act
            var result = await _validator.ValidateAsync(rule, 1, null, formContextData);

            // Assert
            result.Should().NotBeNull();
            result.IsValid.Should().BeTrue();
        }

        [Fact]
        public async Task ValidateAsync_WithLocationOutsideRegion_ReturnsInvalid()
        {
            // Arrange
            var rule = new FieldValidationRule
            {
                Id = 1,
                FormFieldId = 10,
                ValidationType = "LocationInsideRegion",
                DependsOnFieldId = 11,
                ConfigJson = JsonSerializer.Serialize(new
                {
                    regionPropertyPath = "WgRegionId"
                }),
                ErrorMessage = "Location must be within the specified region",
                IsBlocking = true,
                RequiresDependencyFilled = true
            };

            var location = new Location { Id = 1, X = 1000, Z = 2000 };
            var formContextData = new Dictionary<string, object?> { { "parentFieldId_11", 5 } };

            _mockLocationRepository.Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(location);
            _mockRegionService.Setup(s => s.IsLocationInsideRegion(It.IsAny<string>(), location))
                .ReturnsAsync(false);

            // Act
            var result = await _validator.ValidateAsync(rule, 1, null, formContextData);

            // Assert
            result.Should().NotBeNull();
            result.IsValid.Should().BeFalse();
            result.IsBlocking.Should().BeTrue();
        }

        [Fact]
        public async Task ValidateAsync_WithLocationNotFound_ReturnsInvalid()
        {
            // Arrange
            var rule = new FieldValidationRule
            {
                Id = 1,
                FormFieldId = 10,
                ValidationType = "LocationInsideRegion",
                DependsOnFieldId = 11,
                ConfigJson = JsonSerializer.Serialize(new
                {
                    regionPropertyPath = "WgRegionId"
                }),
                ErrorMessage = "Location not found",
                IsBlocking = true
            };

            var formContextData = new Dictionary<string, object?> { { "parentFieldId_11", 5 } };

            _mockLocationRepository.Setup(r => r.GetByIdAsync(999))
                .ReturnsAsync((Location?)null);

            // Act
            var result = await _validator.ValidateAsync(rule, 999, null, formContextData);

            // Assert
            result.Should().NotBeNull();
            result.IsValid.Should().BeFalse();
        }

        [Fact]
        public async Task ValidateAsync_WithMissingDependency_ReturnsPending()
        {
            // Arrange
            var rule = new FieldValidationRule
            {
                Id = 1,
                FormFieldId = 10,
                ValidationType = "LocationInsideRegion",
                DependsOnFieldId = 11,
                ConfigJson = JsonSerializer.Serialize(new
                {
                    regionPropertyPath = "WgRegionId"
                }),
                ErrorMessage = "Location validation failed",
                IsBlocking = true,
                RequiresDependencyFilled = true
            };

            var formContextData = new Dictionary<string, object?>();

            // Act
            var result = await _validator.ValidateAsync(rule, 1, null, formContextData);

            // Assert
            result.Should().NotBeNull();
            result.IsValid.Should().BeFalse();
        }
    }

    /// <summary>
    /// Unit tests for RegionContainmentValidator validation method.
    /// Tests validation logic for regions constrained within parent regions.
    /// </summary>
    public class RegionContainmentValidatorTests
    {
        private readonly Mock<IRegionService> _mockRegionService;
        private readonly Mock<IGenericEntityService> _mockEntityService;
        private readonly RegionContainmentValidator _validator;

        public RegionContainmentValidatorTests()
        {
            _mockRegionService = new Mock<IRegionService>();
            _mockEntityService = new Mock<IGenericEntityService>();

            _validator = new RegionContainmentValidator(
                _mockRegionService.Object,
                _mockEntityService.Object
            );
        }

        [Fact]
        public void ValidationType_ReturnsCorrectValue()
        {
            // Act
            var validationType = _validator.ValidationType;

            // Assert
            validationType.Should().Be("RegionContainment");
        }

        [Fact]
        public async Task ValidateAsync_WithRegionFullyContained_ReturnsValid()
        {
            // Arrange
            var rule = new FieldValidationRule
            {
                Id = 1,
                FormFieldId = 10,
                ValidationType = "RegionContainment",
                DependsOnFieldId = 11,
                ConfigJson = JsonSerializer.Serialize(new
                {
                    parentRegionPropertyPath = "ParentWgRegionId"
                }),
                SuccessMessage = "Region is properly contained",
                IsBlocking = true
            };

            var formContextData = new Dictionary<string, object?> { { "parentFieldId_11", 1 } };

            _mockRegionService.Setup(s => s.IsRegionContainedWithin(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(true);

            // Act
            var result = await _validator.ValidateAsync(rule, "region2", null, formContextData);

            // Assert
            result.Should().NotBeNull();
            result.IsValid.Should().BeTrue();
        }

        [Fact]
        public async Task ValidateAsync_WithRegionNotContained_ReturnsInvalid()
        {
            // Arrange
            var rule = new FieldValidationRule
            {
                Id = 1,
                FormFieldId = 10,
                ValidationType = "RegionContainment",
                DependsOnFieldId = 11,
                ConfigJson = JsonSerializer.Serialize(new
                {
                    parentRegionPropertyPath = "ParentWgRegionId"
                }),
                ErrorMessage = "Region must be contained within parent region",
                IsBlocking = true
            };

            var formContextData = new Dictionary<string, object?> { { "parentFieldId_11", 1 } };

            _mockRegionService.Setup(s => s.IsRegionContainedWithin(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(false);

            // Act
            var result = await _validator.ValidateAsync(rule, "region2", null, formContextData);

            // Assert
            result.Should().NotBeNull();
            result.IsValid.Should().BeFalse();
            result.IsBlocking.Should().BeTrue();
        }

        [Fact]
        public async Task ValidateAsync_WithMissingDependency_ReturnsPending()
        {
            // Arrange
            var rule = new FieldValidationRule
            {
                Id = 1,
                FormFieldId = 10,
                ValidationType = "RegionContainment",
                DependsOnFieldId = 11,
                ConfigJson = JsonSerializer.Serialize(new
                {
                    parentRegionPropertyPath = "ParentWgRegionId"
                }),
                ErrorMessage = "Region validation failed",
                IsBlocking = true,
                RequiresDependencyFilled = true
            };

            var formContextData = new Dictionary<string, object?>();

            // Act
            var result = await _validator.ValidateAsync(rule, "region2", null, formContextData);

            // Assert
            result.IsValid.Should().BeFalse();
        }
    }
}
