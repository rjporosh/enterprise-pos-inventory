"use client";

import { useEffect, useState } from "react";
import { useAppDispatch, useAppSelector } from "@/lib/store/hooks";
import { salesApi, SaleListItem, SaleStatus } from "@/lib/api/sales";
import { voidRequested, voidReset } from "@/features/sale/slice";
import { ApiError, NetworkError } from "@/lib/api/client";
import { Badge, Button, EmptyState, ErrorState, Field, Input, Modal, PageHeader, TableSkeleton } from "@/components/ui";

function statusBadge(status: number) {
  if (status === SaleStatus.Completed) return <Badge tone="success">Completed</Badge>;
  if (status === SaleStatus.Voided) return <Badge tone="danger">Voided</Badge>;
  return <Badge tone="neutral">Draft</Badge>;
}

export default function SalesHistoryPage() {
  const dispatch = useAppDispatch();
  const config = useAppSelector((s) => s.session.config);
  const voidState = useAppSelector((s) => s.sale.void);

  const [items, setItems] = useState<SaleListItem[] | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [loading, setLoading] = useState(true);
  const [voidTargetId, setVoidTargetId] = useState<string | null>(null);
  const [voidReason, setVoidReason] = useState("");

  async function load() {
    if (!config) return;
    setLoading(true);
    setError(null);
    try {
      const result = await salesApi.list({ storeId: config.storeId, pageNumber: 1, pageSize: 30 });
      setItems(result.items);
    } catch (err) {
      if (err instanceof ApiError || err instanceof NetworkError) setError(err.message);
      else setError("Could not load sale history.");
    } finally {
      setLoading(false);
    }
  }

  useEffect(() => {
    load();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [config]);

  useEffect(() => {
    if (voidState.status === "succeeded") {
      setVoidTargetId(null);
      setVoidReason("");
      dispatch(voidReset());
      load();
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [voidState.status]);

  if (!config) {
    return <EmptyState title="Terminal not set up" description="Set up the terminal first to see sale history for this store." />;
  }

  return (
    <>
      <PageHeader title="Sale history" subtitle="Completed and voided sales for this store." />

      {error && <ErrorState message={error} onRetry={load} />}

      {!error && (
        <div className="table-wrap">
          {loading && !items ? (
            <TableSkeleton cols={5} />
          ) : items && items.length === 0 ? (
            <EmptyState title="No sales yet" description="Completed sales will show up here." />
          ) : (
            <table className="table">
              <thead>
                <tr>
                  <th>Sale #</th>
                  <th>Date</th>
                  <th>Status</th>
                  <th>Total</th>
                  <th aria-label="Actions" />
                </tr>
              </thead>
              <tbody>
                {items?.map((s) => (
                  <tr key={s.id}>
                    <td>
                      <code style={{ fontSize: 12.5 }}>{s.saleNumber}</code>
                    </td>
                    <td>{new Date(s.saleDate).toLocaleString()}</td>
                    <td>{statusBadge(s.status)}</td>
                    <td>{s.totalAmount.toFixed(2)}</td>
                    <td>
                      {s.status === SaleStatus.Completed && (
                        <Button size="sm" variant="ghost" onClick={() => setVoidTargetId(s.id)}>
                          Void
                        </Button>
                      )}
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          )}
        </div>
      )}

      {voidTargetId && (
        <Modal
          title="Void sale"
          onClose={() => {
            setVoidTargetId(null);
            setVoidReason("");
          }}
          footer={
            <>
              <Button
                variant="secondary"
                onClick={() => {
                  setVoidTargetId(null);
                  setVoidReason("");
                }}
                disabled={voidState.status === "voiding"}
              >
                Cancel
              </Button>
              <Button
                variant="danger"
                loading={voidState.status === "voiding"}
                disabled={!voidReason.trim()}
                onClick={() => dispatch(voidRequested({ saleId: voidTargetId, reason: voidReason.trim() }))}
              >
                Void sale
              </Button>
            </>
          }
        >
          <p style={{ marginTop: 0, color: "var(--color-text-muted)", fontSize: 13.5 }}>
            This cannot be undone.
          </p>
          <Field label="Reason" htmlFor="voidReason" required>
            <Input id="voidReason" value={voidReason} onChange={(e) => setVoidReason(e.target.value)} placeholder="e.g. entered by mistake" />
          </Field>
        </Modal>
      )}
    </>
  );
}
