using System.Net;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Gateway.Tests;

/// <summary>
/// Hermetic tests only — no downstream service needs to be running. GET /health/services (the
/// fan-out check against the 4 real backend services) is deliberately not exercised here, since
/// it depends on live services being reachable at the configured addresses; it was verified
/// manually against a running docker-compose stack instead (see AI-HANDOVER.md).
/// </summary>
public class HealthAndRoutingTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public HealthAndRoutingTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Health_ShouldReturn200()
    {
        var response = await _client.GetAsync("/health");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task UnmatchedRoute_ShouldReturn404()
    {
        var response = await _client.GetAsync("/api/v1/does-not-exist");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Metrics_ShouldReturn200()
    {
        var response = await _client.GetAsync("/metrics");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
