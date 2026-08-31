using System.Reflection;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace SharedInfrastructure;

public static class DependencyInjection
{
    /// <summary>
    /// Registers shared cross-cutting infrastructure: MediatR (pipeline + handlers) and FluentValidation validators.
    /// </summary>
    /// <param name="applicationAssemblies">
    /// The Application-layer assembl(y/ies) of the calling service (e.g. InventoryService.Application,
    /// PosService.Application) that contain the CQRS commands/queries, handlers, and validators to register.
    /// Without passing these, MediatR has no handlers to resolve and every mediator.Send(...) call fails at runtime.
    /// </param>
    public static IServiceCollection AddSharedInfrastructure(this IServiceCollection services, params Assembly[] applicationAssemblies)
    {
        var assemblies = new List<Assembly> { typeof(DependencyInjection).Assembly };
        assemblies.AddRange(applicationAssemblies);

        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssemblies(assemblies.ToArray());
        });

        // Scoped, not Singleton: this behavior depends on IValidator<T>, which
        // AddValidatorsFromAssemblies registers as Scoped. A Singleton pipeline
        // behavior can't consume a Scoped dependency — DI throws
        // "Cannot consume scoped service ... from singleton" the moment scope
        // validation is enabled (e.g. ASPNETCORE_ENVIRONMENT=Development, which
        // is what docker-compose.yml sets for every service), turning every
        // validated MediatR request into a 400.
        services.AddScoped(
            typeof(IPipelineBehavior<,>),
            typeof(Behaviors.ValidationBehavior<,>));

        if (applicationAssemblies.Length > 0)
        {
            services.AddValidatorsFromAssemblies(applicationAssemblies);
        }

        return services;
    }
}
