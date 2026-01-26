using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using knkwebapi_v2.Dtos;
using knkwebapi_v2.Services;

namespace knkwebapi_v2.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private const string RefreshTokenCookieName = "refreshToken";
        private readonly IAuthService _authService;
        private readonly ITokenService _tokenService;
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<AuthController> _logger;

        public AuthController(
            IAuthService authService,
            ITokenService tokenService,
            IWebHostEnvironment environment,
            ILogger<AuthController> logger)
        {
            _authService = authService;
            _tokenService = tokenService;
            _environment = environment;
            _logger = logger;
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] AuthLoginRequestDto request)
        {
            if (request == null)
            {
                return BadRequest(new { error = "InvalidRequest", message = "Login payload is required." });
            }

            var (ok, result, error) = await _authService.LoginAsync(request.Email, request.Password, request.RememberMe);
            if (!ok || result == null)
            {
                _logger.LogWarning("Login failed for {Email}: {Reason}", request.Email, error ?? "Invalid credentials");
                return Unauthorized(new { error = "InvalidCredentials", message = error ?? "Invalid credentials." });
            }

            await SetRefreshTokenCookieAsync(result.RefreshToken);
            _logger.LogInformation("Login succeeded for {Email}", request.Email);

            return Ok(result);
        }

        [HttpPost("refresh")]
        [AllowAnonymous]
        public async Task<IActionResult> Refresh([FromBody] AuthRefreshRequestDto? request)
        {
            var refreshToken = request?.RefreshToken;
            if (string.IsNullOrWhiteSpace(refreshToken))
            {
                Request.Cookies.TryGetValue(RefreshTokenCookieName, out refreshToken);
            }

            if (string.IsNullOrWhiteSpace(refreshToken))
            {
                return Unauthorized(new { error = "RefreshTokenRequired", message = "Refresh token is required." });
            }

            var (ok, result, error) = await _authService.RefreshAsync(refreshToken);
            if (!ok || result == null)
            {
                _logger.LogWarning("Refresh failed: {Reason}", error ?? "Invalid or expired refresh token");
                return Unauthorized(new { error = "InvalidRefreshToken", message = error ?? "Invalid or expired refresh token." });
            }

            await SetRefreshTokenCookieAsync(result.RefreshToken);
            _logger.LogInformation("Refresh succeeded for token");

            return Ok(result);
        }

        [HttpPost("logout")]
        [AllowAnonymous]
        public async Task<IActionResult> Logout([FromBody] AuthRefreshRequestDto? request)
        {
            var refreshToken = request?.RefreshToken;
            if (string.IsNullOrWhiteSpace(refreshToken))
            {
                Request.Cookies.TryGetValue(RefreshTokenCookieName, out refreshToken);
            }

            await _authService.LogoutAsync(refreshToken);
            ClearRefreshTokenCookie();
            _logger.LogInformation("Logout completed (token provided: {HasToken})", !string.IsNullOrWhiteSpace(refreshToken));

            return NoContent();
        }

        [HttpGet("me")]
        [Authorize]
        public async Task<IActionResult> Me()
        {
            var userId = GetUserIdFromClaims(User);
            if (!userId.HasValue)
            {
                return Unauthorized(new { error = "InvalidToken", message = "User claim missing." });
            }

            var user = await _authService.GetCurrentUserAsync(userId.Value);
            if (user == null)
            {
                return Unauthorized(new { error = "UserNotFound", message = "User not found or inactive." });
            }

            return Ok(user);
        }

        [HttpPost("validate-token")]
        [AllowAnonymous]
        public async Task<IActionResult> ValidateToken([FromBody] AuthValidateTokenRequestDto request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Token))
            {
                return BadRequest(new { error = "InvalidRequest", message = "Token is required." });
            }

            var principal = await _tokenService.ValidateAccessTokenAsync(request.Token);
            var expiresAt = await _tokenService.ExtractExpirationAsync(request.Token);

            return Ok(new AuthValidateTokenResponseDto
            {
                Valid = principal != null,
                ExpiresAt = principal != null ? expiresAt : null
            });
        }

        private async Task SetRefreshTokenCookieAsync(string? refreshToken)
        {
            if (string.IsNullOrWhiteSpace(refreshToken))
            {
                return;
            }

            var options = BuildRefreshCookieOptions();
            var expiresAt = await _tokenService.ExtractExpirationAsync(refreshToken);
            if (expiresAt.HasValue)
            {
                options.Expires = expiresAt.Value;
            }

            Response.Cookies.Append(RefreshTokenCookieName, refreshToken, options);
        }

        private void ClearRefreshTokenCookie()
        {
            var options = BuildRefreshCookieOptions();
            options.Expires = DateTimeOffset.UtcNow.AddDays(-1);
            Response.Cookies.Delete(RefreshTokenCookieName, options);
        }

        private CookieOptions BuildRefreshCookieOptions()
        {
            return new CookieOptions
            {
                HttpOnly = true,
                Secure = !_environment.IsDevelopment(),
                SameSite = _environment.IsDevelopment() ? SameSiteMode.Lax : SameSiteMode.None,
                Path = "/"
            };
        }

        private int? GetUserIdFromClaims(ClaimsPrincipal principal)
        {
            var userIdClaim = principal.FindFirst("uid")
                ?? principal.FindFirst(JwtRegisteredClaimNames.Sub)
                ?? principal.FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim == null)
            {
                return null;
            }

            return int.TryParse(userIdClaim.Value, out var userId) ? userId : (int?)null;
        }
    }
}
