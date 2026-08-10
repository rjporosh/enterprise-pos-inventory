using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using InventoryService.Domain.Products;
using SharedKernel;

namespace InventoryService.Infrastructure.Persistence.Configurations;

public class ProductConfiguration : BaseEntityConfiguration<Product, Guid>
{
    public ProductConfiguration() : base("products") { }

    public override void Configure(EntityTypeBuilder<Product> builder)
    {
        base.Configure(builder);

        builder.Property(p => p.Name).HasColumnName("name").HasMaxLength(300).IsRequired();
        builder.Property(p => p.Description).HasColumnName("description").HasMaxLength(1000);
        builder.Property(p => p.Sku).HasColumnName("sku").HasMaxLength(100).IsRequired();
        builder.Property(p => p.Barcode).HasColumnName("barcode").HasMaxLength(100);
        builder.Property(p => p.CategoryId).HasColumnName("category_id").IsRequired();
        builder.Property(p => p.BrandId).HasColumnName("brand_id").IsRequired();
        builder.Property(p => p.UnitId).HasColumnName("unit_id").IsRequired();
        builder.Property(p => p.SupplierId).HasColumnName("supplier_id");
        builder.Property(p => p.CostPrice).HasColumnName("cost_price").HasPrecision(18, 2).IsRequired();
        builder.Property(p => p.SellingPrice).HasColumnName("selling_price").HasPrecision(18, 2).IsRequired();
        builder.Property(p => p.DiscountPercent).HasColumnName("discount_percent").HasPrecision(5, 2);
        builder.Property(p => p.TaxPercent).HasColumnName("tax_percent").HasPrecision(5, 2);
        builder.Property(p => p.ReorderLevel).HasColumnName("reorder_level").HasDefaultValue(0);
        builder.Property(p => p.MaxStockLevel).HasColumnName("max_stock_level").HasDefaultValue(0);
        builder.Property(p => p.IsActive).HasColumnName("is_active").HasDefaultValue(true);
        builder.Property(p => p.TrackInventory).HasColumnName("track_inventory").HasDefaultValue(true);

        builder.HasIndex(p => p.Sku).HasDatabaseName("idx_products_sku").IsUnique();
        builder.HasIndex(p => p.Barcode).HasDatabaseName("idx_products_barcode").IsUnique();
        builder.HasIndex(p => p.CategoryId).HasDatabaseName("idx_products_category_id");
        builder.HasIndex(p => p.BrandId).HasDatabaseName("idx_products_brand_id");
        builder.HasIndex(p => p.UnitId).HasDatabaseName("idx_products_unit_id");
        builder.HasIndex(p => p.SupplierId).HasDatabaseName("idx_products_supplier_id");
        builder.HasIndex(p => p.IsActive).HasDatabaseName("idx_products_is_active");

        builder.HasOne(p => p.Category)
            .WithMany()
            .HasForeignKey(p => p.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.Brand)
            .WithMany()
            .HasForeignKey(p => p.BrandId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.Unit)
            .WithMany()
            .HasForeignKey(p => p.UnitId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.Supplier)
            .WithMany()
            .HasForeignKey(p => p.SupplierId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
