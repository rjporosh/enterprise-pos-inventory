# Adding a language

Example: add Hindi (`hi`).

## Backend (all 5 services at once — it's one shared layer)

1. `shared/shared-web/src/Localization/PlatformLocalization.cs` — add the code:
   ```csharp
   public static readonly IReadOnlyList<string> SupportedCultures = new[] { "en", "bn", "hi" };
   ```
2. Copy `shared/shared-web/src/Resources/PlatformMessages.bn.resx` to `PlatformMessages.hi.resx`
   and translate every `<value>`. (Keys with no `hi` translation fall back to English
   automatically — you can ship partial and fill in later.)
3. `dotnet build shared/shared-web/src/shared-web.csproj` — the `hi` satellite assembly builds
   automatically.
4. Per-service domain resx that you want translated: `notification`'s
   `NotificationService.Infrastructure/Localization/Resources/Messages.hi.resx`, etc.
5. Rebuild the Docker images (the `icu-libs` already in every Dockerfile covers all cultures).

That's it — `?lang=hi` and `Accept-Language: hi` now work platform-wide.

## Frontend (per app, milestone M4 onward)

1. `frontend/<app>/messages/hi.json` — copy `bn.json`, translate.
2. `frontend/<app>/src/i18n/…` — add `hi` to the supported-locales list.
3. Add it to the language switcher options.
4. `npm run typecheck && npm run lint && npm test && npm run build`.

## Verify

```bash
curl -s -X POST "http://localhost:5010/api/v1/products?lang=hi" -H 'Content-Type: application/json' -d '{}'
# -> "message" should be the Hindi validation string
```
