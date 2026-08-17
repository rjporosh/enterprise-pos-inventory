using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InventoryService.Infrastructure.Migrations
{
    public partial class SeedInitialData : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                schema: "inventory",
                table: "units",
                columns: new[] { "id", "created_at", "created_by", "deleted_at", "deleted_by", "description", "is_active", "is_deleted", "name", "symbol", "updated_at", "updated_by" },
                values: new object[,]
                {
                    { new Guid("10000000-0000-0000-0000-000000000001"), DateTime.Parse("2025-01-01T00:00:00Z"), null, null, null, "Piece", true, false, "Piece", "pcs", null, null },
                    { new Guid("10000000-0000-0000-0000-000000000002"), DateTime.Parse("2025-01-01T00:00:00Z"), null, null, null, "Kilogram", true, false, "Kilogram", "kg", null, null },
                    { new Guid("10000000-0000-0000-0000-000000000003"), DateTime.Parse("2025-01-01T00:00:00Z"), null, null, null, "Liter", true, false, "Liter", "l", null, null },
                    { new Guid("10000000-0000-0000-0000-000000000004"), DateTime.Parse("2025-01-01T00:00:00Z"), null, null, null, "Box", true, false, "Box", "box", null, null },
                    { new Guid("10000000-0000-0000-0000-000000000005"), DateTime.Parse("2025-01-01T00:00:00Z"), null, null, null, "Meter", true, false, "Meter", "m", null, null }
                });

            migrationBuilder.InsertData(
                schema: "inventory",
                table: "categories",
                columns: new[] { "id", "created_at", "created_by", "deleted_at", "deleted_by", "description", "is_active", "is_deleted", "name", "parent_category_id", "sort_order", "updated_at", "updated_by" },
                values: new object[,]
                {
                    { new Guid("20000000-0000-0000-0000-000000000001"), DateTime.Parse("2025-01-01T00:00:00Z"), null, null, null, "All products", true, false, "All", null, 0, null, null },
                    { new Guid("20000000-0000-0000-0000-000000000002"), DateTime.Parse("2025-01-01T00:00:00Z"), null, null, null, "Grocery items", true, false, "Grocery", new Guid("20000000-0000-0000-0000-000000000001"), 1, null, null },
                    { new Guid("20000000-0000-0000-0000-000000000003"), DateTime.Parse("2025-01-01T00:00:00Z"), null, null, null, "Electronics", true, false, "Electronics", new Guid("20000000-0000-0000-0000-000000000001"), 2, null, null },
                    { new Guid("20000000-0000-0000-0000-000000000004"), DateTime.Parse("2025-01-01T00:00:00Z"), null, null, null, "Clothing", true, false, "Clothing", new Guid("20000000-0000-0000-0000-000000000001"), 3, null, null },
                    { new Guid("20000000-0000-0000-0000-000000000005"), DateTime.Parse("2025-01-01T00:00:00Z"), null, null, null, "Beverages", true, false, "Beverages", new Guid("20000000-0000-0000-0000-000000000002"), 1, null, null }
                });

            migrationBuilder.InsertData(
                schema: "inventory",
                table: "brands",
                columns: new[] { "id", "created_at", "created_by", "deleted_at", "deleted_by", "description", "is_active", "is_deleted", "name", "updated_at", "updated_by", "website" },
                values: new object[,]
                {
                    { new Guid("30000000-0000-0000-0000-000000000001"), DateTime.Parse("2025-01-01T00:00:00Z"), null, null, null, "Generic brand", true, false, "Generic", null, null, null },
                    { new Guid("30000000-0000-0000-0000-000000000002"), DateTime.Parse("2025-01-01T00:00:00Z"), null, null, null, "Premium electronics", true, false, "TechPro", null, null, "https://techpro.example.com" },
                    { new Guid("30000000-0000-0000-0000-000000000003"), DateTime.Parse("2025-01-01T00:00:00Z"), null, null, null, "Clothing brand", true, false, "StyleWear", null, null, null }
                });

            migrationBuilder.InsertData(
                schema: "inventory",
                table: "warehouses",
                columns: new[] { "id", "address", "city", "contact_name", "country", "created_at", "created_by", "deleted_at", "deleted_by", "is_active", "is_default", "is_deleted", "name", "phone", "updated_at", "updated_by" },
                values: new object[,]
                {
                    { new Guid("40000000-0000-0000-0000-000000000001"), "123 Main St", "Dhaka", "Warehouse Manager", "Bangladesh", DateTime.Parse("2025-01-01T00:00:00Z"), null, null, null, true, true, false, "Main Warehouse", "+8801700000001", null, null },
                    { new Guid("40000000-0000-0000-0000-000000000002"), "456 Side St", "Chittagong", "Branch Manager", "Bangladesh", DateTime.Parse("2025-01-01T00:00:00Z"), null, null, null, true, false, false, "Branch Warehouse", "+8801700000002", null, null }
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(schema: "inventory", table: "warehouses", keyColumn: "id", keyValue: new Guid("40000000-0000-0000-0000-000000000001"));
            migrationBuilder.DeleteData(schema: "inventory", table: "warehouses", keyColumn: "id", keyValue: new Guid("40000000-0000-0000-0000-000000000002"));
            migrationBuilder.DeleteData(schema: "inventory", table: "brands", keyColumn: "id", keyValue: new Guid("30000000-0000-0000-0000-000000000001"));
            migrationBuilder.DeleteData(schema: "inventory", table: "brands", keyColumn: "id", keyValue: new Guid("30000000-0000-0000-0000-000000000002"));
            migrationBuilder.DeleteData(schema: "inventory", table: "brands", keyColumn: "id", keyValue: new Guid("30000000-0000-0000-0000-000000000003"));
            migrationBuilder.DeleteData(schema: "inventory", table: "categories", keyColumn: "id", keyValue: new Guid("20000000-0000-0000-0000-000000000001"));
            migrationBuilder.DeleteData(schema: "inventory", table: "categories", keyColumn: "id", keyValue: new Guid("20000000-0000-0000-0000-000000000002"));
            migrationBuilder.DeleteData(schema: "inventory", table: "categories", keyColumn: "id", keyValue: new Guid("20000000-0000-0000-0000-000000000003"));
            migrationBuilder.DeleteData(schema: "inventory", table: "categories", keyColumn: "id", keyValue: new Guid("20000000-0000-0000-0000-000000000004"));
            migrationBuilder.DeleteData(schema: "inventory", table: "categories", keyColumn: "id", keyValue: new Guid("20000000-0000-0000-0000-000000000005"));
            migrationBuilder.DeleteData(schema: "inventory", table: "units", keyColumn: "id", keyValue: new Guid("10000000-0000-0000-0000-000000000001"));
            migrationBuilder.DeleteData(schema: "inventory", table: "units", keyColumn: "id", keyValue: new Guid("10000000-0000-0000-0000-000000000002"));
            migrationBuilder.DeleteData(schema: "inventory", table: "units", keyColumn: "id", keyValue: new Guid("10000000-0000-0000-0000-000000000003"));
            migrationBuilder.DeleteData(schema: "inventory", table: "units", keyColumn: "id", keyValue: new Guid("10000000-0000-0000-0000-000000000004"));
            migrationBuilder.DeleteData(schema: "inventory", table: "units", keyColumn: "id", keyValue: new Guid("10000000-0000-0000-0000-000000000005"));
        }
    }
}
