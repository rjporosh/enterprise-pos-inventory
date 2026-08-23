"use client";

import { useEffect, useState } from "react";
import { useRouter } from "next/navigation";
import { Button, Card, Field, Input, PageHeader } from "@/components/ui";
import { useAppDispatch, useAppSelector } from "@/lib/store/hooks";
import { stockAdjustmentRequested, stockMovementReset } from "@/features/stock/slice";

export default function StockAdjustmentPage() {
  const dispatch = useAppDispatch();
  const router = useRouter();
  const movement = useAppSelector((s) => s.stock.movement);

  const [productId, setProductId] = useState("");
  const [warehouseId, setWarehouseId] = useState("");
  const [quantityChange, setQuantityChange] = useState("0");
  const [notes, setNotes] = useState("");

  useEffect(() => {
    dispatch(stockMovementReset());
  }, [dispatch]);

  useEffect(() => {
    if (movement.status === "succeeded" && movement.kind === "adjustment") {
      router.push("/stock");
    }
  }, [movement.status, movement.kind, router]);

  function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    dispatch(
      stockAdjustmentRequested({
        productId,
        warehouseId,
        quantityChange: Number(quantityChange),
        notes: notes || null,
      })
    );
  }

  return (
    <>
      <PageHeader title="Stock adjustment" subtitle="Correct a stock count after a physical audit." />
      <Card>
        <form onSubmit={handleSubmit}>
          {movement.status === "failed" && movement.kind === "adjustment" && (
            <div
              role="alert"
              style={{
                background: "var(--color-danger-soft)",
                color: "var(--color-danger)",
                padding: "10px 14px",
                borderRadius: 6,
                marginBottom: 16,
                fontSize: 13.5,
              }}
            >
              {movement.error}
            </div>
          )}
          <div className="form-grid">
            <Field label="Product ID" htmlFor="productId" required>
              <Input id="productId" value={productId} onChange={(e) => setProductId(e.target.value)} required />
            </Field>
            <Field label="Warehouse ID" htmlFor="warehouseId" required>
              <Input id="warehouseId" value={warehouseId} onChange={(e) => setWarehouseId(e.target.value)} required />
            </Field>
            <Field label="Quantity change" htmlFor="quantityChange" required hint="Positive to increase, negative to decrease (e.g. -3).">
              <Input id="quantityChange" type="number" value={quantityChange} onChange={(e) => setQuantityChange(e.target.value)} required />
            </Field>
            <div className="form-grid-full">
              <Field label="Reason" htmlFor="notes" required hint="Required for audit trail.">
                <Input id="notes" value={notes} onChange={(e) => setNotes(e.target.value)} placeholder="e.g. physical count correction" />
              </Field>
            </div>
          </div>
          <div className="form-actions">
            <Button type="button" variant="secondary" onClick={() => router.push("/stock")}>
              Cancel
            </Button>
            <Button type="submit" loading={movement.status === "saving" && movement.kind === "adjustment"}>
              Record adjustment
            </Button>
          </div>
        </form>
      </Card>
    </>
  );
}
