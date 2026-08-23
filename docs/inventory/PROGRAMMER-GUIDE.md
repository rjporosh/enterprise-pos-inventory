# Inventory Frontend — Programmer Guide

Read `docs/inventory/ARCHITECTURE.md` first for the big picture. This file is the practical
"where does X go" reference.

## Where things belong

| What | Where | Example |
|---|---|---|
| A new API endpoint call | `src/lib/api/<domain>.ts` | `productsApi.list(...)` |
| DTO/request/response types | Same file as the API call that uses them | `CreateProductInput` in `products.ts` |
| Redux state + async logic for a feature | `src/features/<feature>/slice.ts` | `productsSlice`, `productsSaga` |
| Client-side form validation | `src/features/<feature>/validation.ts` | `validateProductForm` |
| Feature-specific components (forms, etc.) | `src/features/<feature>/components/` | `ProductForm.tsx` |
| Reusable, feature-agnostic UI (buttons, tables, modals) | `src/components/ui/` | `Button.tsx`, `Modal.tsx` |
| Layout shell (sidebar, page chrome) | `src/components/layout/` | `AppShell.tsx`, `Sidebar.tsx` |
| A page/route | `src/app/<route>/page.tsx` | `app/products/page.tsx` |

## Naming conventions

- Slice action names read as past-tense events for things that already happened
  (`productsLoaded`, `productCreateSucceeded`), and `*Requested` for the action that kicks off a
  saga (`productsRequested`, `productCreateRequested`). This makes it unambiguous in a saga
  watcher which action starts a worker vs which action is just a state update.
- Status fields are one of a small fixed set of strings, never booleans:
  `"idle" | "loading" | "succeeded" | "failed"` for reads, `"idle" | "saving" | "succeeded" |
  "failed"` for writes. Don't add a second `isLoading: boolean` alongside a status string — check
  `status === "loading"` everywhere.
- File names: `slice.ts` (not `productsSlice.ts` — the feature folder already says what it is),
  `validation.ts`, `components/<PascalCaseName>.tsx`.
- Routes follow REST-ish conventions: list at `/products`, create at `/products/new`, edit at
  `/products/[id]`. Follow this for any new entity.

## The request/response/error pattern, end to end

Using Products as the reference (`src/features/products/slice.ts`):

1. **Action dispatched from a page**: `dispatch(productsRequested({ pageNumber: 1, searchTerm }))`
2. **Reducer sets loading state immediately**: `state.list.status = "loading"`
3. **Saga watcher** (`takeLatest(productsRequested.type, fetchProductsWorker)`) — `takeLatest` so a
   new search cancels an in-flight older one automatically.
4. **Worker calls the typed API client**: `const result = yield call(productsApi.list, action.payload)`
5. **On success**: `yield put(productsLoaded(result))` — reducer sets `status = "succeeded"` and
   stores the result.
6. **On failure**: caught in a try/catch around the `call`, `describeError(err)` turns an
   `ApiError`/`NetworkError`/generic `Error` into a string, `yield put(productsFailed(message))`.
7. **Page reads state**: `const { status, error, result } = useAppSelector(s => s.products.list)`
   and renders `TableSkeleton` / `ErrorState` / `EmptyState` / the actual table based on `status`.

Mutations (create/update/delete) add one more step: on success, also
`yield put(toastShown("success", "..."))` so the user gets confirmation without the page needing
to render anything extra. On failure, also `yield put(toastShown("error", message))` **in addition
to** setting the mutation's own error state — the toast is a passive notification, the inline
`serverError` on the form is what the user actually needs to fix the problem.

## Pagination, search, filters

See `app/products/page.tsx`. Pattern:
- Search input is debounced (350ms `setTimeout`, cleared on each keystroke) before dispatching —
  don't fire a request per keystroke.
- Filter `<Select>` changes dispatch immediately (no debounce needed for a discrete change) and
  reset `pageNumber` to 1.
- `Pagination` component (`components/ui/Pagination.tsx`) is a pure display component — it takes
  `pageNumber`/`pageSize`/`totalCount` and calls `onPageChange(page)`; the page component owns
  actually re-dispatching with the new page number merged into the existing params.

## Delete confirmation

Always via `ConfirmDialog` (`components/ui/ConfirmDialog.tsx`), never a bare `window.confirm` or an
inline "are you sure" toggle. Track the pending target as local `useState` on the page (not in
Redux — it's transient UI state, not server state), e.g. `pendingDeleteId`. On confirm, dispatch
the remove action; on the mutation's `succeeded` status (watched via `useEffect`), clear the local
state and re-run the list load.

## Create/edit form reuse

`ProductForm` (`features/products/components/ProductForm.tsx`) is used by both
`app/products/new/page.tsx` and `app/products/[id]/page.tsx`. It takes `initialValues`,
`submitLabel`, `saving`, `serverError`, and an `isEdit` flag (which toggles the "Active" checkbox,
since a brand-new product doesn't need one). The pages own fetching/dispatching; the form owns
local field state and client-side validation only. Don't push form field state into Redux — it's
not needed anywhere else and would just add reducer boilerplate.

## Typed hooks and selectors

Always use `useAppDispatch`/`useAppSelector` from `lib/store/hooks.ts`, never the raw
`react-redux` `useDispatch`/`useSelector` — the typed versions give you full autocomplete and
type-checking against `RootState`/`AppDispatch` without needing to annotate every call site.

There are no separate `selectors.ts` files in this codebase — state shape is simple enough that
`useAppSelector(s => s.products.list)` inline is clearer than an indirection layer. If a selector
starts being reused in 3+ places or needs memoization (e.g. a derived/computed value), that's the
signal to extract it into a `selectors.ts` in that feature folder.

## Error normalization

All three error types the app can encounter — `ApiError` (backend responded with an error status),
`NetworkError` (fetch failed entirely), and anything else (a bug) — are normalized to a plain
string via each slice's local `describeError(err)` helper before being put into Redux state or a
toast. Redux state should never hold a raw `Error` object (not serializable, breaks
`configureStore`'s default serializability checks if strict mode is added later).
