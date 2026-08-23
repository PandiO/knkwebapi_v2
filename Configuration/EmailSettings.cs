namespace knkwebapi_v2.Configuration;

/// <summary>
/// Outbound email delivery configuration. SmtpPassword is intentionally left out of
/// appsettings*.json and must be supplied via environment variable (Email__SmtpPassword)
/// or user-secrets - never commit it to source control.
/// </summary>
public class EmailSettings
{
    /// <summary>
    /// "Log" (default, dev-safe: logs instead of sending) or "Smtp" (sends real email).
    /// </summary>
    public string Provider { get; set; } = "Log";

    public string SmtpHost { get; set; } = "smtp.gmail.com";

    public int SmtpPort { get; set; } = 587;

    public bool UseStartTls { get; set; } = true;

    /// <summary>
    /// Mailbox/account used to authenticate with the SMTP server.
    /// </summary>
    public string SmtpUsername { get; set; } = string.Empty;

    /// <summary>
    /// SMTP password / app password. Populate via environment variable or user-secrets only.
    /// </summary>
    public string SmtpPassword { get; set; } = string.Empty;

    public string FromAddress { get; set; } = string.Empty;

    public string FromName { get; set; } = "Knights & Kings";
}
