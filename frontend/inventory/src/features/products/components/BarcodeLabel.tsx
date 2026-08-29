"use client";

import { useEffect, useRef, useState } from "react";
import JsBarcode from "jsbarcode";

interface BarcodeLabelProps {
  /** The raw value from `Product.barcode`. */
  value: string;
  /** Shown under the barcode. */
  productName?: string;
  sku?: string;
  /** Rendered bar height, in px, before the value/text row. Defaults to a compact preview size. */
  height?: number;
  /** Rendered bar width multiplier. Defaults to a compact preview size. */
  width?: number;
  className?: string;
}

/**
 * Renders `value` as a scannable Code128 barcode (SVG). Code128 is used because it accepts the
 * full printable ASCII range with no format-specific validation on our side -- `Product.barcode`
 * is a free-text field on the backend (see `lib/api/products.ts`), not a pre-validated
 * EAN/UPC/ISBN value, so a symbology that can encode whatever was typed or scanned in is the only
 * one that won't reject legitimately-saved data.
 *
 * Renders nothing (a small "no barcode" note) when `value` is empty, and a plain error note if
 * jsbarcode itself throws (e.g. a value containing characters outside its printable-ASCII
 * support) rather than crashing the surrounding form/page.
 */
export function BarcodeLabel({ value, productName, sku, height = 50, width = 1.6, className }: BarcodeLabelProps) {
  const svgRef = useRef<SVGSVGElement | null>(null);
  const [renderError, setRenderError] = useState<string | null>(null);

  useEffect(() => {
    const svg = svgRef.current;
    if (!svg) return;

    if (!value.trim()) {
      setRenderError(null);
      return;
    }

    try {
      JsBarcode(svg, value, {
        format: "CODE128",
        height,
        width,
        displayValue: false,
        margin: 0,
      });
      setRenderError(null);
    } catch {
      setRenderError("Couldn't render this value as a barcode.");
    }
  }, [value, height, width]);

  if (!value.trim()) {
    return (
      <div className={className} style={{ fontSize: 12.5, color: "var(--color-text-faint)" }}>
        No barcode set for this product.
      </div>
    );
  }

  return (
    <div className={className} style={{ textAlign: "center" }}>
      <svg ref={svgRef} role="img" aria-label={`Barcode ${value}`} />
      {renderError ? (
        <div style={{ fontSize: 12, color: "var(--color-danger)" }}>{renderError}</div>
      ) : (
        <div style={{ fontFamily: "monospace", fontSize: 12, letterSpacing: 1, marginTop: 2 }}>{value}</div>
      )}
      {(productName || sku) && (
        <div style={{ fontSize: 11.5, marginTop: 4 }}>
          {productName}
          {productName && sku ? " — " : ""}
          {sku}
        </div>
      )}
    </div>
  );
}
