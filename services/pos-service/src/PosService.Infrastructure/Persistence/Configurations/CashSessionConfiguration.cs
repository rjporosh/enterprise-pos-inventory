using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PosService.Domain.Registers;

namespace PosService.Infrastructure.Persistence.Configurations;

public class CashSessionConfiguration : BaseEntityConfiguration<CashSession, Guid>
{
    public CashSessionConfiguration() : base("cash_sessions") { }

    public override void Configure(EntityTypeBuilder<CashSession> builder)
    {
        base.Configure(builder);

        builder.Property(cs => cs.RegisterId).HasColumnName("register_id").IsRequired();
        builder.Property(cs => cs.CashierId).HasColumnName("cashier_id").IsRequired();
        builder.Property(cs => cs.OpenedAt).HasColumnName("opened_at").IsRequired();
        builder.Property(cs => cs.ClosedAt).HasColumnName("closed_at");
        builder.Property(cs => cs.OpeningBalance).HasColumnName("opening_balance").HasPrecision(18, 2).IsRequired();
        builder.Property(cs => cs.ClosingBalance).HasColumnName("closing_balance").HasPrecision(18, 2);
        builder.Property(cs => cs.ExpectedBalance).HasColumnName("expected_balance").HasPrecision(18, 2);
        builder.Property(cs => cs.Variance).HasColumnName("variance").HasPrecision(18, 2);
        builder.Property(cs => cs.Status).HasColumnName("status").HasMaxLength(20)
            .HasConversion<string>().IsRequired();
        builder.Property(cs => cs.Notes).HasColumnName("notes").HasMaxLength(1000);

        builder.HasIndex(cs => cs.RegisterId).HasDatabaseName("idx_cash_sessions_register_id");
        builder.HasIndex(cs => cs.CashierId).HasDatabaseName("idx_cash_sessions_cashier_id");
        builder.HasIndex(cs => cs.Status).HasDatabaseName("idx_cash_sessions_status");

        builder.HasOne(cs => cs.Register)
            .WithMany()
            .HasForeignKey(cs => cs.RegisterId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(cs => cs.Cashier)
            .WithMany()
            .HasForeignKey(cs => cs.CashierId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
