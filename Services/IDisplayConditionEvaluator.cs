using System.Collections.Generic;
using knkwebapi_v2.Models;

namespace knkwebapi_v2.Services
{
    public interface IDisplayConditionEvaluator
    {
        /// <summary>
        /// Evaluates a target's condition groups against the values that are currently visible.
        /// A target without active groups is always visible.
        /// </summary>
        bool IsVisible(
            IEnumerable<DisplayConditionGroup> groups,
            IReadOnlyDictionary<string, object?> visibleValues,
            IReadOnlyDictionary<System.Guid, string> fieldNameByGuid);

        /// <summary>
        /// Walks the configuration in form order and returns only the values that belong to
        /// steps and fields the user could actually see. Anything hidden is dropped, so data left
        /// behind by a toggled-away branch can never reach entity creation.
        /// </summary>
        Dictionary<string, object?> FilterVisibleValues(
            FormConfiguration config,
            IReadOnlyDictionary<string, object?> submittedValues);
    }
}
