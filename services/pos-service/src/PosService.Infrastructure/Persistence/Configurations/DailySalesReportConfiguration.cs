using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PosService.Domain.Reporting;

namespace PosService.Infrastructure.Persistence.Configurations;

public class DailySalesReportConfiguration : BaseEntityConfiguration<DailySalesReport, Guid>
{
    public DailySalesReportConfiguration() : base("daily_sales_reports") { }

    public override void Configure(EntityTypeBuilder<DailySalesReport> builder)
    {
        base.Configure(builder);

        builder.Property(r => r.StoreId).HasColumnName("store_id").IsRequired();
        builder.Property(r => r.ReportDate).HasColumnName("report_date").HasColumnType("date").IsRequired();

        builder.Property(r => r.TotalSalesCount).HasColumnName("total_sales_count").IsRequired();
        builder.Property(r => r.VoidedSalesCount).HasColumnName("voided_sales_count").IsRequired();
        builder.Property(r => r.GrossRevenue).HasColumnName("gross_revenue").HasPrecision(18, 2).IsRequired();
        builder.Property(r => r.TotalDiscount).HasColumnName("total_discount").HasPrecision(18, 2).IsRequired();
        builder.Property(r => r.TotalTax).HasColumnName("total_tax").HasPrecision(18, 2).IsRequired();
        builder.Property(r => r.NetRevenue).HasColumnName("net_revenue").HasPrecision(18, 2).IsRequired();

        builder.Property(r => r.CashCollected).HasColumnName("cash_collected").HasPrecision(18, 2).IsRequired();
        builder.Property(r => r.CardCollected).HasColumnName("card_collected").HasPrecision(18, 2).IsRequired();
        builder.Property(r => r.MobileMoneyCollected).HasColumnName("mobile_money_collected").HasPrecision(18, 2).IsRequired();
        builder.Property(r => r.OtherCollected).HasColumnName("other_collected").HasPrecision(18, 2).IsRequired();

        builder.Property(r => r.TopProductsJson).HasColumnName("top_products_json").HasColumnType("text").IsRequired();
        builder.Property(r => r.CashSessionSummaryJson).HasColumnName("cash_session_summary_json").HasColumnType("text").IsRequired();
        builder.Property(r => r.GeneratedAtUtc).HasColumnName("generated_at_utc").IsRequired();

        builder.HasIndex(r => new { r.StoreId, r.ReportDate }).HasDatabaseName("idx_daily_sales_reports_store_date").IsUnique();
    }
}
