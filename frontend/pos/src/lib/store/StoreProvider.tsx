"use client";

import { useEffect, useRef } from "react";
import { Provider } from "react-redux";
import { makeStore, AppStore } from "./store";
import { sessionHydrated } from "@/features/session/slice";
import { authHydrated } from "@/features/auth/slice";

export function StoreProvider({ children }: { children: React.ReactNode }) {
  const storeRef = useRef<AppStore | null>(null);
  if (!storeRef.current) {
    storeRef.current = makeStore();
  }

  useEffect(() => {
    storeRef.current?.dispatch(sessionHydrated());
    storeRef.current?.dispatch(authHydrated());
  }, []);

  return <Provider store={storeRef.current}>{children}</Provider>;
}
