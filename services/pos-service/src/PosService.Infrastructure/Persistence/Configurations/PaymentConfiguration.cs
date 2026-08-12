using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PosService.Domain.Sales;

namespace PosService.Infrastructure.Persistence.Configurations;

public class PaymentConfiguration : BaseEntityConfiguration<Payment, Guid>
{
    public PaymentConfiguration() : base("payments") { }

    public override void Configure(EntityTypeBuilder<Payment> builder)
    {
        base.Configure(builder);

        builder.Property(p => p.SaleId).HasColumnName("sale_id").IsRequired();
        builder.Property(p => p.Method).HasColumnName("method").HasMaxLength(20)
            .HasConversion<string>().IsRequired();
        builder.Property(p => p.Amount).HasColumnName("amount").HasPrecision(18, 2).IsRequired();
        builder.Property(p => p.ReferenceNumber).HasColumnName("reference_number").HasMaxLength(200);
        builder.Property(p => p.PaidAt).HasColumnName("paid_at").IsRequired();

        builder.HasIndex(p => p.SaleId).HasDatabaseName("idx_payments_sale_id");
        builder.HasIndex(p => p.Method).HasDatabaseName("idx_payments_method");
    }
}
