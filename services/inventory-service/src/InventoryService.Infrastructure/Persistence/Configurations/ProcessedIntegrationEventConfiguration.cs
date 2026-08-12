using InventoryService.Domain.Integration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InventoryService.Infrastructure.Persistence.Configurations;

public class ProcessedIntegrationEventConfiguration : IEntityTypeConfiguration<ProcessedIntegrationEvent>
{
    public void Configure(EntityTypeBuilder<ProcessedIntegrationEvent> builder)
    {
        builder.ToTable("processed_integration_events", "inventory");

        builder.HasKey(e => e.EventId);

        builder.Property(e => e.EventId).HasColumnName("event_id").ValueGeneratedNever();
        builder.Property(e => e.EventType).HasColumnName("event_type").HasMaxLength(200).IsRequired();
        builder.Property(e => e.ProcessedAtUtc).HasColumnName("processed_at_utc").IsRequired();

        builder.HasIndex(e => e.ProcessedAtUtc).HasDatabaseName("idx_processed_integration_events_processed_at");
    }
}
