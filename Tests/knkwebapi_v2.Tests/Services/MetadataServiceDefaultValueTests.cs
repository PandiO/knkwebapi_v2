using System.Reflection;
using FluentAssertions;
using knkwebapi_v2.Dtos;
using knkwebapi_v2.Enums;
using knkwebapi_v2.Models;
using knkwebapi_v2.Services;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace knkwebapi_v2.Tests.Services;

public class MetadataServiceDefaultValueTests
{
    [Fact]
    public void GateRegionIds_HaveExplicitEmptyDefaults_WithoutMakingNameOptional()
    {
        var service = new MetadataService(Mock.Of<IServiceScopeFactory>());
        var getFieldMetadata = typeof(MetadataService).GetMethod(
            "GetFieldMetadata",
            BindingFlags.Instance | BindingFlags.NonPublic);

        getFieldMetadata.Should().NotBeNull();
        var fields = getFieldMetadata!.Invoke(service, new object[] { typeof(GateStructure) })
            .Should().BeAssignableTo<List<FieldMetadataDto>>().Subject;

        fields.Single(field => field.FieldName == nameof(GateStructure.RegionClosedId))
            .Should().Match<FieldMetadataDto>(field => field.HasDefaultValue && field.DefaultValue == string.Empty);
        fields.Single(field => field.FieldName == nameof(GateStructure.RegionOpenedId))
            .Should().Match<FieldMetadataDto>(field => field.HasDefaultValue && field.DefaultValue == string.Empty);
        fields.Single(field => field.FieldName == nameof(GateStructure.Name)).HasDefaultValue
            .Should().BeFalse();

        var validationResult = new FormTemplateValidationService().ValidateField(
            new FormField
            {
                FieldName = nameof(GateStructure.RegionClosedId),
                Label = "Closed Region Id",
                FieldType = FieldType.String,
                Required = false
            },
            new EntityMetadataDto
            {
                EntityName = nameof(GateStructure),
                Fields = fields
            });

        validationResult.IsCompatible.Should().BeTrue();
        validationResult.Issues.Should().BeEmpty();
    }
}