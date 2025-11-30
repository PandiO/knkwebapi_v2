using System;

namespace knkwebapi_v2.Attributes
{
    /// <summary>
    /// Marks an entity as available for dynamic form configuration.
    /// The DisplayName is shown in the FormBuilder UI.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class FormConfigurableEntityAttribute : Attribute
    {
        public string DisplayName { get; }

        public FormConfigurableEntityAttribute(string displayName)
        {
            DisplayName = displayName;
        }
    }

    /// <summary>
    /// Marks a property as a relationship to another entity.
    /// Allows the FormBuilder to treat it as a foreign key reference
    /// and provide appropriate UI (dropdown, autocomplete, nested form).
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class RelatedEntityFieldAttribute : Attribute
    {
        public Type RelatedEntityType { get; }

        public RelatedEntityFieldAttribute(Type relatedEntityType)
        {
            RelatedEntityType = relatedEntityType;
        }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class NavigationPairAttribute : Attribute
    {
        public string NaviationPropertyName { get; }

        public NavigationPairAttribute(string naviationPropertyName)
        {
            NaviationPropertyName = naviationPropertyName;
        }
    }
}
