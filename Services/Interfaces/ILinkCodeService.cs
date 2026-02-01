using knkwebapi_v2.Dtos;
using knkwebapi_v2.Models;

namespace knkwebapi_v2.Services
{
    /// <summary>
    /// Service for generating and managing link codes for account linking.
    /// </summary>
    public interface ILinkCodeService
    {
        /// <summary>
        /// Generates a cryptographically secure 8-character alphanumeric code.
        /// Format: ABC12XYZ
        /// </summary>
        /// <returns>8-character alphanumeric code</returns>
        Task<string> GenerateCodeAsync();

        /// <summary>
        /// Generates and persists a new link code for a user.
        /// </summary>
        /// <param name="userId">Optional user ID to associate with code (null for web-first flow)</param>
        /// <returns>Link code response with code and expiration</returns>
        Task<LinkCodeResponseDto> GenerateLinkCodeAsync(int? userId);

        /// <summary>
        /// Validates a link code without consuming it.
        /// </summary>
        /// <param name="code">Link code to validate</param>
        /// <returns>Validation result with link code data if valid</returns>
        Task<(bool IsValid, LinkCode? LinkCode, string? Error)> ValidateLinkCodeAsync(string code);

        /// <summary>
        /// Validates and consumes a link code, marking it as used.
        /// </summary>
        /// <param name="code">Link code to consume</param>
        /// <returns>Consumption result with link code data if successful</returns>
        Task<(bool Success, LinkCode? LinkCode, string? Error)> ConsumeLinkCodeAsync(string code);

        /// <summary>
        /// Gets all expired link codes for cleanup.
        /// </summary>
        /// <returns>Collection of expired link codes</returns>
        Task<IEnumerable<LinkCode>> GetExpiredCodesAsync();

        /// <summary>
        /// Deletes expired link codes and returns count deleted.
        /// </summary>
        /// <returns>Number of codes deleted</returns>
        Task<int> CleanupExpiredCodesAsync();
    }
}
