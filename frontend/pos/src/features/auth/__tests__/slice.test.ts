import { beforeEach, describe, expect, it } from "vitest";
import {
  authReducer,
  authHydrated,
  loginRequested,
  loginSucceeded,
  loginFailed,
  profileLoaded,
  loggedOut,
  sessionExpired,
} from "../slice";
import { tokenStorage } from "@/lib/auth/tokenStorage";
import { TokenPair } from "@/lib/api/auth";

const tokens: TokenPair = {
  accessToken: "access-1",
  accessTokenExpiresAtUtc: "2030-01-01T00:00:00Z",
  refreshToken: "refresh-1",
  refreshTokenExpiresAtUtc: "2030-02-01T00:00:00Z",
  userId: "u1",
  email: "owner@shop.test",
  roles: ["Owner"],
};

describe("auth slice", () => {
  beforeEach(() => {
    tokenStorage.clear();
  });

  it("starts in the hydrating state", () => {
    const state = authReducer(undefined, { type: "@@INIT" });
    expect(state.status).toBe("hydrating");
    expect(state.user).toBeNull();
  });

  it("hydrates to unauthenticated when nothing is stored", () => {
    const state = authReducer(undefined, authHydrated());
    expect(state.status).toBe("unauthenticated");
  });

  it("hydrates to authenticated from a stored session", () => {
    tokenStorage.save(tokens);
    const state = authReducer(undefined, authHydrated());
    expect(state.status).toBe("authenticated");
    expect(state.user).toMatchObject({ id: "u1", email: "owner@shop.test", roles: ["Owner"] });
  });

  it("marks the login form submitting on request", () => {
    const state = authReducer(undefined, loginRequested({ email: "a@b.com", password: "x" }));
    expect(state.login.status).toBe("submitting");
  });

  it("authenticates on login success", () => {
    const state = authReducer(undefined, loginSucceeded(tokens));
    expect(state.status).toBe("authenticated");
    expect(state.user?.email).toBe("owner@shop.test");
    expect(state.login.status).toBe("idle");
  });

  it("records the error message on login failure", () => {
    const state = authReducer(undefined, loginFailed("Invalid credentials"));
    expect(state.login.status).toBe("failed");
    expect(state.login.error).toBe("Invalid credentials");
  });

  it("fills in the full name once the profile loads", () => {
    let state = authReducer(undefined, loginSucceeded(tokens));
    state = authReducer(
      state,
      profileLoaded({
        id: "u1",
        email: "owner@shop.test",
        firstName: "Dana",
        lastName: "Owner",
        phoneNumber: null,
        isEmailVerified: true,
        createdAtUtc: "2026-01-01T00:00:00Z",
        lastLoginAtUtc: null,
        roles: ["Owner"],
      })
    );
    expect(state.user?.firstName).toBe("Dana");
    expect(state.user?.lastName).toBe("Owner");
  });

  it("clears the session on logout", () => {
    let state = authReducer(undefined, loginSucceeded(tokens));
    state = authReducer(state, loggedOut());
    expect(state.status).toBe("unauthenticated");
    expect(state.user).toBeNull();
  });

  it("clears the session when it expires", () => {
    let state = authReducer(undefined, loginSucceeded(tokens));
    state = authReducer(state, sessionExpired());
    expect(state.status).toBe("unauthenticated");
    expect(state.user).toBeNull();
  });
});
