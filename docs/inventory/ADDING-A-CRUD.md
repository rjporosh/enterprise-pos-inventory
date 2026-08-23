# Adding a New CRUD Feature (Inventory app)

Worked example: adding "Categories" (list/create/edit/delete). Follow this exact file order.
**Prerequisite: the backend endpoint must already exist.** If it doesn't, add it to
`docs/API-GAPS.md` instead — do not invent it.

## 1. Types + API client — `src/lib/api/categories.ts`

```ts
import { apiClient, PagedResult } from "./client";

export interface Category { id: string; name: string; }
export interface CategoryListParams { pageNumber?: number; pageSize?: number; searchTerm?: string; }
export interface CreateCategoryInput { name: string; }
export interface UpdateCategoryInput extends CreateCategoryInput { id: string; }

export const categoriesApi = {
  list: (params: CategoryListParams = {}) =>
    apiClient.get<PagedResult<Category>>("/api/v1/categories", { pageNumber: params.pageNumber ?? 1, pageSize: params.pageSize ?? 20, searchTerm: params.searchTerm }),
  getById: (id: string) => apiClient.get<Category>(`/api/v1/categories/${id}`),
  create: (input: CreateCategoryInput) => apiClient.post<string>("/api/v1/categories", input),
  update: (input: UpdateCategoryInput) => apiClient.put<void>(`/api/v1/categories/${input.id}`, input),
  remove: (id: string) => apiClient.delete<void>(`/api/v1/categories/${id}`),
};
```

Copy the exact request/response shape from the backend DTO/controller — don't guess field names.

## 2. Validation — `src/features/categories/validation.ts`

Mirror the backend's validator rules. Follow `features/products/validation.ts` for the shape:
`emptyForm`, `FormValues` type, `FormErrors` type, `validateForm(values)`, `toCreateInput(values)`.

## 3. Slice + saga — `src/features/categories/slice.ts`

Copy `src/features/products/slice.ts` wholesale and rename: `products` → `categories`,
`Product` → `Category`, drop the fields Categories doesn't need (categories are simpler than
products — you likely only need `list`, `create`, `update`, `remove`, no separate `detail` fetch
if the list item already has everything). Keep the same action-naming and status-string
conventions (see PROGRAMMER-GUIDE.md).

## 4. Register the saga — `src/lib/store/store.ts`

```ts
import { categoriesReducer, categoriesSaga } from "@/features/categories/slice";
// in reducer: categories: categoriesReducer,
// in rootSaga: yield all([..., fork(categoriesSaga)]);
```

## 5. Form component — `src/features/categories/components/CategoryForm.tsx`

Copy `features/products/components/ProductForm.tsx`'s shape (props: `initialValues`,
`submitLabel`, `saving`, `serverError`, `onSubmit`, `onCancel`) but with only the fields Categories
needs.

## 6. Pages — `src/app/categories/`

- `page.tsx` — list, copy `app/products/page.tsx`: search box (debounced), table, `Pagination`,
  `ConfirmDialog` for delete.
- `new/page.tsx` — copy `app/products/new/page.tsx`.
- `[id]/page.tsx` — copy `app/products/[id]/page.tsx`.

## 7. Navigation — `src/components/layout/Sidebar.tsx`

Add `{ href: "/categories", label: "Categories", icon: "…" }` to the `links` array.

## 8. Tests — `src/features/categories/__tests__/`

At minimum: `validation.test.ts` (mirror `features/products/__tests__/validation.test.ts`) and,
if the slice has any non-trivial reducer logic beyond the standard CRUD status transitions, a
`slice.test.ts` (mirror `features/stock/__tests__/slice.test.ts`).

## 9. Before committing

```bash
npm run typecheck && npm run lint && npm test && npm run build
```

All four must pass. Update `docs/inventory/README.md`'s feature list and this repo's
`AI-HANDOVER.md` checklist to mark Categories as implemented.
