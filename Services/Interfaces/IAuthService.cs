using System.Threading.Tasks;
using knkwebapi_v2.Dtos;

namespace knkwebapi_v2.Services
{
    /// <summary>
    /// Authentication service contract for login, refresh, logout, and current-user retrieval.
    /// </summary>
    public interface IAuthService
    {
        /// <summary>
        /// Authenticate a user with email and password, issuing access and refresh tokens.
        /// </summary>
        /// <param name="email">Email address (case-insensitive)</param>
        /// <param name="password">Plain text password</param>
        /// <param name="rememberMe">Extended session flag</param>
        /// <returns>Tuple with success flag, response DTO, and optional error message</returns>
        Task<(bool Ok, AuthLoginResponseDto? Result, string? Error)> LoginAsync(string email, string password, bool rememberMe);

        /// <summary>
        /// Refresh an access token using a refresh token, rotating the refresh token.
        /// </summary>
        /// <param name="refreshToken">Refresh token (JWT)</param>
        /// <returns>Tuple with success flag, response DTO, and optional error message</returns>
        Task<(bool Ok, AuthRefreshResponseDto? Result, string? Error)> RefreshAsync(string refreshToken);

        /// <summary>
        /// Logout the current session by revoking refresh token (stateless placeholder).
        /// </summary>
        /// <param name="refreshToken">Refresh token to revoke (optional if stateless)</param>
        Task LogoutAsync(string? refreshToken);

        /// <summary>
        /// Get the current authenticated user by ID.
        /// </summary>
        /// <param name="userId">Authenticated user ID</param>
        /// <returns>UserDto or null if not found/inactive</returns>
        Task<UserDto?> GetCurrentUserAsync(int userId);

        /// <summary>
        /// Update user account (email and/or password).
        /// </summary>
        /// <param name="userId">Authenticated user ID</param>
        /// <param name="request">Update request DTO</param>
        /// <returns>Tuple with success flag, updated user DTO, and optional error message</returns>
        Task<(bool Ok, UserDto? Result, string? Error)> UpdateUserAsync(int userId, AuthUpdateRequestDto request);
    }
}
