using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using InventoryService.Domain.Catalog;
using SharedKernel;

namespace InventoryService.Infrastructure.Persistence.Configurations;

public class UnitConfiguration : BaseEntityConfiguration<Unit, Guid>
{
    public UnitConfiguration() : base("units") { }

    public override void Configure(EntityTypeBuilder<Unit> builder)
    {
        base.Configure(builder);

        builder.Property(u => u.Name).HasColumnName("name").HasMaxLength(100).IsRequired();
        builder.Property(u => u.Symbol).HasColumnName("symbol").HasMaxLength(20).IsRequired();
        builder.Property(u => u.Description).HasColumnName("description").HasMaxLength(500);
        builder.Property(u => u.IsActive).HasColumnName("is_active").HasDefaultValue(true);

        builder.HasIndex(u => u.Symbol).HasDatabaseName("idx_units_symbol").IsUnique();
    }
}
