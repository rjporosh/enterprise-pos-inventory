import { configureStore } from "@reduxjs/toolkit";
import createSagaMiddleware from "redux-saga";
import { all, fork } from "redux-saga/effects";

import { catalogReducer, catalogSaga } from "@/features/catalog/slice";
import { cartReducer } from "@/features/cart/slice";
import { sessionReducer, sessionSaga } from "@/features/session/slice";
import { saleReducer, saleSaga } from "@/features/sale/slice";
import { authReducer, authSaga, sessionExpired } from "@/features/auth/slice";
import { toastReducer } from "@/components/ui/toastSlice";
import { registerSessionExpiredHandler } from "@/lib/api/client";

function* rootSaga() {
  yield all([fork(catalogSaga), fork(sessionSaga), fork(saleSaga), fork(authSaga)]);
}

export function makeStore() {
  const sagaMiddleware = createSagaMiddleware();

  const store = configureStore({
    reducer: {
      catalog: catalogReducer,
      cart: cartReducer,
      session: sessionReducer,
      sale: saleReducer,
      auth: authReducer,
      toast: toastReducer,
    },
    middleware: (getDefaultMiddleware) => getDefaultMiddleware({ thunk: false }).concat(sagaMiddleware),
  });

  sagaMiddleware.run(rootSaga);

  // The API client has no store/router reference of its own — a hard-expired session (refresh
  // token also rejected) is reported back here so the UI can react (route guard in AppShell
  // redirects to /login once auth.status flips to "unauthenticated").
  registerSessionExpiredHandler(() => store.dispatch(sessionExpired()));

  return store;
}

export type AppStore = ReturnType<typeof makeStore>;
export type RootState = ReturnType<AppStore["getState"]>;
export type AppDispatch = AppStore["dispatch"];
