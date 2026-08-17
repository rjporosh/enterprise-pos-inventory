using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net.Http.Json;
using Xunit;

namespace PosService.FunctionalTests;

/// <summary>
/// Black-box functional tests that hit the running HTTP layer in-process. No database is
/// involved — these tests verify the endpoints that are always available regardless of
/// infrastructure (health, release, OpenAPI).
/// </summary>
public class ReleaseEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public ReleaseEndpointTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task HealthEndpoint_ShouldReturnOk()
    {
        var response = await _client.GetAsync("/health");

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
    }

    [Fact]
    public async Task ReleaseEndpoint_ShouldReturnServiceInfo()
    {
        var response = await _client.GetAsync("/api/v1/system/release");

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<ReleaseResponse>();
        body.Should().NotBeNull();
        body!.Service.Should().Be("pos-service");
        body.ApiVersion.Should().Be("v1");
    }

    [Fact]
    public async Task OpenApiSpec_ShouldBeServed()
    {
        var response = await _client.GetAsync("/openapi/v1.json");

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("openapi");
    }

    [Fact]
    public async Task ScalarUi_ShouldBeServed()
    {
        var response = await _client.GetAsync("/scalar/v1");

        // Scalar serves the UI; any 2xx or redirect is acceptable
        ((int)response.StatusCode).Should().BeInRange(200, 399);
    }

    [Fact]
    public async Task HealthLiveEndpoint_ShouldReturnOk()
    {
        var response = await _client.GetAsync("/health/live");

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
    }

    [Fact]
    public async Task HealthReadyEndpoint_ShouldReturnOk()
    {
        var response = await _client.GetAsync("/health/ready");

        // /health/ready may return 200 (healthy) or 503 (DB not connected) — either is a valid
        // HTTP-layer response, so we just assert the endpoint responds at all.
        var status = (int)response.StatusCode;
        status.Should().BeOneOf(200, 503);
    }

    [Fact]
    public async Task MetricsEndpoint_ShouldBeServed()
    {
        var response = await _client.GetAsync("/metrics");

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        // Prometheus text format starts with # HELP or similar
        content.Should().NotBeNullOrWhiteSpace();
    }

    private sealed record ReleaseResponse(
        string Service,
        string Version,
        string Environment,
        string ApiVersion);
}
