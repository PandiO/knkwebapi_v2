using System;
using System.Security.Claims;
using System.Threading.Tasks;
using knkwebapi_v2.Models;

namespace knkwebapi_v2.Services
{
    /// <summary>
    /// Service for JWT token generation, validation, and management.
    /// Handles both access tokens (short-lived) and refresh tokens (long-lived).
    /// Implements HMAC-SHA256 signing with configurable expiration.
    /// </summary>
    public interface ITokenService
    {
        /// <summary>
        /// Generate a new access token for a user.
        /// Access token is short-lived (default: 30 minutes).
        /// Contains user ID and email claims.
        /// </summary>
        /// <param name="user">User entity to generate token for</param>
        /// <param name="rememberMe">Extended expiration flag (for refresh token flow; affects access token duration if used here)</param>
        /// <returns>Signed JWT access token string</returns>
        Task<string> GenerateAccessTokenAsync(User user, bool rememberMe = false);

        /// <summary>
        /// Generate a new refresh token for a user.
        /// Refresh token is long-lived (default: 30 days for rememberMe, otherwise shorter).
        /// Used by clients to obtain new access tokens without re-entering credentials.
        /// </summary>
        /// <param name="user">User entity to generate token for</param>
        /// <param name="rememberMe">If true, extend refresh token lifetime (30 days). If false, shorter lifetime.</param>
        /// <returns>Signed JWT refresh token string</returns>
        Task<string> GenerateRefreshTokenAsync(User user, bool rememberMe = false);

        /// <summary>
        /// Validate an access token and extract claims if valid.
        /// Checks signature, expiration, issuer, and audience.
        /// </summary>
        /// <param name="token">JWT access token to validate</param>
        /// <returns>ClaimsPrincipal if valid; null if invalid or expired</returns>
        Task<ClaimsPrincipal?> ValidateAccessTokenAsync(string token);

        /// <summary>
        /// Validate a refresh token and extract claims if valid.
        /// Checks signature, expiration, issuer, and audience.
        /// </summary>
        /// <param name="token">JWT refresh token to validate</param>
        /// <returns>ClaimsPrincipal if valid; null if invalid or expired</returns>
        Task<ClaimsPrincipal?> ValidateRefreshTokenAsync(string token);

        /// <summary>
        /// Extract claims from a token without validation (unsafe; use only for informational purposes).
        /// Useful for reading claims before full validation or for debugging.
        /// </summary>
        /// <param name="token">JWT token to read claims from</param>
        /// <returns>ClaimsPrincipal with extracted claims, or null if malformed</returns>
        Task<ClaimsPrincipal?> ReadTokenClaimsAsync(string token);

        /// <summary>
        /// Extract the user ID from an access token's claims.
        /// Assumes token has been validated before calling.
        /// </summary>
        /// <param name="principal">ClaimsPrincipal from validated token</param>
        /// <returns>User ID if found; null otherwise</returns>
        Task<int?> ExtractUserIdFromPrincipalAsync(ClaimsPrincipal principal);

        /// <summary>
        /// Extract the expiration time from a token.
        /// </summary>
        /// <param name="token">JWT token</param>
        /// <returns>Expiration time if token contains 'exp' claim; null otherwise</returns>
        Task<DateTime?> ExtractExpirationAsync(string token);

        /// <summary>
        /// Check if a token is expired based on its claims.
        /// </summary>
        /// <param name="token">JWT token to check</param>
        /// <returns>True if token is expired; false if still valid or invalid token</returns>
        Task<bool> IsTokenExpiredAsync(string token);
    }
}
