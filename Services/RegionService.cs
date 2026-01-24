using System;
using System.Net.Http;
using System.Threading.Tasks;
using knkwebapi_v2.Models;
using knkwebapi_v2.Repositories;
using Microsoft.Extensions.Logging;

namespace knkwebapi_v2.Services
{
    /// <summary>
    /// Service for managing WorldGuard region operations via Minecraft plugin.
    /// Handles region renaming after entity creation/updates.
    /// </summary>
    public interface IRegionService
    {
        /// <summary>
        /// Rename a region from oldRegionId to newRegionId via the Minecraft plugin.
        /// </summary>
        Task<bool> RenameRegionAsync(string oldRegionId, string newRegionId);

        /// <summary>
        /// Check whether a location (x,z) lies inside the specified region.
        /// </summary>
        Task<bool> IsLocationInsideRegionAsync(string regionId, double x, double z, bool allowBoundary = false);

        /// <summary>
        /// Check whether a child region is contained within a parent region.
        /// </summary>
        Task<bool> IsRegionContainedAsync(string parentRegionId, string childRegionId, bool requireFullContainment = true);
    }

    public class RegionService : IRegionService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<RegionService> _logger;
        private readonly string _minecraftPluginBaseUrl;

        public RegionService(IHttpClientFactory httpClientFactory, ILogger<RegionService> logger, string minecraftPluginBaseUrl)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
            _minecraftPluginBaseUrl = minecraftPluginBaseUrl;
        }

        public async Task<bool> IsLocationInsideRegionAsync(string regionId, double x, double z, bool allowBoundary = false)
        {
            if (string.IsNullOrWhiteSpace(regionId))
            {
                throw new ArgumentException("regionId is required.");
            }

            try
            {
                using var client = _httpClientFactory.CreateClient();
                var url = $"{_minecraftPluginBaseUrl.TrimEnd('/')}/api/regions/{Uri.EscapeDataString(regionId)}/contains?x={x}&z={z}&allowBoundary={allowBoundary.ToString().ToLowerInvariant()}";
                var response = await client.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Region containment check failed for {RegionId} (HTTP {StatusCode})", regionId, response.StatusCode);
                    return false;
                }

                var content = await response.Content.ReadAsStringAsync();
                if (bool.TryParse(content, out var result))
                {
                    return result;
                }

                _logger.LogWarning("Unexpected response when checking region containment for {RegionId}: {Content}", regionId, content);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if location is inside region {RegionId}", regionId);
                throw;
            }
        }

        public async Task<bool> IsRegionContainedAsync(string parentRegionId, string childRegionId, bool requireFullContainment = true)
        {
            if (string.IsNullOrWhiteSpace(parentRegionId) || string.IsNullOrWhiteSpace(childRegionId))
            {
                throw new ArgumentException("Both parentRegionId and childRegionId are required.");
            }

            try
            {
                using var client = _httpClientFactory.CreateClient();
                var url = $"{_minecraftPluginBaseUrl.TrimEnd('/')}/api/regions/{Uri.EscapeDataString(parentRegionId)}/contains-region/{Uri.EscapeDataString(childRegionId)}?requireFullContainment={requireFullContainment.ToString().ToLowerInvariant()}";
                var response = await client.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Region containment check failed for {ParentRegionId}->{ChildRegionId} (HTTP {StatusCode})", parentRegionId, childRegionId, response.StatusCode);
                    return false;
                }

                var content = await response.Content.ReadAsStringAsync();
                if (bool.TryParse(content, out var result))
                {
                    return result;
                }

                _logger.LogWarning("Unexpected response when checking region containment {ParentRegionId}->{ChildRegionId}: {Content}", parentRegionId, childRegionId, content);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking region containment {ParentRegionId}->{ChildRegionId}", parentRegionId, childRegionId);
                throw;
            }
        }

        public async Task<bool> RenameRegionAsync(string oldRegionId, string newRegionId)
        {
            if (string.IsNullOrWhiteSpace(oldRegionId) || string.IsNullOrWhiteSpace(newRegionId))
            {
                throw new ArgumentException("Both oldRegionId and newRegionId are required.");
            }

            if (oldRegionId.Equals(newRegionId, StringComparison.OrdinalIgnoreCase))
            {
                // Already the desired name
                _logger.LogInformation($"Region already has the desired name: {oldRegionId}");
                return true;
            }

            try
            {
                // Call the Minecraft plugin API to rename the region
                using (var client = _httpClientFactory.CreateClient())
                {
                    var url = $"{_minecraftPluginBaseUrl.TrimEnd('/')}/Regions/rename?oldRegionId={Uri.EscapeDataString(oldRegionId)}&newRegionId={Uri.EscapeDataString(newRegionId)}";
                    var response = await client.PostAsync(url, new StringContent(""));

                    if (response.IsSuccessStatusCode)
                    {
                        var content = await response.Content.ReadAsStringAsync();
                        bool result = content.Equals("true", StringComparison.OrdinalIgnoreCase);
                        _logger.LogInformation($"Region rename completed: {oldRegionId} -> {newRegionId}, result: {result}");
                        return result;
                    }
                    else
                    {
                        _logger.LogWarning($"Failed to rename region: HTTP {response.StatusCode}");
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error renaming region from {oldRegionId} to {newRegionId}: {ex.Message}");
                throw;
            }
        }
    }
}
