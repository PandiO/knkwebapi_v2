using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using knkwebapi_v2.Enums;
using knkwebapi_v2.Models;

namespace knkwebapi_v2.Services
{
    public class DisplayConditionEvaluator : IDisplayConditionEvaluator
    {
        public bool IsVisible(
            IEnumerable<DisplayConditionGroup> groups,
            IReadOnlyDictionary<string, object?> visibleValues,
            IReadOnlyDictionary<Guid, string> fieldNameByGuid)
        {
            var active = groups?.Where(g => g.IsActive).OrderBy(g => g.Order).ToList()
                         ?? new List<DisplayConditionGroup>();

            if (active.Count == 0) return true;

            bool result = EvaluateGroup(active[0], visibleValues, fieldNameByGuid);

            for (int i = 1; i < active.Count; i++)
            {
                var groupResult = EvaluateGroup(active[i], visibleValues, fieldNameByGuid);
                result = active[i].CombineWithPreviousLogic == DisplayConditionLogic.And
                    ? result && groupResult
                    : result || groupResult;
            }

            return result;
        }

        public Dictionary<string, object?> FilterVisibleValues(
            FormConfiguration config,
            IReadOnlyDictionary<string, object?> submittedValues)
        {
            if (config == null) throw new ArgumentNullException(nameof(config));
            submittedValues ??= new Dictionary<string, object?>();

            var fieldNameByGuid = config.Steps
                .SelectMany(s => s.Fields)
                .GroupBy(f => f.FieldGuid)
                .ToDictionary(g => g.Key, g => g.First().FieldName);

            var visible = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);

            foreach (var step in OrderSteps(config))
            {
                if (!IsVisible(step.DisplayConditionGroups, visible, fieldNameByGuid)) continue;

                foreach (var field in OrderFields(step))
                {
                    if (!IsVisible(field.DisplayConditionGroups, visible, fieldNameByGuid)) continue;

                    if (submittedValues.TryGetValue(field.FieldName, out var value))
                    {
                        visible[field.FieldName] = value;
                    }
                }
            }

            return visible;
        }

        private bool EvaluateGroup(
            DisplayConditionGroup group,
            IReadOnlyDictionary<string, object?> visibleValues,
            IReadOnlyDictionary<Guid, string> fieldNameByGuid)
        {
            var conditions = group.Conditions.OrderBy(c => c.Order).ToList();
            if (conditions.Count == 0) return true;

            var results = conditions.Select(c => EvaluateCondition(c, visibleValues, fieldNameByGuid));

            return group.InnerLogic == DisplayConditionLogic.And
                ? results.All(r => r)
                : results.Any(r => r);
        }

        private bool EvaluateCondition(
            DisplayCondition condition,
            IReadOnlyDictionary<string, object?> visibleValues,
            IReadOnlyDictionary<Guid, string> fieldNameByGuid)
        {
            object? actual = null;
            if (fieldNameByGuid.TryGetValue(condition.SourceFormFieldGuid, out var fieldName))
            {
                visibleValues.TryGetValue(fieldName, out actual);
            }

            return Compare(actual, condition.Operator, condition.ValueJson);
        }

        private static bool Compare(object? actual, ConditionOperator op, string valueJson)
        {
            var actualText = ToText(actual);

            switch (op)
            {
                case ConditionOperator.IsEmpty:
                    return string.IsNullOrEmpty(actualText);
                case ConditionOperator.IsNotEmpty:
                    return !string.IsNullOrEmpty(actualText);
            }

            var expected = ParseJson(valueJson);

            switch (op)
            {
                case ConditionOperator.Equals:
                    return ScalarEquals(actual, expected);
                case ConditionOperator.NotEquals:
                    return !ScalarEquals(actual, expected);
                case ConditionOperator.In:
                    return ToList(expected).Any(e => ScalarEquals(actual, e));
                case ConditionOperator.NotIn:
                    return !ToList(expected).Any(e => ScalarEquals(actual, e));
                case ConditionOperator.Contains:
                    return actualText != null && ToText(expected) is string needle
                           && actualText.Contains(needle, StringComparison.OrdinalIgnoreCase);
                case ConditionOperator.GreaterThan:
                case ConditionOperator.GreaterOrEqual:
                case ConditionOperator.LessThan:
                case ConditionOperator.LessOrEqual:
                    return CompareNumeric(actual, expected, op);
                default:
                    return false;
            }
        }

        private static bool CompareNumeric(object? actual, object? expected, ConditionOperator op)
        {
            var a = ToNumber(actual);
            var b = ToNumber(expected);
            if (a == null || b == null) return false;

            return op switch
            {
                ConditionOperator.GreaterThan => a > b,
                ConditionOperator.GreaterOrEqual => a >= b,
                ConditionOperator.LessThan => a < b,
                ConditionOperator.LessOrEqual => a <= b,
                _ => false
            };
        }

        private static bool ScalarEquals(object? actual, object? expected)
        {
            var a = ToNumber(actual);
            var b = ToNumber(expected);
            if (a != null && b != null) return a == b;

            var aBool = ToBool(actual);
            var bBool = ToBool(expected);
            if (aBool != null && bBool != null) return aBool == bBool;

            return string.Equals(ToText(actual), ToText(expected), StringComparison.OrdinalIgnoreCase);
        }

        private static object? ParseJson(string? json)
        {
            if (string.IsNullOrWhiteSpace(json)) return null;

            try
            {
                using var doc = JsonDocument.Parse(json);
                return Unwrap(doc.RootElement);
            }
            catch (JsonException)
            {
                // Tolerate values that were stored as a bare string instead of JSON.
                return json;
            }
        }

        private static object? Unwrap(JsonElement element)
        {
            return element.ValueKind switch
            {
                JsonValueKind.String => element.GetString(),
                JsonValueKind.Number => element.GetDouble(),
                JsonValueKind.True => true,
                JsonValueKind.False => false,
                JsonValueKind.Null or JsonValueKind.Undefined => null,
                JsonValueKind.Array => element.EnumerateArray().Select(Unwrap).ToList(),
                _ => element.GetRawText()
            };
        }

        private static IEnumerable<object?> ToList(object? value)
        {
            if (value is IEnumerable<object?> list) return list;
            return value == null ? Enumerable.Empty<object?>() : new[] { value };
        }

        private static string? ToText(object? value)
        {
            if (value == null) return null;
            if (value is JsonElement je) return ToText(Unwrap(je));
            if (value is bool b) return b ? "true" : "false";
            if (value is double d) return d.ToString("R", CultureInfo.InvariantCulture);
            return Convert.ToString(value, CultureInfo.InvariantCulture);
        }

        private static double? ToNumber(object? value)
        {
            if (value == null) return null;
            if (value is JsonElement je) return ToNumber(Unwrap(je));
            if (value is bool) return null;
            if (value is double d) return d;
            if (value is int i) return i;
            if (value is long l) return l;
            if (value is decimal m) return (double)m;

            var text = Convert.ToString(value, CultureInfo.InvariantCulture);
            return double.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out var parsed)
                ? parsed
                : null;
        }

        private static bool? ToBool(object? value)
        {
            if (value is bool b) return b;
            if (value is JsonElement je) return ToBool(Unwrap(je));

            var text = ToText(value);
            if (bool.TryParse(text, out var parsed)) return parsed;
            return null;
        }

        private static IEnumerable<FormStep> OrderSteps(FormConfiguration config)
            => FormOrdering.OrderSteps(config);

        private static IEnumerable<FormField> OrderFields(FormStep step)
            => FormOrdering.OrderFields(step);
    }
}
