namespace knkwebapi_v2.Enums
{
    /// <summary>
    /// What a <see cref="Models.DisplayConditionGroup"/> controls the visibility of.
    /// </summary>
    public enum DisplayConditionTargetType
    {
        FormStep = 0,
        FormField = 1
    }

    /// <summary>
    /// Boolean combinator used both between the conditions inside a group
    /// and between consecutive groups of the same target.
    /// </summary>
    public enum DisplayConditionLogic
    {
        And = 0,
        Or = 1
    }
}
