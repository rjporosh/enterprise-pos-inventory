using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace InventoryService.FunctionalTests;

public class ReleaseEndpointTests : IClassFixture<WebApplicationFactory<object>>
{
    private readonly HttpClient _client;

    public ReleaseEndpointTests(WebApplicationFactory<object> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetReleaseInfo_ShouldReturnServiceInfo()
    {
        var response = await _client.GetAsync("/api/v1/system/release");
        
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("inventory-service");
        content.Should().Contain("version");
        content.Should().Contain("build");
    }
}
