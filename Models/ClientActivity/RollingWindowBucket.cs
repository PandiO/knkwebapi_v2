namespace knkwebapi_v2.Models.ClientActivity;

/// <summary>
/// Represents activity metrics for a single minute bucket.
/// Used in a rolling 60-minute window for client activity tracking.
/// </summary>
public class RollingWindowBucket
{
    /// <summary>
    /// The minute index (0-59) representing which minute of the hour this bucket is for.
    /// </summary>
    public int MinuteIndex { get; set; }

    /// <summary>
    /// Start time (UTC) of this bucket.
    /// </summary>
    public DateTime BucketStartUtc { get; set; }

    /// <summary>
    /// Total number of requests in this minute.
    /// </summary>
    public long TotalRequests { get; set; }

    /// <summary>
    /// Number of successful requests (2xx, 3xx).
    /// </summary>
    public long SuccessRequests { get; set; }

    /// <summary>
    /// Number of error requests (4xx, 5xx).
    /// </summary>
    public long ErrorRequests { get; set; }

    /// <summary>
    /// Sum of durations for all requests in this minute.
    /// </summary>
    public long SumDurationMs { get; set; }

    /// <summary>
    /// Average duration in milliseconds (calculated as SumDurationMs / TotalRequests).
    /// </summary>
    public double AvgDurationMs => TotalRequests > 0 ? (double)SumDurationMs / TotalRequests : 0;

    public void RecordRequest(RequestInfo request)
    {
        TotalRequests++;
        if (request.IsSuccess)
            SuccessRequests++;
        else
            ErrorRequests++;
        SumDurationMs += request.DurationMs;
    }
}
