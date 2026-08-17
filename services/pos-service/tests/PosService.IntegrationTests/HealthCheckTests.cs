using Xunit;

namespace PosService.IntegrationTests;

public class HealthCheckTests : IntegrationTestBase
{
    [Fact]
    public async Task HealthEndpoint_ShouldReturnHealthy()
    {
        var client = CreateClient();
        var response = await client.GetAsync("/health");

        response.EnsureSuccessStatusCode();
    }
}
