"use client";

import Link from "next/link";
import { usePathname } from "next/navigation";

const links = [
  { href: "/", label: "Dashboard", icon: "◆" },
  { href: "/products", label: "Products", icon: "▤" },
  { href: "/stock", label: "Stock", icon: "▦" },
];

export function Sidebar() {
  const pathname = usePathname();
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
    </nav>
  );
}
