using Xunit;
using knkwebapi_v2.Models.ClientActivity;
using FluentAssertions;

namespace knkwebapi_v2.Tests;

public class HeaderParsingTests
{
    [Fact]
    public void ClientInfoCreatesUniqueKey()
    {
        var client = new ClientInfo { ClientId = "abc", ClientType = "web-admin" };
        client.GetKey().Should().Be("web-admin:abc");
    }
}
