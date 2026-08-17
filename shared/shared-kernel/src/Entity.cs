namespace SharedKernel;

public abstract class Entity<TId> : IHasId<TId> where TId : struct
{
    public TId Id { get; protected set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public Guid? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public Guid? UpdatedBy { get; set; }

    protected Entity() { }

    protected Entity(TId id)
    {
        Id = id;
    }

    public static bool operator ==(Entity<TId>? first, Entity<TId>? second)
    {
        if (first is null && second is null) return true;
        if (first is null || second is null) return false;
        return EqualityComparer<TId>.Default.Equals(first.Id, second.Id);
    }

    public static bool operator !=(Entity<TId>? first, Entity<TId>? second) => !(first == second);

    public override bool Equals(object? obj) => obj is Entity<TId> entity && EqualityComparer<TId>.Default.Equals(Id, entity.Id);

    public override int GetHashCode() => HashCode.Combine(Id);

    public override string ToString() => $"{GetType().Name} [Id={Id}]";
}

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
    protected BaseEntity() : base() { Id = Guid.NewGuid(); }
}
