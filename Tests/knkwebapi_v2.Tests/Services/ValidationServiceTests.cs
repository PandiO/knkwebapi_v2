using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using FluentAssertions;
using Moq;
using AutoMapper;
using knkwebapi_v2.Models;
using knkwebapi_v2.Repositories;
using knkwebapi_v2.Services;
using knkwebapi_v2.Services.Interfaces;
using knkwebapi_v2.Dtos;

namespace knkwebapi_v2.Tests.Services
{
    /// <summary>
    /// Unit tests for ValidationService focusing on validation execution
    /// and placeholder aggregation. CRUD operations are tested in FieldValidationRuleServiceTests.
    /// </summary>
    public class ValidationServiceTests
    {
        private readonly Mock<IFieldValidationRuleRepository> _mockRuleRepository;
        private readonly Mock<IPlaceholderResolutionService> _mockPlaceholderService;
        private readonly ValidationService _validationService;

        public ValidationServiceTests()
        {
            _mockRuleRepository = new Mock<IFieldValidationRuleRepository>();
            _mockPlaceholderService = new Mock<IPlaceholderResolutionService>();

            // Setup validation methods collection
            var validationMethods = new List<IValidationMethod>();
            _validationService = new ValidationService(
                _mockRuleRepository.Object,
                validationMethods,
                _mockPlaceholderService.Object
            );
        }

        #region Validation Execution Tests

        [Fact]
        public async Task ValidateFieldAsync_WithNoRules_ReturnsSuccess()
        {
            // Arrange
            var fieldId = 10;
            _mockRuleRepository.Setup(r => r.GetByFormFieldIdAsync(fieldId))
                .ReturnsAsync(new List<FieldValidationRule>());

            // Act
            var result = await _validationService.ValidateFieldAsync(fieldId, "someValue", null, null);

            // Assert
            result.Should().NotBeNull();
            result.IsValid.Should().BeTrue();
            result.IsBlocking.Should().BeFalse();
        }

        [Fact]
        public async Task ValidateFieldAsync_WithPassingRule_ReturnsSuccess()
        {
            // Arrange
            var fieldId = 10;
            var rule = new FieldValidationRule
            {
                Id = 1,
                FormFieldId = fieldId,
                ValidationType = "ConditionalRequired",
                IsBlocking = true
            };

            var mockValidator = new Mock<IValidationMethod>();
            var successResult = new ValidationResultDto
            {
                IsValid = true,
                IsBlocking = false,
                Message = "Validation passed"
            };

            _mockRuleRepository.Setup(r => r.GetByFormFieldIdAsync(fieldId))
                .ReturnsAsync(new List<FieldValidationRule> { rule });
            mockValidator.Setup(v => v.ValidationType).Returns("ConditionalRequired");
            mockValidator.Setup(v => v.ValidateAsync(rule, "someValue", null, null))
                .ReturnsAsync(successResult);

            var validationMethods = new List<IValidationMethod> { mockValidator.Object };
            _mockValidationMethods.Setup(m => m.GetEnumerator())
                .Returns(validationMethods.GetEnumerator());

            // Act
            var result = await _validationService.ValidateFieldAsync(fieldId, "someValue", null, null);

            // Assert
            result.Should().NotBeNull();
            result.IsValid.Should().BeTrue();
        }

        [Fact]
        public async Task ValidateFieldAsync_WithFailingBlockingRule_ReturnsError()
        {
            // Arrange
            var fieldId = 10;
            var rule = new FieldValidationRule
            {
                Id = 1,
                FormFieldId = fieldId,
                ValidationType = "ConditionalRequired",
                IsBlocking = true,
                ErrorMessage = "This field is required"
            };

            var mockValidator = new Mock<IValidationMethod>();
            var failureResult = new ValidationResultDto
            {
                IsValid = false,
                IsBlocking = true,
                Message = "This field is required"
            };

            _mockRuleRepository.Setup(r => r.GetByFormFieldIdAsync(fieldId))
                .ReturnsAsync(new List<FieldValidationRule> { rule });
            mockValidator.Setup(v => v.ValidationType).Returns("ConditionalRequired");
            mockValidator.Setup(v => v.ValidateAsync(rule, null, null, null))
                .ReturnsAsync(failureResult);

            var validationMethods = new List<IValidationMethod> { mockValidator.Object };
            _mockValidationMethods.Setup(m => m.GetEnumerator())
                .Returns(validationMethods.GetEnumerator());

            // Act
            var result = await _validationService.ValidateFieldAsync(fieldId, null, null, null);

            // Assert
            result.Should().NotBeNull();
            result.IsValid.Should().BeFalse();
            result.IsBlocking.Should().BeTrue();
            result.Message.Should().Be("This field is required");
        }

        [Fact]
        public async Task ValidateFieldAsync_WithMultipleRules_ExecutesAllRules()
        {
            // Arrange
            var fieldId = 10;
            var rule1 = new FieldValidationRule
            {
                Id = 1,
                FormFieldId = fieldId,
                ValidationType = "ConditionalRequired",
                IsBlocking = true
            };
            var rule2 = new FieldValidationRule
            {
                Id = 2,
                FormFieldId = fieldId,
                ValidationType = "LocationInsideRegion",
                IsBlocking = false
            };

            var mockValidator1 = new Mock<IValidationMethod>();
            var mockValidator2 = new Mock<IValidationMethod>();
            var result1 = new ValidationResultDto { IsValid = true, IsBlocking = false };
            var result2 = new ValidationResultDto { IsValid = true, IsBlocking = false };

            _mockRuleRepository.Setup(r => r.GetByFormFieldIdAsync(fieldId))
                .ReturnsAsync(new List<FieldValidationRule> { rule1, rule2 });

            mockValidator1.Setup(v => v.ValidationType).Returns("ConditionalRequired");
            mockValidator1.Setup(v => v.ValidateAsync(rule1, "value", null, null))
                .ReturnsAsync(result1);

            mockValidator2.Setup(v => v.ValidationType).Returns("LocationInsideRegion");
            mockValidator2.Setup(v => v.ValidateAsync(rule2, "value", null, null))
                .ReturnsAsync(result2);

            var validationMethods = new List<IValidationMethod> { mockValidator1.Object, mockValidator2.Object };
            _mockValidationMethods.Setup(m => m.GetEnumerator())
                .Returns(validationMethods.GetEnumerator());

            // Act
            var result = await _validationService.ValidateFieldAsync(fieldId, "value", null, null);

            // Assert
            result.Should().NotBeNull();
            result.IsValid.Should().BeTrue();
            mockValidator1.Verify(v => v.ValidateAsync(It.IsAny<FieldValidationRule>(), It.IsAny<object?>(), It.IsAny<object?>(), It.IsAny<Dictionary<string, object>?>()), Times.Once);
            mockValidator2.Verify(v => v.ValidateAsync(It.IsAny<FieldValidationRule>(), It.IsAny<object?>(), It.IsAny<object?>(), It.IsAny<Dictionary<string, object>?>()), Times.Once);
        }

        [Fact]
        public async Task ValidateFieldAsync_WithValidateFieldRequestDto_ExecutesValidation()
        {
            // Arrange
            var request = new ValidateFieldRequestDto
            {
                FieldId = 10,
                FieldValue = "testValue",
                DependencyValue = 5,
                FormContextData = new Dictionary<string, object> { { "key", "value" } }
            };

            _mockRuleRepository.Setup(r => r.GetByFormFieldIdAsync(10))
                .ReturnsAsync(new List<FieldValidationRule>());

            // Act
            var result = await _validationService.ValidateFieldAsync(request);

            // Assert
            result.Should().NotBeNull();
            result.IsValid.Should().BeTrue();
        }

        [Fact]
        public async Task ValidateFieldAsync_WithNullRequest_ThrowsException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => _validationService.ValidateFieldAsync(null!));
        }

        #endregion

        #region Multi-Field Validation Tests

        [Fact]
        public async Task ValidateMultipleFieldsAsync_WithMultipleFields_ValidatesAll()
        {
            // Arrange
            var fieldIds = new List<int> { 10, 11 };
            var fieldValues = new Dictionary<int, object?> { { 10, "value1" }, { 11, "value2" } };

            _mockRuleRepository.Setup(r => r.GetByFormFieldIdAsync(It.IsAny<int>()))
                .ReturnsAsync(new List<FieldValidationRule>());

            // Act
            var result = await _validationService.ValidateMultipleFieldsAsync(fieldIds, fieldValues, null);

            // Assert
            result.Should().HaveCount(2);
            result.Should().ContainKey(10);
            result.Should().ContainKey(11);
            _mockRuleRepository.Verify(r => r.GetByFormFieldIdAsync(It.IsAny<int>()), Times.Exactly(2));
        }

        #endregion
    }
}
