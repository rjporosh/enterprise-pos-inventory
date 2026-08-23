import { ReactNode } from "react";
import { Topbar } from "./Topbar";
import { ToastStack } from "@/components/ui";

export function AppShell({ children }: { children: ReactNode }) {
  return (
    <div>
      <Topbar />
      <main style={{ padding: "20px 24px 64px", maxWidth: 1100, margin: "0 auto" }}>{children}</main>
      <ToastStack />
    </div>
  );
}
