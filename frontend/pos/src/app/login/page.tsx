"use client";

import { FormEvent, useEffect, useState } from "react";
import { useRouter } from "next/navigation";
import { Button, Field, Input } from "@/components/ui";
import { loginRequested } from "@/features/auth/slice";
import { useAppDispatch, useAppSelector } from "@/lib/store/hooks";

export default function LoginPage() {
  const dispatch = useAppDispatch();
  const router = useRouter();
  const authStatus = useAppSelector((s) => s.auth.status);
  const loginState = useAppSelector((s) => s.auth.login);

  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");

  useEffect(() => {
    if (authStatus === "authenticated") {
      router.replace("/");
    }
  }, [authStatus, router]);

  function handleSubmit(e: FormEvent) {
    e.preventDefault();
    dispatch(loginRequested({ email, password }));
  }

  return (
    <div className="auth-page">
      <div className="auth-card">
        <div className="auth-brand">
          <span className="auth-brand-mark" aria-hidden="true">
            ▣
          </span>
          <div>
            <div className="auth-brand-title">POS</div>
            <div className="auth-brand-subtitle">Enterprise POS &amp; Inventory</div>
          </div>
        </div>

        <h1 className="auth-heading">Sign in</h1>
        <p className="auth-subheading">Cashier access for checkout and cash session management.</p>

        <form onSubmit={handleSubmit} className="auth-form" noValidate>
          <Field label="Email" htmlFor="email" required>
            <Input
              id="email"
              type="email"
              autoComplete="email"
              required
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              placeholder="you@company.com"
            />
          </Field>
          <Field label="Password" htmlFor="password" required>
            <Input
              id="password"
              type="password"
              autoComplete="current-password"
              required
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              placeholder="••••••••"
            />
          </Field>

          {loginState.status === "failed" && loginState.error ? (
            <div className="auth-error" role="alert">
              {loginState.error}
            </div>
          ) : null}

          <Button type="submit" block loading={loginState.status === "submitting"}>
            Sign in
          </Button>
        </form>
      </div>
    </div>
  );
}
