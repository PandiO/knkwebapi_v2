namespace knkwebapi_v2.Models.ClientActivity;

/// <summary>
/// Represents the identity and metadata of a client application.
/// This is application-level identity, NOT player/user-level.
/// </summary>
public class ClientInfo
{
    /// <summary>
    /// Unique identifier for this client instance (GUID or stable unique id per app instance).
    /// </summary>
    public string ClientId { get; set; } = string.Empty;

    /// <summary>
    /// Type of client: paper-plugin | web-admin | web-player | worker | unknown
    /// </summary>
    public string ClientType { get; set; } = "unknown";

    /// <summary>
    /// Semantic version or build identifier for the client.
    /// </summary>
    public string? ClientVersion { get; set; }

    /// <summary>
    /// Human-readable name, e.g. "DEV_SERVER_1.21.10".
    /// </summary>
    public string? ClientName { get; set; }

    /// <summary>
    /// Unique key combining type and id. Used as dictionary key in store.
    /// </summary>
    public string GetKey() => $"{ClientType}:{ClientId}";

    public override string ToString() => $"{ClientType}/{ClientId} v{ClientVersion ?? "unknown"}";
}
