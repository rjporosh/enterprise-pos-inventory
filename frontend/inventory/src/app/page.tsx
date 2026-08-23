"use client";

import { useEffect, useState } from "react";
import Link from "next/link";
import { PageHeader, Card, Button, ErrorState } from "@/components/ui";
import { productsApi } from "@/lib/api/products";
import { stockApi } from "@/lib/api/stock";
import { ApiError, NetworkError } from "@/lib/api/client";

interface DashboardCounts {
  totalProducts: number;
  lowStockLines: number;
  outOfStockLines: number;
}

export default function DashboardPage() {
  const [counts, setCounts] = useState<DashboardCounts | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [loading, setLoading] = useState(true);

  async function load() {
    setLoading(true);
    setError(null);
    try {
      const [products, lowStock, outOfStock] = await Promise.all([
        productsApi.list({ pageNumber: 1, pageSize: 1 }),
        stockApi.list({ pageNumber: 1, pageSize: 1, lowStock: true }),
        stockApi.list({ pageNumber: 1, pageSize: 1, outOfStock: true }),
      ]);
      setCounts({
        totalProducts: products.totalCount,
        lowStockLines: lowStock.totalCount,
        outOfStockLines: outOfStock.totalCount,
      });
    } catch (err) {
      if (err instanceof ApiError || err instanceof NetworkError) setError(err.message);
      else setError("Could not load dashboard data.");
    } finally {
      setLoading(false);
    }
  }

  useEffect(() => {
    load();
  }, []);

  return (
    <>
      <PageHeader
        title="Dashboard"
        subtitle="Live counts from the inventory service. Sales and revenue metrics require the POS reporting API and are shown in the POS app."
      />

      {error && <ErrorState message={error} onRetry={load} />}

      {!error && (
        <div className="stat-grid">
          <Card className="stat-card">
            <div className="stat-card-label">Active &amp; inactive products</div>
            <div className="stat-card-value">{loading ? "—" : counts?.totalProducts}</div>
          </Card>
          <Card className="stat-card">
            <div className="stat-card-label">Low stock lines</div>
            <div className="stat-card-value" style={{ color: (counts?.lowStockLines ?? 0) > 0 ? "var(--color-warning)" : undefined }}>
              {loading ? "—" : counts?.lowStockLines}
            </div>
          </Card>
          <Card className="stat-card">
            <div className="stat-card-label">Out of stock lines</div>
            <div className="stat-card-value" style={{ color: (counts?.outOfStockLines ?? 0) > 0 ? "var(--color-danger)" : undefined }}>
              {loading ? "—" : counts?.outOfStockLines}
            </div>
          </Card>
        </div>
      )}

      <div style={{ display: "grid", gridTemplateColumns: "1fr 1fr", gap: 16 }}>
        <Card>
          <h2 style={{ marginTop: 0, fontSize: 15 }}>Products</h2>
          <p style={{ color: "var(--color-text-muted)", fontSize: 13.5 }}>
            Create and manage the product catalog: pricing, SKU, barcode and reorder levels.
          </p>
          <Link href="/products">
            <Button variant="secondary" size="sm">
              Go to products
            </Button>
          </Link>
        </Card>
        <Card>
          <h2 style={{ marginTop: 0, fontSize: 15 }}>Stock</h2>
          <p style={{ color: "var(--color-text-muted)", fontSize: 13.5 }}>
            Receive, issue, adjust and transfer stock between warehouses.
          </p>
          <Link href="/stock">
            <Button variant="secondary" size="sm">
              Go to stock
            </Button>
          </Link>
        </Card>
      </div>

      <Card className="card-padded" style={{ marginTop: 16, background: "var(--color-surface-alt)" }}>
        <strong style={{ fontSize: 13 }}>Coming in a future version</strong>
        <p style={{ margin: "6px 0 0", color: "var(--color-text-muted)", fontSize: 13 }}>
          Revenue, sales trends and cashier performance will appear here once the backend exposes a
          consolidated dashboard/reporting endpoint. See <code>docs/API-GAPS.md</code>.
        </p>
      </Card>
    </>
  );
}
