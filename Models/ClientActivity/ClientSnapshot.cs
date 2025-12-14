namespace knkwebapi_v2.Models.ClientActivity;

/// <summary>
/// Snapshot of a client's activity metrics.
/// Typically returned by admin endpoints.
/// </summary>
public class ClientSnapshot
{
    /// <summary>
    /// Client identification information.
    /// </summary>
    public ClientInfo Client { get; set; } = new();

    /// <summary>
    /// When this client was last seen (UTC).
    /// </summary>
    public DateTime LastSeenUtc { get; set; }

    /// <summary>
    /// Last request details (method, route, status, duration, timestamp).
    /// </summary>
    public RequestInfo? LastRequest { get; set; }

    /// <summary>
    /// Activity metrics for the last 60 minutes, indexed by minute (0-59).
    /// Array[i] is either null or the bucket for minute i (current time -> 59, 59 minutes ago -> 0).
    /// </summary>
    public RollingWindowBucket?[] BucketsLast60Minutes { get; set; } = new RollingWindowBucket?[60];

    /// <summary>
    /// Total requests in the last 60 minutes.
    /// </summary>
    public long TotalRequestsLast60Min => 
        BucketsLast60Minutes.Where(b => b != null).Sum(b => b!.TotalRequests);

    /// <summary>
    /// Successful requests (2xx, 3xx) in the last 60 minutes.
    /// </summary>
    public long SuccessRequestsLast60Min => 
        BucketsLast60Minutes.Where(b => b != null).Sum(b => b!.SuccessRequests);

    /// <summary>
    /// Error requests (4xx, 5xx) in the last 60 minutes.
    /// </summary>
    public long ErrorRequestsLast60Min => 
        BucketsLast60Minutes.Where(b => b != null).Sum(b => b!.ErrorRequests);

    /// <summary>
    /// Error rate as percentage (0-100). 0 if no requests.
    /// </summary>
    public double ErrorRateLast60Min => 
        TotalRequestsLast60Min > 0 ? (double)ErrorRequestsLast60Min / TotalRequestsLast60Min * 100 : 0;

    /// <summary>
    /// Average request duration (ms) over last 60 minutes.
    /// </summary>
    public double AvgDurationMsLast60Min =>
        TotalRequestsLast60Min > 0
            ? BucketsLast60Minutes.Where(b => b != null).Sum(b => b!.SumDurationMs) / (double)TotalRequestsLast60Min
            : 0;
}
