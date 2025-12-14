namespace knkwebapi_v2.Models.ClientActivity;

/// <summary>
/// Represents information about a single HTTP request from a client.
/// </summary>
public class RequestInfo
{
    /// <summary>
    /// HTTP method (GET, POST, etc.)
    /// </summary>
    public string Method { get; set; } = string.Empty;

    /// <summary>
    /// Route template, e.g. "/api/towns/{id}" or Endpoint.DisplayName.
    /// Should not include query string.
    /// </summary>
    public string RouteTemplate { get; set; } = string.Empty;

    /// <summary>
    /// HTTP response status code (200, 404, 500, etc.)
    /// </summary>
    public int StatusCode { get; set; }

    /// <summary>
    /// Request duration in milliseconds.
    /// </summary>
    public long DurationMs { get; set; }

    /// <summary>
    /// Timestamp in UTC when the request completed.
    /// </summary>
    public DateTime TimestampUtc { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Whether the request was successful (status code 2xx or 3xx).
    /// </summary>
    public bool IsSuccess => StatusCode >= 200 && StatusCode < 400;

    /// <summary>
    /// Short summary of the request for logging/display.
    /// </summary>
    public override string ToString() => 
        $"{Method} {RouteTemplate} -> {StatusCode} ({DurationMs}ms)";
}
