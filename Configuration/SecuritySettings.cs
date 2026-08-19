namespace knkwebapi_v2.Configuration;

/// <summary>
/// Security configuration options for authentication and authorization.
/// </summary>
public class SecuritySettings
{
    /// <summary>
    /// Number of bcrypt rounds for password hashing. Default: 10
    /// </summary>
    public int BcryptRounds { get; set; } = 10;

    /// <summary>
    /// Link code expiration time in minutes. Default: 20
    /// </summary>
    public int LinkCodeExpirationMinutes { get; set; } = 20;

    /// <summary>
    /// Retention period for soft-deleted records in days. Default: 90
    /// </summary>
    public int SoftDeleteRetentionDays { get; set; } = 90;

    /// <summary>
    /// Password reset token lifetime in minutes. Default: 30.
    /// </summary>
    public int PasswordResetTokenExpirationMinutes { get; set; } = 30;

    /// <summary>
    /// Cooldown per email/IP pair for requesting password reset in seconds. Default: 60.
    /// </summary>
    public int PasswordResetRequestCooldownSeconds { get; set; } = 60;

    /// <summary>
    /// Frontend base URL used to compose password reset links.
    /// </summary>
    public string PasswordResetFrontendBaseUrl { get; set; } = "http://localhost:3000";

    /// <summary>
    /// When true in development, forgot-password response includes debug token/link payload.
    /// Must remain false in production.
    /// </summary>
    public bool PasswordResetExposeTokenInDevelopment { get; set; } = false;
}
