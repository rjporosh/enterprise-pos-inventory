"use client";

import Link from "next/link";
import { usePathname } from "next/navigation";
import { useAppDispatch, useAppSelector } from "@/lib/store/hooks";
import { Badge } from "@/components/ui";
import { logoutRequested } from "@/features/auth/slice";

const links = [
  { href: "/", label: "Terminal" },
  { href: "/sales", label: "Sale history" },
  { href: "/reports", label: "Daily report" },
];

export function Topbar() {
  const pathname = usePathname();
  const dispatch = useAppDispatch();
  const openSession = useAppSelector((s) => s.session.openSession);
  const user = useAppSelector((s) => s.auth.user);

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
      <div style={{ display: "flex", alignItems: "center", gap: 16 }}>
        {openSession ? (
          <Badge tone="success">Session open · {openSession.openingBalance.toFixed(2)} opening</Badge>
        ) : (
          <Badge tone="warning">No cash session open</Badge>
        )}
        {user ? (
          <div className="topbar-user">
            <span className="topbar-user-email">
              {user.firstName ? `${user.firstName} ${user.lastName ?? ""}`.trim() : user.email}
            </span>
            <button
              type="button"
              className="topbar-logout"
              onClick={() => dispatch(logoutRequested())}
              title="Sign out"
            >
              ↪
            </button>
          </div>
        ) : null}
      </div>
    </header>
  );
}
