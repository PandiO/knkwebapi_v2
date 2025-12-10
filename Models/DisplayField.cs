using System;
using System.ComponentModel.DataAnnotations;

namespace knkwebapi_v2.Models
{
    /// <summary>
    /// Represents a single display field (read-only, no input).
    /// Can reference entity properties directly or use template text with variables.
    /// 
    /// TEMPLATE TEXT FEATURE:
    /// Supports variable interpolation with ${...} syntax:
    /// - Simple properties: ${Name}
    /// - Nested properties (max 2 levels): ${TownHouse.Street.Name}
    /// - Collection counts: ${Districts.Count}
    /// - Basic calculations: ${Districts.Count + Streets.Count}
    /// 
    /// REUSABILITY PATTERN (same as FormField):
    /// - Fields can be marked as reusable templates.
    /// - COPY mode: Independent clone.
    /// - LINK mode: References source, changes propagate.
    /// </summary>
    public class DisplayField
    {
        public int Id { get; set; }
        
        public Guid FieldGuid { get; set; } = Guid.NewGuid();
        
        /// <summary>
        /// Property name from entity (e.g., "Name", "TownHouse.Street.Name").
        /// Can be NULL if TemplateText is used instead.
        /// </summary>
        [MaxLength(200)]
        public string? FieldName { get; set; }
        
        [Required]
        [MaxLength(200)]
        public string Label { get; set; } = null!;
        
        [MaxLength(500)]
        public string? Description { get; set; }
        
        /// <summary>
        /// Custom display text with ${...} variable interpolation.
        /// Takes precedence over FieldName if both are set.
        /// Example: "This town has ${Districts.Count} districts"
        /// </summary>
        public string? TemplateText { get; set; }
        
        /// <summary>
        /// Type hint for formatting (e.g., "String", "DateTime", "Integer").
        /// Used by frontend for proper value formatting.
        /// </summary>
        [MaxLength(50)]
        public string? FieldType { get; set; }
        
        public bool IsReusable { get; set; } = false;
        
        public int? SourceFieldId { get; set; }
        
        public bool IsLinkedToSource { get; set; } = false;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime? UpdatedAt { get; set; }
        
        // Foreign keys
        public int? DisplaySectionId { get; set; }
        
        // Navigation properties
        public DisplaySection? DisplaySection { get; set; }
    }
}
