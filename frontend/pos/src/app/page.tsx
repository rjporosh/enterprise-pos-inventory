"use client";

import { useEffect, useRef, useState } from "react";
import { useRouter } from "next/navigation";
import { useAppDispatch, useAppSelector } from "@/lib/store/hooks";
import { searchRequested } from "@/features/catalog/slice";
import { itemAdded, quantityChanged, itemRemoved, cartCleared, cartSubtotal } from "@/features/cart/slice";
import { checkoutRequested, checkoutReset } from "@/features/sale/slice";
import { PaymentMethodType, PaymentMethodTypeValue, PAYMENT_METHOD_LABELS } from "@/lib/api/sales";
import { Badge, Button, Card, EmptyState, Field, Input, Select, SearchInput } from "@/components/ui";
import { Receipt } from "@/features/sale/components/Receipt";

export default function TerminalPage() {
  const dispatch = useAppDispatch();
  const router = useRouter();
  const { config, openSession } = useAppSelector((s) => s.session);
  const catalog = useAppSelector((s) => s.catalog);
  const cart = useAppSelector((s) => s.cart);
  const checkout = useAppSelector((s) => s.sale.checkout);

  const [query, setQuery] = useState("");
  const [paymentMethod, setPaymentMethod] = useState<PaymentMethodTypeValue>(PaymentMethodType.Cash);
  const [amountReceived, setAmountReceived] = useState("0");
  const searchRef = useRef<HTMLInputElement>(null);

  const subtotal = cartSubtotal(cart.lines);

  useEffect(() => {
    setAmountReceived(subtotal.toFixed(2));
  }, [subtotal]);

  useEffect(() => {
    if (!query.trim()) return;
    const timer = setTimeout(() => dispatch(searchRequested(query.trim())), 250);
    return () => clearTimeout(timer);
  }, [query, dispatch]);

  useEffect(() => {
    searchRef.current?.focus();
  }, []);

  function handleAdd(productId: string) {
    const product = catalog.results.find((p) => p.id === productId);
    if (!product) return;
    dispatch(itemAdded(product));
    setQuery("");
    searchRef.current?.focus();
  }

  function handleSearchKeyDown(e: React.KeyboardEvent<HTMLInputElement>) {
    if (e.key === "Enter" && catalog.results.length > 0) {
      e.preventDefault();
      const first = catalog.results[0];
      if (first) handleAdd(first.id);
    }
    if (e.key === "Escape") {
      setQuery("");
    }
  }

  function handleCheckout() {
    if (!config || !openSession || cart.lines.length === 0) return;
    dispatch(
      checkoutRequested({
        saleHeader: {
          storeId: config.storeId,
          registerId: config.registerId,
          cashierId: config.cashierId,
          cashSessionId: openSession.id,
        },
        lines: cart.lines,
        payments: [{ method: paymentMethod, amount: Number(amountReceived) }],
      })
    );
  }

  function startNewSale() {
    dispatch(cartCleared());
    dispatch(checkoutReset());
  }

  if (!config || !openSession) {
    return (
      <EmptyState
        title="Terminal not ready"
        description={!config ? "Set up this terminal before starting a sale." : "Open a cash session before starting a sale."}
        action={
          <Button onClick={() => router.push("/setup")} size="sm">
            Go to setup
          </Button>
        }
      />
    );
  }

  if (checkout.status === "succeeded" && checkout.completedSale) {
    return <Receipt sale={checkout.completedSale} onNewSale={startNewSale} />;
  }

  return (
    <div style={{ display: "grid", gridTemplateColumns: "1.3fr 1fr", gap: 20, alignItems: "start" }}>
      <div>
        <Card style={{ marginBottom: 16 }}>
          <SearchInput
            ref={searchRef}
            placeholder="Search product by name or SKU… (Enter adds the first result)"
            value={query}
            onChange={(e) => setQuery(e.target.value)}
            onKeyDown={handleSearchKeyDown}
            aria-label="Search products"
            autoFocus
          />
          <p style={{ fontSize: 12, color: "var(--color-text-faint)", margin: "8px 0 0" }}>
            Scanner-friendly: a USB barcode scanner types here like a keyboard, then presses Enter. Note:
            search currently matches product name/SKU only — barcode matching isn&apos;t wired up on the
            backend yet (see docs/API-GAPS.md).
          </p>
        </Card>

        {catalog.status === "loading" && <p style={{ color: "var(--color-text-muted)" }}>Searching…</p>}
        {catalog.status === "failed" && <p style={{ color: "var(--color-danger)" }}>{catalog.error}</p>}

        {query.trim() && catalog.status === "succeeded" && (
          <div className="table-wrap">
            {catalog.results.length === 0 ? (
              <EmptyState title="No matching products" description="Try a different name or SKU." />
            ) : (
              <table className="table">
                <thead>
                  <tr>
                    <th>Name</th>
                    <th>SKU</th>
                    <th>Price</th>
                    <th aria-label="Add" />
                  </tr>
                </thead>
                <tbody>
                  {catalog.results.map((p) => (
                    <tr key={p.id}>
                      <td>{p.name}</td>
                      <td>
                        <code style={{ fontSize: 12 }}>{p.sku}</code>
                      </td>
                      <td>{p.sellingPrice.toFixed(2)}</td>
                      <td>
                        <Button size="sm" onClick={() => handleAdd(p.id)}>
                          Add
                        </Button>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            )}
          </div>
        )}
      </div>

      <div>
        <Card padded={false}>
          <div style={{ padding: "16px 18px 10px", display: "flex", justifyContent: "space-between", alignItems: "center" }}>
            <strong style={{ fontSize: 14 }}>Cart</strong>
            {cart.lines.length > 0 && (
              <Button size="sm" variant="ghost" onClick={() => dispatch(cartCleared())}>
                Clear
              </Button>
            )}
          </div>

          {cart.lines.length === 0 ? (
            <EmptyState title="Cart is empty" description="Search and add a product to start a sale." />
          ) : (
            <div style={{ padding: "0 18px" }}>
              {cart.lines.map((line) => (
                <div
                  key={line.productId}
                  style={{
                    display: "flex",
                    alignItems: "center",
                    justifyContent: "space-between",
                    padding: "10px 0",
                    borderBottom: "1px solid var(--color-border)",
                    gap: 8,
                  }}
                >
                  <div style={{ flex: 1, minWidth: 0 }}>
                    <div style={{ fontWeight: 600, fontSize: 13.5, whiteSpace: "nowrap", overflow: "hidden", textOverflow: "ellipsis" }}>
                      {line.productName}
                    </div>
                    <div style={{ fontSize: 12, color: "var(--color-text-faint)" }}>{line.unitPrice.toFixed(2)} each</div>
                  </div>
                  <input
                    type="number"
                    min={1}
                    value={line.quantity}
                    onChange={(e) => dispatch(quantityChanged({ productId: line.productId, quantity: Number(e.target.value) }))}
                    style={{ width: 52, textAlign: "center", border: "1px solid var(--color-border-strong)", borderRadius: 6, padding: "4px 2px" }}
                    aria-label={`Quantity for ${line.productName}`}
                  />
                  <div style={{ width: 62, textAlign: "right", fontWeight: 600, fontSize: 13.5 }}>
                    {(line.unitPrice * line.quantity).toFixed(2)}
                  </div>
                  <button
                    className="btn btn-ghost btn-sm"
                    onClick={() => dispatch(itemRemoved(line.productId))}
                    aria-label={`Remove ${line.productName}`}
                  >
                    ✕
                  </button>
                </div>
              ))}
            </div>
          )}

          <div style={{ padding: 18, borderTop: "1px solid var(--color-border)" }}>
            <div style={{ display: "flex", justifyContent: "space-between", fontSize: 15, fontWeight: 700, marginBottom: 14 }}>
              <span>Total</span>
              <span>{subtotal.toFixed(2)}</span>
            </div>

            {checkout.status === "failed" && (
              <div role="alert" style={{ background: "var(--color-danger-soft)", color: "var(--color-danger)", padding: "10px 14px", borderRadius: 6, marginBottom: 14, fontSize: 13 }}>
                {checkout.error}
              </div>
            )}

            <div className="form-grid" style={{ marginBottom: 14 }}>
              <Field label="Payment method" htmlFor="paymentMethod">
                <Select
                  id="paymentMethod"
                  value={paymentMethod}
                  onChange={(e) => setPaymentMethod(Number(e.target.value) as PaymentMethodTypeValue)}
                >
                  {Object.entries(PAYMENT_METHOD_LABELS).map(([value, label]) => (
                    <option key={value} value={value}>
                      {label}
                    </option>
                  ))}
                </Select>
              </Field>
              <Field label="Amount received" htmlFor="amountReceived">
                <Input id="amountReceived" type="number" min={0} step="0.01" value={amountReceived} onChange={(e) => setAmountReceived(e.target.value)} />
              </Field>
            </div>

            <Button
              block
              size="lg"
              disabled={cart.lines.length === 0}
              loading={checkout.status !== "idle" && checkout.status !== "failed" && checkout.status !== "succeeded"}
              onClick={handleCheckout}
            >
              {checkoutStageLabel(checkout.status)}
            </Button>
          </div>
        </Card>
      </div>
    </div>
  );
}

function checkoutStageLabel(status: string): string {
  switch (status) {
    case "creating-sale":
      return "Starting sale…";
    case "adding-items":
      return "Adding items…";
    case "completing":
      return "Completing sale…";
    default:
      return "Complete sale";
  }
}
