import { createSlice, PayloadAction } from "@reduxjs/toolkit";
import { call, put, takeLatest } from "redux-saga/effects";
import { ApiError, NetworkError } from "@/lib/api/client";
import { CatalogProduct, catalogApi } from "@/lib/api/catalog";

interface CatalogState {
  status: "idle" | "loading" | "succeeded" | "failed";
  error: string | null;
  results: CatalogProduct[];
  query: string;
}

const initialState: CatalogState = { status: "idle", error: null, results: [], query: "" };

const catalogSlice = createSlice({
  name: "catalog",
  initialState,
  reducers: {
    searchRequested(state, action: PayloadAction<string>) {
      state.status = "loading";
      state.error = null;
      state.query = action.payload;
    },
    searchSucceeded(state, action: PayloadAction<CatalogProduct[]>) {
      state.status = "succeeded";
      state.results = action.payload;
    },
    searchFailed(state, action: PayloadAction<string>) {
      state.status = "failed";
      state.error = action.payload;
    },
  },
});

export const { searchRequested, searchSucceeded, searchFailed } = catalogSlice.actions;
export const catalogReducer = catalogSlice.reducer;

function describeError(err: unknown): string {
  if (err instanceof ApiError) return err.message;
  if (err instanceof NetworkError) return err.message;
  if (err instanceof Error) return err.message;
  return "An unexpected error occurred.";
}

function* searchWorker(action: ReturnType<typeof searchRequested>) {
  try {
    const result: { items: CatalogProduct[] } = yield call(catalogApi.search, action.payload);
    yield put(searchSucceeded(result.items));
  } catch (err) {
    yield put(searchFailed(describeError(err)));
  }
}

export function* catalogSaga() {
  yield takeLatest(searchRequested.type, searchWorker);
}
