namespace knkwebapi_v2.Dtos.Forms;

/// <summary>
/// DTO for reading a FieldValidationRule.
/// </summary>
public class FieldValidationRuleDto
{
    public int Id { get; set; }
    public int FormFieldId { get; set; }
    public string ValidationType { get; set; } = string.Empty;
    public int? DependsOnFieldId { get; set; }
    public string ConfigJson { get; set; } = "{}";
    public string ErrorMessage { get; set; } = string.Empty;
    public string? SuccessMessage { get; set; }
    public bool IsBlocking { get; set; } = true;
    public bool RequiresDependencyFilled { get; set; } = false;
}

/// <summary>
/// DTO for creating a FieldValidationRule.
/// </summary>
public class CreateFieldValidationRuleDto
{
    public int FormFieldId { get; set; }
    public string ValidationType { get; set; } = string.Empty;
    public int? DependsOnFieldId { get; set; }
    public string ConfigJson { get; set; } = "{}";
    public string ErrorMessage { get; set; } = string.Empty;
    public string? SuccessMessage { get; set; }
    public bool IsBlocking { get; set; } = true;
}

/// <summary>
/// DTO for updating a FieldValidationRule.
/// </summary>
public class UpdateFieldValidationRuleDto
{
    public string ValidationType { get; set; } = string.Empty;
    public int? DependsOnFieldId { get; set; }
    public string ConfigJson { get; set; } = "{}";
    public string ErrorMessage { get; set; } = string.Empty;
    public string? SuccessMessage { get; set; }
    public bool IsBlocking { get; set; } = true;
}

/// <summary>
/// DTO for requesting validation of a field value.
/// Frontend sends this to backend when field value changes.
/// </summary>
public class ValidateFieldDto
{
    public int FieldId { get; set; }
    public object? FieldValue { get; set; }
    /// <summary>
    /// Form context data: {fieldName: value}
    /// Contains all fields filled out so far in the current form session.
    /// Used to resolve dependencies (e.g., "TownId": 123)
    /// </summary>
    public Dictionary<string, object?> FormContextData { get; set; } = new();
}

/// <summary>
/// DTO returned by validation API.
/// Contains result of all validations for a field.
/// </summary>
public class ValidationResultDto
{
    public bool IsValid { get; set; }
    public string? Message { get; set; }
    /// <summary>
    /// Placeholders used in error/success message.
    /// Frontend replaces {key} with values from this dictionary.
    /// Example: {parentEntityName: "MainTown", regionName: "town_main_123"}
    /// </summary>
    public Dictionary<string, string>? Placeholders { get; set; }
    public bool IsBlocking { get; set; }
}

/// <summary>
/// Represents configuration issues found in a FormConfiguration.
/// Used for admin UI "Configuration Health" checking.
/// </summary>
public class ValidationIssueDto
{
    public string Severity { get; set; } = "Warning";  // "Error" or "Warning"
    public string Message { get; set; } = string.Empty;
    public int? FieldId { get; set; }
    public int? RuleId { get; set; }
}
