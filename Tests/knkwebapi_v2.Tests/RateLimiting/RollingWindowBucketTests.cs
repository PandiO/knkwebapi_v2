using Xunit;
using knkwebapi_v2.Models.ClientActivity;
using FluentAssertions;

namespace knkwebapi_v2.Tests;

public class RollingWindowBucketTests
{
    [Fact]
    public void RecordsSuccessRequests()
    {
        var bucket = new RollingWindowBucket { MinuteIndex = 0, BucketStartUtc = DateTime.UtcNow };
        bucket.RecordRequest(new RequestInfo
        {
            StatusCode = 200,
            DurationMs = 50,
            Method = "GET",
            RouteTemplate = "/test",
            TimestampUtc = DateTime.UtcNow
        });

        bucket.TotalRequests.Should().Be(1);
        bucket.SuccessRequests.Should().Be(1);
        bucket.ErrorRequests.Should().Be(0);
        bucket.AvgDurationMs.Should().Be(50);
    }

    [Fact]
    public void RecordsErrorRequests()
    {
        var bucket = new RollingWindowBucket { MinuteIndex = 0, BucketStartUtc = DateTime.UtcNow };
        bucket.RecordRequest(new RequestInfo
        {
            StatusCode = 500,
            DurationMs = 100,
            Method = "POST",
            RouteTemplate = "/error",
            TimestampUtc = DateTime.UtcNow
        });

        bucket.TotalRequests.Should().Be(1);
        bucket.SuccessRequests.Should().Be(0);
        bucket.ErrorRequests.Should().Be(1);
    }

    [Fact]
    public void CalculatesAvgDuration()
    {
        var bucket = new RollingWindowBucket { MinuteIndex = 0, BucketStartUtc = DateTime.UtcNow };
        bucket.RecordRequest(new RequestInfo { StatusCode = 200, DurationMs = 100 });
        bucket.RecordRequest(new RequestInfo { StatusCode = 200, DurationMs = 200 });
        bucket.RecordRequest(new RequestInfo { StatusCode = 200, DurationMs = 300 });

        bucket.TotalRequests.Should().Be(3);
        bucket.SumDurationMs.Should().Be(600);
        bucket.AvgDurationMs.Should().Be(200);
    }
}

