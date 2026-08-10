namespace SharedKernel;

public interface IAuditContext
{
    Guid? UserId { get; }
    string? CorrelationId { get; }
    DateTime Timestamp { get; }
}
