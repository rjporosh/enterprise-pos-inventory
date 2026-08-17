using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PosService.Domain.Registers;

namespace PosService.Infrastructure.Persistence.Configurations;

public class CashRegisterConfiguration : BaseEntityConfiguration<CashRegister, Guid>
{
    public CashRegisterConfiguration() : base("cash_registers") { }

    public override void Configure(EntityTypeBuilder<CashRegister> builder)
    {
        base.Configure(builder);

        builder.Property(r => r.Name).HasColumnName("name").HasMaxLength(200).IsRequired();
        builder.Property(r => r.Code).HasColumnName("code").HasMaxLength(50).IsRequired();
        builder.Property(r => r.StoreId).HasColumnName("store_id").IsRequired();
        builder.Property(r => r.IsActive).HasColumnName("is_active").HasDefaultValue(true);

        builder.HasIndex(r => r.Code).HasDatabaseName("idx_cash_registers_code").IsUnique();
        builder.HasIndex(r => r.StoreId).HasDatabaseName("idx_cash_registers_store_id");

        builder.HasOne(r => r.Store)
            .WithMany()
            .HasForeignKey(r => r.StoreId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
