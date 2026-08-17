using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Reflection;
using SharedKernel;

namespace PosService.Infrastructure.Persistence;

public abstract class BaseDbContext(DbContextOptions options) : DbContext(options)
{
    private Guid? _tenantId;

    public void SetTenantId(Guid? tenantId) => _tenantId = tenantId;

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var auditEntries = OnBeforeSaving();
        var result = base.SaveChangesAsync(cancellationToken);
        OnAfterSaving(auditEntries);
        return result;
    }

    public override int SaveChanges()
    {
        var auditEntries = OnBeforeSaving();
        var result = base.SaveChanges();
        OnAfterSaving(auditEntries);
        return result;
    }

    private List<EntityEntry> OnBeforeSaving()
    {
        var entries = ChangeTracker.Entries()
            .Where(e => e.Entity is IAuditableEntity)
            .ToList();

        var now = DateTime.UtcNow;
        var userId = _tenantId;

        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Added)
            {
                if (entry.Entity is IAuditableEntity auditable)
                {
                    typeof(IAuditableEntity).GetProperty("CreatedAt")?.SetValue(entry.Entity, now);
                    typeof(IAuditableEntity).GetProperty("CreatedBy")?.SetValue(entry.Entity, userId);
                }
            }

            if (entry.State == EntityState.Modified)
            {
                if (entry.Entity is IAuditableEntity auditable)
                {
                    typeof(IAuditableEntity).GetProperty("UpdatedAt")?.SetValue(entry.Entity, now);
                    typeof(IAuditableEntity).GetProperty("UpdatedBy")?.SetValue(entry.Entity, userId);
                }

                if (entry.Entity is ISoftDeletable soft)
                {
                    typeof(ISoftDeletable).GetProperty("IsDeleted")?.SetValue(entry.Entity, false);
                }
            }
        }

        return entries;
    }

    private void OnAfterSaving(List<EntityEntry> entries) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(ISoftDeletable).IsAssignableFrom(entityType.ClrType))
            {
                var method = typeof(BaseDbContext).GetMethod(nameof(ApplySoftDeleteFilter), BindingFlags.Instance | BindingFlags.NonPublic);
                method = method!.MakeGenericMethod(entityType.ClrType);
                method.Invoke(this, new object[] { modelBuilder });
            }
        }
    }

    private void ApplySoftDeleteFilter<TEntity>(ModelBuilder builder) where TEntity : class, ISoftDeletable
    {
        builder.Entity<TEntity>().HasQueryFilter(e => !e.IsDeleted);
    }
}
