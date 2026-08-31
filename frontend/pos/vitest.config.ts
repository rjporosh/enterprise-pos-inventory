import { defineConfig } from "vitest/config";
import path from "path";

export default defineConfig({
  test: {
    environment: "jsdom",
    // Without an explicit URL, jsdom serves pages from the opaque "about:blank" origin, where
    // `window.localStorage` is undefined rather than a Storage instance (this is spec-correct
    // jsdom behavior, not a bug) — needed once any test touches localStorage (see
    // lib/auth/tokenStorage.ts).
    environmentOptions: {
      jsdom: { url: "http://localhost" },
    },
    globals: true,
    setupFiles: ["./vitest.setup.ts"],
  },
  resolve: {
    alias: { "@": path.resolve(__dirname, "./src") },
  },
});
