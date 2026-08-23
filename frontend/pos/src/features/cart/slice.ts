import { createSlice, PayloadAction } from "@reduxjs/toolkit";
import { CatalogProduct } from "@/lib/api/catalog";

export interface CartLine {
  productId: string;
  productName: string;
  sku: string;
  unitPrice: number;
  quantity: number;
}

interface CartState {
  lines: CartLine[];
}

const initialState: CartState = { lines: [] };

const cartSlice = createSlice({
  name: "cart",
  initialState,
  reducers: {
    itemAdded(state, action: PayloadAction<CatalogProduct>) {
      const product = action.payload;
      const existing = state.lines.find((l) => l.productId === product.id);
      if (existing) {
        existing.quantity += 1;
      } else {
        state.lines.push({
          productId: product.id,
          productName: product.name,
          sku: product.sku,
          unitPrice: product.sellingPrice,
          quantity: 1,
        });
      }
    },
    quantityChanged(state, action: PayloadAction<{ productId: string; quantity: number }>) {
      const line = state.lines.find((l) => l.productId === action.payload.productId);
      if (!line) return;
      if (action.payload.quantity <= 0) {
        state.lines = state.lines.filter((l) => l.productId !== action.payload.productId);
      } else {
        line.quantity = action.payload.quantity;
      }
    },
    itemRemoved(state, action: PayloadAction<string>) {
      state.lines = state.lines.filter((l) => l.productId !== action.payload);
    },
    cartCleared(state) {
      state.lines = [];
    },
  },
});

export const { itemAdded, quantityChanged, itemRemoved, cartCleared } = cartSlice.actions;
export const cartReducer = cartSlice.reducer;

export function cartSubtotal(lines: CartLine[]): number {
  return lines.reduce((sum, l) => sum + l.unitPrice * l.quantity, 0);
}

export function cartItemCount(lines: CartLine[]): number {
  return lines.reduce((sum, l) => sum + l.quantity, 0);
}
