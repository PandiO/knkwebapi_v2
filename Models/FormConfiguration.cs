using System;
using System.Collections.Generic;

namespace knkwebapi_v2.Models
{
    /// <summary>
    /// Represents a complete multi-step form wizard configuration for creating/editing a specific entity.
    /// Each entity type (e.g., "Shop", "Structure") can have multiple configurations, but only one can be marked as default.
    /// 
    /// Purpose: Define the complete structure of a dynamic form that will be rendered in the React frontend.
    /// The frontend fetches this configuration and generates a wizard UI based on the steps and fields defined here.
    /// </summary>
    public class FormConfiguration
    {
        public int Id { get; set; }
        
        /// <summary>
        /// Unique identifier for tracking configurations across systems.
        /// Useful when syncing configurations between environments or for auditing.
        /// </summary>
        public Guid ConfigurationGuid { get; set; } = Guid.NewGuid();
        
        /// <summary>
        /// The name of the domain entity this configuration is for (e.g., "Shop", "User", "Structure").
        /// Multiple configurations can exist for the same EntityName (e.g., "Basic Shop Form", "Advanced Shop Form").
        /// </summary>
        public string EntityName { get; set; } = null!;
        
        /// <summary>
        /// Indicates if this is the default configuration to use when creating/editing this entity type.
        /// Only one configuration per EntityName should have IsDefault = true.
        /// When the frontend requests a form for "Shop", it will get the default one unless a specific config ID is requested.
        /// </summary>
        public bool IsDefault { get; set; } = false;
        
        /// <summary>
        /// Optional human-readable description of this configuration (e.g., "Simplified shop creation for new users").
        /// </summary>
        public string? Description { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        
        /// <summary>
        /// JSON array storing the ordered IDs of steps: ["step-guid-1", "step-guid-2", ...].
        /// 
        /// WHY JSON and not a simple int order column on FormStep?
        /// 1. FLEXIBILITY: Allows reordering steps without updating multiple database rows.
        ///    Just update this single JSON field to change the entire step sequence.
        /// 2. REUSABLE STEPS: The same FormStep can appear in multiple configurations.
        ///    Each config can have its own order without conflicts.
        /// 3. DYNAMIC INSERTION: Easy to insert a step between two existing steps without recalculating order values.
        /// 4. COPY-ON-REUSE: When reusing a step, we clone it. This JSON tracks the specific clone instances.
        /// 
        /// Alternative considered: Using a junction table (FormConfigurationStep) with an Order column.
        /// Trade-off: JSON is simpler for this use case, but loses database-level referential integrity.
        /// We accept this trade-off because step order is configuration metadata, not critical relational data.
        /// </summary>
        public string StepOrderJson { get; set; } = "[]";
        
        // Navigation properties
        public List<FormStep> Steps { get; set; } = new();
        public List<FormSubmissionProgress> SubmissionProgresses { get; set; } = new();
    }
}
