using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PosService.Domain.Customers;

namespace PosService.Infrastructure.Persistence.Configurations;

public class CustomerConfiguration : BaseEntityConfiguration<Customer, Guid>
{
    public CustomerConfiguration() : base("customers") { }

    public override void Configure(EntityTypeBuilder<Customer> builder)
    {
        base.Configure(builder);

        builder.Property(c => c.FullName).HasColumnName("full_name").HasMaxLength(200).IsRequired();
        builder.Property(c => c.Email).HasColumnName("email").HasMaxLength(200);
        builder.Property(c => c.Phone).HasColumnName("phone").HasMaxLength(50);
        builder.Property(c => c.Address).HasColumnName("address").HasMaxLength(500);
        builder.Property(c => c.IsActive).HasColumnName("is_active").HasDefaultValue(true);

        builder.HasIndex(c => c.Email).HasDatabaseName("idx_customers_email");
        builder.HasIndex(c => c.Phone).HasDatabaseName("idx_customers_phone");
    }
}
