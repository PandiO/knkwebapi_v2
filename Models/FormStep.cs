using System;
using System.Collections.Generic;

namespace knkwebapi_v2.Models
{
    /// <summary>
    /// Represents a single step (page) in a multi-step form wizard.
    /// A step contains multiple fields that are displayed together.
    /// 
    /// REUSABILITY PATTERN:
    /// - Steps can be marked as "reusable templates" (IsReusable = true, FormConfigurationId = null).
    /// - When adding a reusable step to a configuration, it is CLONED (not referenced).
    /// - This prevents accidental modifications across multiple configurations.
    /// - The SourceStepId tracks which template was used to create this step instance.
    /// 
    /// Example: A "Basic Info" step (name, description) can be reused across Shop, Structure, and Item forms.
    /// Each configuration gets its own copy, so customizing fields in one doesn't affect others.
    /// </summary>
    public class FormStep
    {
        public int Id { get; set; }
        
        /// <summary>
        /// Unique identifier for this step instance. Used in StepOrderJson to reference this specific step.
        /// </summary>
        public Guid StepGuid { get; set; } = Guid.NewGuid();
        
        /// <summary>
        /// Display name shown in the wizard UI (e.g., "Basic Information", "Location Settings").
        /// </summary>
        public string StepName { get; set; } = null!;
        
        /// <summary>
        /// Optional description shown to users (e.g., "Provide the basic details for your shop").
        /// </summary>
        public string? Description { get; set; }
        
        /// <summary>
        /// If true, this step is a template stored in the reusable step library.
        /// Reusable steps have FormConfigurationId = null and are copied when added to configurations.
        /// </summary>
        public bool IsReusable { get; set; } = false;
        
        /// <summary>
        /// Tracks which reusable step template was used to create this step instance.
        /// NULL if this is an original step (not cloned from a template).
        /// 
        /// USE CASE: When updating a reusable template, you can optionally offer to update all derived steps.
        /// Also useful for analytics: "How many configurations use the 'Location Settings' template?"
        /// 
        /// BEHAVIOR DEPENDS ON IsLinkedToSource:
        /// - If IsLinkedToSource = false (Copy mode): This is a full clone. SourceStepId is for traceability only.
        /// - If IsLinkedToSource = true (Link mode): This is a reference to the source. Display properties come from source.
        /// </summary>
        public int? SourceStepId { get; set; }
        
        /// <summary>
        /// Indicates whether this step is linked to a source template (true) or is an independent copy (false).
        /// 
        /// Link mode (IsLinkedToSource = true):
        /// - This step references the source template (SourceStepId).
        /// - Display properties (StepName, Description, FieldOrderJson) are loaded from the source at read-time.
        /// - Changes to the source template are immediately visible in linked instances.
        /// - Limited to FieldOrderJson and field order for customization.
        /// 
        /// Copy mode (IsLinkedToSource = false):
        /// - This step is a full clone of the source, fully independent after creation.
        /// - All properties (StepName, Description, FieldOrderJson) are owned by this instance.
        /// - Changes to the source template do NOT affect this copy.
        /// - SourceStepId is kept only for traceability/analytics purposes.
        /// 
        /// Default: false (copy mode is the standard behavior).
        /// </summary>
        public bool IsLinkedToSource { get; set; } = false;
        
        /// <summary>
        /// JSON array storing the ordered GUIDs of fields: ["field-guid-1", "field-guid-2", ...].
        /// 
        /// WHY JSON for field ordering?
        /// Same reasons as StepOrderJson in FormConfiguration:
        /// 1. Easy reordering without updating multiple rows.
        /// 2. Supports dynamic field insertion/removal.
        /// 3. Each step instance can have its own field order (important for copy-on-reuse pattern).
        /// 4. Simplifies API: Send entire step with ordered field list in one JSON payload.
        /// 
        /// Example scenario:
        /// - Reusable step "Basic Info" has fields: [Name, Description, Category].
        /// - Config A uses it as: [Name, Category, Description] (reordered).
        /// - Config B uses it as: [Category, Name, Description] (different order).
        /// Since each config clones the step, each has its own FieldOrderJson.
        /// </summary>
        public string FieldOrderJson { get; set; } = "[]";
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        /// <summary>
        /// Foreign key to the configuration this step belongs to.
        /// NULL for reusable template steps (stored in the library for reuse).
        /// NON-NULL for steps that are part of a specific configuration instance.
        /// </summary>
        public int? FormConfigurationId { get; set; }
        public FormConfiguration? FormConfiguration { get; set; }
        
        /// <summary>
        /// If true, this step represents a many-to-many relationship editor.
        /// Used to configure join entity instances with extra fields (e.g., ItemBlueprintDefaultEnchantment with Level).
        /// When true, the step displays:
        /// 1. PagedEntityTable for selecting related entities
        /// 2. Cards showing selected relationships with editable join entity fields
        /// 3. Uses childFormSteps to define the join entity field template
        /// </summary>
        public bool IsManyToManyRelationship { get; set; } = false;
        
        /// <summary>
        /// The property name on the parent entity that holds the many-to-many relationship collection.
        /// Example: For ItemBlueprint editing enchantments, this would be "DefaultForBlueprints" or "DefaultEnchantments".
        /// Used to determine which related entity collection to modify.
        /// </summary>
        public string? RelatedEntityPropertyName { get; set; }
        
        /// <summary>
        /// The fully qualified type name of the join entity (e.g., "ItemBlueprintDefaultEnchantment").
        /// This entity holds the many-to-many relationship and any extra fields (like Level, Priority, etc.).
        /// System will retrieve metadata for this entity to allow editing its fields.
        /// </summary>
        public string? JoinEntityType { get; set; }

        /// <summary>
        /// Optional linked form configuration used to create/edit join entity entries in modal mode.
        /// When set for many-to-many steps, this configuration is used as the primary source for join fields.
        /// </summary>
        public int? SubConfigurationId { get; set; }
        public FormConfiguration? SubConfiguration { get; set; }
        
        /// <summary>
        /// Foreign key to parent step if this is a child step defining join entity fields.
        /// Child steps define the template for editing join entity instances in many-to-many relationships.
        /// NULL for top-level steps, NON-NULL for child steps that belong to a many-to-many relationship step.
        /// </summary>
        public int? ParentStepId { get; set; }
        public FormStep? ParentStep { get; set; }
        
        /// <summary>
        /// Collection of child steps that define the join entity field template for many-to-many relationships.
        /// These steps are displayed when editing individual relationship instances (the cards).
        /// Example: For ItemBlueprint → EnchantmentDefinition with ItemBlueprintDefaultEnchantment join entity,
        /// child steps define fields like "Level", "ApplyByDefault", etc.
        /// </summary>
        public List<FormStep> ChildFormSteps { get; set; } = new();
        
        // Navigation properties
        public List<FormField> Fields { get; set; } = new();
        public List<StepCondition> StepConditions { get; set; } = new();
    }
}
