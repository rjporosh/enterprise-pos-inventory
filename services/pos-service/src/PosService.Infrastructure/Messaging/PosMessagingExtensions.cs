using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PosService.Application.Sales.Events;

namespace PosService.Infrastructure.Messaging;

public static class PosMessagingExtensions
{
    /// <summary>
    /// Registers RabbitMQ-backed Sale event publishing if — and only if — RabbitMQ:Host is present in
    /// configuration. If it is absent, this is a no-op and the NullSaleEventPublisher registered by
    /// default in Program.cs remains active, so POS's own checkout functionality never requires RabbitMQ
    /// to be running (PRIMARY GOAL).
    /// </summary>
    public static IServiceCollection AddPosMessaging(this IServiceCollection services, IConfiguration configuration)
    {
        var section = configuration.GetSection(RabbitMqOptions.SectionName);
        services.Configure<RabbitMqOptions>(section);

        var options = section.Get<RabbitMqOptions>() ?? new RabbitMqOptions();
        if (options.IsConfigured)
        {
            services.AddSingleton<ISaleEventPublisher, RabbitMqSaleEventPublisher>();
        }

        return services;
    }
}
