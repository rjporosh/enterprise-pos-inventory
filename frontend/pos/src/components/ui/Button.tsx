"use client";

import { ButtonHTMLAttributes, forwardRef } from "react";

type Variant = "primary" | "secondary" | "danger" | "ghost";
type Size = "sm" | "md" | "lg";

interface ButtonProps extends ButtonHTMLAttributes<HTMLButtonElement> {
  variant?: Variant;
  size?: Size;
  block?: boolean;
  loading?: boolean;
}

export const Button = forwardRef<HTMLButtonElement, ButtonProps>(
  ({ variant = "primary", size = "md", block, loading, disabled, className, children, ...rest }, ref) => {
    const sizeClass = size === "sm" ? "btn-sm" : size === "lg" ? "btn-lg" : "";
    return (
      <button
        ref={ref}
        className={["btn", `btn-${variant}`, sizeClass, block ? "btn-block" : "", className ?? ""]
          .filter(Boolean)
          .join(" ")}
        disabled={disabled || loading}
        aria-busy={loading || undefined}
        {...rest}
      >
        {loading ? "…" : children}
      </button>
    );
  }
);
Button.displayName = "Button";
