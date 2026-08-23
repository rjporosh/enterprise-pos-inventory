# Rules for Future AI Agents

This file exists because this project has been, and will likely continue to be, worked on across
multiple AI coding sessions. Read this before touching `frontend/`.

## Before writing anything

1. Read `AI-HANDOVER.md` in full. It has the real, current state — don't trust your own guess at
   what's implemented.
2. Read `docs/API-GAPS.md`. It's the source of truth for what the backend actually supports.
3. Inspect the existing code in `frontend/inventory/src/features/products` (the reference CRUD
   implementation) before adding anything new. Reuse its pattern.

## Hard rules

- **Don't invent backend endpoints.** If a feature needs an endpoint that doesn't exist, add it to
  `docs/API-GAPS.md` and either skip the feature or build the honestly-degraded version (manual
  GUID input, "coming soon" state, etc.) — see existing examples in the Products/Stock/POS forms.
- **Don't fabricate metrics or data.** Every number shown in the UI must come from a real API
  response. If data isn't available, show an empty/unavailable state, not a placeholder number.
- **Don't introduce a second state-management approach.** This project uses Redux Toolkit +
  redux-saga throughout. Don't add React Query, Zustand, SWR, MobX, or raw Context-based global
  state alongside it.
- **Don't duplicate design-system components.** Check `components/ui/` in the relevant app before
  creating a new Button/Input/Modal/etc. If an existing component is close but not quite right,
  extend it with a new prop rather than forking it.
- **Don't bypass client-side validation** in `features/<x>/validation.ts` files — but also
  remember it's a UX nicety, not a security boundary; the backend's FluentValidation is
  authoritative and errors from it must still surface to the user (see `serverError` props on the
  form components).
- **Don't silently change an API client's request/response shape** without re-reading the actual
  backend DTO/controller first. The DTOs in `lib/api/*.ts` were written by reading the C# source
  directly — if the backend changes, re-read the source, don't guess.
- **Don't rewrite working architecture unnecessarily.** If a page/slice/saga already does the job,
  extend it. A full rewrite needs a real reason (not "I'd have done it differently"), stated
  explicitly to the user before doing it.
- **Preserve TypeScript strictness.** Both `tsconfig.json` files have `strict: true` and
  `noUncheckedIndexedAccess: true`. Don't loosen either to make an error go away — fix the code.
- **The two apps (`frontend/pos`, `frontend/inventory`) are independently deployable.** Code is
  deliberately duplicated between them (the design system, the API client shell) rather than
  shared via a workspace package. Don't "fix" this by merging them into a shared package unless
  explicitly asked — that would break independent deployability.

## Definition of done, for any change

Run, from the relevant app directory (`frontend/inventory` or `frontend/pos`):

```bash
npm install      # only if package.json changed
npm run typecheck
npm run lint
npm test
npm run build
```

All four must pass before a change is considered complete. If one fails for a reason unrelated to
your change (e.g. a pre-existing issue), say so explicitly — don't silently mark the task done.

## After a meaningful change

- Update `AI-HANDOVER.md`'s checklist (DONE/PARTIAL/TODO) to match reality.
- Update the relevant per-app docs if the architecture, folder structure, or a documented pattern
  changed.
- Update `docs/API-GAPS.md` if you discovered a new gap, or if a gap was closed by a backend change.
- Do not mark something DONE in any doc unless you've actually run the verification commands above
  against it in this session.
