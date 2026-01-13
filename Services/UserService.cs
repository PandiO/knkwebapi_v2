using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using knkwebapi_v2.Dtos;
using knkwebapi_v2.Models;
using knkwebapi_v2.Repositories;

namespace knkwebapi_v2.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _repo;
        private readonly IMapper _mapper;
        private readonly IPasswordService _passwordService;
        private readonly ILinkCodeService _linkCodeService;

        public UserService(
            IUserRepository repo, 
            IMapper mapper,
            IPasswordService passwordService,
            ILinkCodeService linkCodeService)
        {
            _repo = repo;
            _mapper = mapper;
            _passwordService = passwordService;
            _linkCodeService = linkCodeService;
        }

        public async Task<IEnumerable<UserDto>> GetAllAsync()
        {
            var users = await _repo.GetAllAsync();
            return _mapper.Map<IEnumerable<UserDto>>(users);
        }

        public async Task<UserDto?> GetByIdAsync(int id)
        {
            if (id <= 0) return null;
            var user = await _repo.GetByIdAsync(id);
            return _mapper.Map<UserDto>(user);
        }

        public async Task<UserDto?> GetByUuidAsync(string uuid)
        {
            if (string.IsNullOrWhiteSpace(uuid)) return null;
            var user = await _repo.GetByUuidAsync(uuid);
            return _mapper.Map<UserDto>(user);
        }

        public async Task<UserDto?> GetByUsernameAsync(string username)
        {
            if (string.IsNullOrWhiteSpace(username)) return null;
            var user = await _repo.GetByUsernameAsync(username);
            return _mapper.Map<UserDto>(user);
        }

        public async Task<UserDto> CreateAsync(UserCreateDto userDto)
        {
            if (userDto == null) throw new ArgumentNullException(nameof(userDto));
            if (string.IsNullOrWhiteSpace(userDto.Username)) throw new ArgumentException("Username is required.", nameof(userDto));
            // if (string.IsNullOrWhiteSpace(userDto.Email)) throw new ArgumentException("Email is required.", nameof(userDto));

            var user = _mapper.Map<User>(userDto);
            user.CreatedAt = DateTime.UtcNow;
            
            await _repo.AddUserAsync(user);
            return _mapper.Map<UserDto>(user);
        }

        public async Task UpdateAsync(int id, UserDto userDto)
        {
            if (userDto == null) throw new ArgumentNullException(nameof(userDto));
            if (id <= 0) throw new ArgumentException("Invalid id.", nameof(id));
            if (string.IsNullOrWhiteSpace(userDto.Username)) throw new ArgumentException("Username is required.", nameof(userDto));
            if (string.IsNullOrWhiteSpace(userDto.Email)) throw new ArgumentException("Email is required.", nameof(userDto));

            var existing = await _repo.GetByIdAsync(id);
            if (existing == null) throw new KeyNotFoundException($"User with id {id} not found.");

            existing.Username = userDto.Username;
            existing.Email = userDto.Email;
            existing.Coins = userDto.Coins;

            await _repo.UpdateUserAsync(existing);
        }

        public async Task UpdateCoinsAsync(int id, int coins)
        {
            if (id <= 0) throw new ArgumentException("Invalid id.", nameof(id));
            var existing = await _repo.GetByIdAsync(id);
            if (existing == null) throw new KeyNotFoundException($"User with id {id} not found.");

            await _repo.UpdateUserCoinsAsync(id, coins);
        }

        public async Task UpdateCoinsByUuidAsync(string uuid, int coins)
        {
            if (string.IsNullOrWhiteSpace(uuid)) throw new ArgumentException("Invalid uuid.", nameof(uuid));
            var existing = await _repo.GetByUuidAsync(uuid);
            if (existing == null) throw new KeyNotFoundException($"User with uuid {uuid} not found.");

            await _repo.UpdateUserCoinsByUuidAsync(uuid, coins);
        }

        public async Task DeleteAsync(int id)
        {
            if (id <= 0) throw new ArgumentException("Invalid id.", nameof(id));
            var existing = await _repo.GetByIdAsync(id);
            if (existing == null) throw new KeyNotFoundException($"User with id {id} not found.");

            await _repo.DeleteUserAsync(id);
        }

        public async Task<PagedResultDto<UserListDto>> SearchAsync(PagedQueryDto queryDto)
        {
            if (queryDto == null) throw new ArgumentNullException(nameof(queryDto));

            var query = _mapper.Map<PagedQuery>(queryDto);
            var result = await _repo.SearchAsync(query);
            var resultDto = _mapper.Map<PagedResultDto<UserListDto>>(result);

            return resultDto;
        }

        // ===== NEW METHODS: VALIDATION =====

        public async Task<(bool IsValid, string? ErrorMessage)> ValidateUserCreationAsync(UserCreateDto dto)
        {
            if (dto == null)
            {
                return (false, "User data is required.");
            }

            // Validate username
            if (string.IsNullOrWhiteSpace(dto.Username))
            {
                return (false, "Username is required.");
            }

            if (dto.Username.Length < 3 || dto.Username.Length > 50)
            {
                return (false, "Username must be between 3 and 50 characters.");
            }

            // Check username uniqueness
            var (usernameTaken, _) = await CheckUsernameTakenAsync(dto.Username);
            if (usernameTaken)
            {
                return (false, "Username is already taken.");
            }

            // Validate email (if provided)
            if (!string.IsNullOrWhiteSpace(dto.Email))
            {
                if (!IsValidEmail(dto.Email))
                {
                    return (false, "Invalid email format.");
                }

                var (emailTaken, _) = await CheckEmailTakenAsync(dto.Email);
                if (emailTaken)
                {
                    return (false, "Email is already registered.");
                }
            }

            // Validate UUID (if provided)
            if (!string.IsNullOrWhiteSpace(dto.Uuid))
            {
                var (uuidTaken, _) = await CheckUuidTakenAsync(dto.Uuid);
                if (uuidTaken)
                {
                    return (false, "UUID is already registered.");
                }
            }

            // Validate password (if provided)
            if (!string.IsNullOrWhiteSpace(dto.Password))
            {
                var (isValidPassword, passwordError) = await ValidatePasswordAsync(dto.Password);
                if (!isValidPassword)
                {
                    return (false, passwordError);
                }

                // Check password confirmation
                if (dto.Password != dto.PasswordConfirmation)
                {
                    return (false, "Password and confirmation do not match.");
                }
            }

            return (true, null);
        }

        public async Task<(bool IsValid, string? ErrorMessage)> ValidatePasswordAsync(string password)
        {
            return await _passwordService.ValidatePasswordAsync(password);
        }

        // ===== NEW METHODS: UNIQUE CONSTRAINT CHECKS =====

        public async Task<(bool IsTaken, int? ConflictingUserId)> CheckUsernameTakenAsync(string username, int? excludeUserId = null)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                return (false, null);
            }

            var isTaken = await _repo.IsUsernameTakenAsync(username, excludeUserId);
            
            if (isTaken)
            {
                var user = await _repo.GetByUsernameAsync(username);
                return (true, user?.Id);
            }

            return (false, null);
        }

        public async Task<(bool IsTaken, int? ConflictingUserId)> CheckEmailTakenAsync(string email, int? excludeUserId = null)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return (false, null);
            }

            var isTaken = await _repo.IsEmailTakenAsync(email, excludeUserId);
            
            if (isTaken)
            {
                var user = await _repo.GetByEmailAsync(email);
                return (true, user?.Id);
            }

            return (false, null);
        }

        public async Task<(bool IsTaken, int? ConflictingUserId)> CheckUuidTakenAsync(string uuid, int? excludeUserId = null)
        {
            if (string.IsNullOrWhiteSpace(uuid))
            {
                return (false, null);
            }

            var isTaken = await _repo.IsUuidTakenAsync(uuid, excludeUserId);
            
            if (isTaken)
            {
                var user = await _repo.GetByUuidAsync(uuid);
                return (true, user?.Id);
            }

            return (false, null);
        }

        // ===== NEW METHODS: CREDENTIALS MANAGEMENT =====

        public async Task ChangePasswordAsync(int userId, string currentPassword, string newPassword, string passwordConfirmation)
        {
            if (userId <= 0)
            {
                throw new ArgumentException("Invalid user ID.", nameof(userId));
            }

            if (string.IsNullOrWhiteSpace(currentPassword))
            {
                throw new ArgumentException("Current password is required.", nameof(currentPassword));
            }

            if (string.IsNullOrWhiteSpace(newPassword))
            {
                throw new ArgumentException("New password is required.", nameof(newPassword));
            }

            if (newPassword != passwordConfirmation)
            {
                throw new ArgumentException("New password and confirmation do not match.");
            }

            var user = await _repo.GetByIdAsync(userId);
            if (user == null)
            {
                throw new KeyNotFoundException($"User with ID {userId} not found.");
            }

            // Verify current password
            if (!string.IsNullOrEmpty(user.PasswordHash))
            {
                var isValidPassword = await _passwordService.VerifyPasswordAsync(currentPassword, user.PasswordHash);
                if (!isValidPassword)
                {
                    throw new UnauthorizedAccessException("Current password is incorrect.");
                }
            }

            // Validate new password
            var (isValid, error) = await _passwordService.ValidatePasswordAsync(newPassword);
            if (!isValid)
            {
                throw new ArgumentException(error ?? "Invalid password.");
            }

            // Hash and update password
            var newHash = await _passwordService.HashPasswordAsync(newPassword);
            await _repo.UpdatePasswordHashAsync(userId, newHash);
        }

        public async Task<bool> VerifyPasswordAsync(string plainPassword, string? passwordHash)
        {
            if (string.IsNullOrEmpty(passwordHash))
            {
                return false;
            }

            return await _passwordService.VerifyPasswordAsync(plainPassword, passwordHash);
        }

        public async Task UpdateEmailAsync(int userId, string newEmail, string? currentPassword = null)
        {
            if (userId <= 0)
            {
                throw new ArgumentException("Invalid user ID.", nameof(userId));
            }

            if (string.IsNullOrWhiteSpace(newEmail))
            {
                throw new ArgumentException("Email is required.", nameof(newEmail));
            }

            if (!IsValidEmail(newEmail))
            {
                throw new ArgumentException("Invalid email format.", nameof(newEmail));
            }

            var user = await _repo.GetByIdAsync(userId);
            if (user == null)
            {
                throw new KeyNotFoundException($"User with ID {userId} not found.");
            }

            // Verify current password if provided and user has password set
            if (!string.IsNullOrEmpty(currentPassword) && !string.IsNullOrEmpty(user.PasswordHash))
            {
                var isValidPassword = await _passwordService.VerifyPasswordAsync(currentPassword, user.PasswordHash);
                if (!isValidPassword)
                {
                    throw new UnauthorizedAccessException("Current password is incorrect.");
                }
            }

            // Check email uniqueness
            var (emailTaken, _) = await CheckEmailTakenAsync(newEmail, userId);
            if (emailTaken)
            {
                throw new InvalidOperationException("Email is already registered to another account.");
            }

            await _repo.UpdateEmailAsync(userId, newEmail);
        }

        // ===== NEW METHODS: BALANCES (COINS, GEMS, XP) =====

        public async Task AdjustBalancesAsync(int userId, int coinsDelta, int gemsDelta, int experienceDelta, string reason, string? metadata = null)
        {
            if (userId <= 0)
            {
                throw new ArgumentException("Invalid user ID.", nameof(userId));
            }

            if (string.IsNullOrWhiteSpace(reason))
            {
                throw new ArgumentException("Reason is required for balance adjustments.", nameof(reason));
            }

            var user = await _repo.GetByIdAsync(userId);
            if (user == null)
            {
                throw new KeyNotFoundException($"User with ID {userId} not found.");
            }

            // Calculate new balances
            int newCoins = user.Coins + coinsDelta;
            int newGems = user.Gems + gemsDelta;
            int newExperience = user.ExperiencePoints + experienceDelta;

            // Reject underflows (no negative balances)
            if (newCoins < 0)
            {
                throw new InvalidOperationException($"Insufficient coins. Current: {user.Coins}, Attempted change: {coinsDelta}");
            }

            if (newGems < 0)
            {
                throw new InvalidOperationException($"Insufficient gems. Current: {user.Gems}, Attempted change: {gemsDelta}");
            }

            if (newExperience < 0)
            {
                throw new InvalidOperationException($"Insufficient experience points. Current: {user.ExperiencePoints}, Attempted change: {experienceDelta}");
            }

            // Update balances
            user.Coins = newCoins;
            user.Gems = newGems;
            user.ExperiencePoints = newExperience;

            await _repo.UpdateUserAsync(user);

            // TODO: Log to audit trail (Phase 4 - implement audit logging)
            // Log balance change with reason and metadata
        }

        // ===== NEW METHODS: LINK CODES =====

        public async Task<LinkCodeResponseDto> GenerateLinkCodeAsync(int? userId)
        {
            if (userId.HasValue)
            {
                var user = await _repo.GetByIdAsync(userId.Value);
                if (user == null)
                {
                    throw new KeyNotFoundException($"User with ID {userId} not found.");
                }
            }

            return await _linkCodeService.GenerateLinkCodeAsync(userId);
        }

        public async Task<(bool IsValid, UserDto? User)> ConsumeLinkCodeAsync(string code)
        {
            var (success, linkCode, error) = await _linkCodeService.ConsumeLinkCodeAsync(code);

            if (!success || linkCode == null)
            {
                return (false, null);
            }

            if (linkCode.UserId.HasValue)
            {
                var user = await _repo.GetByIdAsync(linkCode.UserId.Value);
                if (user != null)
                {
                    return (true, _mapper.Map<UserDto>(user));
                }
            }

            return (false, null);
        }

        public async Task<IEnumerable<LinkCode>> GetExpiredLinkCodesAsync()
        {
            return await _linkCodeService.GetExpiredCodesAsync();
        }

        public async Task CleanupExpiredLinksAsync()
        {
            await _linkCodeService.CleanupExpiredCodesAsync();
        }

        // ===== NEW METHODS: MERGING & LINKING =====

        public async Task<(bool HasConflict, int? SecondaryUserId)> CheckForDuplicateAsync(string uuid, string username)
        {
            if (string.IsNullOrWhiteSpace(uuid) || string.IsNullOrWhiteSpace(username))
            {
                return (false, null);
            }

            var duplicate = await _repo.FindDuplicateAsync(uuid, username);
            if (duplicate != null)
            {
                return (true, duplicate.Id);
            }

            return (false, null);
        }

        public async Task<UserDto> MergeAccountsAsync(int primaryUserId, int secondaryUserId)
        {
            if (primaryUserId <= 0)
            {
                throw new ArgumentException("Invalid primary user ID.", nameof(primaryUserId));
            }

            if (secondaryUserId <= 0)
            {
                throw new ArgumentException("Invalid secondary user ID.", nameof(secondaryUserId));
            }

            if (primaryUserId == secondaryUserId)
            {
                throw new ArgumentException("Cannot merge a user with itself.");
            }

            var primaryUser = await _repo.GetByIdAsync(primaryUserId);
            if (primaryUser == null)
            {
                throw new KeyNotFoundException($"Primary user with ID {primaryUserId} not found.");
            }

            var secondaryUser = await _repo.GetByIdAsync(secondaryUserId);
            if (secondaryUser == null)
            {
                throw new KeyNotFoundException($"Secondary user with ID {secondaryUserId} not found.");
            }

            // Perform merge (repository handles soft delete and data preservation)
            await _repo.MergeUsersAsync(primaryUserId, secondaryUserId);

            // Return updated primary user
            var mergedUser = await _repo.GetByIdAsync(primaryUserId);
            return _mapper.Map<UserDto>(mergedUser!);
        }

        // ===== HELPER METHODS =====

        private static bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return false;
            }

            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }
    }
}
