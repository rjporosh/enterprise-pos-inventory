Read AI-HANDOVER.md first, in full. Then read docs/API-GAPS.md. Do not restart or redesign the
project — both are true and current as of the commit you're reading them at.

Inspect the existing repository yourself before writing anything (`frontend/inventory/src`,
`frontend/pos/src`, `docs/`). Do not assume the handover summary is exhaustive — verify against
the actual files.

Continue from the documented state. Task order:

1. Read AI-HANDOVER.md.
2. Read docs/API-GAPS.md.
3. Inspect the current repository (`frontend/inventory`, `frontend/pos`, `docs/`).
4. Verify the existing implementation still builds/tests clean:
   ```
   cd frontend/inventory && npm install && npm run typecheck && npm run lint && npm test && npm run build
   cd ../pos && npm install && npm run typecheck && npm run lint && npm test && npm run build
   ```
5. If a running instance of inventory-service/pos-service is available, point each app's
   `.env.local` at it and manually walk the full flow (see AI-HANDOVER.md §H) — this is the
   highest-value unverified step from the prior session.
6. Only after 4 (and ideally 5) pass, consider new work. Do NOT implement anything from the
   "deferred" rows in docs/API-GAPS.md (auth, multi-tenancy, subscriptions, offline sync,
   returns/refunds, extra report types) unless explicitly asked — those are out of MVP scope by
   design, not by omission.
7. If asked to add a new entity/CRUD feature, follow docs/inventory/ADDING-A-CRUD.md exactly.
8. Run typecheck/lint/test/build again after any change, for whichever app(s) you touched.
9. Fix only genuine errors your change caused. Do not "fix" pre-existing patterns you'd have done
   differently — see docs/AI-CODING-RULES.md.
10. Update AI-HANDOVER.md's checklists (§C/§D) to match reality — mark items DONE only after
    verifying them yourself in this session, not by trusting the prior write-up.
11. Update docs/API-GAPS.md if you closed a gap (backend endpoint added) or found a new one.
12. Commit with descriptive, professional messages, one commit per logical unit of work — do not
    squash, rebase, reset, or force-push existing history.
13. If asked to produce a delivery ZIP, include the full repo (source, docs, tests,
    package manifests, .git/) and exclude only `node_modules/`, `.next/`, `.env.local`, and other
    generated/cache artifacts already covered by `.gitignore`.

Do NOT:
- Rewrite working features because you'd have architected them differently.
- Invent backend APIs, mock data, or fabricated metrics.
- Replace Redux + redux-saga with another state-management approach.
- Introduce dependencies not already justified by the existing stack.
- Mark a TODO/PARTIAL item as DONE without actually running the verification commands against it.
- Stop after generating code without running typecheck/lint/test/build.
- Touch `services/inventory-service` or `services/pos-service` (the backend) unless the task
  explicitly asks for a backend change — this frontend work has intentionally never modified them.
