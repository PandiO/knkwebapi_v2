using System;
using System.Security.Claims;
using AutoMapper;
using knkwebapi_v2.Dtos;
using knkwebapi_v2.Models;
using knkwebapi_v2.Repositories;
using knkwebapi_v2.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace knkwebapi_v2.Tests.Services
{
    /// <summary>
    /// Unit tests for AuthService covering login and refresh flows.
    /// </summary>
    public class AuthServiceTests
    {
        private readonly Mock<IUserRepository> _userRepository;
        private readonly Mock<ITokenService> _tokenService;
        private readonly Mock<IPasswordService> _passwordService;
        private readonly Mock<IMapper> _mapper;
        private readonly Mock<ILogger<AuthService>> _logger;
        private readonly AuthService _authService;

        public AuthServiceTests()
        {
            _userRepository = new Mock<IUserRepository>();
            _tokenService = new Mock<ITokenService>();
            _passwordService = new Mock<IPasswordService>();
            _mapper = new Mock<IMapper>();
            _logger = new Mock<ILogger<AuthService>>();

            _authService = new AuthService(
                _userRepository.Object,
                _tokenService.Object,
                _passwordService.Object,
                _mapper.Object,
                _logger.Object);
        }

        [Fact]
        public async Task LoginAsync_WithValidCredentials_ReturnsTokensAndUser()
        {
            // Arrange
            var user = new User
            {
                Id = 1,
                Email = "test@example.com",
                Username = "tester",
                PasswordHash = "hash",
                IsActive = true,
                DeletedAt = null
            };

            var userDto = new UserDto
            {
                Id = 1,
                Username = "tester",
                Email = user.Email,
                Coins = 0,
                Gems = 0,
                ExperiencePoints = 0,
                EmailVerified = false,
                AccountCreatedVia = AccountCreationMethod.WebApp,
                CreatedAt = DateTime.UtcNow.ToString("O"),
                IsActive = true
            };

            _userRepository
                .Setup(r => r.GetByEmailAsync(user.Email!))
                .ReturnsAsync(user);

            _passwordService
                .Setup(p => p.VerifyPasswordAsync("password", user.PasswordHash!))
                .ReturnsAsync(true);

            _tokenService
                .Setup(t => t.GenerateAccessTokenAsync(user, true))
                .ReturnsAsync("access-token");

            _tokenService
                .Setup(t => t.GenerateRefreshTokenAsync(user, true))
                .ReturnsAsync("refresh-token");

            _tokenService
                .Setup(t => t.ExtractExpirationAsync("access-token"))
                .ReturnsAsync(DateTime.UtcNow.AddMinutes(30));

            _mapper
                .Setup(m => m.Map<UserDto>(user))
                .Returns(userDto);

            // Act
            var (ok, result, error) = await _authService.LoginAsync(user.Email!, "password", true);

            // Assert
            Assert.True(ok);
            Assert.Null(error);
            Assert.NotNull(result);
            Assert.Equal("access-token", result!.AccessToken);
            Assert.Equal("refresh-token", result.RefreshToken);
            Assert.Equal(userDto, result.User);
        }

        [Fact]
        public async Task LoginAsync_WithInvalidPassword_ReturnsError()
        {
            // Arrange
            var user = new User
            {
                Id = 2,
                Email = "test@example.com",
                Username = "tester",
                PasswordHash = "hash",
                IsActive = true
            };

            _userRepository
                .Setup(r => r.GetByEmailAsync(user.Email!))
                .ReturnsAsync(user);

            _passwordService
                .Setup(p => p.VerifyPasswordAsync("wrong", user.PasswordHash!))
                .ReturnsAsync(false);

            // Act
            var (ok, result, error) = await _authService.LoginAsync(user.Email!, "wrong", false);

            // Assert
            Assert.False(ok);
            Assert.Null(result);
            Assert.Equal("Invalid credentials.", error);
        }

        [Fact]
        public async Task LoginAsync_InactiveUser_ReturnsError()
        {
            // Arrange
            var user = new User
            {
                Id = 3,
                Email = "inactive@example.com",
                Username = "inactive",
                PasswordHash = "hash",
                IsActive = false,
                DeletedAt = DateTime.UtcNow
            };

            _userRepository
                .Setup(r => r.GetByEmailAsync(user.Email!))
                .ReturnsAsync(user);

            // Act
            var (ok, result, error) = await _authService.LoginAsync(user.Email!, "password", false);

            // Assert
            Assert.False(ok);
            Assert.Null(result);
            Assert.Equal("Account is inactive or deleted.", error);
        }

        [Fact]
        public async Task RefreshAsync_WithValidToken_RotatesTokens()
        {
            // Arrange
            const string refreshToken = "refresh-token";
            var user = new User
            {
                Id = 4,
                Email = "refresh@example.com",
                Username = "refresh-user",
                IsActive = true
            };

            var principal = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim("uid", user.Id.ToString()),
                new Claim("token_type", "refresh"),
                new Claim("remember_me", "true", ClaimValueTypes.Boolean)
            }));

            _tokenService
                .Setup(t => t.ValidateRefreshTokenAsync(refreshToken))
                .ReturnsAsync(principal);

            _tokenService
                .Setup(t => t.ExtractUserIdFromPrincipalAsync(principal))
                .ReturnsAsync(user.Id);

            _tokenService
                .Setup(t => t.ExtractExpirationAsync(refreshToken))
                .ReturnsAsync(DateTime.UtcNow.AddDays(15));

            _userRepository
                .Setup(r => r.GetByIdAsync(user.Id))
                .ReturnsAsync(user);

            _tokenService
                .Setup(t => t.GenerateAccessTokenAsync(user, true))
                .ReturnsAsync("new-access-token");

            _tokenService
                .Setup(t => t.GenerateRefreshTokenAsync(user, true))
                .ReturnsAsync("new-refresh-token");

            _tokenService
                .Setup(t => t.ExtractExpirationAsync("new-access-token"))
                .ReturnsAsync(DateTime.UtcNow.AddMinutes(25));

            // Act
            var (ok, result, error) = await _authService.RefreshAsync(refreshToken);

            // Assert
            Assert.True(ok);
            Assert.Null(error);
            Assert.NotNull(result);
            Assert.Equal("new-access-token", result!.AccessToken);
            Assert.Equal("new-refresh-token", result.RefreshToken);
        }

        [Fact]
        public async Task RefreshAsync_WithInvalidToken_ReturnsError()
        {
            // Arrange
            const string refreshToken = "expired-token";

            _tokenService
                .Setup(t => t.ValidateRefreshTokenAsync(refreshToken))
                .ReturnsAsync((ClaimsPrincipal?)null);

            // Act
            var (ok, result, error) = await _authService.RefreshAsync(refreshToken);

            // Assert
            Assert.False(ok);
            Assert.Null(result);
            Assert.Equal("Invalid or expired refresh token.", error);
        }
    }
}
