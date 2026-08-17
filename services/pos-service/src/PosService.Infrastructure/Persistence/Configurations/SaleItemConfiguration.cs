using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PosService.Domain.Sales;

namespace PosService.Infrastructure.Persistence.Configurations;

public class SaleItemConfiguration : BaseEntityConfiguration<SaleItem, Guid>
{
    public SaleItemConfiguration() : base("sale_items") { }

    public override void Configure(EntityTypeBuilder<SaleItem> builder)
    {
        base.Configure(builder);

        builder.Property(i => i.SaleId).HasColumnName("sale_id").IsRequired();
        builder.Property(i => i.ProductId).HasColumnName("product_id").IsRequired();
        builder.Property(i => i.ProductName).HasColumnName("product_name").HasMaxLength(300).IsRequired();
        builder.Property(i => i.Sku).HasColumnName("sku").HasMaxLength(100).IsRequired();
        builder.Property(i => i.UnitPrice).HasColumnName("unit_price").HasPrecision(18, 2).IsRequired();
        builder.Property(i => i.Quantity).HasColumnName("quantity").IsRequired();
        builder.Property(i => i.DiscountAmount).HasColumnName("discount_amount").HasPrecision(18, 2).IsRequired();
        builder.Property(i => i.TaxAmount).HasColumnName("tax_amount").HasPrecision(18, 2).IsRequired();
        builder.Property(i => i.LineTotal).HasColumnName("line_total").HasPrecision(18, 2).IsRequired();

        builder.HasIndex(i => i.SaleId).HasDatabaseName("idx_sale_items_sale_id");
        builder.HasIndex(i => i.ProductId).HasDatabaseName("idx_sale_items_product_id");
    }
}
