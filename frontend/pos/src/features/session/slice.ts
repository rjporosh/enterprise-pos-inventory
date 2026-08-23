import { createSlice, PayloadAction } from "@reduxjs/toolkit";
import { call, put, takeLatest } from "redux-saga/effects";
import { ApiError, NetworkError } from "@/lib/api/client";
import { CloseCashSessionInput, OpenCashSessionInput, cashSessionsApi } from "@/lib/api/cashSessionsAndReports";
import { toastShown } from "@/components/ui/toastSlice";

const STORAGE_KEY = "pos.session.v1";

export interface TerminalConfig {
  storeId: string;
  registerId: string;
  cashierId: string;
}

export interface OpenSession {
  id: string;
  registerId: string;
  cashierId: string;
  openingBalance: number;
  openedAt: string;
}

interface SessionState {
  config: TerminalConfig | null;
  openSession: OpenSession | null;
  openStatus: "idle" | "opening" | "failed";
  openError: string | null;
  closeStatus: "idle" | "closing" | "succeeded" | "failed";
  closeError: string | null;
}

const initialState: SessionState = {
  config: null,
  openSession: null,
  openStatus: "idle",
  openError: null,
  closeStatus: "idle",
  closeError: null,
};

function loadPersisted(): Pick<SessionState, "config" | "openSession"> {
  if (typeof window === "undefined") return { config: null, openSession: null };
  try {
    const raw = window.localStorage.getItem(STORAGE_KEY);
    if (!raw) return { config: null, openSession: null };
    const parsed = JSON.parse(raw);
    return { config: parsed.config ?? null, openSession: parsed.openSession ?? null };
  } catch {
    return { config: null, openSession: null };
  }
}

function persist(state: Pick<SessionState, "config" | "openSession">) {
  if (typeof window === "undefined") return;
  window.localStorage.setItem(STORAGE_KEY, JSON.stringify(state));
}

const sessionSlice = createSlice({
  name: "session",
  initialState,
  reducers: {
    sessionHydrated(state) {
      const persisted = loadPersisted();
      state.config = persisted.config;
      state.openSession = persisted.openSession;
    },
    configSaved(state, action: PayloadAction<TerminalConfig>) {
      state.config = action.payload;
      persist({ config: state.config, openSession: state.openSession });
    },

    cashSessionOpenRequested(state, _action: PayloadAction<OpenCashSessionInput>) {
      state.openStatus = "opening";
      state.openError = null;
    },
    cashSessionOpenSucceeded(state, action: PayloadAction<OpenSession>) {
      state.openStatus = "idle";
      state.openSession = action.payload;
      persist({ config: state.config, openSession: state.openSession });
    },
    cashSessionOpenFailed(state, action: PayloadAction<string>) {
      state.openStatus = "failed";
      state.openError = action.payload;
    },

    cashSessionCloseRequested(state, _action: PayloadAction<CloseCashSessionInput>) {
      state.closeStatus = "closing";
      state.closeError = null;
    },
    cashSessionCloseSucceeded(state) {
      state.closeStatus = "succeeded";
      state.openSession = null;
      persist({ config: state.config, openSession: null });
    },
    cashSessionCloseFailed(state, action: PayloadAction<string>) {
      state.closeStatus = "failed";
      state.closeError = action.payload;
    },
    cashSessionCloseReset(state) {
      state.closeStatus = "idle";
      state.closeError = null;
    },
  },
});

export const {
  sessionHydrated,
  configSaved,
  cashSessionOpenRequested,
  cashSessionOpenSucceeded,
  cashSessionOpenFailed,
  cashSessionCloseRequested,
  cashSessionCloseSucceeded,
  cashSessionCloseFailed,
  cashSessionCloseReset,
} = sessionSlice.actions;

export const sessionReducer = sessionSlice.reducer;

function describeError(err: unknown): string {
  if (err instanceof ApiError) return err.message;
  if (err instanceof NetworkError) return err.message;
  if (err instanceof Error) return err.message;
  return "An unexpected error occurred.";
}

function* openCashSessionWorker(action: ReturnType<typeof cashSessionOpenRequested>) {
  try {
    const id: string = yield call(cashSessionsApi.open, action.payload);
    yield put(
      cashSessionOpenSucceeded({
        id,
        registerId: action.payload.registerId,
        cashierId: action.payload.cashierId,
        openingBalance: action.payload.openingBalance,
        openedAt: new Date().toISOString(),
      })
    );
    yield put(toastShown("success", "Cash session opened."));
  } catch (err) {
    const message = describeError(err);
    yield put(cashSessionOpenFailed(message));
    yield put(toastShown("error", message));
  }
}

function* closeCashSessionWorker(action: ReturnType<typeof cashSessionCloseRequested>) {
  try {
    yield call(cashSessionsApi.close, action.payload);
    yield put(cashSessionCloseSucceeded());
    yield put(toastShown("success", "Cash session closed."));
  } catch (err) {
    const message = describeError(err);
    yield put(cashSessionCloseFailed(message));
    yield put(toastShown("error", message));
  }
}

export function* sessionSaga() {
  yield takeLatest(cashSessionOpenRequested.type, openCashSessionWorker);
  yield takeLatest(cashSessionCloseRequested.type, closeCashSessionWorker);
}
