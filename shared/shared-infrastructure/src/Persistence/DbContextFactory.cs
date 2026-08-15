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

public class PostgreSqlProviderFactory(bool enableQueryLogging = false) : IDbProviderFactory
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

        if (enableQueryLogging)
        {
            // Routed through Serilog's static Log.Logger (already configured with service/environment
            // enrichment by SerilogConfiguration) rather than threading ILoggerFactory through
            // IDbContextFactory, since DbContext instances here are constructed outside normal DI
            // activation (see Program.cs's AddScoped(sp => new PosDbContext(options)) pattern).
            builder.LogTo(
                message => Serilog.Log.Logger.Information("{EfQuery}", message),
                new[] { Microsoft.EntityFrameworkCore.DbLoggerCategory.Database.Command.Name },
                Microsoft.Extensions.Logging.LogLevel.Information);
        }

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
        var enableQueryLogging = bool.TryParse(configuration["Database:EnableQueryLogging"], out var queryLoggingFlag) && queryLoggingFlag;

        IDbProviderFactory providerFactory = providerName.ToLowerInvariant() switch
        {
            "sqlserver" => throw new NotImplementedException("SQL Server provider not yet implemented. Use PostgreSQL."),
            "mysql" => throw new NotImplementedException("MySQL provider not yet implemented. Use PostgreSQL."),
            "oracle" => throw new NotImplementedException("Oracle provider not yet implemented. Use PostgreSQL."),
            _ => new PostgreSqlProviderFactory(enableQueryLogging)
        };

        services.AddSingleton(providerFactory);
        services.AddSingleton<IDbContextFactory, DbContextFactory>();

        return services;
    }
}
