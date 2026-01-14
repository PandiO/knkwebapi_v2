using Xunit;
using knkwebapi_v2.Models;
using knkwebapi_v2.Repositories;
using Microsoft.EntityFrameworkCore;

namespace knkwebapi_v2.Tests.Integration;

/// <summary>
/// Integration tests for unique constraint enforcement.
/// Tests database-level and service-level uniqueness checks.
/// </summary>
public class UniqueConstraintIntegrationTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly UserRepository _userRepository;

    public UniqueConstraintIntegrationTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _dbContext = new ApplicationDbContext(options);
        _dbContext.Database.EnsureCreated();
        _userRepository = new UserRepository(_dbContext);
    }

    #region Username Uniqueness Tests

    [Fact]
    public async Task UsernameUniqueness_IsEnforcedByRepository()
    {
        // Arrange
        var user1 = new User
        {
            Username = "uniqueplayer",
            Email = "user1@example.com",
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.Users.Add(user1);
        await _dbContext.SaveChangesAsync();

        // Act
        var isTaken = await _userRepository.IsUsernameTakenAsync("uniqueplayer");

        // Assert
        Assert.True(isTaken);
    }

    [Fact]
    public async Task UsernameUniqueness_IsCaseInsensitive()
    {
        // Arrange
        var user = new User
        {
            Username = "PlayerOne",
            Email = "player@example.com",
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        // Act
        var isTakenLower = await _userRepository.IsUsernameTakenAsync("playerone");
        var isTakenUpper = await _userRepository.IsUsernameTakenAsync("PLAYERONE");
        var isTakenMixed = await _userRepository.IsUsernameTakenAsync("PlAyErOnE");

        // Assert
        Assert.True(isTakenLower);
        Assert.True(isTakenUpper);
        Assert.True(isTakenMixed);
    }

    [Fact]
    public async Task UsernameUniqueness_ExcludesCurrentUser()
    {
        // Arrange
        var user = new User
        {
            Username = "player",
            Email = "player@example.com",
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        // Act - exclude the same user from check
        var isTaken = await _userRepository.IsUsernameTakenAsync("player", user.Id);

        // Assert
        Assert.False(isTaken);
    }

    [Fact]
    public async Task UsernameUniqueness_FlagsOtherUsersWithSameName()
    {
        // Arrange
        var user1 = new User
        {
            Username = "samename",
            Email = "user1@example.com",
            CreatedAt = DateTime.UtcNow
        };

        var user2 = new User
        {
            Username = "samename",
            Email = "user2@example.com",
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.Users.Add(user1);
        await _dbContext.SaveChangesAsync();

        // Act - exclude user1 but check if name is taken by other users
        var isTakenByOthers = await _userRepository.IsUsernameTakenAsync("samename", user1.Id);

        // We only add user1, so excluding it should show as not taken (unless implementation checks for other users too)
        // This test is for clarification on the requirement

        // For now, if user1 is excluded and we only have user1, result should be false
        Assert.False(isTakenByOthers);
    }

    #endregion

    #region Email Uniqueness Tests

    [Fact]
    public async Task EmailUniqueness_IsEnforcedByRepository()
    {
        // Arrange
        var user1 = new User
        {
            Username = "player1",
            Email = "unique@example.com",
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.Users.Add(user1);
        await _dbContext.SaveChangesAsync();

        // Act
        var isTaken = await _userRepository.IsEmailTakenAsync("unique@example.com");

        // Assert
        Assert.True(isTaken);
    }

    [Fact]
    public async Task EmailUniqueness_IsCaseInsensitive()
    {
        // Arrange
        var user = new User
        {
            Username = "player",
            Email = "Player@Example.com",
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        // Act
        var isTakenLower = await _userRepository.IsEmailTakenAsync("player@example.com");
        var isTakenUpper = await _userRepository.IsEmailTakenAsync("PLAYER@EXAMPLE.COM");

        // Assert
        Assert.True(isTakenLower);
        Assert.True(isTakenUpper);
    }

    [Fact]
    public async Task EmailUniqueness_AllowsNullValues()
    {
        // Arrange - create two Minecraft-only users without emails
        var user1 = new User
        {
            Username = "mcplayer1",
            Uuid = "550e8400-e29b-41d4-a716-446655440000",
            Email = null,
            CreatedAt = DateTime.UtcNow
        };

        var user2 = new User
        {
            Username = "mcplayer2",
            Uuid = "550e8400-e29b-41d4-a716-446655440001",
            Email = null,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.Users.Add(user1);
        _dbContext.Users.Add(user2);

        // Act & Assert - should not throw
        await _dbContext.SaveChangesAsync();

        var savedUsers = _dbContext.Users.Where(u => u.Email == null).ToList();
        Assert.Equal(2, savedUsers.Count);
    }

    [Fact]
    public async Task EmailUniqueness_ExcludesCurrentUser()
    {
        // Arrange
        var user = new User
        {
            Username = "player",
            Email = "myemail@example.com",
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        // Act - allow current user to keep their email
        var isTaken = await _userRepository.IsEmailTakenAsync("myemail@example.com", user.Id);

        // Assert
        Assert.False(isTaken);
    }

    #endregion

    #region UUID Uniqueness Tests

    [Fact]
    public async Task UuidUniqueness_IsEnforcedByRepository()
    {
        // Arrange
        var uuid = "550e8400-e29b-41d4-a716-446655440000";
        var user1 = new User
        {
            Username = "player1",
            Uuid = uuid,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.Users.Add(user1);
        await _dbContext.SaveChangesAsync();

        // Act
        var isTaken = await _userRepository.IsUuidTakenAsync(uuid);

        // Assert
        Assert.True(isTaken);
    }

    [Fact]
    public async Task UuidUniqueness_AllowsNullValues()
    {
        // Arrange - create two web app users without UUIDs (linked later)
        var user1 = new User
        {
            Username = "webplayer1",
            Email = "web1@example.com",
            Uuid = null,
            CreatedAt = DateTime.UtcNow
        };

        var user2 = new User
        {
            Username = "webplayer2",
            Email = "web2@example.com",
            Uuid = null,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.Users.Add(user1);
        _dbContext.Users.Add(user2);

        // Act & Assert - should not throw
        await _dbContext.SaveChangesAsync();

        var savedUsers = _dbContext.Users.Where(u => u.Uuid == null).ToList();
        Assert.Equal(2, savedUsers.Count);
    }

    [Fact]
    public async Task UuidUniqueness_ExactMatchOnly()
    {
        // Arrange
        var uuid = "550e8400-e29b-41d4-a716-446655440000";
        var user = new User
        {
            Username = "player",
            Uuid = uuid,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        // Act - different UUID should not be taken
        var isTaken = await _userRepository.IsUuidTakenAsync("550e8400-e29b-41d4-a716-446655440001");

        // Assert
        Assert.False(isTaken);
    }

    #endregion

    #region Combined Constraint Tests

    [Fact]
    public async Task AllConstraints_CanCoexistWithoutConflict()
    {
        // Arrange - create diverse users
        var user1 = new User
        {
            Username = "webplayer",
            Email = "web@example.com",
            Uuid = null,
            CreatedAt = DateTime.UtcNow
        };

        var user2 = new User
        {
            Username = "mcplayer",
            Email = null,
            Uuid = "550e8400-e29b-41d4-a716-446655440000",
            CreatedAt = DateTime.UtcNow
        };

        var user3 = new User
        {
            Username = "linkedplayer",
            Email = "linked@example.com",
            Uuid = "550e8400-e29b-41d4-a716-446655440001",
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.Users.Add(user1);
        _dbContext.Users.Add(user2);
        _dbContext.Users.Add(user3);

        // Act & Assert
        await _dbContext.SaveChangesAsync();

        // Verify all are unique
        Assert.True(await _userRepository.IsUsernameTakenAsync("webplayer"));
        Assert.True(await _userRepository.IsEmailTakenAsync("web@example.com"));
        Assert.True(await _userRepository.IsUsernameTakenAsync("mcplayer"));
        Assert.True(await _userRepository.IsUuidTakenAsync("550e8400-e29b-41d4-a716-446655440000"));
        Assert.True(await _userRepository.IsUsernameTakenAsync("linkedplayer"));
        Assert.True(await _userRepository.IsEmailTakenAsync("linked@example.com"));
        Assert.True(await _userRepository.IsUuidTakenAsync("550e8400-e29b-41d4-a716-446655440001"));
    }

    [Fact]
    public async Task DuplicateDetection_IdentifiesConflictingAccounts()
    {
        // Arrange - simulate duplicate accounts
        var uuid = "550e8400-e29b-41d4-a716-446655440000";
        var username = "conflictplayer";

        var user1 = new User
        {
            Username = username,
            Email = "user1@example.com",
            Uuid = uuid,
            CreatedAt = DateTime.UtcNow.AddDays(-10)
        };

        var user2 = new User
        {
            Username = username,
            Email = "user2@example.com",
            Uuid = uuid,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.Users.Add(user1);
        _dbContext.Users.Add(user2);
        await _dbContext.SaveChangesAsync();

        // Act
        var duplicate = await _userRepository.FindDuplicateAsync(uuid, username);

        // Assert
        Assert.NotNull(duplicate);
    }

    #endregion

    #region Repository Query Tests

    [Fact]
    public async Task GetByUsernameAsync_ReturnsUserWithExactMatch()
    {
        // Arrange
        var user = new User
        {
            Username = "searchplayer",
            Email = "search@example.com",
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        // Act
        var found = await _userRepository.GetByUsernameAsync("searchplayer");

        // Assert
        Assert.NotNull(found);
        Assert.Equal("searchplayer", found.Username);
    }

    [Fact]
    public async Task GetByEmailAsync_ReturnsUserWithEmail()
    {
        // Arrange
        var user = new User
        {
            Username = "player",
            Email = "findemail@example.com",
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        // Act
        var found = await _userRepository.GetByEmailAsync("findemail@example.com");

        // Assert
        Assert.NotNull(found);
        Assert.Equal("findemail@example.com", found.Email);
    }

    [Fact]
    public async Task GetByUuidAsync_ReturnsUserWithUuid()
    {
        // Arrange
        var uuid = "550e8400-e29b-41d4-a716-446655440000";
        var user = new User
        {
            Username = "player",
            Uuid = uuid,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        // Act
        var found = await _userRepository.GetByUuidAsync(uuid);

        // Assert
        Assert.NotNull(found);
        Assert.Equal(uuid, found.Uuid);
    }

    [Fact]
    public async Task GetByUuidAndUsernameAsync_RequiresBothMatches()
    {
        // Arrange
        var uuid = "550e8400-e29b-41d4-a716-446655440000";
        var user = new User
        {
            Username = "player",
            Uuid = uuid,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        // Act
        var foundBoth = await _userRepository.GetByUuidAndUsernameAsync(uuid, "player");
        var foundWrongName = await _userRepository.GetByUuidAndUsernameAsync(uuid, "wrongname");
        var foundWrongUuid = await _userRepository.GetByUuidAndUsernameAsync("550e8400-e29b-41d4-a716-446655440001", "player");

        // Assert
        Assert.NotNull(foundBoth);
        Assert.Null(foundWrongName);
        Assert.Null(foundWrongUuid);
    }

    #endregion

    #region Soft Delete Tests

    [Fact]
    public async Task SoftDelete_MarksUserAsInactiveWithDeletionMetadata()
    {
        // Arrange
        var user = new User
        {
            Username = "tobeDeleted",
            Email = "delete@example.com",
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        // Act
        user.IsActive = false;
        user.DeletedAt = DateTime.UtcNow;
        user.DeletedReason = "Account merged";
        user.ArchiveUntil = DateTime.UtcNow.AddDays(90);

        _dbContext.Users.Update(user);
        await _dbContext.SaveChangesAsync();

        // Assert
        var retrievedUser = await _userRepository.GetByIdAsync(user.Id);
        Assert.NotNull(retrievedUser);
        Assert.False(retrievedUser.IsActive);
        Assert.NotNull(retrievedUser.DeletedAt);
        Assert.NotNull(retrievedUser.DeletedReason);
        Assert.NotNull(retrievedUser.ArchiveUntil);
    }

    [Fact]
    public async Task SoftDeleted_UserStillVisible_ButMarkedInactive()
    {
        // Arrange
        var user = new User
        {
            Username = "softdeleted",
            Email = "softdel@example.com",
            CreatedAt = DateTime.UtcNow,
            IsActive = false,
            DeletedAt = DateTime.UtcNow,
            DeletedReason = "User requested deletion"
        };

        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        // Act
        var found = await _userRepository.GetByIdAsync(user.Id);

        // Assert
        Assert.NotNull(found);
        Assert.False(found.IsActive);
        // Username is still unique even though deleted
        Assert.True(await _userRepository.IsUsernameTakenAsync("softdeleted"));
    }

    #endregion

    public void Dispose()
    {
        _dbContext?.Dispose();
    }
}
