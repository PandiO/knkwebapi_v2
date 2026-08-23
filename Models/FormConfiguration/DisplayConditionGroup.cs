using System.Collections.Generic;
using knkwebapi_v2.Enums;

namespace knkwebapi_v2.Models
{
    /// <summary>
    /// A set of conditions that together decide whether a <see cref="FormStep"/> or
    /// <see cref="FormField"/> is shown to the user while filling in a form.
    ///
    /// Structure (two levels, deliberately not arbitrarily deep):
    /// - Conditions inside one group are combined with <see cref="InnerLogic"/>.
    /// - Groups of the same target are folded left-to-right using each group's
    ///   <see cref="CombineWithPreviousLogic"/> (the first group's value is ignored).
    ///
    /// A target without any active group is always visible.
    /// </summary>
    public class DisplayConditionGroup
    {
        public int Id { get; set; }

        public DisplayConditionTargetType TargetType { get; set; }

        /// <summary>Set when <see cref="TargetType"/> is FormStep; otherwise null.</summary>
        public int? TargetStepId { get; set; }
        public FormStep? TargetStep { get; set; }

        /// <summary>Set when <see cref="TargetType"/> is FormField; otherwise null.</summary>
        public int? TargetFieldId { get; set; }
        public FormField? TargetField { get; set; }

        public DisplayConditionLogic InnerLogic { get; set; } = DisplayConditionLogic.And;

        public DisplayConditionLogic CombineWithPreviousLogic { get; set; } = DisplayConditionLogic.Or;

        public int Order { get; set; }

        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Reserved for future arbitrarily nested groups. Always null in the current version.
        /// </summary>
        public int? ParentGroupId { get; set; }
        public DisplayConditionGroup? ParentGroup { get; set; }

        public List<DisplayConditionGroup> ChildGroups { get; set; } = new();

        public List<DisplayCondition> Conditions { get; set; } = new();
    }
}
