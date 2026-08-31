import { defineConfig } from "vitest/config";
import path from "path";

export default defineConfig({
  // This project's tsconfig.json uses "jsx": "preserve" (Next/SWC handles the actual transform
  // for the app build). Vitest doesn't go through Next's compiler, so esbuild needs to be told
  // explicitly to use the automatic JSX runtime -- otherwise it falls back to the classic
  // transform, which requires `React` to be in scope in every file containing JSX. This wasn't
  // needed before because no *.tsx test files existed yet (see BarcodeLabel.test.tsx, the first).
  esbuild: {
    jsx: "automatic",
  },
  test: {
    environment: "jsdom",
    // Without an explicit URL, jsdom serves pages from the opaque "about:blank" origin, where
    // `window.localStorage` is undefined rather than a Storage instance (this is spec-correct
    // jsdom behavior, not a bug) — needed once any test touches localStorage (see
    // lib/auth/tokenStorage.ts), which no test did before features/auth/__tests__/slice.test.ts.
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
