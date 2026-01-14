using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using knkwebapi_v2.Models;
using knkwebapi_v2.Services;
using knkwebapi_v2.Dtos;

namespace KnKWebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _service;

        public UsersController(IUserService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var items = await _service.GetAllAsync();
            return Ok(items);
        }

        [HttpGet("{id:int}", Name = nameof(GetUserById))]
        public async Task<IActionResult> GetUserById(int id)
        {
            var item = await _service.GetByIdAsync(id);
            if (item == null) return NotFound();
            return Ok(item);
        }

        [HttpGet("uuid/{uuid}", Name = nameof(GetUserSummaryByUuid))]
        public async Task<IActionResult> GetUserSummaryByUuid(string uuid)
        {
            var item = await _service.GetByUuidAsync(uuid);
            if (item == null) return NotFound();
            var dto = new UserSummaryDto
            {
                Id = item.Id,
                Username = item.Username,
                Coins = item.Coins,
                Gems = item.Gems,
                ExperiencePoints = item.ExperiencePoints,
                Uuid = item.Uuid
            };
            return Ok(dto);
        }

        [HttpGet("username/{username}", Name = nameof(GetUserSummaryByUsername))]
        public async Task<IActionResult> GetUserSummaryByUsername(string username)
        {
            var item = await _service.GetByUsernameAsync(username);
            if (item == null) return NotFound();
            var dto = new UserSummaryDto
            {
                Id = item.Id,
                Username = item.Username,
                Coins = item.Coins,
                Gems = item.Gems,
                ExperiencePoints = item.ExperiencePoints,
                Uuid = item.Uuid
            };
            return Ok(dto);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] UserCreateDto user)
        {
            if (user == null) return BadRequest(new { error = "InvalidRequest", message = "User data is required" });
            
            try
            {
                // Phase 4.2: Validate user creation
                var (isValid, errorMessage) = await _service.ValidateUserCreationAsync(user);
                if (!isValid)
                {
                    return BadRequest(new { error = "ValidationFailed", message = errorMessage });
                }

                // Check for duplicates
                if (!string.IsNullOrEmpty(user.Username))
                {
                    var (usernameTaken, _) = await _service.CheckUsernameTakenAsync(user.Username);
                    if (usernameTaken)
                    {
                        return Conflict(new { error = "DuplicateUsername", message = "Username is already taken" });
                    }
                }

                if (!string.IsNullOrEmpty(user.Email))
                {
                    var (emailTaken, _) = await _service.CheckEmailTakenAsync(user.Email);
                    if (emailTaken)
                    {
                        return Conflict(new { error = "DuplicateEmail", message = "Email is already in use" });
                    }
                }

                if (!string.IsNullOrEmpty(user.Uuid))
                {
                    var (uuidTaken, _) = await _service.CheckUuidTakenAsync(user.Uuid);
                    if (uuidTaken)
                    {
                        return Conflict(new { error = "DuplicateUuid", message = "UUID is already registered" });
                    }
                }

                // Create the user (service handles password hashing, link code validation/consumption, etc.)
                var created = await _service.CreateAsync(user);
                
                // Generate link code for response if web app first (has email but no uuid yet)
                LinkCodeResponseDto? linkCode = null;
                if (!string.IsNullOrEmpty(created.Email) && string.IsNullOrEmpty(created.Uuid))
                {
                    linkCode = await _service.GenerateLinkCodeAsync(created.Id);
                }

                var response = new 
                {
                    user = created,
                    linkCode = linkCode
                };

                return CreatedAtRoute(nameof(GetUserById), new { id = created.Id }, response);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = "InvalidArgument", message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = "OperationFailed", message = ex.Message });
            }
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] UserDto user)
        {
            if (user == null) return BadRequest(new { error = "InvalidRequest", message = "User data is required" });
            try
            {
                await _service.UpdateAsync(id, user);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { error = "UserNotFound", message = $"User with ID {id} not found" });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = "ValidationFailed", message = ex.Message });
            }
        }

        [HttpPut("{id:int}/coins")]
        public async Task<IActionResult> UpdateCoins(int id, [FromBody] int coins)
        {
            try
            {
                await _service.UpdateCoinsAsync(id, coins);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { error = "UserNotFound", message = $"User with ID {id} not found" });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = "ValidationFailed", message = ex.Message });
            }
        }

        [HttpPut("{uuid}/coins")]
        public async Task<IActionResult> UpdateCoinsByUuid(string uuid, [FromBody] int coins)
        {
            try
            {
                await _service.UpdateCoinsByUuidAsync(uuid, coins);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { error = "UserNotFound", message = $"User with UUID {uuid} not found" });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = "ValidationFailed", message = ex.Message });
            }
        }

        // ===== PHASE 4.3: AUTHENTICATION ENDPOINTS =====

        /// <summary>
        /// Generate a new link code for a user.
        /// POST /api/users/generate-link-code
        /// </summary>
        [HttpPost("generate-link-code")]
        public async Task<IActionResult> GenerateLinkCode([FromBody] LinkCodeRequestDto request)
        {
            try
            {
                var user = await _service.GetByIdAsync(request.UserId);
                if (user == null)
                {
                    return NotFound(new { error = "UserNotFound", message = $"User with ID {request.UserId} not found" });
                }

                var linkCode = await _service.GenerateLinkCodeAsync(request.UserId);
                return Ok(linkCode);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = "InvalidArgument", message = ex.Message });
            }
        }

        /// <summary>
        /// Validate a link code and return associated user information.
        /// POST /api/users/validate-link-code/{code}
        /// </summary>
        [HttpPost("validate-link-code/{code}")]
        public async Task<IActionResult> ValidateLinkCode(string code)
        {
            try
            {
                var (isValid, user) = await _service.ConsumeLinkCodeAsync(code);
                
                if (!isValid || user == null)
                {
                    return Ok(new ValidateLinkCodeResponseDto
                    {
                        IsValid = false,
                        Error = "Invalid or expired link code"
                    });
                }

                return Ok(new ValidateLinkCodeResponseDto
                {
                    IsValid = true,
                    UserId = user.Id,
                    Username = user.Username,
                    Email = user.Email
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = "InvalidArgument", message = ex.Message });
            }
        }

        /// <summary>
        /// Change user password.
        /// PUT /api/users/{id}/change-password
        /// </summary>
        [HttpPut("{id:int}/change-password")]
        public async Task<IActionResult> ChangePassword(int id, [FromBody] ChangePasswordDto request)
        {
            try
            {
                var user = await _service.GetByIdAsync(id);
                if (user == null)
                {
                    return NotFound(new { error = "UserNotFound", message = $"User with ID {id} not found" });
                }

                await _service.ChangePasswordAsync(id, request.CurrentPassword, request.NewPassword, request.PasswordConfirmation);
                return NoContent();
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { error = "InvalidPassword", message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = "ValidationFailed", message = ex.Message });
            }
        }

        /// <summary>
        /// Update user email.
        /// PUT /api/users/{id}/update-email
        /// </summary>
        [HttpPut("{id:int}/update-email")]
        public async Task<IActionResult> UpdateEmail(int id, [FromBody] UpdateEmailDto request)
        {
            try
            {
                var user = await _service.GetByIdAsync(id);
                if (user == null)
                {
                    return NotFound(new { error = "UserNotFound", message = $"User with ID {id} not found" });
                }

                // Check if email is already taken
                var (emailTaken, conflictingUserId) = await _service.CheckEmailTakenAsync(request.NewEmail, id);
                if (emailTaken)
                {
                    return Conflict(new { error = "DuplicateEmail", message = "Email is already in use by another account" });
                }

                await _service.UpdateEmailAsync(id, request.NewEmail, request.CurrentPassword);
                return NoContent();
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { error = "InvalidPassword", message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = "ValidationFailed", message = ex.Message });
            }
        }

        /// <summary>
        /// Check for duplicate accounts (Minecraft server uses this).
        /// POST /api/users/check-duplicate
        /// </summary>
        [HttpPost("check-duplicate")]
        public async Task<IActionResult> CheckDuplicate([FromBody] DuplicateCheckDto request)
        {
            try
            {
                var (hasDuplicate, secondaryUserId) = await _service.CheckForDuplicateAsync(request.Uuid, request.Username);

                if (!hasDuplicate)
                {
                    return Ok(new DuplicateCheckResponseDto
                    {
                        HasDuplicate = false,
                        Message = "No duplicate accounts found"
                    });
                }

                // Get both users
                var primaryUser = await _service.GetByUuidAsync(request.Uuid);
                var secondaryUser = secondaryUserId.HasValue ? await _service.GetByIdAsync(secondaryUserId.Value) : null;

                return Ok(new DuplicateCheckResponseDto
                {
                    HasDuplicate = true,
                    PrimaryUser = primaryUser != null ? new UserSummaryDto
                    {
                        Id = primaryUser.Id,
                        Username = primaryUser.Username,
                        Uuid = primaryUser.Uuid,
                        Coins = primaryUser.Coins,
                        Gems = primaryUser.Gems,
                        ExperiencePoints = primaryUser.ExperiencePoints
                    } : null,
                    ConflictingUser = secondaryUser != null ? new UserSummaryDto
                    {
                        Id = secondaryUser.Id,
                        Username = secondaryUser.Username,
                        Uuid = secondaryUser.Uuid,
                        Coins = secondaryUser.Coins,
                        Gems = secondaryUser.Gems,
                        ExperiencePoints = secondaryUser.ExperiencePoints
                    } : null,
                    Message = "Duplicate accounts detected. Please merge them."
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = "InvalidArgument", message = ex.Message });
            }
        }

        // ===== PHASE 4.4: ACCOUNT MERGE ENDPOINTS =====

        /// <summary>
        /// Merge two user accounts.
        /// POST /api/users/merge
        /// </summary>
        [HttpPost("merge")]
        public async Task<IActionResult> MergeAccounts([FromBody] AccountMergeDto request)
        {
            try
            {
                // Verify both users exist
                var primaryUser = await _service.GetByIdAsync(request.PrimaryUserId);
                if (primaryUser == null)
                {
                    return NotFound(new { error = "PrimaryUserNotFound", message = $"Primary user with ID {request.PrimaryUserId} not found" });
                }

                var secondaryUser = await _service.GetByIdAsync(request.SecondaryUserId);
                if (secondaryUser == null)
                {
                    return NotFound(new { error = "SecondaryUserNotFound", message = $"Secondary user with ID {request.SecondaryUserId} not found" });
                }

                // Perform merge
                var mergedUser = await _service.MergeAccountsAsync(request.PrimaryUserId, request.SecondaryUserId);

                return Ok(new AccountMergeResultDto
                {
                    User = mergedUser,
                    MergedFromUserId = request.SecondaryUserId,
                    Message = $"Successfully merged account {request.SecondaryUserId} into {request.PrimaryUserId}"
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = "InvalidArgument", message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = "OperationFailed", message = ex.Message });
            }
        }

        /// <summary>
        /// Link an existing Minecraft account with email/password from web app.
        /// POST /api/users/link-account
        /// </summary>
        [HttpPost("link-account")]
        public async Task<IActionResult> LinkAccount([FromBody] LinkAccountDto request)
        {
            try
            {
                // Validate link code and get associated user
                var (isValid, user) = await _service.ConsumeLinkCodeAsync(request.LinkCode);
                
                if (!isValid || user == null)
                {
                    return BadRequest(new { error = "InvalidLinkCode", message = "Invalid or expired link code" });
                }

                // Validate password
                var (passwordValid, passwordError) = await _service.ValidatePasswordAsync(request.Password);
                if (!passwordValid)
                {
                    return BadRequest(new { error = "InvalidPassword", message = passwordError });
                }

                // Check password confirmation
                if (request.Password != request.PasswordConfirmation)
                {
                    return BadRequest(new { error = "PasswordMismatch", message = "Password and confirmation do not match" });
                }

                // Check if email is already taken
                var (emailTaken, conflictingUserId) = await _service.CheckEmailTakenAsync(request.Email, user.Id);
                if (emailTaken)
                {
                    return Conflict(new { error = "DuplicateEmail", message = "Email is already in use by another account" });
                }

                // Update user with email
                await _service.UpdateEmailAsync(user.Id, request.Email, null);
                
                // Set initial password (no current password needed since we're setting it for the first time)
                // Use empty string as currentPassword since ChangePasswordAsync allows null PasswordHash
                await _service.ChangePasswordAsync(user.Id, "", request.Password, request.PasswordConfirmation);
                
                // Get updated user
                var updatedUser = await _service.GetByIdAsync(user.Id);
                
                return Ok(new 
                {
                    user = updatedUser,
                    message = "Account successfully linked with email and password"
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = "InvalidArgument", message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = "OperationFailed", message = ex.Message });
            }
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _service.DeleteAsync(id);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { error = "UserNotFound", message = $"User with ID {id} not found" });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = "ValidationFailed", message = ex.Message });
            }
        }

        [HttpPost("search")]
        public async Task<ActionResult<PagedResultDto<UserListDto>>> SearchUsers([FromBody] PagedQueryDto query)
        {
            var result = await _service.SearchAsync(query);
            return Ok(result);
        }
    }
}
