using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using InventoryService.Domain.Catalog;
using SharedKernel;

namespace InventoryService.Infrastructure.Persistence.Configurations;

public class CategoryConfiguration : BaseEntityConfiguration<Category, Guid>
{
    public CategoryConfiguration() : base("categories") { }

    public override void Configure(EntityTypeBuilder<Category> builder)
    {
        base.Configure(builder);

        builder.Property(c => c.Name).HasColumnName("name").HasMaxLength(200).IsRequired();
        builder.Property(c => c.Description).HasColumnName("description").HasMaxLength(500);
        builder.Property(c => c.ParentCategoryId).HasColumnName("parent_category_id");
        builder.Property(c => c.SortOrder).HasColumnName("sort_order").HasDefaultValue(0);
        builder.Property(c => c.IsActive).HasColumnName("is_active").HasDefaultValue(true);

        builder.HasIndex(c => c.Name).HasDatabaseName("idx_categories_name");
        builder.HasIndex(c => c.ParentCategoryId).HasDatabaseName("idx_categories_parent_id");
    }
}
