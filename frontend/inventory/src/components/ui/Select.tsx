import { SelectHTMLAttributes, forwardRef } from "react";

interface SelectProps extends SelectHTMLAttributes<HTMLSelectElement> {
  hasError?: boolean;
}

export const Select = forwardRef<HTMLSelectElement, SelectProps>(
  ({ hasError, className, children, ...rest }, ref) => (
    <select
      ref={ref}
      className={["select", hasError ? "has-error" : "", className ?? ""].filter(Boolean).join(" ")}
      {...rest}
    >
      {children}
    </select>
  )
);
Select.displayName = "Select";
