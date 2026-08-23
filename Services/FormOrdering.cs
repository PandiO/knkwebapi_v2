using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using knkwebapi_v2.Models;

namespace knkwebapi_v2.Services
{
    /// <summary>
    /// Resolves the order in which steps and fields are presented to the user.
    /// Ordering lives in StepOrderJson/FieldOrderJson as GUID arrays, so both the
    /// evaluator and the configuration validator need the same interpretation.
    /// </summary>
    internal static class FormOrdering
    {
        public static IReadOnlyList<FormStep> OrderSteps(FormConfiguration config)
            => OrderByGuidList(config.Steps, config.StepOrderJson, s => s.StepGuid);

        public static IReadOnlyList<FormField> OrderFields(FormStep step)
            => OrderByGuidList(step.Fields, step.FieldOrderJson, f => f.FieldGuid);

        private static IReadOnlyList<T> OrderByGuidList<T>(IEnumerable<T> items, string? orderJson, Func<T, Guid> guidSelector)
        {
            var list = items.ToList();
            if (string.IsNullOrWhiteSpace(orderJson)) return list;

            List<string>? order;
            try
            {
                order = JsonSerializer.Deserialize<List<string>>(orderJson);
            }
            catch (JsonException)
            {
                return list;
            }

            if (order == null || order.Count == 0) return list;

            var rank = new Dictionary<Guid, int>();
            for (int i = 0; i < order.Count; i++)
            {
                if (Guid.TryParse(order[i], out var guid)) rank[guid] = i;
            }

            return list.OrderBy(item => rank.TryGetValue(guidSelector(item), out var r) ? r : int.MaxValue).ToList();
        }
    }
}
