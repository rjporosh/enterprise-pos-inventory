"use client";

import { useEffect, useState } from "react";
import Link from "next/link";
import { useAppDispatch, useAppSelector } from "@/lib/store/hooks";
import { stockListRequested } from "@/features/stock/slice";
import {
  Badge,
  Button,
  DataToolbar,
  EmptyState,
  ErrorState,
  PageHeader,
  Pagination,
  Select,
  TableSkeleton,
} from "@/components/ui";

export default function StockPage() {
  const dispatch = useAppDispatch();
  const { status, error, result, params } = useAppSelector((s) => s.stock.list);
  const [filter, setFilter] = useState<"" | "low" | "out">("");

  function load(overrides: Partial<typeof params> = {}) {
    dispatch(
      stockListRequested({
        pageNumber: 1,
        pageSize: 20,
        ...params,
        ...overrides,
      })
    );
  }

  useEffect(() => {
    load({ pageNumber: 1 });
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  return (
    <>
      <PageHeader
        title="Stock"
        subtitle="Current stock levels across warehouses."
        actions={
          <>
            <Link href="/stock/in">
              <Button variant="secondary" size="sm">
                Stock in
              </Button>
            </Link>
            <Link href="/stock/out">
              <Button variant="secondary" size="sm">
                Stock out
              </Button>
            </Link>
            <Link href="/stock/adjustment">
              <Button variant="secondary" size="sm">
                Adjust
              </Button>
            </Link>
            <Link href="/stock/transfer">
              <Button size="sm">Transfer</Button>
            </Link>
          </>
        }
      />

      <DataToolbar>
        <Select
          aria-label="Filter"
          value={filter}
          onChange={(e) => {
            const v = e.target.value as typeof filter;
            setFilter(v);
            load({
              lowStock: v === "low" ? true : undefined,
              outOfStock: v === "out" ? true : undefined,
              pageNumber: 1,
            });
          }}
          style={{ maxWidth: 200 }}
        >
          <option value="">All stock</option>
          <option value="low">Low stock only</option>
          <option value="out">Out of stock only</option>
        </Select>
      </DataToolbar>

      {status === "failed" && <ErrorState message={error ?? "Failed to load stock."} onRetry={() => load()} />}

      {status !== "failed" && (
        <div className="table-wrap">
          {status === "loading" && !result ? (
            <TableSkeleton cols={6} />
          ) : result && result.items.length === 0 ? (
            <EmptyState
              title="No stock records"
              description="Create a stock record with a Stock In receipt, or adjust an existing product's stock."
              action={
                <Link href="/stock/in">
                  <Button size="sm">Stock in</Button>
                </Link>
              }
            />
          ) : (
            <>
              <table className="table">
                <thead>
                  <tr>
                    <th>Product</th>
                    <th>Warehouse</th>
                    <th>On hand</th>
                    <th>Reserved</th>
                    <th>Available</th>
                    <th>Status</th>
                  </tr>
                </thead>
                <tbody>
                  {result?.items.map((s) => (
                    <tr key={s.id}>
                      <td>
                        <div style={{ fontWeight: 600 }}>{s.productName}</div>
                        <code style={{ fontSize: 12, color: "var(--color-text-faint)" }}>{s.productSku}</code>
                      </td>
                      <td>
                        {s.warehouseName} <span style={{ color: "var(--color-text-faint)" }}>({s.warehouseCode})</span>
                      </td>
                      <td>{s.quantityOnHand}</td>
                      <td>{s.quantityReserved}</td>
                      <td>{s.availableQuantity}</td>
                      <td>
                        {s.availableQuantity <= 0 ? (
                          <Badge tone="danger">Out of stock</Badge>
                        ) : s.isLowStock ? (
                          <Badge tone="warning">Low stock</Badge>
                        ) : (
                          <Badge tone="success">In stock</Badge>
                        )}
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
              <Pagination
                pageNumber={result?.pageNumber ?? 1}
                pageSize={result?.pageSize ?? 20}
                totalCount={result?.totalCount ?? 0}
                onPageChange={(page) => load({ pageNumber: page })}
              />
            </>
          )}
        </div>
      )}
    </>
  );
}
