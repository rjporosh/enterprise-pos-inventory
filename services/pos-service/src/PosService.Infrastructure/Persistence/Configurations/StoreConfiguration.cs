using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PosService.Domain.Stores;

namespace PosService.Infrastructure.Persistence.Configurations;

public class StoreConfiguration : BaseEntityConfiguration<Store, Guid>
{
    public StoreConfiguration() : base("stores") { }

    public override void Configure(EntityTypeBuilder<Store> builder)
    {
        base.Configure(builder);

        builder.Property(s => s.Name).HasColumnName("name").HasMaxLength(200).IsRequired();
        builder.Property(s => s.Code).HasColumnName("code").HasMaxLength(50).IsRequired();
        builder.Property(s => s.Address).HasColumnName("address").HasMaxLength(500);
        builder.Property(s => s.City).HasColumnName("city").HasMaxLength(100);
        builder.Property(s => s.Country).HasColumnName("country").HasMaxLength(100);
        builder.Property(s => s.Phone).HasColumnName("phone").HasMaxLength(50);
        builder.Property(s => s.Email).HasColumnName("email").HasMaxLength(200);
        builder.Property(s => s.Currency).HasColumnName("currency").HasMaxLength(10).HasDefaultValue("USD");
        builder.Property(s => s.IsActive).HasColumnName("is_active").HasDefaultValue(true);

        builder.HasIndex(s => s.Code).HasDatabaseName("idx_stores_code").IsUnique();
    }
}
