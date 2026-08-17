using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PosService.Domain.Common;
using SharedKernel;

namespace PosService.Infrastructure.Persistence.Configurations;

public abstract class BaseEntityConfiguration<TEntity, TId>(string tableName) : IEntityTypeConfiguration<TEntity>
    where TEntity : class, IHasId<TId>
    where TId : struct
{
    public virtual void Configure(EntityTypeBuilder<TEntity> builder)
    {
        builder.ToTable(tableName, Schema.Pos);
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName("id").ValueGeneratedNever();

        if (typeof(TEntity) is { IsClass: true } && typeof(IAuditableEntity).IsAssignableFrom(typeof(TEntity)))
        {
            builder.Property<DateTime>("CreatedAt").HasColumnName("created_at").HasDefaultValueSql("NOW()").IsRequired();
            builder.Property<Guid?>("CreatedBy").HasColumnName("created_by");
            builder.Property<DateTime?>("UpdatedAt").HasColumnName("updated_at");
            builder.Property<Guid?>("UpdatedBy").HasColumnName("updated_by");
        }

        if (typeof(TEntity) is { IsClass: true } && typeof(ISoftDeletable).IsAssignableFrom(typeof(TEntity)))
        {
            builder.Property<bool>("IsDeleted").HasColumnName("is_deleted").HasDefaultValue(false);
            builder.Property<DateTime?>("DeletedAt").HasColumnName("deleted_at");
            builder.Property<Guid?>("DeletedBy").HasColumnName("deleted_by");
        }

        if (typeof(TEntity) is { IsClass: true } && typeof(ITenantEntity).IsAssignableFrom(typeof(TEntity)))
        {
            builder.Property<Guid?>("TenantId").HasColumnName("tenant_id");
        }
    }
}

public static class Schema
{
    public const string Pos = "pos";
}
