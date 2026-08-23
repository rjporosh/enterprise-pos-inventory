import { describe, expect, it } from "vitest";
import { cartReducer, itemAdded, quantityChanged, itemRemoved, cartCleared, cartSubtotal, cartItemCount } from "../slice";
import { CatalogProduct } from "@/lib/api/catalog";

function product(overrides: Partial<CatalogProduct> = {}): CatalogProduct {
  return {
    id: "p1",
    name: "Cotton Burkha",
    sku: "BRK-001",
    barcode: null,
    categoryName: "Clothing",
    brandName: "Generic",
    unitSymbol: "pcs",
    sellingPrice: 45,
    isActive: true,
    reorderLevel: 5,
    ...overrides,
  };
}

describe("cart slice", () => {
  it("adds a new product as a line with quantity 1", () => {
    const state = cartReducer(undefined, itemAdded(product()));
    expect(state.lines).toHaveLength(1);
    expect(state.lines.at(0)).toMatchObject({ productId: "p1", quantity: 1, unitPrice: 45 });
  });

  it("increments quantity when the same product is added again", () => {
    let state = cartReducer(undefined, itemAdded(product()));
    state = cartReducer(state, itemAdded(product()));
    expect(state.lines).toHaveLength(1);
    expect(state.lines.at(0)?.quantity).toBe(2);
  });

  it("adds a second distinct product as its own line", () => {
    let state = cartReducer(undefined, itemAdded(product()));
    state = cartReducer(state, itemAdded(product({ id: "p2", sku: "ENG-100", sellingPrice: 12.5 })));
    expect(state.lines).toHaveLength(2);
  });

  it("updates quantity directly", () => {
    let state = cartReducer(undefined, itemAdded(product()));
    state = cartReducer(state, quantityChanged({ productId: "p1", quantity: 5 }));
    expect(state.lines.at(0)?.quantity).toBe(5);
  });

  it("removes the line when quantity is set to 0 or below", () => {
    let state = cartReducer(undefined, itemAdded(product()));
    state = cartReducer(state, quantityChanged({ productId: "p1", quantity: 0 }));
    expect(state.lines).toHaveLength(0);
  });

  it("removes a line explicitly", () => {
    let state = cartReducer(undefined, itemAdded(product()));
    state = cartReducer(state, itemRemoved("p1"));
    expect(state.lines).toHaveLength(0);
  });

  it("clears the cart", () => {
    let state = cartReducer(undefined, itemAdded(product()));
    state = cartReducer(state, itemAdded(product({ id: "p2" })));
    state = cartReducer(state, cartCleared());
    expect(state.lines).toHaveLength(0);
  });

  it("computes subtotal across lines", () => {
    let state = cartReducer(undefined, itemAdded(product({ sellingPrice: 10 })));
    state = cartReducer(state, quantityChanged({ productId: "p1", quantity: 3 }));
    state = cartReducer(state, itemAdded(product({ id: "p2", sellingPrice: 5 })));
    expect(cartSubtotal(state.lines)).toBe(35);
  });

  it("computes total item count across lines", () => {
    let state = cartReducer(undefined, itemAdded(product()));
    state = cartReducer(state, quantityChanged({ productId: "p1", quantity: 4 }));
    state = cartReducer(state, itemAdded(product({ id: "p2" })));
    expect(cartItemCount(state.lines)).toBe(5);
  });
});
