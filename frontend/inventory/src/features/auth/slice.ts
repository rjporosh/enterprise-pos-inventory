import { createSlice, PayloadAction } from "@reduxjs/toolkit";
import { call, put, takeLatest } from "redux-saga/effects";

import { ApiError, NetworkError } from "@/lib/api/client";
import { authApi, CurrentUser, LoginInput, RegisterInput, TokenPair } from "@/lib/api/auth";
import { tokenStorage } from "@/lib/auth/tokenStorage";
import { toastShown } from "@/components/ui/toastSlice";

export interface AuthUser {
  id: string;
  email: string;
  roles: string[];
  firstName: string | null;
  lastName: string | null;
}

type SessionStatus = "hydrating" | "authenticated" | "unauthenticated";
type FormStatus = "idle" | "submitting" | "failed";

interface AuthState {
  status: SessionStatus;
  user: AuthUser | null;
  login: { status: FormStatus; error: string | null };
  register: { status: FormStatus; error: string | null };
}

const initialState: AuthState = {
  status: "hydrating",
  user: null,
  login: { status: "idle", error: null },
  register: { status: "idle", error: null },
};

function userFromTokenPair(tokens: Pick<TokenPair, "userId" | "email" | "roles">): AuthUser {
  return { id: tokens.userId, email: tokens.email, roles: tokens.roles, firstName: null, lastName: null };
}

const authSlice = createSlice({
  name: "auth",
  initialState,
  reducers: {
    authHydrated(state) {
      const stored = tokenStorage.load();
      if (stored) {
        state.status = "authenticated";
        state.user = userFromTokenPair(stored);
      } else {
        state.status = "unauthenticated";
      }
    },

    loginRequested(state, _action: PayloadAction<LoginInput>) {
      state.login = { status: "submitting", error: null };
    },
    loginSucceeded(state, action: PayloadAction<TokenPair>) {
      state.login = { status: "idle", error: null };
      state.status = "authenticated";
      state.user = userFromTokenPair(action.payload);
    },
    loginFailed(state, action: PayloadAction<string>) {
      state.login = { status: "failed", error: action.payload };
    },

    registerRequested(state, _action: PayloadAction<RegisterInput>) {
      state.register = { status: "submitting", error: null };
    },
    registerSucceeded(state, action: PayloadAction<TokenPair>) {
      state.register = { status: "idle", error: null };
      state.status = "authenticated";
      state.user = userFromTokenPair(action.payload);
    },
    registerFailed(state, action: PayloadAction<string>) {
      state.register = { status: "failed", error: action.payload };
    },

    profileLoaded(state, action: PayloadAction<CurrentUser>) {
      if (!state.user) return;
      state.user.firstName = action.payload.firstName;
      state.user.lastName = action.payload.lastName;
    },

    logoutRequested() {
      // handled by saga; state clears in loggedOut once the API call (best-effort) settles
    },
    loggedOut(state) {
      state.status = "unauthenticated";
      state.user = null;
    },

    sessionExpired(state) {
      state.status = "unauthenticated";
      state.user = null;
    },
  },
});

export const {
  authHydrated,
  loginRequested,
  loginSucceeded,
  loginFailed,
  registerRequested,
  registerSucceeded,
  registerFailed,
  profileLoaded,
  logoutRequested,
  loggedOut,
  sessionExpired,
} = authSlice.actions;

export const authReducer = authSlice.reducer;

function describeError(err: unknown): string {
  if (err instanceof ApiError) return err.message;
  if (err instanceof NetworkError) return err.message;
  if (err instanceof Error) return err.message;
  return "An unexpected error occurred.";
}

function persistTokenPair(tokens: TokenPair) {
  tokenStorage.save(tokens);
}

function* loginWorker(action: ReturnType<typeof loginRequested>) {
  try {
    const tokens: TokenPair = yield call(authApi.login, action.payload);
    persistTokenPair(tokens);
    yield put(loginSucceeded(tokens));
    try {
      const profile: CurrentUser = yield call(authApi.me);
      yield put(profileLoaded(profile));
    } catch {
      // Non-fatal — the sidebar just shows the email instead of a full name.
    }
  } catch (err) {
    yield put(loginFailed(describeError(err)));
  }
}

function* registerWorker(action: ReturnType<typeof registerRequested>) {
  try {
    const tokens: TokenPair = yield call(authApi.register, action.payload);
    persistTokenPair(tokens);
    yield put(registerSucceeded(tokens));
  } catch (err) {
    yield put(registerFailed(describeError(err)));
  }
}

function* logoutWorker() {
  const stored = tokenStorage.load();
  if (stored) {
    try {
      yield call(authApi.logout, stored.refreshToken);
    } catch {
      // Best-effort: the refresh token may already be expired/revoked — log the user out
      // locally regardless, since that's what they asked for.
    }
  }
  tokenStorage.clear();
  yield put(loggedOut());
  yield put(toastShown("success", "Signed out."));
}

export function* authSaga() {
  yield takeLatest(loginRequested.type, loginWorker);
  yield takeLatest(registerRequested.type, registerWorker);
  yield takeLatest(logoutRequested.type, logoutWorker);
}
