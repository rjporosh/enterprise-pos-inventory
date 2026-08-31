using AuthService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace AuthService.Infrastructure;

public sealed class AuthServiceDesignTimeDbContextFactory : IDesignTimeDbContextFactory<AuthDbContext>
{
    public AuthDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AuthDbContext>();

        // Must match DependencyInjection.cs's runtime AddDbContext<AuthDbContext> registration
        // exactly (same database name, same MigrationsHistoryTable schema/name). Otherwise
        // `dotnet ef database update` records applied migrations in the default
        // public.__EFMigrationsHistory while the running app checks auth.__ef_migrations_history,
        // sees it empty, and tries to re-run every migration against tables that already exist —
        // a guaranteed crash-loop on first boot after a by-the-book migration deploy.
        optionsBuilder.UseNpgsql(
            "Host=localhost;Port=5432;Database=auth_service;Username=postgres;Password=postgres",
            npgsql => npgsql.MigrationsHistoryTable("__ef_migrations_history", "auth"));

        return new AuthDbContext(optionsBuilder.Options);
    }
}
