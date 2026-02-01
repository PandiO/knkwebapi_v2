using Microsoft.Extensions.Options;

namespace knkwebapi_v2.Services
{
    /// <summary>
    /// Service for password hashing, verification, and validation using bcrypt.
    /// Implements OWASP 2023 password guidelines: length over complexity.
    /// </summary>
    public class PasswordService : IPasswordService
    {
        private readonly int _bcryptRounds;
        private static readonly HashSet<string> WeakPasswords = new(StringComparer.OrdinalIgnoreCase)
        {
            // Top 100 most common passwords (OWASP/SecLists)
            "123456", "password", "12345678", "qwerty", "123456789", "12345", "1234", "111111",
            "1234567", "dragon", "123123", "baseball", "abc123", "football", "monkey", "letmein",
            "shadow", "master", "666666", "qwertyuiop", "123321", "mustang", "1234567890",
            "michael", "654321", "superman", "1qaz2wsx", "7777777", "121212", "000000", "qazwsx",
            "admin", "admin123", "root", "toor", "pass", "test", "guest", "info", "adm", "mysql",
            "user", "administrator", "oracle", "ftp", "pi", "puppet", "ansible", "ec2-user", "vagrant",
            "password1", "password123", "welcome", "welcome123", "login", "passw0rd", "Password1",
            "abc123456", "123qwe", "qwerty123", "iloveyou", "princess", "admin1", "1q2w3e4r",
            "sunshine", "ashley", "bailey", "passw0rd", "shadow1", "123456a", "password1!", "trustno1",
            "1qazxsw2", "charlie", "123abc", "password!", "qwerty1", "monkey1", "liverpool",
            "654321a", "master123", "starwars", "passw0rd!", "football1", "batman", "access",
            "1234qwer", "trustno1", "rangers", "jordan23", "hello", "qwertyui", "lovely",
            "ninja", "azerty", "solo", "flower", "000000", "hottie", "loveme", "zaq1zaq1",
            "password12", "Welcome1", "whatever", "donald", "dragon1", "michael1", "michelle",
            "passw0rd1", "password2", "qwerty12", "freedom"
        };

        public PasswordService(IConfiguration configuration)
        {
            _bcryptRounds = configuration.GetValue<int>("Security:BcryptRounds", 10);
            
            // Validate bcrypt rounds (4-31 valid range)
            if (_bcryptRounds < 4 || _bcryptRounds > 31)
            {
                throw new ArgumentException("BcryptRounds must be between 4 and 31");
            }
        }

        /// <inheritdoc/>
        public Task<string> HashPasswordAsync(string password)
        {
            if (string.IsNullOrEmpty(password))
            {
                throw new ArgumentException("Password cannot be null or empty", nameof(password));
            }

            var hash = BCrypt.Net.BCrypt.HashPassword(password, _bcryptRounds);
            return Task.FromResult(hash);
        }

        /// <inheritdoc/>
        public Task<bool> VerifyPasswordAsync(string plainPassword, string hash)
        {
            if (string.IsNullOrEmpty(plainPassword))
            {
                return Task.FromResult(false);
            }

            if (string.IsNullOrEmpty(hash))
            {
                return Task.FromResult(false);
            }

            try
            {
                var isValid = BCrypt.Net.BCrypt.Verify(plainPassword, hash);
                return Task.FromResult(isValid);
            }
            catch
            {
                // Invalid hash format or verification error
                return Task.FromResult(false);
            }
        }

        /// <inheritdoc/>
        public Task<(bool IsValid, string? Error)> ValidatePasswordAsync(string password)
        {
            if (string.IsNullOrEmpty(password))
            {
                return Task.FromResult<(bool, string?)>((false, "Password is required."));
            }

            // Length validation: 8-128 characters (OWASP recommendation)
            if (password.Length < 8)
            {
                return Task.FromResult<(bool, string?)>((false, "Password must be at least 8 characters long."));
            }

            if (password.Length > 128)
            {
                return Task.FromResult<(bool, string?)>((false, "Password must not exceed 128 characters."));
            }

            // Check against weak password list
            if (WeakPasswords.Contains(password))
            {
                return Task.FromResult<(bool, string?)>((false, "This password is too common. Please choose a stronger password."));
            }

            // Check for common patterns
            if (IsCommonPattern(password))
            {
                return Task.FromResult<(bool, string?)>((false, "This password contains a common pattern. Please choose a more unique password."));
            }

            return Task.FromResult<(bool, string?)>((true, null));
        }

        /// <summary>
        /// Detects common password patterns (sequential numbers, repeated chars, keyboard patterns).
        /// </summary>
        private static bool IsCommonPattern(string password)
        {
            var lower = password.ToLowerInvariant();

            // Repeated characters (aaa, 111, etc. - 3+ repeats)
            for (int i = 0; i < lower.Length - 2; i++)
            {
                if (lower[i] == lower[i + 1] && lower[i] == lower[i + 2])
                {
                    return true;
                }
            }

            // Sequential numbers (123, 234, 345, etc.)
            for (int i = 0; i < lower.Length - 2; i++)
            {
                if (char.IsDigit(lower[i]) && char.IsDigit(lower[i + 1]) && char.IsDigit(lower[i + 2]))
                {
                    int val1 = lower[i] - '0';
                    int val2 = lower[i + 1] - '0';
                    int val3 = lower[i + 2] - '0';
                    
                    if (val2 == val1 + 1 && val3 == val2 + 1)
                    {
                        return true; // 123, 234, etc.
                    }
                    if (val2 == val1 - 1 && val3 == val2 - 1)
                    {
                        return true; // 321, 432, etc.
                    }
                }
            }

            // Common keyboard patterns
            string[] keyboardPatterns = { "qwert", "asdfg", "zxcvb", "12345", "54321", "qazwsx", "1qaz2wsx" };
            foreach (var pattern in keyboardPatterns)
            {
                if (lower.Contains(pattern))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
