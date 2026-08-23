import { ReactNode } from "react";

export function EmptyState({
  title,
  description,
  action,
  icon = "▢",
}: {
  title: string;
  description?: string;
  action?: ReactNode;
  icon?: string;
}) {
  return (
    <div className="state-block">
      <div style={{ fontSize: 28, color: "var(--color-text-faint)" }} aria-hidden="true">
        {icon}
      </div>
      <div className="state-block-title">{title}</div>
      {description && <div className="state-block-desc">{description}</div>}
      {action}
    </div>
  );
}
