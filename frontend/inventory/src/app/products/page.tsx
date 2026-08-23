"use client";

import { useEffect, useState } from "react";
import Link from "next/link";
import { useRouter } from "next/navigation";
import { useAppDispatch, useAppSelector } from "@/lib/store/hooks";
import {
  productRemoveRequested,
  productRemoveReset,
  productsRequested,
} from "@/features/products/slice";
import {
  Badge,
  Button,
  ConfirmDialog,
  DataToolbar,
  EmptyState,
  ErrorState,
  PageHeader,
  Pagination,
  SearchInput,
  Select,
  TableSkeleton,
} from "@/components/ui";

export default function ProductsPage() {
  const dispatch = useAppDispatch();
  const router = useRouter();
  const { status, error, result, params } = useAppSelector((s) => s.products.list);
  const removeState = useAppSelector((s) => s.products.remove);

  const [searchTerm, setSearchTerm] = useState("");
  const [isActive, setIsActive] = useState<string>("");
  const [pendingDeleteId, setPendingDeleteId] = useState<string | null>(null);
  const [pendingDeleteName, setPendingDeleteName] = useState<string>("");

  function load(overrides: Partial<typeof params> = {}) {
    dispatch(
      productsRequested({
        pageNumber: 1,
        pageSize: 20,
        sortBy: "name",
        sortDescending: false,
        ...params,
        ...overrides,
      })
    );
  }

  useEffect(() => {
    load({ pageNumber: 1 });
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  useEffect(() => {
    const timer = setTimeout(() => {
      load({ searchTerm: searchTerm || undefined, pageNumber: 1 });
    }, 350);
    return () => clearTimeout(timer);
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [searchTerm]);

  useEffect(() => {
    if (removeState.status === "succeeded") {
      setPendingDeleteId(null);
      dispatch(productRemoveReset());
      load();
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [removeState.status]);

  return (
    <>
      <PageHeader
        title="Products"
        subtitle="Manage the catalog: pricing, SKU, barcode, and reorder thresholds."
        actions={
          <Link href="/products/new">
            <Button>+ New product</Button>
          </Link>
        }
      />

      <DataToolbar>
        <SearchInput
          placeholder="Search by name or SKU…"
          value={searchTerm}
          onChange={(e) => setSearchTerm(e.target.value)}
          aria-label="Search products"
        />
        <Select
          aria-label="Filter by status"
          value={isActive}
          onChange={(e) => {
            setIsActive(e.target.value);
            load({ isActive: e.target.value === "" ? undefined : e.target.value === "true", pageNumber: 1 });
          }}
          style={{ maxWidth: 160 }}
        >
          <option value="">All statuses</option>
          <option value="true">Active</option>
          <option value="false">Inactive</option>
        </Select>
      </DataToolbar>

      {status === "failed" && <ErrorState message={error ?? "Failed to load products."} onRetry={() => load()} />}

      {status !== "failed" && (
        <div className="table-wrap">
          {status === "loading" && !result ? (
            <TableSkeleton cols={6} />
          ) : result && result.items.length === 0 ? (
            <EmptyState
              title="No products found"
              description={searchTerm ? "Try a different search term." : "Get started by creating your first product."}
              action={
                !searchTerm ? (
                  <Link href="/products/new">
                    <Button size="sm">+ New product</Button>
                  </Link>
                ) : undefined
              }
            />
          ) : (
            <>
              <table className="table">
                <thead>
                  <tr>
                    <th>Name</th>
                    <th>SKU</th>
                    <th>Category</th>
                    <th>Brand</th>
                    <th>Price</th>
                    <th>Status</th>
                    <th aria-label="Actions" />
                  </tr>
                </thead>
                <tbody>
                  {result?.items.map((p) => (
                    <tr key={p.id}>
                      <td>
                        <Link href={`/products/${p.id}`} style={{ fontWeight: 600, color: "var(--color-primary)" }}>
                          {p.name}
                        </Link>
                        {p.reorderLevel > 0 && <div style={{ fontSize: 11.5, color: "var(--color-text-faint)" }}>Reorder at {p.reorderLevel}</div>}
                      </td>
                      <td>
                        <code style={{ fontSize: 12.5 }}>{p.sku}</code>
                      </td>
                      <td>{p.categoryName}</td>
                      <td>{p.brandName}</td>
                      <td>
                        {p.sellingPrice.toFixed(2)} <span style={{ color: "var(--color-text-faint)" }}>/{p.unitSymbol}</span>
                      </td>
                      <td>
                        <Badge tone={p.isActive ? "success" : "neutral"}>{p.isActive ? "Active" : "Inactive"}</Badge>
                      </td>
                      <td>
                        <div style={{ display: "flex", gap: 6, justifyContent: "flex-end" }}>
                          <Button size="sm" variant="ghost" onClick={() => router.push(`/products/${p.id}`)}>
                            Edit
                          </Button>
                          <Button
                            size="sm"
                            variant="ghost"
                            onClick={() => {
                              setPendingDeleteId(p.id);
                              setPendingDeleteName(p.name);
                            }}
                          >
                            Delete
                          </Button>
                        </div>
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

      {pendingDeleteId && (
        <ConfirmDialog
          title="Delete product"
          message={`Delete "${pendingDeleteName}"? This can't be undone.`}
          confirmLabel="Delete"
          loading={removeState.status === "saving"}
          onConfirm={() => dispatch(productRemoveRequested(pendingDeleteId))}
          onCancel={() => setPendingDeleteId(null)}
        />
      )}
    </>
  );
}
