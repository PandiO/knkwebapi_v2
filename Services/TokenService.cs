using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using knkwebapi_v2.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace knkwebapi_v2.Services
{
    /// <summary>
    /// JWT token service for generating and validating access/refresh tokens.
    /// Uses HS256 (HMAC-SHA256) for signing.
    /// Configuration from appsettings: Security:Jwt:{Issuer, Audience, Secret, AccessTokenMinutes, RefreshTokenDays}
    /// </summary>
    public class TokenService : ITokenService
    {
        private readonly IConfiguration _config;
        private readonly string _issuer;
        private readonly string _audience;
        private readonly string _secret;
        private readonly int _accessTokenMinutes;
        private readonly int _refreshTokenDays;
        private readonly SymmetricSecurityKey _key;
        private readonly JwtSecurityTokenHandler _tokenHandler;

        public TokenService(IConfiguration config)
        {
            _config = config;
            
            // Load JWT settings from configuration
            var jwtSection = _config.GetSection("Security:Jwt");
            _issuer = jwtSection["Issuer"] ?? "knk-api";
            _audience = jwtSection["Audience"] ?? "knk-app";
            _secret = jwtSection["Secret"] ?? throw new InvalidOperationException("Security:Jwt:Secret is required in appsettings.json");
            
            // Parse token lifetimes
            _accessTokenMinutes = int.TryParse(jwtSection["AccessTokenMinutes"], out var atm) ? atm : 30;
            _refreshTokenDays = int.TryParse(jwtSection["RefreshTokenDays"], out var rtd) ? rtd : 30;
            
            // Validate secret length (minimum 32 bytes recommended for HS256)
            if (_secret.Length < 32)
            {
                throw new InvalidOperationException("JWT secret must be at least 32 characters long for HS256.");
            }
            
            _key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secret));
            _tokenHandler = new JwtSecurityTokenHandler();
        }

        /// <inheritdoc/>
        public async Task<string> GenerateAccessTokenAsync(User user, bool rememberMe = false)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString(), ClaimValueTypes.String),
                new Claim(JwtRegisteredClaimNames.Email, user.Email ?? "", ClaimValueTypes.String),
                new Claim("uid", user.Id.ToString(), ClaimValueTypes.String),
                new Claim("username", user.Username, ClaimValueTypes.String),
            };

            var expiresAt = DateTime.UtcNow.AddMinutes(_accessTokenMinutes);
            
            var token = new JwtSecurityToken(
                issuer: _issuer,
                audience: _audience,
                claims: claims,
                expires: expiresAt,
                signingCredentials: new SigningCredentials(_key, SecurityAlgorithms.HmacSha256Signature)
            );

            var jwt = _tokenHandler.WriteToken(token);
            return await Task.FromResult(jwt);
        }

        /// <inheritdoc/>
        public async Task<string> GenerateRefreshTokenAsync(User user, bool rememberMe = false)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString(), ClaimValueTypes.String),
                new Claim(JwtRegisteredClaimNames.Email, user.Email ?? "", ClaimValueTypes.String),
                new Claim("uid", user.Id.ToString(), ClaimValueTypes.String),
                new Claim("token_type", "refresh", ClaimValueTypes.String),
                new Claim("remember_me", rememberMe ? "true" : "false", ClaimValueTypes.Boolean),
            };

            // Refresh token lifetime: rememberMe = 30 days, otherwise shorter (7 days)
            var expirationDays = rememberMe ? _refreshTokenDays : 7;
            var expiresAt = DateTime.UtcNow.AddDays(expirationDays);
            
            var token = new JwtSecurityToken(
                issuer: _issuer,
                audience: _audience,
                claims: claims,
                expires: expiresAt,
                signingCredentials: new SigningCredentials(_key, SecurityAlgorithms.HmacSha256Signature)
            );

            var jwt = _tokenHandler.WriteToken(token);
            return await Task.FromResult(jwt);
        }

        /// <inheritdoc/>
        public async Task<ClaimsPrincipal?> ValidateAccessTokenAsync(string token)
        {
            if (string.IsNullOrEmpty(token))
                return null;

            try
            {
                var principal = _tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = _key,
                    ValidateIssuer = true,
                    ValidIssuer = _issuer,
                    ValidateAudience = true,
                    ValidAudience = _audience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromSeconds(60),
                }, out SecurityToken validatedToken);

                return await Task.FromResult(principal);
            }
            catch
            {
                // Token validation failed (invalid signature, expired, malformed, etc.)
                return await Task.FromResult<ClaimsPrincipal?>(null);
            }
        }

        /// <inheritdoc/>
        public async Task<ClaimsPrincipal?> ValidateRefreshTokenAsync(string token)
        {
            if (string.IsNullOrEmpty(token))
                return null;

            try
            {
                var principal = _tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = _key,
                    ValidateIssuer = true,
                    ValidIssuer = _issuer,
                    ValidateAudience = true,
                    ValidAudience = _audience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromSeconds(60),
                }, out SecurityToken validatedToken);

                // Optionally check that this is actually a refresh token (has token_type = "refresh" claim)
                var tokenTypeClaim = principal.FindFirst("token_type");
                if (tokenTypeClaim?.Value != "refresh")
                {
                    return await Task.FromResult<ClaimsPrincipal?>(null);
                }

                return await Task.FromResult(principal);
            }
            catch
            {
                // Token validation failed
                return await Task.FromResult<ClaimsPrincipal?>(null);
            }
        }

        /// <inheritdoc/>
        public async Task<ClaimsPrincipal?> ReadTokenClaimsAsync(string token)
        {
            if (string.IsNullOrEmpty(token))
                return null;

            try
            {
                // Read without validating signature or expiration (unsafe; for info purposes only)
                var principal = _tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = false,
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = false,
                }, out SecurityToken validatedToken);

                return await Task.FromResult(principal);
            }
            catch
            {
                return await Task.FromResult<ClaimsPrincipal?>(null);
            }
        }

        /// <inheritdoc/>
        public async Task<int?> ExtractUserIdFromPrincipalAsync(ClaimsPrincipal principal)
        {
            if (principal == null)
                return null;

            var uidClaim = principal.FindFirst("uid") ?? principal.FindFirst(JwtRegisteredClaimNames.Sub);
            
            if (uidClaim != null && int.TryParse(uidClaim.Value, out var userId))
            {
                return await Task.FromResult<int?>(userId);
            }

            return await Task.FromResult<int?>(null);
        }

        /// <inheritdoc/>
        public async Task<DateTime?> ExtractExpirationAsync(string token)
        {
            if (string.IsNullOrEmpty(token))
                return null;

            try
            {
                var principal = await ReadTokenClaimsAsync(token);
                if (principal == null)
                    return null;

                var expClaim = principal.FindFirst("exp");
                if (expClaim != null && long.TryParse(expClaim.Value, out var expUnix))
                {
                    // Convert Unix timestamp to DateTime
                    var expiresAt = UnixTimeStampToDateTime(expUnix);
                    return await Task.FromResult<DateTime?>(expiresAt);
                }

                return null;
            }
            catch
            {
                return null;
            }
        }

        /// <inheritdoc/>
        public async Task<bool> IsTokenExpiredAsync(string token)
        {
            var expiresAt = await ExtractExpirationAsync(token);
            if (!expiresAt.HasValue)
                return true; // If we can't extract expiration, treat as expired

            return DateTime.UtcNow >= expiresAt.Value;
        }

        /// <summary>
        /// Convert Unix timestamp (seconds since epoch) to DateTime.
        /// </summary>
        private static DateTime UnixTimeStampToDateTime(long unixTimeStamp)
        {
            var dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dateTime = dateTime.AddSeconds(unixTimeStamp).ToUniversalTime();
            return dateTime;
        }
    }
}
