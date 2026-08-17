using FluentAssertions;
using Xunit;

namespace InventoryService.IntegrationTests;

public class DatabaseMigrationTests : IntegrationTestBase
{
    [Fact]
    public void MigrationFiles_ShouldExist()
    {
        var migrationPath = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory,
            "..", "..", "..", "..", "..",
            "services/inventory-service/src/InventoryService.Infrastructure/Migrations");
        
        var migrations = Directory.GetFiles(migrationPath, "*.cs");
        migrations.Should().NotBeEmpty();
    }
}
