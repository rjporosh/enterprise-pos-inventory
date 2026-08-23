import { CSSProperties, ReactNode } from "react";

export function Card({
  children,
  className = "",
  padded = true,
  style,
}: {
  children: ReactNode;
  className?: string;
  padded?: boolean;
  style?: CSSProperties;
}) {
  return (
    <div className={["card", padded ? "card-padded" : "", className].filter(Boolean).join(" ")} style={style}>
      {children}
    </div>
  );
}
