using System.Collections.Generic;
using System.Threading.Tasks;
using knkwebapi_v2.Dtos;

namespace knkwebapi_v2.Services.Interfaces
{
    /// <summary>
    /// Service for resolving placeholder variables in validation messages.
    /// Supports multi-layer navigation: direct properties, single navigation, multi-navigation, and aggregates.
    /// </summary>
    public interface IPlaceholderResolutionService
    {
        /// <summary>
        /// Resolve placeholder variables for validation messages.
        /// </summary>
        /// <param name="request">Request containing entity context and placeholder paths to resolve</param>
        /// <returns>Response with resolved placeholders, unresolved paths, and any errors</returns>
        Task<ResolvePlaceholdersResponseDto> ResolvePlaceholdersAsync(ResolvePlaceholdersRequestDto request);
    }
}
