using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using knkwebapi_v2.Dtos;
using knkwebapi_v2.Services.Interfaces;

namespace knkwebapi_v2.Services.ValidationMethods
{
    /// <summary>
    /// Validates that a Location field value is inside a specified WorldGuard region.
    /// 
    /// ConfigJson schema:
    /// {
    ///   "regionPropertyPath": "WgRegionId",  // Path to region property on dependency entity
    ///   "allowBoundary": false               // Whether boundary touches are allowed
    /// }
    /// 
    /// Example: Location must be inside Town's region
    /// - FieldValue: Location object (with X, Y, Z, WorldId)
    /// - DependencyValue: Town object (with WgRegionId)
    /// - ConfigJson: { "regionPropertyPath": "WgRegionId", "allowBoundary": false }
    /// </summary>
    public class LocationInsideRegionValidator : IValidationMethod
    {
        private readonly IRegionService _regionService;
        private readonly ILocationService _locationService;

        public LocationInsideRegionValidator(IRegionService regionService, ILocationService locationService)
        {
            _regionService = regionService;
            _locationService = locationService;
        }

        public string ValidationType => "LocationInsideRegion";

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
                        Message = "Location value is required"
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
                    ? new LocationInsideRegionConfig()
                    : JsonSerializer.Deserialize<LocationInsideRegionConfig>(configJson) 
                        ?? new LocationInsideRegionConfig();

                // Extract Location data
                var location = fieldValue as LocationDto;
                if (location == null)
                {
                    // Try to extract Location ID from the field value
                    if (fieldValue is int locationId)
                    {
                        location = await _locationService.GetByIdAsync(locationId);
                    }
                    else if (fieldValue is string locationIdStr && int.TryParse(locationIdStr, out var locId))
                    {
                        location = await _locationService.GetByIdAsync(locId);
                    }
                    else
                    {
                        return new ValidationMethodResult
                        {
                            IsValid = false,
                            Message = "Invalid location value format"
                        };
                    }

                    if (location == null)
                    {
                        return new ValidationMethodResult
                        {
                            IsValid = false,
                            Message = "Location not found"
                        };
                    }
                }

                // Extract region ID from dependency value using property path
                var regionId = ExtractPropertyValue(dependencyValue, config.RegionPropertyPath);
                if (regionId == null)
                {
                    return new ValidationMethodResult
                    {
                        IsValid = false,
                        Message = $"Cannot extract region from dependency entity using path '{config.RegionPropertyPath}'"
                    };
                }

                // Check if location is inside region
                var isInside = await _regionService.IsLocationInsideRegionAsync(
                    regionId.ToString(),
                    location.X ?? 0,
                    location.Z ?? 0,
                    config.AllowBoundary);

                if (isInside)
                {
                    return new ValidationMethodResult
                    {
                        IsValid = true,
                        Message = "Location is within the allowed region"
                    };
                }

                // Get parent entity name for error message
                var parentEntityName = ExtractPropertyValue(dependencyValue, "Name")?.ToString() ?? "Region";
                var coordinates = $"{location.X}, {location.Z}";

                return new ValidationMethodResult
                {
                    IsValid = false,
                    Message = $"Location {coordinates} is outside {parentEntityName}'s boundaries. Please select a location within the region.",
                    Placeholders = new Dictionary<string, string>
                    {
                        { "parentEntityName", parentEntityName },
                        { "coordinates", coordinates },
                        { "regionName", regionId.ToString() }
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
    /// Configuration for LocationInsideRegionValidator
    /// </summary>
    public class LocationInsideRegionConfig
    {
        /// <summary>
        /// Property path to extract region ID from dependency entity.
        /// Examples: "WgRegionId", "Town.WgRegionId"
        /// </summary>
        public string RegionPropertyPath { get; set; } = "WgRegionId";

        /// <summary>
        /// If true, allows location on region boundary.
        /// If false, strictly inside region.
        /// </summary>
        public bool AllowBoundary { get; set; } = false;
    }
}
