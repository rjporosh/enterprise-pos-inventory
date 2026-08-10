using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SharedKernel;

namespace SharedInfrastructure.Persistence;

public interface IDbProviderFactory
{
    DbContextOptionsBuilder UseProvider(DbContextOptionsBuilder builder, string connectionString);
    string ProviderName { get; }
}

public class PostgreSqlProviderFactory : IDbProviderFactory
{
    public string ProviderName => "PostgreSQL";

    public DbContextOptionsBuilder UseProvider(DbContextOptionsBuilder builder, string connectionString)
    {
        builder.UseNpgsql(connectionString, npgsqlOptions =>
        {
            npgsqlOptions.EnableRetryOnFailure(
                maxRetryCount: 3,
                maxRetryDelay: TimeSpan.FromSeconds(30),
                errorCodesToAdd: null);
        });

        return builder;
    }
}

public interface IDbContextFactory
{
    DbContextOptions<TContext> CreateOptions<TContext>(string connectionString) where TContext : DbContext;
}

public class DbContextFactory : IDbContextFactory
{
    private readonly IDbProviderFactory _providerFactory;

    public DbContextFactory(IDbProviderFactory providerFactory)
    {
        _providerFactory = providerFactory;
    }

    public DbContextOptions<TContext> CreateOptions<TContext>(string connectionString) where TContext : DbContext
    {
        var builder = new DbContextOptionsBuilder<TContext>();
        _providerFactory.UseProvider(builder, connectionString);
        return builder.Options;
    }
}

public static class DbContextServiceCollectionExtensions
{
    public static IServiceCollection AddDatabaseProvider(this IServiceCollection services, IConfiguration configuration, string providerKey = "Database:Provider")
    {
        var providerName = configuration[providerKey] ?? "PostgreSQL";

        IDbProviderFactory providerFactory = providerName.ToLowerInvariant() switch
        {
            "sqlserver" => throw new NotImplementedException("SQL Server provider not yet implemented. Use PostgreSQL."),
            "mysql" => throw new NotImplementedException("MySQL provider not yet implemented. Use PostgreSQL."),
            "oracle" => throw new NotImplementedException("Oracle provider not yet implemented. Use PostgreSQL."),
            _ => new PostgreSqlProviderFactory()
        };

        services.AddSingleton(providerFactory);
        services.AddSingleton<IDbContextFactory, DbContextFactory>();

        return services;
    }
}
