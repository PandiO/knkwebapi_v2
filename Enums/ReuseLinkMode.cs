namespace knkwebapi_v2.Enums
{
    /// <summary>
    /// Specifies how a reusable FormStep or FormField template is used in a configuration.
    /// </summary>
    public enum ReuseLinkMode
    {
        /// <summary>
        /// Copy mode (default): Create an independent clone of the template.
        /// The new step/field is fully detached from the source.
        /// Future changes to the source template are NOT reflected in this copy.
        /// SourceStepId/SourceFieldId is used only for traceability and analytics.
        /// </summary>
        Copy = 0,

        /// <summary>
        /// Link mode: Create a lightweight reference to the source template.
        /// The step/field uses properties from the source template.
        /// Future changes to the source template ARE reflected in linked instances.
        /// Changes are synchronized at read-time via the service layer.
        /// Use IsLinkedToSource to distinguish linked instances from copies.
        /// </summary>
        Link = 1
    }
}
