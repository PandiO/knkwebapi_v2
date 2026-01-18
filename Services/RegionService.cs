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
