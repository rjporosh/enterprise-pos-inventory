"use client";

import { Button, Modal } from "@/components/ui";
import { BarcodeLabel } from "./BarcodeLabel";

interface BarcodeLabelModalProps {
  productName: string;
  sku: string;
  barcode: string;
  onClose: () => void;
}

/**
 * A print-friendly single-label view, opened from the products list or the product form. Follows
 * the same "hide everything except the print area" pattern as
 * `frontend/pos/src/features/sale/components/Receipt.tsx` (visibility-based, not a popup window,
 * so it works with any browser print dialog and driver).
 */
export function BarcodeLabelModal({ productName, sku, barcode, onClose }: BarcodeLabelModalProps) {
  return (
    <Modal
      title="Product barcode label"
      onClose={onClose}
      footer={
        <div className="no-print" style={{ display: "flex", gap: 10, justifyContent: "flex-end" }}>
          <Button variant="secondary" onClick={onClose}>
            Close
          </Button>
          <Button onClick={() => window.print()} disabled={!barcode.trim()}>
            Print label
          </Button>
        </div>
      }
    >
      <div className="barcode-label-print-area">
        <BarcodeLabel value={barcode} productName={productName} sku={sku} height={70} width={2} />
      </div>

      <style jsx global>{`
        @media print {
          body * {
            visibility: hidden;
          }
          .barcode-label-print-area,
          .barcode-label-print-area * {
            visibility: visible;
          }
          .barcode-label-print-area {
            position: absolute;
            top: 0;
            left: 0;
            width: 100%;
            padding: 8mm;
          }
          .no-print {
            display: none !important;
          }
        }
      `}</style>
    </Modal>
  );
}
