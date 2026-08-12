using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using PosService.Infrastructure.Persistence;

namespace PosService.Infrastructure;

public class PosDbContextDesignTimeFactory : IDesignTimeDbContextFactory<PosDbContext>
{
    public PosDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<PosDbContext>();
        optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=pos_db;Username=postgres;Password=postgres");

        return new PosDbContext(optionsBuilder.Options);
    }
}
