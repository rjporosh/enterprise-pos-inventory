using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PosService.Domain.Cashiers;

namespace PosService.Infrastructure.Persistence.Configurations;

public class CashierConfiguration : BaseEntityConfiguration<Cashier, Guid>
{
    public CashierConfiguration() : base("cashiers") { }

    public override void Configure(EntityTypeBuilder<Cashier> builder)
    {
        base.Configure(builder);

        builder.Property(c => c.FullName).HasColumnName("full_name").HasMaxLength(200).IsRequired();
        builder.Property(c => c.Username).HasColumnName("username").HasMaxLength(100).IsRequired();
        builder.Property(c => c.Email).HasColumnName("email").HasMaxLength(200);
        builder.Property(c => c.Phone).HasColumnName("phone").HasMaxLength(50);
        builder.Property(c => c.StoreId).HasColumnName("store_id").IsRequired();
        builder.Property(c => c.IsActive).HasColumnName("is_active").HasDefaultValue(true);

        builder.HasIndex(c => c.Username).HasDatabaseName("idx_cashiers_username").IsUnique();
        builder.HasIndex(c => c.StoreId).HasDatabaseName("idx_cashiers_store_id");

        builder.HasOne(c => c.Store)
            .WithMany()
            .HasForeignKey(c => c.StoreId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
