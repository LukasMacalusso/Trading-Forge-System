using System.Net;
using System.Net.Http.Json;

namespace TraderForge.API.E2ETests;

public class HealthEndpointTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public HealthEndpointTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Get_Prices_ReturnsOk()
    {
        var response = await _client.PostAsJsonAsync("/api/prices", new
        {
            symbols = new[] { "BTCUSDT" }
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Get_SubscriptionPlans_ReturnsOk()
    {
        var response = await _client.GetAsync("/api/subscription/plans");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
