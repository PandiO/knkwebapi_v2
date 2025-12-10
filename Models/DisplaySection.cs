using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace knkwebapi_v2.Models
{
    /// <summary>
    /// Represents a section grouping display fields.
    /// Can be bound to relationship properties for displaying related entities.
    /// Supports nested sections for collection relationships.
    /// 
    /// REUSABILITY PATTERN (same as FormStep):
    /// - Sections can be marked as "reusable templates" (IsReusable = true, DisplayConfigurationId = null).
    /// - When adding a reusable section to a configuration, it is CLONED or LINKED.
    /// - COPY mode: Full independent clone, changes don't propagate.
    /// - LINK mode: References source, changes propagate from source.
    /// </summary>
    public class DisplaySection
    {
        public int Id { get; set; }
        
        public Guid SectionGuid { get; set; } = Guid.NewGuid();
        
        [Required]
        [MaxLength(200)]
        public string SectionName { get; set; } = null!;
        
        [MaxLength(500)]
        public string? Description { get; set; }
        
        public bool IsReusable { get; set; } = false;
        
        public int? SourceSectionId { get; set; }
        
        /// <summary>
        /// Link mode (true): Properties loaded from source, changes propagate.
        /// Copy mode (false): Full clone, independent after creation.
        /// </summary>
        public bool IsLinkedToSource { get; set; } = false;
        
        /// <summary>
        /// JSON array storing ordered Field GUIDs: ["guid-1", "guid-2", ...].
        /// Each section instance has its own field order.
        /// </summary>
        [Required]
        public string FieldOrderJson { get; set; } = "[]";
        
        /// <summary>
        /// Property name of the related entity (e.g., "TownHouse", "Districts").
        /// When set, this section is dedicated to displaying that relationship.
        /// NULL means section displays fields from the main entity.
        /// </summary>
        [MaxLength(100)]
        public string? RelatedEntityPropertyName { get; set; }
        
        /// <summary>
        /// Entity type name of the related property (e.g., "Structure", "District").
        /// Must be NULL if RelatedEntityPropertyName is NULL.
        /// </summary>
        [MaxLength(100)]
        public string? RelatedEntityTypeName { get; set; }
        
        /// <summary>
        /// True if RelatedEntityPropertyName points to an ICollection property.
        /// Determines which action buttons are valid.
        /// </summary>
        public bool IsCollection { get; set; } = false;
        
        /// <summary>
        /// JSON object defining available action buttons.
        /// Structure depends on IsCollection (different buttons for single vs collection).
        /// </summary>
        [Required]
        public string ActionButtonsConfigJson { get; set; } = "{}";
        
        /// <summary>
        /// For nested sections: ID of parent section.
        /// Used to create subsection templates for collection items.
        /// NULL for top-level sections.
        /// </summary>
        public int? ParentSectionId { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime? UpdatedAt { get; set; }
        
        // Foreign keys
        public int? DisplayConfigurationId { get; set; }
        
        // Navigation properties
        public DisplayConfiguration? DisplayConfiguration { get; set; }
        public List<DisplayField> Fields { get; set; } = new();
        public List<DisplaySection> SubSections { get; set; } = new();
        public DisplaySection? ParentSection { get; set; }
    }
}
