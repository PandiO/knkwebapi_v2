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
}
