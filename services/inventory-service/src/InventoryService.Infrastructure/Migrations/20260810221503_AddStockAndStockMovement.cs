using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InventoryService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddStockAndStockMovement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "stocks",
                schema: "inventory",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    product_id = table.Column<Guid>(type: "uuid", nullable: false),
                    warehouse_id = table.Column<Guid>(type: "uuid", nullable: false),
                    quantity_on_hand = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    quantity_reserved = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    reorder_level = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    max_stock_level = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    last_restocked_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_by = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_stocks", x => x.id);
                    table.ForeignKey(
                        name: "FK_stocks_products_product_id",
                        column: x => x.product_id,
                        principalSchema: "inventory",
                        principalTable: "products",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_stocks_warehouses_warehouse_id",
                        column: x => x.warehouse_id,
                        principalSchema: "inventory",
                        principalTable: "warehouses",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "stock_movements",
                schema: "inventory",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    stock_id = table.Column<Guid>(type: "uuid", nullable: false),
                    product_id = table.Column<Guid>(type: "uuid", nullable: false),
                    warehouse_id = table.Column<Guid>(type: "uuid", nullable: false),
                    movement_type = table.Column<int>(type: "integer", nullable: false),
                    quantity = table.Column<int>(type: "integer", nullable: false),
                    balance_after = table.Column<int>(type: "integer", nullable: false),
                    unit_cost = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    reference_type = table.Column<string>(type: "text", nullable: true),
                    reference_id = table.Column<Guid>(type: "uuid", nullable: true),
                    notes = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_by = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_stock_movements", x => x.id);
                    table.ForeignKey(
                        name: "FK_stock_movements_stocks_stock_id",
                        column: x => x.stock_id,
                        principalSchema: "inventory",
                        principalTable: "stocks",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "idx_stock_movements_created_at",
                schema: "inventory",
                table: "stock_movements",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "idx_stock_movements_product_id",
                schema: "inventory",
                table: "stock_movements",
                column: "product_id");

            migrationBuilder.CreateIndex(
                name: "idx_stock_movements_reference",
                schema: "inventory",
                table: "stock_movements",
                columns: new[] { "reference_type", "reference_id" },
                filter: "\"reference_type\" IS NOT NULL AND \"reference_id\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "idx_stock_movements_stock_id",
                schema: "inventory",
                table: "stock_movements",
                column: "stock_id");

            migrationBuilder.CreateIndex(
                name: "idx_stock_movements_type",
                schema: "inventory",
                table: "stock_movements",
                column: "movement_type");

            migrationBuilder.CreateIndex(
                name: "idx_stock_movements_warehouse_id",
                schema: "inventory",
                table: "stock_movements",
                column: "warehouse_id");

            migrationBuilder.CreateIndex(
                name: "idx_stocks_product_warehouse",
                schema: "inventory",
                table: "stocks",
                columns: new[] { "product_id", "warehouse_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_stocks_reorder_level",
                schema: "inventory",
                table: "stocks",
                column: "reorder_level");

            migrationBuilder.CreateIndex(
                name: "idx_stocks_warehouse_id",
                schema: "inventory",
                table: "stocks",
                column: "warehouse_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "stock_movements",
                schema: "inventory");

            migrationBuilder.DropTable(
                name: "stocks",
                schema: "inventory");
        }
    }
}
