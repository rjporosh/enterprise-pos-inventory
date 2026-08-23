"use client";

import { useEffect, useState } from "react";
import { useAppSelector } from "@/lib/store/hooks";
import { DailySalesReport, reportsApi, parseTopProducts } from "@/lib/api/cashSessionsAndReports";
import { PaymentMethodType, PAYMENT_METHOD_LABELS } from "@/lib/api/sales";
import { ApiError, NetworkError } from "@/lib/api/client";
import { Card, EmptyState, ErrorState, Field, Input, PageHeader } from "@/components/ui";

function yesterday(): string {
  const d = new Date();
  d.setDate(d.getDate() - 1);
  return d.toISOString().slice(0, 10);
}

export default function ReportsPage() {
  const config = useAppSelector((s) => s.session.config);
  const [date, setDate] = useState(yesterday());
  const [report, setReport] = useState<DailySalesReport | null>(null);
  const [status, setStatus] = useState<"idle" | "loading" | "found" | "not-found" | "error">("idle");
  const [error, setError] = useState<string | null>(null);

  async function load() {
    if (!config) return;
    setStatus("loading");
    setError(null);
    try {
      const result = await reportsApi.getDailySales(config.storeId, date);
      setReport(result);
      setStatus("found");
    } catch (err) {
      if (err instanceof ApiError && err.isNotFound) {
        setReport(null);
        setStatus("not-found");
      } else if (err instanceof ApiError || err instanceof NetworkError) {
        setError(err.message);
        setStatus("error");
      } else {
        setError("Could not load the report.");
        setStatus("error");
      }
    }
  }

  useEffect(() => {
    load();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [config, date]);

  if (!config) {
    return <EmptyState title="Terminal not set up" description="Set up the terminal first to view reports for this store." />;
  }

  const topProducts = report ? parseTopProducts(report.topProductsJson) : [];

  return (
    <>
      <PageHeader title="Daily sales report" subtitle="Generated overnight by the backend — see the note below." />

      <Card style={{ marginBottom: 16, background: "var(--color-surface-alt)" }}>
        <strong style={{ fontSize: 13 }}>How this report works</strong>
        <p style={{ margin: "6px 0 0", fontSize: 13, color: "var(--color-text-muted)" }}>
          The backend generates each day&apos;s report overnight (around UTC midnight), not on demand.
          That means <strong>today&apos;s</strong> report generally won&apos;t exist yet — pick yesterday
          or an earlier date (up to 7 days back). This is a backend scheduling constraint, documented in
          docs/API-GAPS.md, not a bug in this page.
        </p>
      </Card>

      <div style={{ maxWidth: 220, marginBottom: 20 }}>
        <Field label="Report date" htmlFor="reportDate">
          <Input id="reportDate" type="date" value={date} onChange={(e) => setDate(e.target.value)} />
        </Field>
      </div>

      {status === "loading" && <p style={{ color: "var(--color-text-muted)" }}>Loading…</p>}

      {status === "error" && <ErrorState message={error ?? "Failed to load report."} onRetry={load} />}

      {status === "not-found" && (
        <EmptyState
          title="No report for this date yet"
          description="Either the day hasn't been processed by the overnight job yet, or it's outside the 7-day catch-up window."
        />
      )}

      {status === "found" && report && (
        <>
          <div className="stat-grid">
            <Card className="stat-card">
              <div className="stat-card-label">Sales</div>
              <div className="stat-card-value">{report.totalSalesCount}</div>
            </Card>
            <Card className="stat-card">
              <div className="stat-card-label">Net revenue</div>
              <div className="stat-card-value">{report.netRevenue.toFixed(2)}</div>
            </Card>
            <Card className="stat-card">
              <div className="stat-card-label">Voided sales</div>
              <div className="stat-card-value">{report.voidedSalesCount}</div>
            </Card>
          </div>

          <div style={{ display: "grid", gridTemplateColumns: "1fr 1fr", gap: 16 }}>
            <Card>
              <h2 style={{ marginTop: 0, fontSize: 14 }}>Revenue breakdown</h2>
              <ReportRow label="Gross revenue" value={report.grossRevenue} />
              <ReportRow label="Discounts" value={-report.totalDiscount} />
              <ReportRow label="Tax" value={report.totalTax} />
              <ReportRow label="Net revenue" value={report.netRevenue} bold />
            </Card>
            <Card>
              <h2 style={{ marginTop: 0, fontSize: 14 }}>Collected by method</h2>
              <ReportRow label={PAYMENT_METHOD_LABELS[PaymentMethodType.Cash]} value={report.cashCollected} />
              <ReportRow label={PAYMENT_METHOD_LABELS[PaymentMethodType.Card]} value={report.cardCollected} />
              <ReportRow label={PAYMENT_METHOD_LABELS[PaymentMethodType.MobileMoney]} value={report.mobileMoneyCollected} />
              <ReportRow label="Other / store credit" value={report.otherCollected} />
            </Card>
          </div>

          <Card style={{ marginTop: 16 }}>
            <h2 style={{ marginTop: 0, fontSize: 14 }}>Top products</h2>
            {topProducts.length === 0 ? (
              <p style={{ color: "var(--color-text-muted)", fontSize: 13 }}>No sales recorded for this date.</p>
            ) : (
              <table className="table">
                <thead>
                  <tr>
                    <th>Product</th>
                    <th>SKU</th>
                    <th>Qty sold</th>
                    <th>Revenue</th>
                  </tr>
                </thead>
                <tbody>
                  {topProducts.map((p) => (
                    <tr key={p.productId}>
                      <td>{p.productName}</td>
                      <td>
                        <code style={{ fontSize: 12 }}>{p.sku}</code>
                      </td>
                      <td>{p.quantitySold}</td>
                      <td>{p.revenue.toFixed(2)}</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            )}
          </Card>
        </>
      )}
    </>
  );
}

function ReportRow({ label, value, bold }: { label: string; value: number; bold?: boolean }) {
  return (
    <div style={{ display: "flex", justifyContent: "space-between", padding: "6px 0", fontSize: 13.5, fontWeight: bold ? 700 : 400 }}>
      <span>{label}</span>
      <span>{value.toFixed(2)}</span>
    </div>
  );
}
