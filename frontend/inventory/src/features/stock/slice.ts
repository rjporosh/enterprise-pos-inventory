import { createSlice, PayloadAction } from "@reduxjs/toolkit";
import { call, put, takeLatest } from "redux-saga/effects";

import { ApiError, NetworkError, PagedResult } from "@/lib/api/client";
import {
  StockAdjustmentInput,
  StockInInput,
  StockListItem,
  StockListParams,
  StockMovement,
  StockOutInput,
  StockTransferInput,
  stockApi,
} from "@/lib/api/stock";
import { toastShown } from "@/components/ui/toastSlice";

type LoadStatus = "idle" | "loading" | "succeeded" | "failed";
type MutationStatus = "idle" | "saving" | "succeeded" | "failed";

interface StockState {
  list: {
    status: LoadStatus;
    error: string | null;
    result: PagedResult<StockListItem> | null;
    params: StockListParams;
  };
  movement: {
    status: MutationStatus;
    error: string | null;
    lastMovement: StockMovement | null;
    /** which operation triggered the in-flight mutation, so the right form can show its own spinner */
    kind: "in" | "out" | "adjustment" | "transfer" | null;
  };
}

const initialState: StockState = {
  list: { status: "idle", error: null, result: null, params: { pageNumber: 1, pageSize: 20 } },
  movement: { status: "idle", error: null, lastMovement: null, kind: null },
};

const stockSlice = createSlice({
  name: "stock",
  initialState,
  reducers: {
    stockListRequested(state, action: PayloadAction<StockListParams>) {
      state.list.status = "loading";
      state.list.error = null;
      state.list.params = action.payload;
    },
    stockListLoaded(state, action: PayloadAction<PagedResult<StockListItem>>) {
      state.list.status = "succeeded";
      state.list.result = action.payload;
    },
    stockListFailed(state, action: PayloadAction<string>) {
      state.list.status = "failed";
      state.list.error = action.payload;
    },

    stockInRequested(state, _action: PayloadAction<StockInInput>) {
      state.movement = { status: "saving", error: null, lastMovement: null, kind: "in" };
    },
    stockOutRequested(state, _action: PayloadAction<StockOutInput>) {
      state.movement = { status: "saving", error: null, lastMovement: null, kind: "out" };
    },
    stockAdjustmentRequested(state, _action: PayloadAction<StockAdjustmentInput>) {
      state.movement = { status: "saving", error: null, lastMovement: null, kind: "adjustment" };
    },
    stockTransferRequested(state, _action: PayloadAction<StockTransferInput>) {
      state.movement = { status: "saving", error: null, lastMovement: null, kind: "transfer" };
    },
    stockMovementSucceeded(state, action: PayloadAction<StockMovement>) {
      state.movement.status = "succeeded";
      state.movement.lastMovement = action.payload;
    },
    stockMovementFailed(state, action: PayloadAction<string>) {
      state.movement.status = "failed";
      state.movement.error = action.payload;
    },
    stockMovementReset(state) {
      state.movement = initialState.movement;
    },
  },
});

export const {
  stockListRequested,
  stockListLoaded,
  stockListFailed,
  stockInRequested,
  stockOutRequested,
  stockAdjustmentRequested,
  stockTransferRequested,
  stockMovementSucceeded,
  stockMovementFailed,
  stockMovementReset,
} = stockSlice.actions;

export const stockReducer = stockSlice.reducer;

function describeError(err: unknown): string {
  if (err instanceof ApiError) return err.message;
  if (err instanceof NetworkError) return err.message;
  if (err instanceof Error) return err.message;
  return "An unexpected error occurred.";
}

function* fetchStockListWorker(action: ReturnType<typeof stockListRequested>) {
  try {
    const result: PagedResult<StockListItem> = yield call(stockApi.list, action.payload);
    yield put(stockListLoaded(result));
  } catch (err) {
    yield put(stockListFailed(describeError(err)));
  }
}

function* stockInWorker(action: ReturnType<typeof stockInRequested>) {
  try {
    const movement: StockMovement = yield call(stockApi.stockIn, action.payload);
    yield put(stockMovementSucceeded(movement));
    yield put(toastShown("success", `Received ${action.payload.quantity} unit(s) into stock.`));
  } catch (err) {
    const message = describeError(err);
    yield put(stockMovementFailed(message));
    yield put(toastShown("error", message));
  }
}

function* stockOutWorker(action: ReturnType<typeof stockOutRequested>) {
  try {
    const movement: StockMovement = yield call(stockApi.stockOut, action.payload);
    yield put(stockMovementSucceeded(movement));
    yield put(toastShown("success", `Issued ${action.payload.quantity} unit(s) from stock.`));
  } catch (err) {
    const message = describeError(err);
    yield put(stockMovementFailed(message));
    yield put(toastShown("error", message));
  }
}

function* stockAdjustmentWorker(action: ReturnType<typeof stockAdjustmentRequested>) {
  try {
    const movement: StockMovement = yield call(stockApi.adjustment, action.payload);
    yield put(stockMovementSucceeded(movement));
    yield put(toastShown("success", "Stock adjustment recorded."));
  } catch (err) {
    const message = describeError(err);
    yield put(stockMovementFailed(message));
    yield put(toastShown("error", message));
  }
}

function* stockTransferWorker(action: ReturnType<typeof stockTransferRequested>) {
  try {
    const movement: StockMovement = yield call(stockApi.transfer, action.payload);
    yield put(stockMovementSucceeded(movement));
    yield put(toastShown("success", `Transferred ${action.payload.quantity} unit(s).`));
  } catch (err) {
    const message = describeError(err);
    yield put(stockMovementFailed(message));
    yield put(toastShown("error", message));
  }
}

export function* stockSaga() {
  yield takeLatest(stockListRequested.type, fetchStockListWorker);
  yield takeLatest(stockInRequested.type, stockInWorker);
  yield takeLatest(stockOutRequested.type, stockOutWorker);
  yield takeLatest(stockAdjustmentRequested.type, stockAdjustmentWorker);
  yield takeLatest(stockTransferRequested.type, stockTransferWorker);
}
