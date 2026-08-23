"use client";

import { useEffect, useState } from "react";
import { useRouter } from "next/navigation";
import { Button, Card, Field, Input, PageHeader } from "@/components/ui";
import { useAppDispatch, useAppSelector } from "@/lib/store/hooks";
import { stockOutRequested, stockMovementReset } from "@/features/stock/slice";

export default function StockOutPage() {
  const dispatch = useAppDispatch();
  const router = useRouter();
  const movement = useAppSelector((s) => s.stock.movement);

  const [productId, setProductId] = useState("");
  const [warehouseId, setWarehouseId] = useState("");
  const [quantity, setQuantity] = useState("1");
  const [notes, setNotes] = useState("");

  useEffect(() => {
    dispatch(stockMovementReset());
  }, [dispatch]);

  useEffect(() => {
    if (movement.status === "succeeded" && movement.kind === "out") {
      router.push("/stock");
    }
  }, [movement.status, movement.kind, router]);

  function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    dispatch(
      stockOutRequested({
        productId,
        warehouseId,
        quantity: Number(quantity),
        referenceType: "MANUAL_ISSUE",
        notes: notes || null,
      })
    );
  }

  return (
    <>
      <PageHeader title="Stock out" subtitle="Issue stock out of a warehouse (damage, sample, internal use)." />
      <Card>
        <form onSubmit={handleSubmit}>
          {movement.status === "failed" && movement.kind === "out" && (
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
            <Field label="Quantity" htmlFor="quantity" required>
              <Input id="quantity" type="number" min={1} value={quantity} onChange={(e) => setQuantity(e.target.value)} required />
            </Field>
            <div className="form-grid-full">
              <Field label="Reason / notes" htmlFor="notes">
                <Input id="notes" value={notes} onChange={(e) => setNotes(e.target.value)} placeholder="e.g. damaged in storage" />
              </Field>
            </div>
          </div>
          <div className="form-actions">
            <Button type="button" variant="secondary" onClick={() => router.push("/stock")}>
              Cancel
            </Button>
            <Button type="submit" variant="danger" loading={movement.status === "saving" && movement.kind === "out"}>
              Issue stock
            </Button>
          </div>
        </form>
      </Card>
    </>
  );
}
