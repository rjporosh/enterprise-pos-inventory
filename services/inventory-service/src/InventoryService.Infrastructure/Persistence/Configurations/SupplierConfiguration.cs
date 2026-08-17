using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using InventoryService.Domain.Suppliers;
using SharedKernel;

namespace InventoryService.Infrastructure.Persistence.Configurations;

public class SupplierConfiguration : BaseEntityConfiguration<Supplier, Guid>
{
    public SupplierConfiguration() : base("suppliers") { }

    public override void Configure(EntityTypeBuilder<Supplier> builder)
    {
        base.Configure(builder);

        builder.Property(s => s.Name).HasColumnName("name").HasMaxLength(200).IsRequired();
        builder.Property(s => s.ContactName).HasColumnName("contact_name").HasMaxLength(200);
        builder.Property(s => s.Email).HasColumnName("email").HasMaxLength(200);
        builder.Property(s => s.Phone).HasColumnName("phone").HasMaxLength(50);
        builder.Property(s => s.Address).HasColumnName("address").HasMaxLength(500);
        builder.Property(s => s.City).HasColumnName("city").HasMaxLength(100);
        builder.Property(s => s.Country).HasColumnName("country").HasMaxLength(100);
        builder.Property(s => s.IsActive).HasColumnName("is_active").HasDefaultValue(true);

        builder.HasIndex(s => s.Name).HasDatabaseName("idx_suppliers_name");
    }
}
