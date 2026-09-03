# Localization

Resource-based, English default, Bangla supported. `SharedWeb.PlatformLocalization`
(`shared/shared-web/src/Localization/`).

## How a request's culture is chosen

`AddPlatformLocalization()` + `app.UsePlatformLocalization()` (in every service's `Program.cs`).
`PlatformRequestCultureProvider` tries, in order:

1. `?lang=bn` query parameter
2. `Accept-Language` header (first supported match)
3. a `locale` / `culture` claim on the authenticated user
4. `en` (default)

Only cultures in `PlatformLocalization.SupportedCultures` (`en`, `bn`) are honored.
`CultureInfo.CurrentCulture` / `CurrentUICulture` are set for the request and restored after.

## The three localizable surfaces

| Surface | How it's localized | Status |
|---|---|---|
| Envelope strings (`message`, generic error text) | `shared-web/src/Resources/PlatformMessages[.<c>].resx` | ✅ en + bn |
| Domain `Error.Description` (per handler) | add a resx key **named exactly after the `Error.Code`** (e.g. `PRODUCT_SKU_EXISTS`) → mapper picks it up; else the handler's English text is kept | incremental — add keys as needed, no handler change |
| FluentValidation `.WithMessage("…")` literals | not yet — use `IStringLocalizer` in the validator, or move the text to a resx key | incremental |
| Frontend UI strings | `next-intl` in `frontend/*` (`messages/{en,bn}.json`) | milestone M4 |

## Making a message translatable

**A cross-cutting string** → add a `<data name="…">` to
`shared/shared-web/src/Resources/PlatformMessages.resx` **and** `PlatformMessages.bn.resx`, then
read it with `PlatformMessages.Get("YourKey", "English fallback")`.

**A domain error** → in `PlatformMessages.resx` / `.bn.resx` add
`<data name="PRODUCT_SKU_EXISTS"><value>…</value></data>`. Done — the envelope mapper resolves it
for the request culture, English fallback.

## Requirement: ICU

Localization needs real culture data. The `aspnet:*-alpine` base image ships none — every
service Dockerfile installs `icu-libs` and sets `DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false`.
Without it a container crash-loops on startup with `CultureNotFoundException` (exit 139).

See [adding-a-language.md](adding-a-language.md) to add a third language.
