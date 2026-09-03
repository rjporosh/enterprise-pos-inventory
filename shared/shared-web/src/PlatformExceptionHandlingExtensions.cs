using Microsoft.Extensions.DependencyInjection;

namespace SharedWeb;

public static class PlatformExceptionHandlingExtensions
{
    /// <summary>
    /// Registers <see cref="PlatformExceptionHandler"/> (+ ProblemDetails services). Pair with
    /// <c>app.UseExceptionHandler()</c> in the pipeline, immediately after correlation-id and
    /// request-logging middleware. Register any number of <see cref="IExceptionMapper"/>s
    /// beforehand to handle a service's own domain exceptions.
    /// </summary>
    public static IServiceCollection AddPlatformExceptionHandling(this IServiceCollection services)
    {
        services.AddProblemDetails();
        services.AddExceptionHandler<PlatformExceptionHandler>();
        return services;
    }

    public static IServiceCollection AddExceptionMapper<TMapper>(this IServiceCollection services)
        where TMapper : class, IExceptionMapper
    {
        services.AddSingleton<IExceptionMapper, TMapper>();
        return services;
    }
}
