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
        /// </summary>
        public int? SourceStepId { get; set; }
        
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
        
        // Navigation properties
        public List<FormField> Fields { get; set; } = new();
        public List<StepCondition> StepConditions { get; set; } = new();
    }
}
