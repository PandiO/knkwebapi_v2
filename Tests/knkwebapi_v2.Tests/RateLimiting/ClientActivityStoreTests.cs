using Xunit;
using knkwebapi_v2.Models.ClientActivity;
using knkwebapi_v2.Services;
using FluentAssertions;

namespace knkwebapi_v2.Tests;

public class ClientActivityStoreTests
{
    [Fact]
    public void RecordsRequestsIntoRollingBuckets()
    {
        var store = new InMemoryClientActivityStore(maxClients: 10);
        var client = new ClientInfo { ClientId = "c1", ClientType = "web-admin" };

        var now = DateTime.UtcNow;
        for (int i = 0; i < 5; i++)
        {
            store.RecordRequest(client, new RequestInfo
            {
                Method = "GET",
                RouteTemplate = "/api/test",
                StatusCode = 200,
                DurationMs = 100,
                TimestampUtc = now
            });
        }

        var snapshot = store.GetClient("web-admin", "c1")!;
        snapshot.TotalRequestsLast60Min.Should().Be(5);
        snapshot.SuccessRequestsLast60Min.Should().Be(5);
        snapshot.ErrorRequestsLast60Min.Should().Be(0);
        snapshot.AvgDurationMsLast60Min.Should().Be(100);
        snapshot.BucketsLast60Minutes.Count(b => b != null).Should().BeGreaterOrEqualTo(1);
    }

    [Fact]
    public void GetClientsReturnsOnlyActiveWithinTimeWindow()
    {
        var store = new InMemoryClientActivityStore(maxClients: 10);
        var client1 = new ClientInfo { ClientId = "c1", ClientType = "web-admin" };
        var client2 = new ClientInfo { ClientId = "c2", ClientType = "paper-plugin" };
        
        store.RecordRequest(client1, new RequestInfo { Method = "GET", RouteTemplate = "/test", StatusCode = 200, DurationMs = 50 });
        store.RecordRequest(client2, new RequestInfo { Method = "POST", RouteTemplate = "/test2", StatusCode = 201, DurationMs = 100 });

        var activeClients = store.GetClients(TimeSpan.FromMinutes(5));
        activeClients.Count.Should().Be(2);
    }
}
