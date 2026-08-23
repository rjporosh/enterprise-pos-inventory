import { configureStore } from "@reduxjs/toolkit";
import createSagaMiddleware from "redux-saga";
import { all, fork } from "redux-saga/effects";

import { productsReducer, productsSaga } from "@/features/products/slice";
import { stockReducer, stockSaga } from "@/features/stock/slice";
import { toastReducer } from "@/components/ui/toastSlice";

function* rootSaga() {
  yield all([fork(productsSaga), fork(stockSaga)]);
}

export function makeStore() {
  const sagaMiddleware = createSagaMiddleware();

  const store = configureStore({
    reducer: {
      products: productsReducer,
      stock: stockReducer,
      toast: toastReducer,
    },
    middleware: (getDefaultMiddleware) =>
      getDefaultMiddleware({ thunk: false }).concat(sagaMiddleware),
  });

  sagaMiddleware.run(rootSaga);

  return store;
}

export type AppStore = ReturnType<typeof makeStore>;
export type RootState = ReturnType<AppStore["getState"]>;
export type AppDispatch = AppStore["dispatch"];
