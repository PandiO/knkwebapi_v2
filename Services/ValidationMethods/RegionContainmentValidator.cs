using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using knkwebapi_v2.Services.Interfaces;

namespace knkwebapi_v2.Services.ValidationMethods
{
    /// <summary>
    /// Validates that a child region is fully contained within a parent region.
    /// 
    /// ConfigJson schema:
    /// {
    ///   "parentRegionPath": "WgRegionId",     // Path to parent region property on dependency entity
    ///   "requireFullContainment": true         // Whether child must be fully inside parent
    /// }
    /// 
    /// Example: District region must be inside Town region
    /// - FieldValue: District object (with WgRegionId - the child region)
    /// - DependencyValue: Town object (with WgRegionId - the parent region)
    /// - ConfigJson: { "parentRegionPath": "WgRegionId", "requireFullContainment": true }
    /// </summary>
    public class RegionContainmentValidator : IValidationMethod
    {
        private readonly IRegionService _regionService;

        public RegionContainmentValidator(IRegionService regionService)
        {
            _regionService = regionService;
        }

        public string ValidationType => "RegionContainment";

        public async Task<ValidationMethodResult> ValidateAsync(
            object? fieldValue,
            object? dependencyValue,
            string? configJson,
            Dictionary<string, object>? formContextData)
        {
            try
            {
                // Validate inputs
                if (fieldValue == null)
                {
                    return new ValidationMethodResult
                    {
                        IsValid = false,
                        Message = "Child region value is required"
                    };
                }

                if (dependencyValue == null)
                {
                    return new ValidationMethodResult
                    {
                        IsValid = false,
                        Message = "Dependency value (parent entity) is required"
                    };
                }

                // Parse configuration
                var config = string.IsNullOrEmpty(configJson)
                    ? new RegionContainmentConfig()
                    : JsonSerializer.Deserialize<RegionContainmentConfig>(configJson)
                        ?? new RegionContainmentConfig();

                // Extract child region ID
                var childRegionId = ExtractRegionId(fieldValue);
                if (childRegionId == null)
                {
                    return new ValidationMethodResult
                    {
                        IsValid = false,
                        Message = "Invalid or missing child region value"
                    };
                }

                // Extract parent region ID from dependency value
                var parentRegionId = ExtractPropertyValue(dependencyValue, config.ParentRegionPath);
                if (parentRegionId == null)
                {
                    return new ValidationMethodResult
                    {
                        IsValid = false,
                        Message = $"Cannot extract parent region from dependency entity using path '{config.ParentRegionPath}'"
                    };
                }

                // Check if child region is fully contained in parent region
                var isContained = await _regionService.IsRegionContainedAsync(
                    parentRegionId.ToString(),
                    childRegionId.ToString(),
                    config.RequireFullContainment);

                if (isContained)
                {
                    return new ValidationMethodResult
                    {
                        IsValid = true,
                        Message = "Child region is fully contained within the parent region"
                    };
                }

                // Get parent entity name for error message
                var parentEntityName = ExtractPropertyValue(dependencyValue, "Name")?.ToString() ?? "Parent Region";

                return new ValidationMethodResult
                {
                    IsValid = false,
                    Message = $"The region extends outside {parentEntityName}'s boundaries. All boundary points must be within the parent region.",
                    Placeholders = new Dictionary<string, string>
                    {
                        { "parentEntityName", parentEntityName },
                        { "childRegionId", childRegionId.ToString() },
                        { "parentRegionId", parentRegionId.ToString() }
                    }
                };
            }
            catch (Exception ex)
            {
                return new ValidationMethodResult
                {
                    IsValid = false,
                    Message = $"Validation error: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Extract region ID from a field value (can be an entity with region id, ID, or string).
        /// </summary>
        private object? ExtractRegionId(object fieldValue)
        {
            if (fieldValue == null)
                return null;

            // If it's a region ID integer
            if (fieldValue is int regionId)
                return regionId;

            // If it's a region ID string
            if (fieldValue is string regionIdStr)
                return regionIdStr;

            // Try to extract WgRegionId property (for entity objects)
            var property = fieldValue.GetType().GetProperty("WgRegionId",
                System.Reflection.BindingFlags.IgnoreCase | System.Reflection.BindingFlags.Public);
            if (property != null)
                return property.GetValue(fieldValue);

            return null;
        }

        /// <summary>
        /// Extract a property value from an object using dot notation path.
        /// Example: "Town.WgRegionId" or "WgRegionId"
        /// </summary>
        private object? ExtractPropertyValue(object obj, string path)
        {
            if (obj == null || string.IsNullOrEmpty(path))
                return null;

            var parts = path.Split('.');
            var current = obj;

            foreach (var part in parts)
            {
                if (current == null)
                    return null;

                var property = current.GetType().GetProperty(part,
                    System.Reflection.BindingFlags.IgnoreCase | System.Reflection.BindingFlags.Public);
                
                if (property == null)
                    return null;

                current = property.GetValue(current);
            }

            return current;
        }
    }

    /// <summary>
    /// Configuration for RegionContainmentValidator
    /// </summary>
    public class RegionContainmentConfig
    {
        /// <summary>
        /// Property path to extract parent region ID from dependency entity.
        /// Examples: "WgRegionId", "Town.WgRegionId"
        /// </summary>
        public string ParentRegionPath { get; set; } = "WgRegionId";

        /// <summary>
        /// If true, child region must be fully contained within parent.
        /// If false, allows partial overlap.
        /// </summary>
        public bool RequireFullContainment { get; set; } = true;
    }
}
