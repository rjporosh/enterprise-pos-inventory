using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using InventoryService.Domain.Warehouses;
using SharedKernel;

namespace InventoryService.Infrastructure.Persistence.Configurations;

public class WarehouseConfiguration : BaseEntityConfiguration<Warehouse, Guid>
{
    public WarehouseConfiguration() : base("warehouses") { }

    public override void Configure(EntityTypeBuilder<Warehouse> builder)
    {
        base.Configure(builder);

        builder.Property(w => w.Name).HasColumnName("name").HasMaxLength(200).IsRequired();
        builder.Property(w => w.Code).HasColumnName("code").HasMaxLength(50);
        builder.Property(w => w.Address).HasColumnName("address").HasMaxLength(500);
        builder.Property(w => w.City).HasColumnName("city").HasMaxLength(100);
        builder.Property(w => w.Country).HasColumnName("country").HasMaxLength(100);
        builder.Property(w => w.ContactName).HasColumnName("contact_name").HasMaxLength(200);
        builder.Property(w => w.Phone).HasColumnName("phone").HasMaxLength(50);
        builder.Property(w => w.IsDefault).HasColumnName("is_default").HasDefaultValue(false);
        builder.Property(w => w.IsActive).HasColumnName("is_active").HasDefaultValue(true);

        builder.HasIndex(w => w.Code).HasDatabaseName("idx_warehouses_code").IsUnique();
        builder.HasIndex(w => w.IsDefault).HasDatabaseName("idx_warehouses_default");
    }
}
