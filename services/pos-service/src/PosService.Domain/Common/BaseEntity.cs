using SharedKernel;

namespace PosService.Domain.Common;

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
    // Without this, every PosService entity (Store, CashRegister, Sale, CashSession, Cashier,
    // Customer, Payment, ...) gets inserted with Id = Guid.Empty: Entity<TId>'s parameterless
    // constructor never assigns Id, and unlike InventoryService.Domain.Common.BaseEntity (and
    // SharedKernel.BaseEntity), this constructor never generated one either. A second insert of
    // any entity type then fails on the primary key uniqueness constraint. Found 2026-08-31 while
    // adding Store/Register CRUD — the very first successful "create" surfaced Guid.Empty in the
    // response and in the database row.
    protected BaseEntity() : base()
    {
        Id = Guid.NewGuid();
    }
}
