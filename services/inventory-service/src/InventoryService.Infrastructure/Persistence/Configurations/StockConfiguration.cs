using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using InventoryService.Domain.Stock;
using SharedKernel;

namespace InventoryService.Infrastructure.Persistence.Configurations;

public class StockConfiguration : BaseEntityConfiguration<Stock, Guid>
{
    public StockConfiguration() : base("stocks") { }

    public override void Configure(EntityTypeBuilder<Stock> builder)
    {
        base.Configure(builder);

        builder.Property(s => s.ProductId).HasColumnName("product_id").IsRequired();
        builder.Property(s => s.WarehouseId).HasColumnName("warehouse_id").IsRequired();
        builder.Property(s => s.QuantityOnHand).HasColumnName("quantity_on_hand").HasDefaultValue(0);
        builder.Property(s => s.QuantityReserved).HasColumnName("quantity_reserved").HasDefaultValue(0);
        builder.Property(s => s.ReorderLevel).HasColumnName("reorder_level").HasDefaultValue(0);
        builder.Property(s => s.MaxStockLevel).HasColumnName("max_stock_level").HasDefaultValue(0);
        builder.Property(s => s.LastRestockedAt).HasColumnName("last_restocked_at");

        builder.HasIndex(s => new { s.ProductId, s.WarehouseId })
            .HasDatabaseName("idx_stocks_product_warehouse")
            .IsUnique();

        builder.HasIndex(s => s.WarehouseId).HasDatabaseName("idx_stocks_warehouse_id");
        builder.HasIndex(s => s.ReorderLevel).HasDatabaseName("idx_stocks_reorder_level");

        builder.HasOne(s => s.Product)
            .WithMany()
            .HasForeignKey(s => s.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.Warehouse)
            .WithMany()
            .HasForeignKey(s => s.WarehouseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Ignore(s => s.Movements);
    }
}
