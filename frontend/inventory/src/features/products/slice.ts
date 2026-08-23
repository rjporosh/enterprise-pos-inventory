import { createSlice, PayloadAction } from "@reduxjs/toolkit";
import { call, put, takeLatest } from "redux-saga/effects";

import { ApiError, NetworkError, PagedResult } from "@/lib/api/client";
import {
  CreateProductInput,
  Product,
  ProductListItem,
  ProductListParams,
  productsApi,
  UpdateProductInput,
} from "@/lib/api/products";
import { toastShown } from "@/components/ui/toastSlice";

type LoadStatus = "idle" | "loading" | "succeeded" | "failed";
type MutationStatus = "idle" | "saving" | "succeeded" | "failed";

interface ProductsState {
  list: {
    status: LoadStatus;
    error: string | null;
    result: PagedResult<ProductListItem> | null;
    params: ProductListParams;
  };
  detail: {
    status: LoadStatus;
    error: string | null;
    data: Product | null;
  };
  create: { status: MutationStatus; error: string | null; createdId: string | null };
  update: { status: MutationStatus; error: string | null };
  remove: { status: MutationStatus; error: string | null; targetId: string | null };
}

const initialState: ProductsState = {
  list: { status: "idle", error: null, result: null, params: { pageNumber: 1, pageSize: 20 } },
  detail: { status: "idle", error: null, data: null },
  create: { status: "idle", error: null, createdId: null },
  update: { status: "idle", error: null },
  remove: { status: "idle", error: null, targetId: null },
};

const productsSlice = createSlice({
  name: "products",
  initialState,
  reducers: {
    productsRequested(state, action: PayloadAction<ProductListParams>) {
      state.list.status = "loading";
      state.list.error = null;
      state.list.params = action.payload;
    },
    productsLoaded(state, action: PayloadAction<PagedResult<ProductListItem>>) {
      state.list.status = "succeeded";
      state.list.result = action.payload;
    },
    productsFailed(state, action: PayloadAction<string>) {
      state.list.status = "failed";
      state.list.error = action.payload;
    },

    productDetailRequested(state, _action: PayloadAction<string>) {
      state.detail.status = "loading";
      state.detail.error = null;
      state.detail.data = null;
    },
    productDetailLoaded(state, action: PayloadAction<Product>) {
      state.detail.status = "succeeded";
      state.detail.data = action.payload;
    },
    productDetailFailed(state, action: PayloadAction<string>) {
      state.detail.status = "failed";
      state.detail.error = action.payload;
    },
    productDetailCleared(state) {
      state.detail = initialState.detail;
    },

    productCreateRequested(state, _action: PayloadAction<CreateProductInput>) {
      state.create.status = "saving";
      state.create.error = null;
      state.create.createdId = null;
    },
    productCreateSucceeded(state, action: PayloadAction<string>) {
      state.create.status = "succeeded";
      state.create.createdId = action.payload;
    },
    productCreateFailed(state, action: PayloadAction<string>) {
      state.create.status = "failed";
      state.create.error = action.payload;
    },
    productCreateReset(state) {
      state.create = initialState.create;
    },

    productUpdateRequested(state, _action: PayloadAction<UpdateProductInput>) {
      state.update.status = "saving";
      state.update.error = null;
    },
    productUpdateSucceeded(state) {
      state.update.status = "succeeded";
    },
    productUpdateFailed(state, action: PayloadAction<string>) {
      state.update.status = "failed";
      state.update.error = action.payload;
    },
    productUpdateReset(state) {
      state.update = initialState.update;
    },

    productRemoveRequested(state, action: PayloadAction<string>) {
      state.remove.status = "saving";
      state.remove.error = null;
      state.remove.targetId = action.payload;
    },
    productRemoveSucceeded(state) {
      state.remove.status = "succeeded";
    },
    productRemoveFailed(state, action: PayloadAction<string>) {
      state.remove.status = "failed";
      state.remove.error = action.payload;
    },
    productRemoveReset(state) {
      state.remove = initialState.remove;
    },
  },
});

export const {
  productsRequested,
  productsLoaded,
  productsFailed,
  productDetailRequested,
  productDetailLoaded,
  productDetailFailed,
  productDetailCleared,
  productCreateRequested,
  productCreateSucceeded,
  productCreateFailed,
  productCreateReset,
  productUpdateRequested,
  productUpdateSucceeded,
  productUpdateFailed,
  productUpdateReset,
  productRemoveRequested,
  productRemoveSucceeded,
  productRemoveFailed,
  productRemoveReset,
} = productsSlice.actions;

export const productsReducer = productsSlice.reducer;

function describeError(err: unknown): string {
  if (err instanceof ApiError) return err.message;
  if (err instanceof NetworkError) return err.message;
  if (err instanceof Error) return err.message;
  return "An unexpected error occurred.";
}

function* fetchProductsWorker(action: ReturnType<typeof productsRequested>) {
  try {
    const result: PagedResult<ProductListItem> = yield call(productsApi.list, action.payload);
    yield put(productsLoaded(result));
  } catch (err) {
    yield put(productsFailed(describeError(err)));
  }
}

function* fetchProductDetailWorker(action: ReturnType<typeof productDetailRequested>) {
  try {
    const product: Product = yield call(productsApi.getById, action.payload);
    yield put(productDetailLoaded(product));
  } catch (err) {
    yield put(productDetailFailed(describeError(err)));
  }
}

function* createProductWorker(action: ReturnType<typeof productCreateRequested>) {
  try {
    const id: string = yield call(productsApi.create, action.payload);
    yield put(productCreateSucceeded(id));
    yield put(toastShown("success", `Product "${action.payload.name}" created.`));
  } catch (err) {
    const message = describeError(err);
    yield put(productCreateFailed(message));
    yield put(toastShown("error", message));
  }
}

function* updateProductWorker(action: ReturnType<typeof productUpdateRequested>) {
  try {
    yield call(productsApi.update, action.payload);
    yield put(productUpdateSucceeded());
    yield put(toastShown("success", `Product "${action.payload.name}" updated.`));
  } catch (err) {
    const message = describeError(err);
    yield put(productUpdateFailed(message));
    yield put(toastShown("error", message));
  }
}

function* removeProductWorker(action: ReturnType<typeof productRemoveRequested>) {
  try {
    yield call(productsApi.remove, action.payload);
    yield put(productRemoveSucceeded());
    yield put(toastShown("success", "Product deleted."));
  } catch (err) {
    const message = describeError(err);
    yield put(productRemoveFailed(message));
    yield put(toastShown("error", message));
  }
}

export function* productsSaga() {
  yield takeLatest(productsRequested.type, fetchProductsWorker);
  yield takeLatest(productDetailRequested.type, fetchProductDetailWorker);
  yield takeLatest(productCreateRequested.type, createProductWorker);
  yield takeLatest(productUpdateRequested.type, updateProductWorker);
  yield takeLatest(productRemoveRequested.type, removeProductWorker);
}
