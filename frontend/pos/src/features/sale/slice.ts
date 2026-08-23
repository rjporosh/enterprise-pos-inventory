import { createSlice, PayloadAction } from "@reduxjs/toolkit";
import { call, put, takeLatest } from "redux-saga/effects";
import { ApiError, NetworkError } from "@/lib/api/client";
import {
  CreateSaleInput,
  Sale,
  SalePaymentInput,
  salesApi,
} from "@/lib/api/sales";
import { CartLine } from "@/features/cart/slice";
import { toastShown } from "@/components/ui/toastSlice";

type CheckoutStatus = "idle" | "creating-sale" | "adding-items" | "completing" | "succeeded" | "failed";

export interface CheckoutInput {
  saleHeader: CreateSaleInput;
  lines: CartLine[];
  payments: SalePaymentInput[];
}

interface SaleState {
  checkout: {
    status: CheckoutStatus;
    error: string | null;
    completedSale: Sale | null;
  };
  void: {
    status: "idle" | "voiding" | "succeeded" | "failed";
    error: string | null;
  };
}

const initialState: SaleState = {
  checkout: { status: "idle", error: null, completedSale: null },
  void: { status: "idle", error: null },
};

const saleSlice = createSlice({
  name: "sale",
  initialState,
  reducers: {
    checkoutRequested(state, _action: PayloadAction<CheckoutInput>) {
      state.checkout = { status: "creating-sale", error: null, completedSale: null };
    },
    checkoutStageChanged(state, action: PayloadAction<CheckoutStatus>) {
      state.checkout.status = action.payload;
    },
    checkoutSucceeded(state, action: PayloadAction<Sale>) {
      state.checkout.status = "succeeded";
      state.checkout.completedSale = action.payload;
    },
    checkoutFailed(state, action: PayloadAction<string>) {
      state.checkout.status = "failed";
      state.checkout.error = action.payload;
    },
    checkoutReset(state) {
      state.checkout = initialState.checkout;
    },

    voidRequested(state, _action: PayloadAction<{ saleId: string; reason: string }>) {
      state.void = { status: "voiding", error: null };
    },
    voidSucceeded(state) {
      state.void.status = "succeeded";
    },
    voidFailed(state, action: PayloadAction<string>) {
      state.void.status = "failed";
      state.void.error = action.payload;
    },
    voidReset(state) {
      state.void = initialState.void;
    },
  },
});

export const {
  checkoutRequested,
  checkoutStageChanged,
  checkoutSucceeded,
  checkoutFailed,
  checkoutReset,
  voidRequested,
  voidSucceeded,
  voidFailed,
  voidReset,
} = saleSlice.actions;

export const saleReducer = saleSlice.reducer;

function describeError(err: unknown): string {
  if (err instanceof ApiError) return err.message;
  if (err instanceof NetworkError) return err.message;
  if (err instanceof Error) return err.message;
  return "An unexpected error occurred.";
}

/**
 * Checkout is a sequential, backend-authoritative pipeline:
 * create draft sale -> add each cart line as a real sale item -> complete with payments ->
 * re-fetch the completed sale (Complete returns 204, not the sale) for the receipt.
 * The cart itself stays local/client-side until this point (see features/cart) so building the
 * cart is instant for the cashier; nothing is "sold" until this saga runs.
 */
function* checkoutWorker(action: ReturnType<typeof checkoutRequested>) {
  const { saleHeader, lines, payments } = action.payload;
  try {
    const saleId: string = yield call(salesApi.create, saleHeader);

    yield put(checkoutStageChanged("adding-items"));
    for (const line of lines) {
      yield call(salesApi.addItem, {
        saleId,
        productId: line.productId,
        productName: line.productName,
        sku: line.sku,
        unitPrice: line.unitPrice,
        quantity: line.quantity,
      });
    }

    yield put(checkoutStageChanged("completing"));
    yield call(salesApi.complete, saleId, payments);

    const completedSale: Sale = yield call(salesApi.getById, saleId);
    yield put(checkoutSucceeded(completedSale));
    yield put(toastShown("success", `Sale ${completedSale.saleNumber} completed.`));
  } catch (err) {
    const message = describeError(err);
    yield put(checkoutFailed(message));
    yield put(toastShown("error", message));
  }
}

function* voidWorker(action: ReturnType<typeof voidRequested>) {
  try {
    yield call(salesApi.void, action.payload.saleId, action.payload.reason);
    yield put(voidSucceeded());
    yield put(toastShown("success", "Sale voided."));
  } catch (err) {
    const message = describeError(err);
    yield put(voidFailed(message));
    yield put(toastShown("error", message));
  }
}

export function* saleSaga() {
  yield takeLatest(checkoutRequested.type, checkoutWorker);
  yield takeLatest(voidRequested.type, voidWorker);
}
