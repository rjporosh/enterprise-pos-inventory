using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace SharedInfrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddSharedInfrastructure(this IServiceCollection services)
    {
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssemblies(
                typeof(SharedInfrastructure.DependencyInjection).Assembly);
        });

        services.AddSingleton(
            typeof(IPipelineBehavior<,>),
            typeof(Behaviors.ValidationBehavior<,>));

        return services;
    }
}
