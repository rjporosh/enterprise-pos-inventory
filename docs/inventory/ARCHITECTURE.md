# Inventory Frontend — Architecture

## Stack

- Next.js 15 (App Router), React 19, TypeScript (strict, `noUncheckedIndexedAccess: true`)
- Redux Toolkit + redux-saga for all server-state and async workflows
- No CSS framework — a hand-written design-system CSS file (`components/ui/ui.css`) with CSS
  custom properties as design tokens (`app/globals.css`), applied via plain className strings
- Vitest + Testing Library for unit tests

## Data flow (the one pattern used everywhere)

```
Page component (app/**/page.tsx)
  → dispatch(xRequested(params))          [Redux Toolkit action]
      → saga watcher (takeLatest)          [features/x/slice.ts]
          → call(xApi.method, params)      [lib/api/x.ts — typed fetch]
              → backend
          ← put(xLoaded(result)) | put(xFailed(message))
  ← useAppSelector(state => state.x...)    [re-render with new state]
```

Mutations (create/update/delete) follow the same shape with their own status field
(`idle | saving | succeeded | failed`) so a page can show a spinner on just the button that
triggered it, not the whole page.

## Why redux-saga generators instead of thunks/RTK Query

This was a technology requirement from the project brief, not a preference decision made here.
Given that constraint: sagas are used for **every** async operation (list fetch, create, update,
delete, stock movements) for consistency — there's exactly one way to make a server call in this
codebase, which is what makes `docs/inventory/ADDING-A-CRUD.md` a copy-paste-and-rename exercise
rather than a series of one-off decisions.

## API client layer (`lib/api/`)

- `client.ts` is the only place that calls `fetch`. It:
  - reads `NEXT_PUBLIC_INVENTORY_API_URL` and throws a clear `ApiError` if it's unset (rather than
    a confusing network error)
  - parses RFC7807 `ProblemDetails` error bodies into a typed `ApiError` (`.message`, `.status`,
    `.isValidation`, `.isNotFound`)
  - distinguishes `NetworkError` (fetch itself failed — offline, DNS, CORS) from `ApiError`
    (backend responded with a non-2xx status)
  - handles `204 No Content` responses (several endpoints return no body)
- `products.ts` / `stock.ts` are thin, fully-typed wrappers: one function per endpoint, typed
  request and response shapes copied directly from the C# DTOs (see file-level comments citing
  the source DTO). No business logic lives here — just the HTTP call and its types.

## State layer (`features/*/slice.ts`)

Each feature slice contains, in one file:
- The Redux Toolkit `createSlice` (state shape + reducers + actions)
- The saga workers (one generator function per async operation)
- The saga watcher (`export function* xSaga()`), registered once in `lib/store/store.ts`

This is a deliberate deviation from splitting slice/saga into separate files — for a CRUD feature
of this size, keeping the state transitions and the async logic that drives them in one file makes
it easier to see the whole lifecycle of one operation (e.g. `productCreateRequested` →
`createProductWorker` → `productCreateSucceeded`/`productCreateFailed`) without file-switching.

## Design system (`components/ui/`)

Plain React components + one shared `ui.css`. No Tailwind, no CSS-in-JS runtime — chosen for build
simplicity and to keep the two apps (`inventory`, `pos`) independently buildable without a shared
build pipeline. Design tokens (colors, spacing, radii, shadows) are CSS custom properties in
`app/globals.css`; components reference them via `var(--color-primary)` etc., never hardcoded hex
values, so a future rebrand is a one-file change.

## Validation (`features/*/validation.ts`)

Client-side validation mirrors the backend's FluentValidation rules (see comments in
`features/products/validation.ts` citing which validator it mirrors) so the form fails fast for
the common cases. It is explicitly **not** a replacement for server-side validation — every form
also surfaces `serverError` from a failed API call, because the backend remains authoritative.

## Routing

Standard Next.js App Router file-based routing. Route params come from folder names
(`app/products/[id]/page.tsx`). No route-level data fetching (`generateStaticParams`, server
components fetching data) is used — every page is a client component (`"use client"`) that
dispatches into Redux on mount, keeping the entire data-fetching story in one place (sagas) rather
than split between server components and client-side Redux.

## What's intentionally NOT here

- No authentication — see `docs/API-GAPS.md`. No auth token handling exists anywhere in
  `lib/api/client.ts`; there's a comment marking where it would go.
- No multi-tenant/branch switching — single implicit tenant for this MVP.
- No offline support.
