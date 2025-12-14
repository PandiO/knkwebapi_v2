namespace knkwebapi_v2.Configuration;

/// <summary>
/// Configuration for client activity tracking.
/// </summary>
public class ClientActivityOptions
{
    public const string SectionName = "ClientActivity";

    /// <summary>
    /// Enable/disable client activity tracking.
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Maximum number of clients to track in memory.
    /// When exceeded, cleanup removes old inactive clients.
    /// </summary>
    public int MaxClients { get; set; } = 1000;

    /// <summary>
    /// How long to keep a client before it's considered for cleanup (if inactive).
    /// </summary>
    public TimeSpan InactivityThreshold { get; set; } = TimeSpan.FromHours(24);

    /// <summary>
    /// How often to run cleanup.
    /// </summary>
    public TimeSpan CleanupInterval { get; set; } = TimeSpan.FromMinutes(10);

    /// <summary>
    /// Number of minutes to retain activity data (rolling window).
    /// </summary>
    public int RollingWindowMinutes { get; set; } = 60;
}
