using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using knkwebapi_v2.Controllers;
using knkwebapi_v2.Services.Interfaces;
using knkwebapi_v2.Models;
using knkwebapi_v2.Dtos;
using AutoMapper;

namespace knkwebapi_v2.Tests.Api;

/// <summary>
/// API endpoint tests for UsersController.
/// Tests HTTP response codes, error handling, and request/response contracts.
/// </summary>
public class UsersControllerTests
{
    private readonly Mock<IUserService> _mockUserService;
    private readonly Mock<IMapper> _mockMapper;
    private readonly UsersController _controller;

    public UsersControllerTests()
    {
        _mockUserService = new Mock<IUserService>();
        _mockMapper = new Mock<IMapper>();
        _controller = new UsersController(_mockUserService.Object, _mockMapper.Object);
    }

    #region CreateUserAsync Tests

    [Fact]
    public async Task CreateAsync_WithValidWebAppSignup_Returns201Created()
    {
        // Arrange
        var createDto = new UserCreateDto
        {
            Username = "newplayer",
            Email = "new@example.com",
            Password = "SecurePass123!",
            PasswordConfirmation = "SecurePass123!"
        };

        var createdUser = new UserDto
        {
            Id = 1,
            Username = "newplayer",
            Email = "new@example.com",
            Coins = 250,
            Gems = 50,
            ExperiencePoints = 0
        };

        var linkCodeDto = new LinkCodeResponseDto
        {
            Code = "ABC12XYZ",
            ExpiresAt = DateTime.UtcNow.AddMinutes(20)
        };

        _mockUserService
            .Setup(s => s.ValidateUserCreationAsync(createDto))
            .ReturnsAsync((true, null));

        _mockUserService
            .Setup(s => s.CheckUsernameTakenAsync("newplayer", null))
            .ReturnsAsync((false, null));

        _mockUserService
            .Setup(s => s.CheckEmailTakenAsync("new@example.com", null))
            .ReturnsAsync((false, null));

        _mockUserService
            .Setup(s => s.CreateAsync(createDto))
            .ReturnsAsync(createdUser);

        _mockUserService
            .Setup(s => s.GenerateLinkCodeAsync(1))
            .ReturnsAsync(linkCodeDto);

        // Act
        var result = await _controller.CreateAsync(createDto);

        // Assert
        var createdResult = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(201, createdResult.StatusCode);
        Assert.Equal(nameof(UsersController.GetByIdAsync), createdResult.ActionName);
    }

    [Fact]
    public async Task CreateAsync_WithDuplicateUsername_Returns409Conflict()
    {
        // Arrange
        var createDto = new UserCreateDto
        {
            Username = "existingplayer",
            Email = "new@example.com",
            Password = "SecurePass123!",
            PasswordConfirmation = "SecurePass123!"
        };

        _mockUserService
            .Setup(s => s.ValidateUserCreationAsync(createDto))
            .ReturnsAsync((true, null));

        _mockUserService
            .Setup(s => s.CheckUsernameTakenAsync("existingplayer", null))
            .ReturnsAsync((true, 5));

        // Act
        var result = await _controller.CreateAsync(createDto);

        // Assert
        var conflictResult = Assert.IsType<ConflictObjectResult>(result);
        Assert.Equal(409, conflictResult.StatusCode);
    }

    [Fact]
    public async Task CreateAsync_WithDuplicateEmail_Returns409Conflict()
    {
        // Arrange
        var createDto = new UserCreateDto
        {
            Username = "newplayer",
            Email = "existing@example.com",
            Password = "SecurePass123!",
            PasswordConfirmation = "SecurePass123!"
        };

        _mockUserService
            .Setup(s => s.ValidateUserCreationAsync(createDto))
            .ReturnsAsync((true, null));

        _mockUserService
            .Setup(s => s.CheckUsernameTakenAsync("newplayer", null))
            .ReturnsAsync((false, null));

        _mockUserService
            .Setup(s => s.CheckEmailTakenAsync("existing@example.com", null))
            .ReturnsAsync((true, 7));

        // Act
        var result = await _controller.CreateAsync(createDto);

        // Assert
        var conflictResult = Assert.IsType<ConflictObjectResult>(result);
        Assert.Equal(409, conflictResult.StatusCode);
    }

    [Fact]
    public async Task CreateAsync_WithValidationFailure_Returns400BadRequest()
    {
        // Arrange
        var createDto = new UserCreateDto
        {
            Username = string.Empty,
            Email = "test@example.com",
            Password = "weak",
            PasswordConfirmation = "weak"
        };

        _mockUserService
            .Setup(s => s.ValidateUserCreationAsync(createDto))
            .ReturnsAsync((false, "Username is required"));

        // Act
        var result = await _controller.CreateAsync(createDto);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(400, badRequestResult.StatusCode);
    }

    [Fact]
    public async Task CreateAsync_WithMinecraftOnlySignup_Returns201Created()
    {
        // Arrange
        var createDto = new UserCreateDto
        {
            Username = "mcplayer",
            Uuid = "550e8400-e29b-41d4-a716-446655440000"
        };

        var createdUser = new UserDto
        {
            Id = 2,
            Username = "mcplayer",
            Uuid = "550e8400-e29b-41d4-a716-446655440000",
            Coins = 0,
            Gems = 0,
            ExperiencePoints = 0
        };

        _mockUserService
            .Setup(s => s.ValidateUserCreationAsync(createDto))
            .ReturnsAsync((true, null));

        _mockUserService
            .Setup(s => s.CheckUsernameTakenAsync("mcplayer", null))
            .ReturnsAsync((false, null));

        _mockUserService
            .Setup(s => s.CreateAsync(createDto))
            .ReturnsAsync(createdUser);

        // Act
        var result = await _controller.CreateAsync(createDto);

        // Assert
        var createdResult = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(201, createdResult.StatusCode);
    }

    #endregion

    #region GenerateLinkCodeAsync Tests

    [Fact]
    public async Task GenerateLinkCodeAsync_WithValidUserId_Returns200WithCode()
    {
        // Arrange
        var request = new LinkCodeRequestDto { UserId = 1 };
        var linkCodeDto = new LinkCodeResponseDto
        {
            Code = "ABC12XYZ",
            ExpiresAt = DateTime.UtcNow.AddMinutes(20)
        };

        _mockUserService
            .Setup(s => s.GenerateLinkCodeAsync(1))
            .ReturnsAsync(linkCodeDto);

        // Act
        var result = await _controller.GenerateLinkCodeAsync(request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(200, okResult.StatusCode);
        Assert.NotNull(okResult.Value);
    }

    [Fact]
    public async Task GenerateLinkCodeAsync_WithNullUserId_Returns200WithCode()
    {
        // Arrange - for web app first flow
        var request = new LinkCodeRequestDto { UserId = 0 };
        var linkCodeDto = new LinkCodeResponseDto
        {
            Code = "XYZ12ABC",
            ExpiresAt = DateTime.UtcNow.AddMinutes(20)
        };

        _mockUserService
            .Setup(s => s.GenerateLinkCodeAsync(null))
            .ReturnsAsync(linkCodeDto);

        // Act
        var result = await _controller.GenerateLinkCodeAsync(request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(200, okResult.StatusCode);
    }

    #endregion

    #region ValidateLinkCodeAsync Tests

    [Fact]
    public async Task ValidateLinkCodeAsync_WithValidCode_Returns200WithDetails()
    {
        // Arrange
        const string code = "ABC12XYZ";
        var userDto = new UserDto
        {
            Id = 1,
            Username = "player",
            Email = "player@example.com"
        };

        _mockUserService
            .Setup(s => s.ConsumeLinkCodeAsync(code))
            .ReturnsAsync((true, userDto));

        // Act
        var result = await _controller.ValidateLinkCodeAsync(code);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(200, okResult.StatusCode);
    }

    [Fact]
    public async Task ValidateLinkCodeAsync_WithExpiredCode_Returns400BadRequest()
    {
        // Arrange
        const string code = "EXPIRED99";

        _mockUserService
            .Setup(s => s.ConsumeLinkCodeAsync(code))
            .ReturnsAsync((false, null));

        // Act
        var result = await _controller.ValidateLinkCodeAsync(code);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(400, badRequestResult.StatusCode);
    }

    #endregion

    #region ChangePasswordAsync Tests

    [Fact]
    public async Task ChangePasswordAsync_WithCorrectCurrentPassword_Returns204NoContent()
    {
        // Arrange
        const int userId = 1;
        var changePasswordDto = new ChangePasswordDto
        {
            CurrentPassword = "OldPassword123!",
            NewPassword = "NewPassword456!",
            PasswordConfirmation = "NewPassword456!"
        };

        _mockUserService
            .Setup(s => s.ChangePasswordAsync(userId, "OldPassword123!", "NewPassword456!", "NewPassword456!"))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.ChangePasswordAsync(userId, changePasswordDto);

        // Assert
        var noContentResult = Assert.IsType<NoContentResult>(result);
        Assert.Equal(204, noContentResult.StatusCode);
    }

    [Fact]
    public async Task ChangePasswordAsync_WithIncorrectCurrentPassword_Returns401Unauthorized()
    {
        // Arrange
        const int userId = 1;
        var changePasswordDto = new ChangePasswordDto
        {
            CurrentPassword = "WrongPassword",
            NewPassword = "NewPassword456!",
            PasswordConfirmation = "NewPassword456!"
        };

        _mockUserService
            .Setup(s => s.ChangePasswordAsync(userId, "WrongPassword", "NewPassword456!", "NewPassword456!"))
            .ThrowsAsync(new InvalidOperationException("Current password is incorrect"));

        // Act
        var result = await _controller.ChangePasswordAsync(userId, changePasswordDto);

        // Assert
        var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
        Assert.Equal(401, unauthorizedResult.StatusCode);
    }

    [Fact]
    public async Task ChangePasswordAsync_WithPasswordMismatch_Returns400BadRequest()
    {
        // Arrange
        const int userId = 1;
        var changePasswordDto = new ChangePasswordDto
        {
            CurrentPassword = "OldPassword123!",
            NewPassword = "NewPassword456!",
            PasswordConfirmation = "DifferentPassword!"
        };

        _mockUserService
            .Setup(s => s.ChangePasswordAsync(userId, "OldPassword123!", "NewPassword456!", "DifferentPassword!"))
            .ThrowsAsync(new ArgumentException("Passwords do not match"));

        // Act
        var result = await _controller.ChangePasswordAsync(userId, changePasswordDto);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(400, badRequestResult.StatusCode);
    }

    [Fact]
    public async Task ChangePasswordAsync_WithNonExistentUser_Returns404NotFound()
    {
        // Arrange
        const int userId = 999;
        var changePasswordDto = new ChangePasswordDto
        {
            CurrentPassword = "OldPassword123!",
            NewPassword = "NewPassword456!",
            PasswordConfirmation = "NewPassword456!"
        };

        _mockUserService
            .Setup(s => s.ChangePasswordAsync(userId, "OldPassword123!", "NewPassword456!", "NewPassword456!"))
            .ThrowsAsync(new InvalidOperationException("User not found"));

        // Act
        var result = await _controller.ChangePasswordAsync(userId, changePasswordDto);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal(404, notFoundResult.StatusCode);
    }

    #endregion

    #region UpdateEmailAsync Tests

    [Fact]
    public async Task UpdateEmailAsync_WithValidEmail_Returns204NoContent()
    {
        // Arrange
        const int userId = 1;
        var updateEmailDto = new UpdateEmailDto
        {
            NewEmail = "newemail@example.com",
            CurrentPassword = "MyPassword123!"
        };

        _mockUserService
            .Setup(s => s.UpdateEmailAsync(userId, "newemail@example.com", "MyPassword123!"))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.UpdateEmailAsync(userId, updateEmailDto);

        // Assert
        var noContentResult = Assert.IsType<NoContentResult>(result);
        Assert.Equal(204, noContentResult.StatusCode);
    }

    [Fact]
    public async Task UpdateEmailAsync_WithDuplicateEmail_Returns409Conflict()
    {
        // Arrange
        const int userId = 1;
        var updateEmailDto = new UpdateEmailDto
        {
            NewEmail = "taken@example.com",
            CurrentPassword = "MyPassword123!"
        };

        _mockUserService
            .Setup(s => s.UpdateEmailAsync(userId, "taken@example.com", "MyPassword123!"))
            .ThrowsAsync(new InvalidOperationException("Email is already in use"));

        // Act
        var result = await _controller.UpdateEmailAsync(userId, updateEmailDto);

        // Assert
        var conflictResult = Assert.IsType<ConflictObjectResult>(result);
        Assert.Equal(409, conflictResult.StatusCode);
    }

    #endregion

    #region CheckDuplicateAsync Tests

    [Fact]
    public async Task CheckDuplicateAsync_WithNoDuplicate_Returns200False()
    {
        // Arrange
        var checkDto = new DuplicateCheckDto
        {
            Uuid = "550e8400-e29b-41d4-a716-446655440000",
            Username = "newplayer"
        };

        _mockUserService
            .Setup(s => s.CheckForDuplicateAsync("550e8400-e29b-41d4-a716-446655440000", "newplayer"))
            .ReturnsAsync((false, null));

        // Act
        var result = await _controller.CheckDuplicateAsync(checkDto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(200, okResult.StatusCode);
    }

    [Fact]
    public async Task CheckDuplicateAsync_WithDuplicate_Returns200WithDetails()
    {
        // Arrange
        var checkDto = new DuplicateCheckDto
        {
            Uuid = "550e8400-e29b-41d4-a716-446655440000",
            Username = "existingplayer"
        };

        _mockUserService
            .Setup(s => s.CheckForDuplicateAsync("550e8400-e29b-41d4-a716-446655440000", "existingplayer"))
            .ReturnsAsync((true, 42));

        // Act
        var result = await _controller.CheckDuplicateAsync(checkDto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(200, okResult.StatusCode);
    }

    #endregion

    #region MergeAsync Tests

    [Fact]
    public async Task MergeAsync_WithValidAccounts_Returns200WithMergedUser()
    {
        // Arrange
        var mergeDto = new AccountMergeDto
        {
            PrimaryUserId = 1,
            SecondaryUserId = 2
        };

        var mergedUser = new UserDto
        {
            Id = 1,
            Username = "primary",
            Coins = 500,
            Gems = 100,
            ExperiencePoints = 5000
        };

        _mockUserService
            .Setup(s => s.MergeAccountsAsync(1, 2))
            .ReturnsAsync(mergedUser);

        // Act
        var result = await _controller.MergeAsync(mergeDto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(200, okResult.StatusCode);
    }

    [Fact]
    public async Task MergeAsync_WithNonExistentPrimary_Returns404NotFound()
    {
        // Arrange
        var mergeDto = new AccountMergeDto
        {
            PrimaryUserId = 999,
            SecondaryUserId = 2
        };

        _mockUserService
            .Setup(s => s.MergeAccountsAsync(999, 2))
            .ThrowsAsync(new InvalidOperationException("Primary user not found"));

        // Act
        var result = await _controller.MergeAsync(mergeDto);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal(404, notFoundResult.StatusCode);
    }

    [Fact]
    public async Task MergeAsync_WithInvalidArguments_Returns400BadRequest()
    {
        // Arrange
        var mergeDto = new AccountMergeDto
        {
            PrimaryUserId = 1,
            SecondaryUserId = 1  // Same user
        };

        _mockUserService
            .Setup(s => s.MergeAccountsAsync(1, 1))
            .ThrowsAsync(new ArgumentException("Cannot merge account with itself"));

        // Act
        var result = await _controller.MergeAsync(mergeDto);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(400, badRequestResult.StatusCode);
    }

    #endregion

    #region LinkAccountAsync Tests

    [Fact]
    public async Task LinkAccountAsync_WithValidCode_Returns200LinkedUser()
    {
        // Arrange
        var linkAccountDto = new LinkAccountDto
        {
            LinkCode = "ABC12XYZ",
            Email = "player@example.com",
            Password = "SecurePass123!",
            PasswordConfirmation = "SecurePass123!"
        };

        var linkedUser = new UserDto
        {
            Id = 1,
            Username = "player",
            Email = "player@example.com"
        };

        _mockUserService
            .Setup(s => s.LinkExistingAccountAsync(It.IsAny<int>(), It.IsAny<UserUpdateDto>()))
            .ReturnsAsync(linkedUser);

        // Act
        var result = await _controller.LinkAccountAsync(linkAccountDto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(200, okResult.StatusCode);
    }

    [Fact]
    public async Task LinkAccountAsync_WithInvalidCode_Returns400BadRequest()
    {
        // Arrange
        var linkAccountDto = new LinkAccountDto
        {
            LinkCode = "INVALID99",
            Email = "player@example.com",
            Password = "SecurePass123!",
            PasswordConfirmation = "SecurePass123!"
        };

        // Act
        var result = await _controller.LinkAccountAsync(linkAccountDto);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(400, badRequestResult.StatusCode);
    }

    [Fact]
    public async Task LinkAccountAsync_WithPasswordMismatch_Returns400BadRequest()
    {
        // Arrange
        var linkAccountDto = new LinkAccountDto
        {
            LinkCode = "ABC12XYZ",
            Email = "player@example.com",
            Password = "SecurePass123!",
            PasswordConfirmation = "DifferentPass123!"
        };

        // Act
        var result = await _controller.LinkAccountAsync(linkAccountDto);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(400, badRequestResult.StatusCode);
    }

    #endregion

    #region Error Response Tests

    [Fact]
    public async Task ErrorResponses_UseConsistentFormat()
    {
        // Arrange
        var createDto = new UserCreateDto
        {
            Username = "test",
            Email = "invalid-email"
        };

        _mockUserService
            .Setup(s => s.ValidateUserCreationAsync(createDto))
            .ReturnsAsync((false, "Invalid email format"));

        // Act
        var result = await _controller.CreateAsync(createDto);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.NotNull(badRequestResult.Value);
        var errorResponse = badRequestResult.Value as dynamic;
        Assert.NotNull(errorResponse.error);
        Assert.NotNull(errorResponse.message);
    }

    #endregion
}
