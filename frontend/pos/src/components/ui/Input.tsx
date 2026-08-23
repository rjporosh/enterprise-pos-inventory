import { InputHTMLAttributes, forwardRef } from "react";

interface InputProps extends InputHTMLAttributes<HTMLInputElement> {
  hasError?: boolean;
}

export const Input = forwardRef<HTMLInputElement, InputProps>(({ hasError, className, ...rest }, ref) => (
  <input
    ref={ref}
    className={["input", hasError ? "has-error" : "", className ?? ""].filter(Boolean).join(" ")}
    {...rest}
  />
));
Input.displayName = "Input";
