import type { Metadata } from "next";
import "./globals.css";
import "@/components/ui/ui.css";
import { StoreProvider } from "@/lib/store/StoreProvider";
import { AppShell } from "@/components/layout/AppShell";

export const metadata: Metadata = {
  title: "Inventory — Enterprise POS & Inventory",
  description: "Product, stock and inventory administration.",
};

export default function RootLayout({ children }: { children: React.ReactNode }) {
  return (
    <html lang="en">
      <body>
        <StoreProvider>
          <AppShell>{children}</AppShell>
        </StoreProvider>
      </body>
    </html>
  );
}
