"use client";

import { useEffect, useState } from "react";
import { useRouter } from "next/navigation";
import { Button, Card, Field, Input, PageHeader } from "@/components/ui";
import { useAppDispatch, useAppSelector } from "@/lib/store/hooks";
import { stockInRequested, stockMovementReset } from "@/features/stock/slice";

export default function StockInPage() {
  const dispatch = useAppDispatch();
  const router = useRouter();
  const movement = useAppSelector((s) => s.stock.movement);

  const [productId, setProductId] = useState("");
  const [warehouseId, setWarehouseId] = useState("");
  const [quantity, setQuantity] = useState("1");
  const [unitCost, setUnitCost] = useState("");
  const [notes, setNotes] = useState("");

  useEffect(() => {
    dispatch(stockMovementReset());
  }, [dispatch]);

  useEffect(() => {
    if (movement.status === "succeeded" && movement.kind === "in") {
      router.push("/stock");
    }
  }, [movement.status, movement.kind, router]);

  function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    dispatch(
      stockInRequested({
        productId,
        warehouseId,
        quantity: Number(quantity),
        unitCost: unitCost ? Number(unitCost) : null,
        referenceType: "MANUAL_RECEIPT",
        notes: notes || null,
      })
    );
  }

  return (
    <>
      <PageHeader title="Stock in" subtitle="Receive stock into a warehouse." />
      <Card>
        <form onSubmit={handleSubmit}>
          {movement.status === "failed" && movement.kind === "in" && (
            <div role="alert" style={{ background: "var(--color-danger-soft)", color: "var(--color-danger)", padding: "10px 14px", borderRadius: 6, marginBottom: 16, fontSize: 13.5 }}>
              {movement.error}
            </div>
          )}
          <div className="form-grid">
            <Field label="Product ID" htmlFor="productId" required hint="Copy the product ID from the Products list.">
              <Input id="productId" value={productId} onChange={(e) => setProductId(e.target.value)} required />
            </Field>
            <Field label="Warehouse ID" htmlFor="warehouseId" required hint="Warehouse management isn't available yet — paste an existing warehouse GUID.">
              <Input id="warehouseId" value={warehouseId} onChange={(e) => setWarehouseId(e.target.value)} required />
            </Field>
            <Field label="Quantity" htmlFor="quantity" required>
              <Input id="quantity" type="number" min={1} value={quantity} onChange={(e) => setQuantity(e.target.value)} required />
            </Field>
            <Field label="Unit cost" htmlFor="unitCost" hint="Optional.">
              <Input id="unitCost" type="number" min={0} step="0.01" value={unitCost} onChange={(e) => setUnitCost(e.target.value)} />
            </Field>
            <div className="form-grid-full">
              <Field label="Notes" htmlFor="notes" hint="e.g. supplier invoice number.">
                <Input id="notes" value={notes} onChange={(e) => setNotes(e.target.value)} />
              </Field>
            </div>
          </div>
          <div className="form-actions">
            <Button type="button" variant="secondary" onClick={() => router.push("/stock")}>
              Cancel
            </Button>
            <Button type="submit" loading={movement.status === "saving" && movement.kind === "in"}>
              Receive stock
            </Button>
          </div>
        </form>
      </Card>
    </>
  );
}
