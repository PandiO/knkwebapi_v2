using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace knkwebapi_v2.Dtos
{
    /// <summary>
    /// DTO for reading FieldValidationRule data.
    /// </summary>
    public class FieldValidationRuleDto
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }
        
        [JsonPropertyName("formFieldId")]
        public int FormFieldId { get; set; }
        
        [JsonPropertyName("validationType")]
        public string ValidationType { get; set; } = null!;
        
        [JsonPropertyName("dependsOnFieldId")]
        public int? DependsOnFieldId { get; set; }
        
        [JsonPropertyName("configJson")]
        public string ConfigJson { get; set; } = "{}";
        
        [JsonPropertyName("errorMessage")]
        public string ErrorMessage { get; set; } = null!;
        
        [JsonPropertyName("successMessage")]
        public string? SuccessMessage { get; set; }
        
        [JsonPropertyName("isBlocking")]
        public bool IsBlocking { get; set; }
        
        [JsonPropertyName("requiresDependencyFilled")]
        public bool RequiresDependencyFilled { get; set; }
        
        [JsonPropertyName("createdAt")]
        public string CreatedAt { get; set; } = null!;
    }
    
    /// <summary>
    /// DTO for creating a new FieldValidationRule.
    /// </summary>
    public class CreateFieldValidationRuleDto
    {
        [JsonPropertyName("formFieldId")]
        public int FormFieldId { get; set; }
        
        [JsonPropertyName("validationType")]
        public string ValidationType { get; set; } = null!;
        
        [JsonPropertyName("dependsOnFieldId")]
        public int? DependsOnFieldId { get; set; }
        
        [JsonPropertyName("configJson")]
        public string ConfigJson { get; set; } = "{}";
        
        [JsonPropertyName("errorMessage")]
        public string ErrorMessage { get; set; } = null!;
        
        [JsonPropertyName("successMessage")]
        public string? SuccessMessage { get; set; }
        
        [JsonPropertyName("isBlocking")]
        public bool IsBlocking { get; set; } = true;
        
        [JsonPropertyName("requiresDependencyFilled")]
        public bool RequiresDependencyFilled { get; set; } = false;
    }
    
    /// <summary>
    /// DTO for updating an existing FieldValidationRule.
    /// </summary>
    public class UpdateFieldValidationRuleDto
    {
        [JsonPropertyName("validationType")]
        public string ValidationType { get; set; } = null!;
        
        [JsonPropertyName("dependsOnFieldId")]
        public int? DependsOnFieldId { get; set; }
        
        [JsonPropertyName("configJson")]
        public string ConfigJson { get; set; } = "{}";
        
        [JsonPropertyName("errorMessage")]
        public string ErrorMessage { get; set; } = null!;
        
        [JsonPropertyName("successMessage")]
        public string? SuccessMessage { get; set; }
        
        [JsonPropertyName("isBlocking")]
        public bool IsBlocking { get; set; }
        
        [JsonPropertyName("requiresDependencyFilled")]
        public bool RequiresDependencyFilled { get; set; }
    }
    
    /// <summary>
    /// Request DTO for validating a field value.
    /// </summary>
    public class ValidateFieldRequestDto
    {
        [JsonPropertyName("fieldId")]
        public int FieldId { get; set; }
        
        [JsonPropertyName("fieldValue")]
        public object? FieldValue { get; set; }
        
        [JsonPropertyName("dependencyValue")]
        public object? DependencyValue { get; set; }
        
        [JsonPropertyName("formContextData")]
        public Dictionary<string, object>? FormContextData { get; set; }
    }
    
    /// <summary>
    /// Response DTO containing validation result.
    /// </summary>
    public class ValidationResultDto
    {
        [JsonPropertyName("isValid")]
        public bool IsValid { get; set; }
        
        [JsonPropertyName("isBlocking")]
        public bool IsBlocking { get; set; }
        
        [JsonPropertyName("message")]
        public string Message { get; set; } = null!;
        
        [JsonPropertyName("placeholders")]
        public Dictionary<string, string>? Placeholders { get; set; }
        
        [JsonPropertyName("metadata")]
        public ValidationMetadataDto? Metadata { get; set; }
    }
    
    /// <summary>
    /// Metadata about the validation execution.
    /// </summary>
    public class ValidationMetadataDto
    {
        [JsonPropertyName("validationType")]
        public string ValidationType { get; set; } = null!;
        
        [JsonPropertyName("executedAt")]
        public string ExecutedAt { get; set; } = null!;
        
        [JsonPropertyName("dependencyFieldName")]
        public string? DependencyFieldName { get; set; }
        
        [JsonPropertyName("dependencyValue")]
        public object? DependencyValue { get; set; }
    }
    
    /// <summary>
    /// DTO for configuration health check issues.
    /// </summary>
    public class ValidationIssueDto
    {
        [JsonPropertyName("severity")]
        public string Severity { get; set; } = null!; // "Error", "Warning", "Info"
        
        [JsonPropertyName("message")]
        public string Message { get; set; } = null!;
        
        [JsonPropertyName("fieldId")]
        public int? FieldId { get; set; }
        
        [JsonPropertyName("ruleId")]
        public int? RuleId { get; set; }
    }
    
    /// <summary>
    /// Request DTO for resolving placeholders in validation messages.
    /// </summary>
    public class ResolvePlaceholdersRequestDto
    {
        [JsonPropertyName("currentEntityType")]
        public string CurrentEntityType { get; set; } = null!;
        
        [JsonPropertyName("currentEntityId")]
        public int? CurrentEntityId { get; set; }
        
        [JsonPropertyName("placeholderPaths")]
        public List<string> PlaceholderPaths { get; set; } = new();
        
        [JsonPropertyName("currentEntityPlaceholders")]
        public Dictionary<string, string>? CurrentEntityPlaceholders { get; set; }
    }
    
    /// <summary>
    /// Response DTO containing resolved placeholder values.
    /// </summary>
    public class ResolvePlaceholdersResponseDto
    {
        [JsonPropertyName("resolvedPlaceholders")]
        public Dictionary<string, string> ResolvedPlaceholders { get; set; } = new();
        
        [JsonPropertyName("unresolvedPlaceholders")]
        public List<string> UnresolvedPlaceholders { get; set; } = new();
        
        [JsonPropertyName("resolutionErrors")]
        public List<string> ResolutionErrors { get; set; } = new();
    }
}
