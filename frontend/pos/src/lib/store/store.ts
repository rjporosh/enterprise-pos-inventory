import { configureStore } from "@reduxjs/toolkit";
import createSagaMiddleware from "redux-saga";
import { all, fork } from "redux-saga/effects";

import { catalogReducer, catalogSaga } from "@/features/catalog/slice";
import { cartReducer } from "@/features/cart/slice";
import { sessionReducer, sessionSaga } from "@/features/session/slice";
import { saleReducer, saleSaga } from "@/features/sale/slice";
import { toastReducer } from "@/components/ui/toastSlice";

function* rootSaga() {
  yield all([fork(catalogSaga), fork(sessionSaga), fork(saleSaga)]);
}

export function makeStore() {
  const sagaMiddleware = createSagaMiddleware();

  const store = configureStore({
    reducer: {
      catalog: catalogReducer,
      cart: cartReducer,
      session: sessionReducer,
      sale: saleReducer,
      toast: toastReducer,
    },
    middleware: (getDefaultMiddleware) => getDefaultMiddleware({ thunk: false }).concat(sagaMiddleware),
  });

  sagaMiddleware.run(rootSaga);

  return store;
}

export type AppStore = ReturnType<typeof makeStore>;
export type RootState = ReturnType<AppStore["getState"]>;
export type AppDispatch = AppStore["dispatch"];
