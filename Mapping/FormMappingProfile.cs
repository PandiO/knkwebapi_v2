using System;
using System.Linq;
using System.Text.Json;
using AutoMapper;
using knkwebapi_v2.Dtos;
using knkwebapi_v2.Enums;
using knkwebapi_v2.Models;

namespace knkwebapi_v2.Mapping
{
    public class FormMappingProfile : Profile
    {
        public FormMappingProfile()
        {
            // FieldValidation
            CreateMap<FieldValidationDto, FieldValidation>()
                .ForMember(d => d.Id, o => o.MapFrom(s => ToInt(s.Id)))
                .ForMember(d => d.FormFieldId, o => o.MapFrom(s => ToInt(s.FormFieldId)))
                .ForMember(d => d.Type, o => o.MapFrom(s => s.ValidationType))
                .ForMember(d => d.ParametersJson, o => o.MapFrom(s => string.IsNullOrWhiteSpace(s.ParametersJson) ? "{}" : s.ParametersJson))
                .ForMember(d => d.ErrorMessage, o => o.MapFrom(s => s.ErrorMessage))
                // Ignore IsActive in domain for now
                ;

            CreateMap<FieldValidation, FieldValidationDto>()
                .ForMember(d => d.Id, o => o.MapFrom(s => s.Id.ToString()))
                .ForMember(d => d.FormFieldId, o => o.MapFrom(s => s.FormFieldId.ToString()))
                .ForMember(d => d.ValidationType, o => o.MapFrom(s => s.Type))
                .ForMember(d => d.ParametersJson, o => o.MapFrom(s => s.ParametersJson))
                .ForMember(d => d.ErrorMessage, o => o.MapFrom(s => s.ErrorMessage))
                .ForMember(d => d.IsActive, o => o.Ignore());

            // StepCondition
            CreateMap<StepConditionDto, StepCondition>()
                .ForMember(d => d.Id, o => o.MapFrom(s => ToInt(s.Id)))
                .ForMember(d => d.FormStepId, o => o.MapFrom(s => ToInt(s.FormStepId)))
                .ForMember(d => d.ConditionType, o => o.MapFrom(s => s.ConditionType == "Completion" ? "CompletionCondition" : "StartCondition"))
                .ForMember(d => d.ConditionLogicJson, o => o.MapFrom(s => string.IsNullOrWhiteSpace(s.ConditionJson) ? "{}" : s.ConditionJson))
                .ForMember(d => d.ErrorMessage, o => o.MapFrom(s => s.ErrorMessage));

            CreateMap<StepCondition, StepConditionDto>()
                .ForMember(d => d.Id, o => o.MapFrom(s => s.Id.ToString()))
                .ForMember(d => d.FormStepId, o => o.MapFrom(s => s.FormStepId.ToString()))
                .ForMember(d => d.ConditionType, o => o.MapFrom(s => s.ConditionType == "CompletionCondition" ? "Completion" : "Entry"))
                .ForMember(d => d.ConditionJson, o => o.MapFrom(s => s.ConditionLogicJson))
                .ForMember(d => d.ErrorMessage, o => o.MapFrom(s => s.ErrorMessage))
                .ForMember(d => d.IsActive, o => o.Ignore());

            // FormField
            CreateMap<FormFieldDto, FormField>()
                .ForMember(d => d.Id, o => o.MapFrom(s => ToInt(s.Id)))
                .ForMember(d => d.FormStepId, o => o.MapFrom(s => ToInt(s.FormStepId)))
                .ForMember(d => d.FieldName, o => o.MapFrom(s => s.FieldName))
                .ForMember(d => d.Label, o => o.MapFrom(s => s.Label))
                .ForMember(d => d.Placeholder, o => o.MapFrom(s => s.Placeholder))
                .ForMember(d => d.Description, o => o.MapFrom(s => s.Description))
                .ForMember(d => d.FieldType, o => o.MapFrom(s => s.FieldType))
                .ForMember(d => d.ElementType, o => o.MapFrom(s => s.ElementType.HasValue ? (FieldType?)s.ElementType.Value : null))
                .ForMember(d => d.DefaultValue, o => o.MapFrom(s => s.DefaultValue))
                .ForMember(d => d.Required, o => o.MapFrom(s => s.IsRequired))
                .ForMember(d => d.ReadOnly, o => o.MapFrom(s => s.IsReadOnly))
                .ForMember(d => d.DependencyConditionJson, o => o.MapFrom(s => s.DependencyConditionJson))
                .ForMember(d => d.ObjectType, o => o.MapFrom(s => s.ObjectType))
                .ForMember(d => d.SubConfigurationId, o => o.MapFrom(s => ToInt(s.SubConfigurationId)))
                .ForMember(d => d.IsReusable, o => o.MapFrom(s => s.IsReusable))
                .ForMember(d => d.SourceFieldId, o => o.MapFrom(s => ToInt(s.SourceFieldId)))
                .ForMember(d => d.IsLinkedToSource, o => o.MapFrom(s => s.IsLinkedToSource))
                .ForMember(d => d.Validations, o => o.MapFrom(s => s.Validations))
                // ignore DTO.Order here; handled at step mapping level via FieldOrderJson
                // ignore compatibility issues; calculated at runtime
                .ForMember(d => d.DependentFields, o => o.Ignore());

            CreateMap<FormField, FormFieldDto>()
                .ForMember(d => d.Id, o => o.MapFrom(s => s.Id.ToString()))
                .ForMember(d => d.FormStepId, o => o.MapFrom(s => s.FormStepId.HasValue ? s.FormStepId.Value.ToString() : null))
                .ForMember(d => d.FieldName, o => o.MapFrom(s => s.FieldName))
                .ForMember(d => d.Label, o => o.MapFrom(s => s.Label))
                .ForMember(d => d.Placeholder, o => o.MapFrom(s => s.Placeholder))
                .ForMember(d => d.Description, o => o.MapFrom(s => s.Description))
                .ForMember(d => d.FieldType, o => o.MapFrom(s => s.FieldType))
                .ForMember(d => d.ElementType, o => o.MapFrom(s => s.ElementType))
                .ForMember(d => d.DefaultValue, o => o.MapFrom(s => s.DefaultValue))
                .ForMember(d => d.IsRequired, o => o.MapFrom(s => s.Required))
                .ForMember(d => d.IsReadOnly, o => o.MapFrom(s => s.ReadOnly))
                .ForMember(d => d.DependencyConditionJson, o => o.MapFrom(s => s.DependencyConditionJson))
                .ForMember(d => d.ObjectType, o => o.MapFrom(s => s.ObjectType))
                .ForMember(d => d.SubConfigurationId, o => o.MapFrom(s => s.SubConfigurationId.HasValue ? s.SubConfigurationId.Value.ToString() : null))
                .ForMember(d => d.IsReusable, o => o.MapFrom(s => s.IsReusable))
                .ForMember(d => d.SourceFieldId, o => o.MapFrom(s => s.SourceFieldId.HasValue ? s.SourceFieldId.Value.ToString() : null))
                .ForMember(d => d.IsLinkedToSource, o => o.MapFrom(s => s.IsLinkedToSource))
                .ForMember(d => d.Validations, o => o.MapFrom(s => s.Validations))
                .ForMember(d => d.Order, o => o.Ignore())
                // compatibility issues handled at runtime by service
                .ForMember(d => d.HasCompatibilityIssues, o => o.MapFrom(s => false))
                .ForMember(d => d.CompatibilityIssues, o => o.MapFrom(s => (List<string>?)null));

            // FormStep
            CreateMap<FormStepDto, FormStep>()
                .ForMember(d => d.Id, o => o.MapFrom(s => ToInt(s.Id)))
                .ForMember(d => d.FormConfigurationId, o => o.MapFrom(s => ToInt(s.FormConfigurationId)))
                .ForMember(d => d.StepName, o => o.MapFrom(s => s.StepName))
                .ForMember(d => d.Description, o => o.MapFrom(s => s.Description ?? s.Title))
                .ForMember(d => d.IsReusable, o => o.MapFrom(s => s.IsReusable))
                .ForMember(d => d.SourceStepId, o => o.MapFrom(s => ToInt(s.SourceStepId)))
                .ForMember(d => d.IsLinkedToSource, o => o.MapFrom(s => s.IsLinkedToSource))
                .ForMember(d => d.IsManyToManyRelationship, o => o.MapFrom(s => s.IsManyToManyRelationship))
                .ForMember(d => d.RelatedEntityPropertyName, o => o.MapFrom(s => s.RelatedEntityPropertyName))
                .ForMember(d => d.JoinEntityType, o => o.MapFrom(s => s.JoinEntityType))
                .ForMember(d => d.ParentStepId, o => o.MapFrom(s => ToInt(s.ParentStepId)))
                .ForMember(d => d.ChildFormSteps, o => o.MapFrom(s => s.ChildFormSteps))
                .ForMember(d => d.Fields, o => o.MapFrom(s => s.Fields.OrderBy(f => f.Order)))
                .ForMember(d => d.StepConditions, o => o.MapFrom(s => s.Conditions))
                .AfterMap((src, dest) =>
                {
                    if (!string.IsNullOrWhiteSpace(src.FieldOrderJson))
                    {
                        dest.FieldOrderJson = src.FieldOrderJson!;
                    }
                    else
                    {
                        var order = dest.Fields.Select(f => f.FieldGuid).ToArray();
                        dest.FieldOrderJson = JsonSerializer.Serialize(order);
                    }
                });

            CreateMap<FormStep, FormStepDto>()
                .ForMember(d => d.Id, o => o.MapFrom(s => s.Id.ToString()))
                .ForMember(d => d.FormConfigurationId, o => o.MapFrom(s => s.FormConfigurationId.HasValue ? s.FormConfigurationId.Value.ToString() : null))
                .ForMember(d => d.StepName, o => o.MapFrom(s => s.StepName))
                .ForMember(d => d.Title, o => o.MapFrom(s => s.StepName))
                .ForMember(d => d.Description, o => o.MapFrom(s => s.Description))
                .ForMember(d => d.Order, o => o.Ignore())
                .ForMember(d => d.FieldOrderJson, o => o.MapFrom(s => s.FieldOrderJson))
                .ForMember(d => d.IsReusable, o => o.MapFrom(s => s.IsReusable))
                .ForMember(d => d.SourceStepId, o => o.MapFrom(s => s.SourceStepId.HasValue ? s.SourceStepId.Value.ToString() : null))
                .ForMember(d => d.IsLinkedToSource, o => o.MapFrom(s => s.IsLinkedToSource))
                .ForMember(d => d.IsManyToManyRelationship, o => o.MapFrom(s => s.IsManyToManyRelationship))
                .ForMember(d => d.RelatedEntityPropertyName, o => o.MapFrom(s => s.RelatedEntityPropertyName))
                .ForMember(d => d.JoinEntityType, o => o.MapFrom(s => s.JoinEntityType))
                .ForMember(d => d.ParentStepId, o => o.MapFrom(s => s.ParentStepId.HasValue ? s.ParentStepId.Value.ToString() : null))
                .ForMember(d => d.ChildFormSteps, o => o.MapFrom(s => s.ChildFormSteps))
                .ForMember(d => d.Fields, o => o.MapFrom(s => s.Fields))
                .ForMember(d => d.Conditions, o => o.MapFrom(s => s.StepConditions))
                // compatibility issues handled at runtime
                .ForMember(d => d.HasCompatibilityIssues, o => o.MapFrom(s => false))
                .ForMember(d => d.StepLevelIssues, o => o.MapFrom(s => (List<string>?)null));

            // FormConfiguration
            CreateMap<FormConfigurationDto, FormConfiguration>()
                .ForMember(d => d.Id, o => o.MapFrom(s => ToInt(s.Id)))
                .ForMember(d => d.Name, o => o.MapFrom(s => s.ConfigurationName))
                .ForMember(d => d.EntityTypeName, o => o.MapFrom(s => s.EntityTypeName))
                .ForMember(d => d.Description, o => o.MapFrom(s => s.Description ?? s.ConfigurationName))
                .ForMember(d => d.IsDefault, o => o.MapFrom(s => s.IsDefault))
                .ForMember(d => d.Steps, o => o.MapFrom(s => s.Steps.OrderBy(st => st.Order)))
                .AfterMap((src, dest) =>
                {
                    if (!string.IsNullOrWhiteSpace(src.StepOrderJson))
                    {
                        dest.StepOrderJson = src.StepOrderJson!;
                    }
                    else
                    {
                        var order = dest.Steps.Select(s => s.StepGuid).ToArray();
                        dest.StepOrderJson = JsonSerializer.Serialize(order);
                    }
                });

            CreateMap<FormConfiguration, FormConfigurationDto>()
                .ForMember(d => d.Id, o => o.MapFrom(s => s.Id.ToString()))
                .ForMember(d => d.EntityTypeName, o => o.MapFrom(s => s.EntityTypeName))
                .ForMember(d => d.ConfigurationName, o => o.MapFrom(s => s.Name))
                .ForMember(d => d.Description, o => o.MapFrom(s => s.Description))
                .ForMember(d => d.IsDefault, o => o.MapFrom(s => s.IsDefault))
                .ForMember(d => d.StepOrderJson, o => o.MapFrom(s => s.StepOrderJson))
                .ForMember(d => d.IsActive, o => o.Ignore())
                .ForMember(d => d.CreatedAt, o => o.MapFrom(s => s.CreatedAt.ToString("O")))
                .ForMember(d => d.UpdatedAt, o => o.MapFrom(s => s.UpdatedAt.HasValue ? s.UpdatedAt.Value.ToString("O") : null))
                .ForMember(d => d.Steps, o => o.MapFrom(s => s.Steps));

            // FormSubmissionProgress
            CreateMap<FormSubmissionProgressDto, FormSubmissionProgress>()
                .ForMember(d => d.Id, o => o.MapFrom(s => ToInt(s.Id)))
                .ForMember(d => d.FormConfigurationId, o => o.MapFrom(s => ToInt(s.FormConfigurationId)!))
                .ForMember(d => d.UserId, o => o.MapFrom(s => ToInt(s.UserId)!))
                .ForMember(d => d.EntityId, o => o.MapFrom(s => ToInt(s.EntityId)))
                // .ForMember(d => d.FormConfiguration.EntityTypeName, o => o.MapFrom(s => s.EntityTypeName))
                .ForMember(d => d.CurrentStepIndex, o => o.MapFrom(s => s.CurrentStepIndex))
                .ForMember(d => d.CurrentStepDataJson, o => o.MapFrom(s => string.IsNullOrWhiteSpace(s.CurrentStepDataJson) ? "{}" : s.CurrentStepDataJson))
                .ForMember(d => d.AllStepsDataJson, o => o.MapFrom(s => string.IsNullOrWhiteSpace(s.AllStepsDataJson) ? "{}" : s.AllStepsDataJson))
                .ForMember(d => d.ParentProgressId, o => o.MapFrom(s => ToInt(s.ParentProgressId)))
                .ForMember(d => d.Status, o => o.MapFrom(s => s.Status.ToString()))
                .ForMember(d => d.CreatedAt, o => o.MapFrom(s => ParseDateOrDefault(s.CreatedAt)))
                .ForMember(d => d.UpdatedAt, o => o.MapFrom(s => ParseNullableDate(s.UpdatedAt)))
                .ForMember(d => d.CompletedAt, o => o.MapFrom(s => ParseNullableDate(s.CompletedAt)))
                .ForMember(d => d.FormConfiguration, o => o.Ignore())
                .ForMember(d => d.ParentProgress, o => o.Ignore());

            CreateMap<FormSubmissionProgress, FormSubmissionProgressDto>()
                .ForMember(d => d.Id, o => o.MapFrom(s => s.Id.ToString()))
                .ForMember(d => d.FormConfigurationId, o => o.MapFrom(s => s.FormConfigurationId.ToString()))
                .ForMember(d => d.UserId, o => o.MapFrom(s => s.UserId.ToString()))
                .ForMember(d => d.EntityTypeName, o => o.MapFrom(s => s.FormConfiguration != null ? s.FormConfiguration.EntityTypeName : string.Empty))    
                .ForMember(d => d.EntityId, o => o.MapFrom(s => s.EntityId.HasValue ? s.EntityId.Value.ToString() : null))
                .ForMember(d => d.CurrentStepIndex, o => o.MapFrom(s => s.CurrentStepIndex))
                .ForMember(d => d.CurrentStepDataJson, o => o.MapFrom(s => s.CurrentStepDataJson))
                .ForMember(d => d.AllStepsDataJson, o => o.MapFrom(s => s.AllStepsDataJson))
                .ForMember(d => d.ParentProgressId, o => o.MapFrom(s => s.ParentProgressId.HasValue ? s.ParentProgressId.Value.ToString() : null))
                .ForMember(d => d.Status, o => o.MapFrom(s => ParseStatus(s.Status)))
                .ForMember(d => d.CreatedAt, o => o.MapFrom(s => s.CreatedAt.ToString("O")))
                .ForMember(d => d.UpdatedAt, o => o.MapFrom(s => s.UpdatedAt.HasValue ? s.UpdatedAt.Value.ToString("O") : null))
                .ForMember(d => d.CompletedAt, o => o.MapFrom(s => s.CompletedAt.HasValue ? s.CompletedAt.Value.ToString("O") : null))
                .ForMember(d => d.Configuration, o => o.Ignore())
                .ForMember(d => d.ChildProgresses, o => o.Ignore());

            CreateMap<FormSubmissionProgress, FormSubmissionProgressSummaryDto>()
                .ForMember(d => d.Id, o => o.MapFrom(s => s.Id.ToString()))
                .ForMember(d => d.FormConfigurationId, o => o.MapFrom(s => s.FormConfigurationId.ToString()))
                .ForMember(d => d.FormConfigurationName, o => o.MapFrom(s => s.FormConfiguration != null ? (s.FormConfiguration.Description ?? s.FormConfiguration.EntityTypeName) : string.Empty))
                .ForMember(d => d.UserId, o => o.MapFrom(s => s.UserId.ToString()))
                .ForMember(d => d.EntityTypeName, o => o.MapFrom(s => s.FormConfiguration != null ? s.FormConfiguration.EntityTypeName : string.Empty))    
                .ForMember(d => d.EntityId, o => o.MapFrom(s => s.EntityId.HasValue ? s.EntityId.Value.ToString() : null))
                .ForMember(d => d.ParentProgressId, o => o.MapFrom(s => s.ParentProgressId.HasValue ? s.ParentProgressId.Value.ToString() : null))
                .ForMember(d => d.CurrentStepIndex, o => o.MapFrom(s => s.CurrentStepIndex))
                .ForMember(d => d.Status, o => o.MapFrom(s => ParseStatus(s.Status)))
                .ForMember(d => d.CreatedAt, o => o.MapFrom(s => s.CreatedAt.ToString("O")))
                .ForMember(d => d.UpdatedAt, o => o.MapFrom(s => s.UpdatedAt.HasValue ? s.UpdatedAt.Value.ToString("O") : null));
        }

        private static int? ToInt(string? s) => int.TryParse(s, out var v) ? v : (int?)null;
        private static DateTime ParseDateOrDefault(string? s) => DateTime.TryParse(s, out var d) ? d : DateTime.UtcNow;
        private static DateTime? ParseNullableDate(string? s) => DateTime.TryParse(s, out var d) ? d : (DateTime?)null;
        private static FormSubmissionStatus ParseStatus(string s) => Enum.TryParse<FormSubmissionStatus>(s, out var st) ? st : FormSubmissionStatus.InProgress;
    }
}
