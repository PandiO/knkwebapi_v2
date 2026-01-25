using System;
using System.Text.Json.Serialization;

namespace knkwebapi_v2.Dtos
{
    /// <summary>
    /// DTO for login request.
    /// </summary>
    public class AuthLoginRequestDto
    {
        [JsonPropertyName("email")]
        public string Email { get; set; } = null!;

        [JsonPropertyName("password")]
        public string Password { get; set; } = null!;

        [JsonPropertyName("rememberMe")]
        public bool RememberMe { get; set; } = false;
    }

    /// <summary>
    /// DTO for login response.
    /// Contains access token, optional refresh token, expiration info, and user data.
    /// CRITICAL: Never includes password hash.
    /// </summary>
    public class AuthLoginResponseDto
    {
        [JsonPropertyName("accessToken")]
        public string AccessToken { get; set; } = null!;

        [JsonPropertyName("refreshToken")]
        public string? RefreshToken { get; set; }

        [JsonPropertyName("expiresIn")]
        public int ExpiresIn { get; set; }

        [JsonPropertyName("user")]
        public UserDto User { get; set; } = null!;
    }

    /// <summary>
    /// DTO for token refresh request.
    /// RefreshToken can come from body or httpOnly cookie.
    /// </summary>
    public class AuthRefreshRequestDto
    {
        [JsonPropertyName("refreshToken")]
        public string? RefreshToken { get; set; }
    }

    /// <summary>
    /// DTO for token refresh response.
    /// Contains new access token, optional rotated refresh token, and expiration.
    /// </summary>
    public class AuthRefreshResponseDto
    {
        [JsonPropertyName("accessToken")]
        public string AccessToken { get; set; } = null!;

        [JsonPropertyName("refreshToken")]
        public string? RefreshToken { get; set; }

        [JsonPropertyName("expiresIn")]
        public int ExpiresIn { get; set; }
    }

    /// <summary>
    /// DTO for token validation request (optional endpoint).
    /// </summary>
    public class AuthValidateTokenRequestDto
    {
        [JsonPropertyName("token")]
        public string Token { get; set; } = null!;
    }

    /// <summary>
    /// DTO for token validation response (optional endpoint).
    /// </summary>
    public class AuthValidateTokenResponseDto
    {
        [JsonPropertyName("valid")]
        public bool Valid { get; set; }

        [JsonPropertyName("expiresAt")]
        public DateTime? ExpiresAt { get; set; }
    }
}
