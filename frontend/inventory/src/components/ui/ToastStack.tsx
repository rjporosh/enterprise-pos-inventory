"use client";

import { useEffect } from "react";
import { useAppDispatch, useAppSelector } from "@/lib/store/hooks";
import { toastDismissed } from "./toastSlice";

export function ToastStack() {
  const toasts = useAppSelector((s) => s.toast.items);
  const dispatch = useAppDispatch();

  useEffect(() => {
    if (toasts.length === 0) return;
    const timers = toasts.map((t) =>
      setTimeout(() => dispatch(toastDismissed(t.id)), t.variant === "error" ? 6000 : 3500)
    );
    return () => timers.forEach(clearTimeout);
  }, [toasts, dispatch]);

  if (toasts.length === 0) return null;

  return (
    <div className="toast-stack" role="status" aria-live="polite">
      {toasts.map((t) => (
        <div key={t.id} className={`toast toast-${t.variant}`}>
          <span>{t.message}</span>
          <button className="toast-dismiss" onClick={() => dispatch(toastDismissed(t.id))} aria-label="Dismiss">
            ✕
          </button>
        </div>
      ))}
    </div>
  );
}
