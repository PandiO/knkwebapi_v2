using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using knkwebapi_v2.Services.Interfaces;

namespace knkwebapi_v2.Services.ValidationMethods
{
    /// <summary>
    /// Validates that a field is required only when a condition is met on a dependency field.
    /// 
    /// ConfigJson schema:
    /// {
    ///   "operator": "equals",        // "equals", "notEquals", "greaterThan", "lessThan", "contains", "in"
    ///   "value": "specific_value"    // Value to compare against
    /// }
    /// 
    /// Example: A "description" field becomes required when the user selects status="complex"
    /// - FieldValue: Description text (current value of field being validated)
    /// - DependencyValue: "complex" (value of dependency field - Status)
    /// - ConfigJson: { "operator": "equals", "value": "complex" }
    /// 
    /// Logic:
    /// - If dependency value matches condition: field MUST be filled
    /// - If dependency value doesn't match condition: field can be empty
    /// </summary>
    public class ConditionalRequiredValidator : IValidationMethod
    {
        public string ValidationType => "ConditionalRequired";

        public async Task<ValidationMethodResult> ValidateAsync(
            object? fieldValue,
            object? dependencyValue,
            string? configJson,
            Dictionary<string, object>? formContextData)
        {
            try
            {
                // Parse configuration
                var config = string.IsNullOrEmpty(configJson)
                    ? new ConditionalRequiredConfig()
                    : JsonSerializer.Deserialize<ConditionalRequiredConfig>(configJson)
                        ?? new ConditionalRequiredConfig();

                // Evaluate condition: does dependency value match the configured value?
                var conditionMet = EvaluateCondition(dependencyValue, config.Operator, config.Value);

                // If condition NOT met: validation passes (field doesn't need to be filled)
                if (!conditionMet)
                {
                    return await Task.FromResult(new ValidationMethodResult
                    {
                        IsValid = true,
                        Message = "Condition not met; field is optional"
                    });
                }

                // Condition IS met: field MUST be filled
                var isEmpty = IsValueEmpty(fieldValue);

                if (isEmpty)
                {
                    return await Task.FromResult(new ValidationMethodResult
                    {
                        IsValid = false,
                        Message = "This field is required when the condition is met",
                        Placeholders = new Dictionary<string, string>
                        {
                            { "operator", config.Operator },
                            { "conditionValue", config.Value?.ToString() ?? "null" }
                        }
                    });
                }

                // Field is filled and condition is met: validation passes
                return await Task.FromResult(new ValidationMethodResult
                {
                    IsValid = true,
                    Message = "Field is properly filled based on condition"
                });
            }
            catch (Exception ex)
            {
                return new ValidationMethodResult
                {
                    IsValid = false,
                    Message = $"Validation error: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Evaluate if dependency value meets the configured condition.
        /// </summary>
        private bool EvaluateCondition(object? dependencyValue, string op, object? conditionValue)
        {
            if (dependencyValue == null)
                return false;

            return op.ToLowerInvariant() switch
            {
                "equals" => Equals(dependencyValue, conditionValue),
                "notequals" => !Equals(dependencyValue, conditionValue),
                "greaterthan" => Compare(dependencyValue, conditionValue) > 0,
                "lessthan" => Compare(dependencyValue, conditionValue) < 0,
                "contains" => dependencyValue.ToString()?.Contains(conditionValue?.ToString() ?? "") ?? false,
                "in" => IsInList(dependencyValue, conditionValue),
                _ => false
            };
        }

        /// <summary>
        /// Check if a value is considered "empty" for validation purposes.
        /// </summary>
        private bool IsValueEmpty(object? value)
        {
            return value == null
                || string.Empty.Equals(value.ToString())
                || (value is string str && string.IsNullOrWhiteSpace(str))
                || (value is int intVal && intVal == 0)
                || (value is int? intNullVal && (!intNullVal.HasValue || intNullVal.Value == 0));
        }

        /// <summary>
        /// Compare two objects using their comparable interface.
        /// </summary>
        private int Compare(object left, object? right)
        {
            if (left is IComparable comparable)
            {
                return comparable.CompareTo(right);
            }

            // Try numeric comparison
            if (double.TryParse(left.ToString(), out var leftNum) 
                && double.TryParse(right?.ToString() ?? "0", out var rightNum))
            {
                return leftNum.CompareTo(rightNum);
            }

            return 0;
        }

        /// <summary>
        /// Check if value is in a comma-separated list or array.
        /// </summary>
        private bool IsInList(object value, object? listValue)
        {
            if (listValue == null)
                return false;

            var stringValue = value.ToString();

            // If listValue is a comma-separated string
            if (listValue is string listStr)
            {
                var items = listStr.Split(',');
                return Array.Exists(items, item => item.Trim().Equals(stringValue, StringComparison.OrdinalIgnoreCase));
            }

            // If listValue is an array or collection
            if (listValue is System.Collections.IEnumerable enumerable)
            {
                foreach (var item in enumerable)
                {
                    if (item.ToString()?.Equals(stringValue, StringComparison.OrdinalIgnoreCase) ?? false)
                        return true;
                }
            }

            return false;
        }
    }

    /// <summary>
    /// Configuration for ConditionalRequiredValidator
    /// </summary>
    public class ConditionalRequiredConfig
    {
        /// <summary>
        /// Comparison operator.
        /// Valid values: "equals", "notEquals", "greaterThan", "lessThan", "contains", "in"
        /// </summary>
        public string Operator { get; set; } = "equals";

        /// <summary>
        /// Value to compare against.
        /// For "in" operator, can be comma-separated: "value1,value2,value3"
        /// </summary>
        public object? Value { get; set; }
    }
}
