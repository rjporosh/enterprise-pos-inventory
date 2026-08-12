using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using InventoryService.Application.Integration;
using InventoryService.Application.Warehouses;
using InventoryService.Infrastructure.Repositories;

namespace InventoryService.Infrastructure.Messaging;

public static class InventoryMessagingExtensions
{
    /// <summary>
    /// Registers the optional POS-integration event consumer. Binding the RabbitMQ section and starting
    /// the hosted service is safe even when RabbitMQ:Host is empty — SaleEventsConsumer no-ops in that
    /// case (see its ExecuteAsync) rather than making RabbitMQ a hard dependency for Inventory startup.
    /// </summary>
    public static IServiceCollection AddInventoryMessaging(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<RabbitMqOptions>(configuration.GetSection(RabbitMqOptions.SectionName));
        services.AddScoped<IProcessedEventStore, ProcessedEventStore>();
        services.AddScoped<IWarehouseRepository, WarehouseRepository>();
        services.AddHostedService<SaleEventsConsumer>();

        return services;
    }
}
