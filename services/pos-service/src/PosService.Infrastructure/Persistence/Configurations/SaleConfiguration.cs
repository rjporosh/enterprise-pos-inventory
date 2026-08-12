using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PosService.Domain.Sales;

namespace PosService.Infrastructure.Persistence.Configurations;

public class SaleConfiguration : BaseEntityConfiguration<Sale, Guid>
{
    public SaleConfiguration() : base("sales") { }

    public override void Configure(EntityTypeBuilder<Sale> builder)
    {
        base.Configure(builder);

        builder.Property(s => s.SaleNumber).HasColumnName("sale_number").HasMaxLength(50).IsRequired();
        builder.Property(s => s.StoreId).HasColumnName("store_id").IsRequired();
        builder.Property(s => s.RegisterId).HasColumnName("register_id").IsRequired();
        builder.Property(s => s.CashierId).HasColumnName("cashier_id").IsRequired();
        builder.Property(s => s.CashSessionId).HasColumnName("cash_session_id").IsRequired();
        builder.Property(s => s.CustomerId).HasColumnName("customer_id");
        builder.Property(s => s.SaleDate).HasColumnName("sale_date").IsRequired();
        builder.Property(s => s.Status).HasColumnName("status").HasMaxLength(20)
            .HasConversion<string>().IsRequired();
        builder.Property(s => s.SubtotalAmount).HasColumnName("subtotal_amount").HasPrecision(18, 2).IsRequired();
        builder.Property(s => s.DiscountAmount).HasColumnName("discount_amount").HasPrecision(18, 2).IsRequired();
        builder.Property(s => s.TaxAmount).HasColumnName("tax_amount").HasPrecision(18, 2).IsRequired();
        builder.Property(s => s.TotalAmount).HasColumnName("total_amount").HasPrecision(18, 2).IsRequired();
        builder.Property(s => s.PaidAmount).HasColumnName("paid_amount").HasPrecision(18, 2).IsRequired();
        builder.Property(s => s.ChangeAmount).HasColumnName("change_amount").HasPrecision(18, 2).IsRequired();
        builder.Property(s => s.VoidReason).HasColumnName("void_reason").HasMaxLength(500);
        builder.Property(s => s.Notes).HasColumnName("notes").HasMaxLength(1000);

        builder.HasIndex(s => s.SaleNumber).HasDatabaseName("idx_sales_sale_number").IsUnique();
        builder.HasIndex(s => s.StoreId).HasDatabaseName("idx_sales_store_id");
        builder.HasIndex(s => s.RegisterId).HasDatabaseName("idx_sales_register_id");
        builder.HasIndex(s => s.CashierId).HasDatabaseName("idx_sales_cashier_id");
        builder.HasIndex(s => s.CashSessionId).HasDatabaseName("idx_sales_cash_session_id");
        builder.HasIndex(s => s.CustomerId).HasDatabaseName("idx_sales_customer_id");
        builder.HasIndex(s => s.Status).HasDatabaseName("idx_sales_status");
        builder.HasIndex(s => s.SaleDate).HasDatabaseName("idx_sales_sale_date");

        builder.HasOne(s => s.Store)
            .WithMany()
            .HasForeignKey(s => s.StoreId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.Register)
            .WithMany()
            .HasForeignKey(s => s.RegisterId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.Cashier)
            .WithMany()
            .HasForeignKey(s => s.CashierId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.CashSession)
            .WithMany()
            .HasForeignKey(s => s.CashSessionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.Customer)
            .WithMany()
            .HasForeignKey(s => s.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(s => s.Items)
            .WithOne(i => i.Sale)
            .HasForeignKey(i => i.SaleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(s => s.Payments)
            .WithOne(p => p.Sale)
            .HasForeignKey(p => p.SaleId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
