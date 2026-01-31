using System;
using System.Threading.Tasks;
using AutoMapper;
using knkwebapi_v2.Dtos;
using knkwebapi_v2.Models;
using knkwebapi_v2.Repositories;
using Microsoft.Extensions.Logging;

namespace knkwebapi_v2.Services
{
    /// <summary>
    /// Service for authentication workflows: login, refresh, logout, and current-user retrieval.
    /// </summary>
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly ITokenService _tokenService;
        private readonly IPasswordService _passwordService;
        private readonly IMapper _mapper;
        private readonly ILogger<AuthService> _logger;

        public AuthService(
            IUserRepository userRepository,
            ITokenService tokenService,
            IPasswordService passwordService,
            IMapper mapper,
            ILogger<AuthService> logger)
        {
            _userRepository = userRepository;
            _tokenService = tokenService;
            _passwordService = passwordService;
            _mapper = mapper;
            _logger = logger;
        }

        /// <inheritdoc/>
        public async Task<(bool Ok, AuthLoginResponseDto? Result, string? Error)> LoginAsync(string email, string password, bool rememberMe)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                return (false, null, "Email and password are required.");
            }

            var normalizedEmail = email.Trim().ToLowerInvariant();
            var user = await _userRepository.GetByEmailAsync(normalizedEmail);

            if (user == null || string.IsNullOrWhiteSpace(user.PasswordHash))
            {
                _logger.LogWarning("Login failed for {Email}: user not found or missing password hash", normalizedEmail);
                return (false, null, "Invalid credentials.");
            }

            if (!user.IsActive || user.DeletedAt.HasValue)
            {
                _logger.LogWarning("Login blocked for {Email}: inactive or deleted", normalizedEmail);
                return (false, null, "Account is inactive or deleted.");
            }

            var passwordValid = await _passwordService.VerifyPasswordAsync(password, user.PasswordHash);
            if (!passwordValid)
            {
                _logger.LogWarning("Login failed for {Email}: invalid password", normalizedEmail);
                return (false, null, "Invalid credentials.");
            }

            var accessToken = await _tokenService.GenerateAccessTokenAsync(user, rememberMe);
            var refreshToken = await _tokenService.GenerateRefreshTokenAsync(user, rememberMe);
            var expiresIn = await CalculateExpiresInSecondsAsync(accessToken);

            var response = new AuthLoginResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiresIn = expiresIn,
                User = _mapper.Map<UserDto>(user)
            };

            _logger.LogInformation("Login succeeded for user {UserId} ({Email})", user.Id, normalizedEmail);
            return (true, response, null);
        }

        /// <inheritdoc/>
        public async Task<(bool Ok, AuthRefreshResponseDto? Result, string? Error)> RefreshAsync(string refreshToken)
        {
            if (string.IsNullOrWhiteSpace(refreshToken))
            {
                return (false, null, "Refresh token is required.");
            }

            var principal = await _tokenService.ValidateRefreshTokenAsync(refreshToken);
            if (principal == null)
            {
                _logger.LogWarning("Refresh failed: token invalid or expired");
                return (false, null, "Invalid or expired refresh token.");
            }

            var userId = await _tokenService.ExtractUserIdFromPrincipalAsync(principal);
            if (!userId.HasValue)
            {
                _logger.LogWarning("Refresh failed: missing user id in token");
                return (false, null, "Invalid refresh token payload.");
            }

            var user = await _userRepository.GetByIdAsync(userId.Value);
            if (user == null || !user.IsActive || user.DeletedAt.HasValue)
            {
                _logger.LogWarning("Refresh failed for user {UserId}: not found or inactive", userId);
                return (false, null, "User not found or inactive.");
            }

            var rememberMe = await IsLongLivedRefreshTokenAsync(refreshToken);

            // TODO: Persist and revoke refresh tokens when refresh token repository is available.
            var newAccessToken = await _tokenService.GenerateAccessTokenAsync(user, rememberMe);
            var newRefreshToken = await _tokenService.GenerateRefreshTokenAsync(user, rememberMe);
            var expiresIn = await CalculateExpiresInSecondsAsync(newAccessToken);

            var response = new AuthRefreshResponseDto
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken,
                ExpiresIn = expiresIn
            };

            _logger.LogInformation("Refresh succeeded for user {UserId}", user.Id);
            return (true, response, null);
        }

        /// <inheritdoc/>
        public Task LogoutAsync(string? refreshToken)
        {
            // TODO: Add refresh token persistence + revoke when repository is implemented.
            _logger.LogInformation("Logout requested (token provided: {HasToken})", !string.IsNullOrWhiteSpace(refreshToken));
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public async Task<UserDto?> GetCurrentUserAsync(int userId)
        {
            if (userId <= 0)
            {
                return null;
            }

            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null || !user.IsActive || user.DeletedAt.HasValue)
            {
                return null;
            }

            return _mapper.Map<UserDto>(user);
        }

        /// <inheritdoc/>
        public async Task<(bool Ok, UserDto? Result, string? Error)> UpdateUserAsync(int userId, AuthUpdateRequestDto request)
        {
            if (userId <= 0)
            {
                return (false, null, "Invalid user ID.");
            }

            if (request == null)
            {
                return (false, null, "Update request is required.");
            }

            // Validate that at least one field is being updated
            var hasEmailUpdate = !string.IsNullOrWhiteSpace(request.Email);
            var hasPasswordUpdate = !string.IsNullOrWhiteSpace(request.NewPassword);

            if (!hasEmailUpdate && !hasPasswordUpdate)
            {
                return (false, null, "At least one field (email or password) must be provided for update.");
            }

            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null || !user.IsActive || user.DeletedAt.HasValue)
            {
                _logger.LogWarning("Update failed for user {UserId}: not found or inactive", userId);
                return (false, null, "User not found or inactive.");
            }

            // Handle password update
            if (hasPasswordUpdate)
            {
                if (string.IsNullOrWhiteSpace(request.CurrentPassword))
                {
                    return (false, null, "Current password is required to change password.");
                }

                if (string.IsNullOrWhiteSpace(user.PasswordHash))
                {
                    return (false, null, "Cannot update password for account without existing password.");
                }

                // Verify current password
                var passwordValid = await _passwordService.VerifyPasswordAsync(request.CurrentPassword, user.PasswordHash);
                if (!passwordValid)
                {
                    _logger.LogWarning("Password update failed for user {UserId}: incorrect current password", userId);
                    return (false, null, "Current password is incorrect.");
                }

                // Validate new password
                if (request.NewPassword.Length < 8)
                {
                    return (false, null, "New password must be at least 8 characters long.");
                }

                // Hash and update password
                var newPasswordHash = await _passwordService.HashPasswordAsync(request.NewPassword);
                user.PasswordHash = newPasswordHash;

                _logger.LogInformation("Password updated for user {UserId}", userId);
            }

            // Handle email update
            if (hasEmailUpdate)
            {
                var normalizedEmail = request.Email!.Trim().ToLowerInvariant();

                // Check if email is already in use by another user
                var existingUser = await _userRepository.GetByEmailAsync(normalizedEmail);
                if (existingUser != null && existingUser.Id != userId)
                {
                    return (false, null, "Email is already in use by another account.");
                }

                user.Email = normalizedEmail;
                user.EmailVerified = false; // Reset verification status when email changes

                _logger.LogInformation("Email updated for user {UserId}", userId);
            }

            // Save changes
            await _userRepository.UpdateUserAsync(user);

            var updatedUserDto = _mapper.Map<UserDto>(user);
            return (true, updatedUserDto, null);
        }

        private async Task<int> CalculateExpiresInSecondsAsync(string accessToken)
        {
            var expiresAt = await _tokenService.ExtractExpirationAsync(accessToken);
            if (!expiresAt.HasValue)
            {
                return 0;
            }

            var remaining = expiresAt.Value - DateTime.UtcNow;
            return remaining <= TimeSpan.Zero ? 0 : (int)Math.Round(remaining.TotalSeconds);
        }

        private async Task<bool> IsLongLivedRefreshTokenAsync(string refreshToken)
        {
            var expiresAt = await _tokenService.ExtractExpirationAsync(refreshToken);
            if (!expiresAt.HasValue)
            {
                return false;
            }

            var remaining = expiresAt.Value - DateTime.UtcNow;
            return remaining > TimeSpan.FromDays(10);
        }
    }
}
