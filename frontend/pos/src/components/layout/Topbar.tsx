"use client";

import Link from "next/link";
import { usePathname } from "next/navigation";
import { useAppSelector } from "@/lib/store/hooks";
import { Badge } from "@/components/ui";

const links = [
  { href: "/", label: "Terminal" },
  { href: "/sales", label: "Sale history" },
  { href: "/reports", label: "Daily report" },
];

export function Topbar() {
  const pathname = usePathname();
  const openSession = useAppSelector((s) => s.session.openSession);

  return (
    <header
      style={{
        display: "flex",
        alignItems: "center",
        justifyContent: "space-between",
        padding: "12px 20px",
        background: "#10231e",
        color: "#fff",
      }}
    >
      <div style={{ display: "flex", alignItems: "center", gap: 24 }}>
        <strong style={{ fontSize: 15 }}>POS</strong>
        <nav style={{ display: "flex", gap: 4 }}>
          {links.map((l) => (
            <Link
              key={l.href}
              href={l.href}
              style={{
                padding: "7px 12px",
                borderRadius: 6,
                fontSize: 13.5,
                fontWeight: 600,
                color: pathname === l.href ? "#fff" : "#c7d2cd",
                background: pathname === l.href ? "var(--color-primary)" : "transparent",
                textDecoration: "none",
              }}
            >
              {l.label}
            </Link>
          ))}
        </nav>
      </div>
      <div>
        {openSession ? (
          <Badge tone="success">Session open · {openSession.openingBalance.toFixed(2)} opening</Badge>
        ) : (
          <Badge tone="warning">No cash session open</Badge>
        )}
      </div>
    </header>
  );
}
