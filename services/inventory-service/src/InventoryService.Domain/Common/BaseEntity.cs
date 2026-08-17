using SharedKernel;

namespace InventoryService.Domain.Common;

public abstract class BaseEntity<TId> : Entity<TId>, IAuditableEntity, ISoftDeletable, ITenantEntity, IAggregateRoot
    where TId : struct
{
    public Guid? TenantId { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public Guid? DeletedBy { get; set; }
    DateTime IAuditableEntity.CreatedAt { get => base.CreatedAt; set => base.CreatedAt = value; }
    Guid? IAuditableEntity.CreatedBy { get => base.CreatedBy; set => base.CreatedBy = value; }
    DateTime? IAuditableEntity.UpdatedAt { get => base.UpdatedAt; set => base.UpdatedAt = value; }
    Guid? IAuditableEntity.UpdatedBy { get => base.UpdatedBy; set => base.UpdatedBy = value; }
}

public abstract class BaseEntity : BaseEntity<Guid>, IAggregateRoot
{
    protected BaseEntity() : base()
    {
        Id = Guid.NewGuid();
    }
}
