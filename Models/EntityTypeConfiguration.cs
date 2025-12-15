using System;

namespace knkwebapi_v2.Models
{
    /// <summary>
    /// Stores admin-configurable display and UI settings for entity types.
    /// 
    /// PURPOSE:
    /// Extends the base EntityMetadata (derived from model annotations) with
    /// runtime-configurable properties like icons, display colors, visibility, etc.
    /// 
    /// This allows administrators to customize how entity types appear in the UI
    /// without modifying code.
    /// 
    /// DESIGN DECISION:
    /// Separate from EntityMetadata because:
    /// 1. Base metadata is auto-discovered from model annotations (read-only)
    /// 2. Configuration is stored in DB and managed via admin UI
    /// 3. Keeps concerns separate: structure vs. presentation
    /// 
    /// USAGE:
    /// - Stores icon key (references icon registry in frontend)
    /// - Stores custom icon URL (override for icon registry)
    /// - Stores display color/styling hints
    /// - Stores visibility and sort order for UI listings
    /// 
    /// RELATIONSHIP:
    /// EntityTypeConfiguration extends EntityMetadata at runtime via merge in service layer.
    /// UI receives complete merged metadata for rendering.
    /// </summary>
    public class EntityTypeConfiguration
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// Entity type name (must match EntityMetadata.EntityName).
        /// Example: "Town", "Structure", "District"
        /// Unique constraint: only one configuration per entity type.
        /// </summary>
        public string EntityTypeName { get; set; } = null!;

        /// <summary>
        /// Icon identifier matching the frontend icon registry.
        /// Example: "map-pin", "home", "building", "tag"
        /// If null, UI falls back to default icon or customIconUrl.
        /// 
        /// Reference to Lucide icon names in kebab-case.
        /// </summary>
        public string? IconKey { get; set; }

        /// <summary>
        /// URL to a custom icon image (override for icon registry).
        /// If provided, takes precedence over IconKey.
        /// Supports: PNG, SVG, JPG, or data URIs.
        /// </summary>
        public string? CustomIconUrl { get; set; }

        /// <summary>
        /// Optional display color for UI elements (badges, highlights, etc.).
        /// Format: hex color (#RRGGBB) or Tailwind class name (e.g., "bg-blue-100", "text-purple-800")
        /// Example: "#E8F5E9" or "bg-green-100"
        /// </summary>
        public string? DisplayColor { get; set; }

        /// <summary>
        /// Sort order for entity listings in UI sidebars and dropdowns.
        /// Lower values appear first.
        /// Default: 0 (or entity name alphabetically if not set).
        /// </summary>
        public int SortOrder { get; set; } = 0;

        /// <summary>
        /// Controls visibility of this entity type in UI listings.
        /// If false, entity is hidden from FormWizard sidebar, entity dropdowns, etc.
        /// Useful for deprecated or internal-only entities.
        /// Default: true
        /// </summary>
        public bool IsVisible { get; set; } = true;

        /// <summary>
        /// Timestamp when this configuration was created.
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Timestamp when this configuration was last updated.
        /// </summary>
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
