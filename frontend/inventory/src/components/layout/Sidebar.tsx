"use client";

import Link from "next/link";
import { usePathname } from "next/navigation";
import { logoutRequested } from "@/features/auth/slice";
import { useAppDispatch, useAppSelector } from "@/lib/store/hooks";

const links = [
  { href: "/", label: "Dashboard", icon: "◆" },
  { href: "/products", label: "Products", icon: "▤" },
  { href: "/stock", label: "Stock", icon: "▦" },
];

export function Sidebar() {
  const pathname = usePathname();
  const dispatch = useAppDispatch();
  const user = useAppSelector((s) => s.auth.user);

  return (
    <nav className="sidebar" aria-label="Main navigation">
      <div className="sidebar-brand">
        Inventory
        <span>Enterprise POS &amp; Inventory</span>
      </div>
      {links.map((link) => {
        const active = link.href === "/" ? pathname === "/" : pathname.startsWith(link.href);
        return (
          <Link key={link.href} href={link.href} className={`sidebar-link${active ? " active" : ""}`}>
            <span aria-hidden="true">{link.icon}</span>
            {link.label}
          </Link>
        );
      })}

      {user ? (
        <div className="sidebar-user">
          <div className="sidebar-user-info">
            <span className="sidebar-user-name">
              {user.firstName ? `${user.firstName} ${user.lastName ?? ""}`.trim() : user.email}
            </span>
            {user.firstName ? <span className="sidebar-user-email">{user.email}</span> : null}
          </div>
          <button
            type="button"
            className="sidebar-logout"
            onClick={() => dispatch(logoutRequested())}
            title="Sign out"
          >
            ↪
          </button>
        </div>
      ) : null}
    </nav>
  );
}
