"use client";

import { ReactNode, useEffect } from "react";
import { usePathname, useRouter } from "next/navigation";
import { Topbar } from "./Topbar";
import { ToastStack } from "@/components/ui";
import { useAppSelector } from "@/lib/store/hooks";

const PUBLIC_PATHS = new Set(["/login"]);

export function AppShell({ children }: { children: ReactNode }) {
  const pathname = usePathname();
  const router = useRouter();
  const authStatus = useAppSelector((s) => s.auth.status);
  const isPublicPath = PUBLIC_PATHS.has(pathname);

  useEffect(() => {
    if (authStatus === "unauthenticated" && !isPublicPath) {
      router.replace("/login");
    }
  }, [authStatus, isPublicPath, router]);

  if (isPublicPath) {
    return (
      <>
        {children}
        <ToastStack />
      </>
    );
  }

  // "hydrating" (session not yet read from storage) or "unauthenticated" (redirect in flight):
  // render nothing rather than flash protected content or a topbar with no user in it.
  if (authStatus !== "authenticated") {
    return null;
  }

  return (
    <div>
      <Topbar />
      <main style={{ padding: "20px 24px 64px", maxWidth: 1100, margin: "0 auto" }}>{children}</main>
      <ToastStack />
    </div>
  );
}
