using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using InventoryService.Infrastructure.Persistence;

#nullable disable

namespace InventoryService.Infrastructure.Migrations
{
    /// <summary>
    /// Hand-authored migration (no .NET SDK / dotnet-ef tooling was available in the environment this was
    /// written in). Adds the idempotency inbox table used by the POS-integration event consumer.
    ///
    /// IMPORTANT: this was added on top of an existing, tool-generated migration chain without a matching
    /// Designer.cs/snapshot update. Before running `dotnet ef database update` against a real database, run
    /// `dotnet ef migrations add AddIntegrationEventInbox --project services/inventory-service/src/InventoryService.Infrastructure
    /// --startup-project services/inventory-service/src/InventoryService.API --output-dir Migrations --force`
    /// once the SDK is available to regenerate this migration (and InventoryDbContextModelSnapshot.cs) correctly
    /// from the live model — delete this hand-written file first so the tool doesn't collide with it.
    /// </summary>
    [DbContext(typeof(InventoryDbContext))]
    [Migration("20260812010000_AddIntegrationEventInbox")]
    public partial class AddIntegrationEventInbox : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "processed_integration_events",
                schema: "inventory",
                columns: table => new
                {
                    event_id = table.Column<Guid>(type: "uuid", nullable: false),
                    event_type = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    processed_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_processed_integration_events", x => x.event_id);
                });

            migrationBuilder.CreateIndex(
                name: "idx_processed_integration_events_processed_at",
                schema: "inventory",
                table: "processed_integration_events",
                column: "processed_at_utc");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "processed_integration_events",
                schema: "inventory");
        }
    }
}
