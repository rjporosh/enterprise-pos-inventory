using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace InventoryService.IntegrationTests;

public class IntegrationTestBase : IAsyncLifetime
{
    private readonly WebApplicationFactory<object> _factory;

    public IntegrationTestBase()
    {
        _factory = new WebApplicationFactory<object>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                });
            });
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync()
    {
        await _factory.DisposeAsync();
    }

    protected HttpClient CreateClient()
    {
        return _factory.CreateClient();
    }
}
