"use client";

import { Sale, PAYMENT_METHOD_LABELS } from "@/lib/api/sales";
import { Button, Card } from "@/components/ui";

export function Receipt({ sale, onNewSale }: { sale: Sale; onNewSale: () => void }) {
  return (
    <div style={{ maxWidth: 420, margin: "0 auto" }}>
      <div className="receipt-print-area">
        <Card>
          <div style={{ textAlign: "center", marginBottom: 14 }}>
            <div style={{ fontWeight: 800, fontSize: 16 }}>Sale receipt</div>
            <div style={{ fontSize: 12, color: "var(--color-text-muted)" }}>{sale.saleNumber}</div>
            <div style={{ fontSize: 11.5, color: "var(--color-text-faint)" }}>{new Date(sale.saleDate).toLocaleString()}</div>
          </div>

          <div style={{ borderTop: "1px dashed var(--color-border-strong)", borderBottom: "1px dashed var(--color-border-strong)", padding: "10px 0", margin: "10px 0" }}>
            {sale.items.map((item) => (
              <div key={item.id} style={{ display: "flex", justifyContent: "space-between", fontSize: 13, marginBottom: 6 }}>
                <span>
                  {item.productName} <span style={{ color: "var(--color-text-faint)" }}>× {item.quantity}</span>
                </span>
                <span>{item.lineTotal.toFixed(2)}</span>
              </div>
            ))}
          </div>

          <div style={{ fontSize: 13 }}>
            <Row label="Subtotal" value={sale.subtotalAmount} />
            {sale.discountAmount > 0 && <Row label="Discount" value={-sale.discountAmount} />}
            {sale.taxAmount > 0 && <Row label="Tax" value={sale.taxAmount} />}
            <Row label="Total" value={sale.totalAmount} bold />
            <Row label="Paid" value={sale.paidAmount} />
            {sale.changeAmount > 0 && <Row label="Change" value={sale.changeAmount} />}
          </div>

          <div style={{ borderTop: "1px dashed var(--color-border-strong)", marginTop: 10, paddingTop: 10, fontSize: 12, color: "var(--color-text-muted)" }}>
            {sale.payments.map((p) => (
              <div key={p.id}>
                {PAYMENT_METHOD_LABELS[p.method]}: {p.amount.toFixed(2)}
                {p.referenceNumber ? ` (ref ${p.referenceNumber})` : ""}
              </div>
            ))}
          </div>
        </Card>
      </div>

      <div style={{ display: "flex", gap: 10, marginTop: 16 }} className="no-print">
        <Button variant="secondary" onClick={() => window.print()} block>
          Print receipt
        </Button>
        <Button onClick={onNewSale} block>
          New sale
        </Button>
      </div>

      <style jsx global>{`
        @media print {
          body * {
            visibility: hidden;
          }
          .receipt-print-area,
          .receipt-print-area * {
            visibility: visible;
          }
          .receipt-print-area {
            position: absolute;
            top: 0;
            left: 0;
            width: 100%;
          }
        }
      `}</style>
    </div>
  );
}

function Row({ label, value, bold }: { label: string; value: number; bold?: boolean }) {
  return (
    <div style={{ display: "flex", justifyContent: "space-between", padding: "3px 0", fontWeight: bold ? 700 : 400 }}>
      <span>{label}</span>
      <span>{value.toFixed(2)}</span>
    </div>
  );
}
