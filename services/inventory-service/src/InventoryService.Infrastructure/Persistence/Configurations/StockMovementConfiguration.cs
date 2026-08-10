using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using InventoryService.Domain.Stock;
using SharedKernel;

namespace InventoryService.Infrastructure.Persistence.Configurations;

public class StockMovementConfiguration : BaseEntityConfiguration<StockMovement, Guid>
{
    public StockMovementConfiguration() : base("stock_movements") { }

    public override void Configure(EntityTypeBuilder<StockMovement> builder)
    {
        base.Configure(builder);

        builder.Property(sm => sm.StockId).HasColumnName("stock_id").IsRequired();
        builder.Property(sm => sm.ProductId).HasColumnName("product_id").IsRequired();
        builder.Property(sm => sm.WarehouseId).HasColumnName("warehouse_id").IsRequired();
        builder.Property(sm => sm.MovementType).HasColumnName("movement_type").IsRequired();
        builder.Property(sm => sm.Quantity).HasColumnName("quantity").IsRequired();
        builder.Property(sm => sm.BalanceAfter).HasColumnName("balance_after").IsRequired();
        builder.Property(sm => sm.UnitCost).HasColumnName("unit_cost").HasPrecision(18, 2);
        builder.Property(sm => sm.ReferenceType).HasColumnName("reference_type");
        builder.Property(sm => sm.ReferenceId).HasColumnName("reference_id");
        builder.Property(sm => sm.Notes).HasColumnName("notes");

        builder.HasIndex(sm => sm.StockId).HasDatabaseName("idx_stock_movements_stock_id");
        builder.HasIndex(sm => sm.ProductId).HasDatabaseName("idx_stock_movements_product_id");
        builder.HasIndex(sm => sm.WarehouseId).HasDatabaseName("idx_stock_movements_warehouse_id");
        builder.HasIndex(sm => sm.MovementType).HasDatabaseName("idx_stock_movements_type");
        builder.HasIndex(sm => sm.CreatedAt).HasDatabaseName("idx_stock_movements_created_at");
        builder.HasIndex(sm => new { sm.ReferenceType, sm.ReferenceId })
            .HasDatabaseName("idx_stock_movements_reference")
            .HasFilter("\"reference_type\" IS NOT NULL AND \"reference_id\" IS NOT NULL");

        builder.HasOne<Stock>()
            .WithMany()
            .HasForeignKey(sm => sm.StockId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
