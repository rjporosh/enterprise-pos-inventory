import { describe, expect, it } from "vitest";
import { render, screen } from "@testing-library/react";
import { BarcodeLabel } from "../components/BarcodeLabel";

describe("BarcodeLabel", () => {
  it("shows a placeholder when there is no barcode value", () => {
    render(<BarcodeLabel value="" />);
    expect(screen.getByText(/no barcode set/i)).toBeInTheDocument();
  });

  it("renders a scannable barcode for a valid value", () => {
    const { container } = render(<BarcodeLabel value="8901234567890" />);
    const svg = container.querySelector("svg");
    expect(svg).toBeInTheDocument();
    // jsbarcode draws the bars as <rect> children of the svg it's given.
    expect(container.querySelectorAll("svg rect").length).toBeGreaterThan(0);
    expect(screen.getByText("8901234567890")).toBeInTheDocument();
  });

  it("renders the product name and SKU under the barcode when provided", () => {
    render(<BarcodeLabel value="ABC-100" productName="Cotton Burkha" sku="BRK-001" />);
    expect(screen.getByText("Cotton Burkha — BRK-001")).toBeInTheDocument();
  });
});
