import { ReactNode } from "react";
import { Sidebar } from "./Sidebar";
import { ToastStack } from "@/components/ui";

export function AppShell({ children }: { children: ReactNode }) {
  return (
    <div className="app-shell">
      <Sidebar />
      <main className="app-main">{children}</main>
      <ToastStack />
    </div>
  );
}
