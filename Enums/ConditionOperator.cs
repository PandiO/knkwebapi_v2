namespace knkwebapi_v2.Enums
{
    public enum ConditionOperator
    {
        Equals,
        NotEquals,
        GreaterThan,
        GreaterOrEqual,
        LessThan,
        LessOrEqual,
        In,
        NotIn,
        And,
        Or,
        // Appended so existing ordinal values stay stable.
        Contains,
        IsEmpty,
        IsNotEmpty
    }
}
