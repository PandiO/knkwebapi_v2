using System;
using knkwebapi_v2.Enums;

namespace knkwebapi_v2.Models
{
    /// <summary>
    /// A single comparison inside a <see cref="DisplayConditionGroup"/>:
    /// "the value entered in <see cref="SourceFormField"/> <see cref="Operator"/> <see cref="ValueJson"/>".
    ///
    /// The source field must appear earlier in the form than the target of the group,
    /// so a condition can never depend on input the user has not reached yet.
    /// </summary>
    public class DisplayCondition
    {
        public int Id { get; set; }

        public int DisplayConditionGroupId { get; set; }
        public DisplayConditionGroup DisplayConditionGroup { get; set; } = null!;

        public int SourceFormFieldId { get; set; }
        public FormField SourceFormField { get; set; } = null!;

        /// <summary>
        /// Authoring-stable reference to the source field. The builder works with GUIDs because
        /// fields created in the same save do not have a database id yet; the FK is resolved from
        /// this GUID server-side.
        /// </summary>
        public Guid SourceFormFieldGuid { get; set; }

        public ConditionOperator Operator { get; set; } = ConditionOperator.Equals;

        /// <summary>
        /// JSON-encoded comparison value. A scalar for most operators
        /// (e.g. <c>"DRAWBRIDGE"</c>, <c>6</c>, <c>true</c>) and an array for In/NotIn
        /// (e.g. <c>["SLIDING","TRAP"]</c>). Ignored by IsEmpty/IsNotEmpty.
        /// </summary>
        public string ValueJson { get; set; } = "null";

        public int Order { get; set; }
    }
}
