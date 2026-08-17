using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace InventoryService.FunctionalTests;

public class ReleaseEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public ReleaseEndpointTests(WebApplicationFactory<Program> factory)
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
