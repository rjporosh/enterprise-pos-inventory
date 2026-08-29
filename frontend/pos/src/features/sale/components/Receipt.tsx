"use client";

import { Sale, PAYMENT_METHOD_LABELS } from "@/lib/api/sales";
import { Button, Card } from "@/components/ui";
import { useAppSelector } from "@/lib/store/hooks";
import type { ReceiptPaperWidth } from "@/features/session/slice";

/**
 * Thermal printer paper profiles. `printableWidthMm` is the printable area, not the roll width —
 * roll manufacturers commonly quote 58mm/80mm rolls with ~48mm/~72mm actual printable width once
 * margins are accounted for. `fontSizePt` is tuned for common 58mm/80mm ESC/POS thermal printers
 * rendering via the browser print dialog (not raw ESC/POS commands — a "local print bridge" is
 * still not-started per docs/ROADMAP-v3.0.md Phase 7; this renders an HTML receipt sized to the
 * paper and lets the OS/printer driver rasterize it, which works with any thermal printer that
 * has a Windows/macOS/Linux print driver).
 */
const PAPER_PROFILES: Record<ReceiptPaperWidth, { printableWidthMm: number; fontSizePt: number }> = {
  58: { printableWidthMm: 48, fontSizePt: 9 },
  80: { printableWidthMm: 72, fontSizePt: 10.5 },
};

export function Receipt({ sale, onNewSale }: { sale: Sale; onNewSale: () => void }) {
  const paperWidthMm = useAppSelector((s) => s.session.config?.receiptPaperWidthMm ?? 80);
  const profile = PAPER_PROFILES[paperWidthMm];

  return (
    <div style={{ maxWidth: 420, margin: "0 auto" }}>
      <div className={`receipt-print-area receipt-paper-${paperWidthMm}`}>
        <Card>
          <div style={{ textAlign: "center", marginBottom: 14 }}>
            <div style={{ fontWeight: 800, fontSize: 16 }}>Sale receipt</div>
            <div style={{ fontSize: 12, color: "var(--color-text-muted)" }}>{sale.saleNumber}</div>
            <div style={{ fontSize: 11.5, color: "var(--color-text-faint)" }}>{new Date(sale.saleDate).toLocaleString()}</div>
          </div>

          <div style={{ borderTop: "1px dashed var(--color-border-strong)", borderBottom: "1px dashed var(--color-border-strong)", padding: "10px 0", margin: "10px 0" }}>
            {sale.items.map((item) => (
              <div key={item.id} style={{ display: "flex", justifyContent: "space-between", fontSize: 13, marginBottom: 6, gap: 8 }}>
                <span>
                  {item.productName} <span style={{ color: "var(--color-text-faint)" }}>× {item.quantity}</span>
                </span>
                <span style={{ whiteSpace: "nowrap" }}>{item.lineTotal.toFixed(2)}</span>
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

          <div className="print-only" style={{ textAlign: "center", marginTop: 14, fontSize: 11 }}>
            Thank you
          </div>
        </Card>
      </div>

      <div style={{ display: "flex", gap: 10, marginTop: 16 }} className="no-print">
        <Button variant="secondary" onClick={() => window.print()} block>
          Print receipt ({paperWidthMm}mm)
        </Button>
        <Button onClick={onNewSale} block>
          New sale
        </Button>
      </div>

      <style jsx global>{`
        .print-only {
          display: none;
        }
        @media print {
          @page {
            size: ${profile.printableWidthMm}mm auto;
            margin: 0;
          }
          html,
          body {
            width: ${profile.printableWidthMm}mm;
          }
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
            width: ${profile.printableWidthMm}mm;
            font-family: "Courier New", Consolas, monospace;
            font-size: ${profile.fontSizePt}pt;
            color: #000;
          }
          .receipt-print-area .card {
            box-shadow: none !important;
            border: none !important;
            padding: 2mm !important;
          }
          .print-only {
            display: block;
          }
          .no-print {
            display: none !important;
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
