using System;
using System.Collections.Generic;
using knkwebapi_v2.Enums;
using System.Text.Json.Serialization;

namespace knkwebapi_v2.Dtos
{
    public class FieldValidationDto
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }
        [JsonPropertyName("formFieldId")]
        public string? FormFieldId { get; set; }
        [JsonPropertyName("validationType")]
        public ValidationType ValidationType { get; set; }
        [JsonPropertyName("parametersJson")]
        public string? ParametersJson { get; set; }
        [JsonPropertyName("errorMessage")]
        public string? ErrorMessage { get; set; }
        [JsonPropertyName("isActive")]
        public bool IsActive { get; set; }
    }

    public class StepConditionDto
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }
        [JsonPropertyName("formStepId")]
        public string? FormStepId { get; set; }
        // "Entry" or "Completion"
        [JsonPropertyName("conditionType")]
        public string ConditionType { get; set; } = "Entry";
        [JsonPropertyName("conditionJson")]
        public string ConditionJson { get; set; } = "{}";
        [JsonPropertyName("errorMessage")]
        public string? ErrorMessage { get; set; }
        [JsonPropertyName("isActive")]
        public bool IsActive { get; set; }
    }

    public class FormFieldDto
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }
        [JsonPropertyName("formStepId")]
        public string? FormStepId { get; set; }
        [JsonPropertyName("fieldName")]
        public string FieldName { get; set; } = null!;
        [JsonPropertyName("label")]
        public string Label { get; set; } = null!;
        [JsonPropertyName("placeholder")]
        public string? Placeholder { get; set; }
        [JsonPropertyName("description")]
        public string? Description { get; set; }
        [JsonPropertyName("fieldType")]
        public FieldType FieldType { get; set; }
        [JsonPropertyName("defaultValue")]
        public string? DefaultValue { get; set; }
        [JsonPropertyName("isRequired")]
        public bool IsRequired { get; set; }
        [JsonPropertyName("isReadOnly")]
        public bool IsReadOnly { get; set; }
        [JsonPropertyName("order")]
        public int Order { get; set; }
        [JsonPropertyName("dependencyConditionJson")]
        public string? DependencyConditionJson { get; set; }
        [JsonPropertyName("objectType")]
        public string? ObjectType { get; set; }
        [JsonPropertyName("subConfigurationId")]
        public string? SubConfigurationId { get; set; }
        [JsonPropertyName("incrementValue")]
        public int? IncrementValue { get; set; }
        [JsonPropertyName("isReusable")]
        public bool IsReusable { get; set; }
        [JsonPropertyName("sourceFieldId")]
        public string? SourceFieldId { get; set; }
        [JsonPropertyName("validations")]
        public List<FieldValidationDto> Validations { get; set; } = new();
    }

    public class FormStepDto
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }
        [JsonPropertyName("formConfigurationId")]
        public string? FormConfigurationId { get; set; }
        [JsonPropertyName("stepName")]
        public string StepName { get; set; } = null!;
        [JsonPropertyName("title")]
        public string Title { get; set; } = null!;
        [JsonPropertyName("description")]
        public string? Description { get; set; }
        [JsonPropertyName("order")]
        public int Order { get; set; }
        [JsonPropertyName("fieldOrderJson")]
        public string? FieldOrderJson { get; set; }
        [JsonPropertyName("isReusable")]
        public bool IsReusable { get; set; }
        [JsonPropertyName("sourceStepId")]
        public string? SourceStepId { get; set; }
        [JsonPropertyName("fields")]
        public List<FormFieldDto> Fields { get; set; } = new();
        [JsonPropertyName("conditions")]
        public List<StepConditionDto> Conditions { get; set; } = new();
    }

    public class FormConfigurationDto
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }
        [JsonPropertyName("entityName")]
        public string EntityName { get; set; } = null!;
        [JsonPropertyName("configurationName")]
        public string ConfigurationName { get; set; } = null!;
        [JsonPropertyName("description")]
        public string? Description { get; set; }
        [JsonPropertyName("isDefault")]
        public bool IsDefault { get; set; }
        [JsonPropertyName("stepOrderJson")]
        public string? StepOrderJson { get; set; }
        [JsonPropertyName("isActive")]
        public bool IsActive { get; set; }
        [JsonPropertyName("createdAt")]
        public string? CreatedAt { get; set; }
        [JsonPropertyName("updatedAt")]
        public string? UpdatedAt { get; set; }
        [JsonPropertyName("steps")]
        public List<FormStepDto> Steps { get; set; } = new();
    }

    public class FormSubmissionProgressDto
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }
        [JsonPropertyName("formConfigurationId")]
        public string FormConfigurationId { get; set; } = null!;
        [JsonPropertyName("userId")]
        public string UserId { get; set; } = null!;
        [JsonPropertyName("entityId")]
        public string? EntityId { get; set; }
        [JsonPropertyName("currentStepIndex")]
        public int CurrentStepIndex { get; set; }
        [JsonPropertyName("currentStepDataJson")]
        public string? CurrentStepDataJson { get; set; }
        [JsonPropertyName("allStepsDataJson")]
        public string? AllStepsDataJson { get; set; }
        [JsonPropertyName("parentProgressId")]
        public string? ParentProgressId { get; set; }
        [JsonPropertyName("status")]
        public FormSubmissionStatus Status { get; set; } = FormSubmissionStatus.InProgress;
        [JsonPropertyName("createdAt")]
        public string? CreatedAt { get; set; }
        [JsonPropertyName("updatedAt")]
        public string? UpdatedAt { get; set; }
        [JsonPropertyName("completedAt")]
        public string? CompletedAt { get; set; }
        [JsonPropertyName("configuration")]
        public FormConfigurationDto? Configuration { get; set; }
        [JsonPropertyName("childProgresses")]
        public List<FormSubmissionProgressDto>? ChildProgresses { get; set; }
    }
}
