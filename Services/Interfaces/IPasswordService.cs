namespace knkwebapi_v2.Services
{
    /// <summary>
    /// Service for password hashing, verification, and validation.
    /// </summary>
    public interface IPasswordService
    {
        /// <summary>
        /// Hashes a plain text password using bcrypt.
        /// </summary>
        /// <param name="password">Plain text password</param>
        /// <returns>Bcrypt hashed password</returns>
        Task<string> HashPasswordAsync(string password);

        /// <summary>
        /// Verifies a plain text password against a bcrypt hash.
        /// </summary>
        /// <param name="plainPassword">Plain text password</param>
        /// <param name="hash">Bcrypt hash to verify against</param>
        /// <returns>True if password matches hash</returns>
        Task<bool> VerifyPasswordAsync(string plainPassword, string hash);

        /// <summary>
        /// Validates a password against security policy.
        /// Checks: length (8-128 chars), weak password blacklist.
        /// Does NOT enforce complexity requirements (no uppercase/numbers/symbols required).
        /// </summary>
        /// <param name="password">Password to validate</param>
        /// <returns>Tuple indicating if valid and error message if invalid</returns>
        Task<(bool IsValid, string? Error)> ValidatePasswordAsync(string password);
    }
}
