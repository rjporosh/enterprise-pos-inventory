"use client";

import { useEffect, useState } from "react";
import { useRouter } from "next/navigation";
import { Button, Card, Field, Input, PageHeader } from "@/components/ui";
import { useAppDispatch, useAppSelector } from "@/lib/store/hooks";
import { stockTransferRequested, stockMovementReset } from "@/features/stock/slice";

export default function StockTransferPage() {
  const dispatch = useAppDispatch();
  const router = useRouter();
  const movement = useAppSelector((s) => s.stock.movement);

  const [productId, setProductId] = useState("");
  const [fromWarehouseId, setFromWarehouseId] = useState("");
  const [toWarehouseId, setToWarehouseId] = useState("");
  const [quantity, setQuantity] = useState("1");
  const [notes, setNotes] = useState("");

  useEffect(() => {
    dispatch(stockMovementReset());
  }, [dispatch]);

  useEffect(() => {
    if (movement.status === "succeeded" && movement.kind === "transfer") {
      router.push("/stock");
    }
  }, [movement.status, movement.kind, router]);

  function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    dispatch(
      stockTransferRequested({
        productId,
        fromWarehouseId,
        toWarehouseId,
        quantity: Number(quantity),
        notes: notes || null,
      })
    );
  }

  return (
    <>
      <PageHeader title="Stock transfer" subtitle="Move stock between two warehouses/branches." />
      <Card>
        <form onSubmit={handleSubmit}>
          {movement.status === "failed" && movement.kind === "transfer" && (
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
            <Field label="Quantity" htmlFor="quantity" required>
              <Input id="quantity" type="number" min={1} value={quantity} onChange={(e) => setQuantity(e.target.value)} required />
            </Field>
            <Field label="From warehouse ID" htmlFor="fromWarehouseId" required>
              <Input id="fromWarehouseId" value={fromWarehouseId} onChange={(e) => setFromWarehouseId(e.target.value)} required />
            </Field>
            <Field label="To warehouse ID" htmlFor="toWarehouseId" required>
              <Input id="toWarehouseId" value={toWarehouseId} onChange={(e) => setToWarehouseId(e.target.value)} required />
            </Field>
            <div className="form-grid-full">
              <Field label="Notes" htmlFor="notes">
                <Input id="notes" value={notes} onChange={(e) => setNotes(e.target.value)} />
              </Field>
            </div>
          </div>
          <div className="form-actions">
            <Button type="button" variant="secondary" onClick={() => router.push("/stock")}>
              Cancel
            </Button>
            <Button type="submit" loading={movement.status === "saving" && movement.kind === "transfer"}>
              Transfer stock
            </Button>
          </div>
        </form>
      </Card>
    </>
  );
}
