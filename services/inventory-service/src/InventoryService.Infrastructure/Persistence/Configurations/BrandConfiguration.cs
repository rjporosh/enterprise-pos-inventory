using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using InventoryService.Domain.Catalog;
using SharedKernel;

namespace InventoryService.Infrastructure.Persistence.Configurations;

public class BrandConfiguration : BaseEntityConfiguration<Brand, Guid>
{
    public BrandConfiguration() : base("brands") { }

    public override void Configure(EntityTypeBuilder<Brand> builder)
    {
        base.Configure(builder);

        builder.Property(b => b.Name).HasColumnName("name").HasMaxLength(200).IsRequired();
        builder.Property(b => b.Description).HasColumnName("description").HasMaxLength(500);
        builder.Property(b => b.Website).HasColumnName("website").HasMaxLength(500);
        builder.Property(b => b.IsActive).HasColumnName("is_active").HasDefaultValue(true);

        builder.HasIndex(b => b.Name).HasDatabaseName("idx_brands_name").IsUnique();
    }
}
