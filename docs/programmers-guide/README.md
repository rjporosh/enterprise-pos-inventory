# Programmer's Guide

Developer-facing how-to for the cross-cutting platform layers. Start here, then read the specific
guide for the task at hand. All five services (`auth`, `notification`, `inventory`, `pos`, `gateway`)
share these layers via `shared/shared-web` and `shared/shared-kernel` — see
[`decisions/ADR-010`](../../decisions/ADR-010-cross-cutting-web-layer.md).

| Guide | Read it when you are… |
|---|---|
| [result-pattern.md](result-pattern.md) | writing a command/query handler, or deciding failure-vs-exception |
| [exception-handling.md](exception-handling.md) | adding a domain exception, or seeing an unexpected 500 |
| [api-response-contract.md](api-response-contract.md) | writing or consuming an endpoint |
| [localization.md](localization.md) | making a message translatable |
| [adding-a-language.md](adding-a-language.md) | adding support for a new UI/API language |
| [../../MIGRATIONS.md](../../MIGRATIONS.md) | creating or applying an EF Core migration |
| [troubleshooting.md](troubleshooting.md) | a service won't start / behaves oddly |

More guides (logging, database-provider-factory, quartz-jobs, publishing/consuming events, gRPC,
testing) land with milestones M2–M10 — see `AI-HANDOVER.md`.
