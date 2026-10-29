# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## What this is

`Limbo.Umbraco.Feedback` — a NuGet-distributed Umbraco package (successor to `Skybrud.Umbraco.Feedback`) that lets front-end visitors rate/comment on pages, and lets editors triage those entries from a **workspace view** in the backoffice. One class library plus a TypeScript/Lit client; no test project, no consuming website in the repo.

Targets **.NET 10 / Umbraco 17** (`VersionPrefix` `17.0.0-alpha000`). The Umbraco 13 version lives on `v13/main`. `documentation/UPGRADE-UMBRACO-17.md` records exactly what changed between the two — read it before touching anything that looks like it was ported.

## Commands

```bash
dotnet build src/Limbo.Umbraco.Feedback.sln          # C# only; Debug appends a build<timestamp> version suffix
dotnet pack src/Limbo.Umbraco.Feedback.sln -c Release -p:BuildClientAssets=true

cd src/Limbo.Umbraco.Feedback/Client
npm install && npm run build                          # tsc + vite → ../wwwroot/App_Plugins/Limbo.Umbraco.Feedback
npm run watch
```

The npm build is opt-in from MSBuild (`BuildClientAssets`, default `false`) so a plain `dotnet build` doesn't need Node. `wwwroot/App_Plugins/` is generated and gitignored — never edit it.

There are no tests and no CI workflows. Verification means installing the built package into a real Umbraco 17 site.

## Architecture

### Plugin collection is the extension seam

Nearly all behaviour is resolved through `FeedbackPluginCollection` (an Umbraco `BuilderCollection` of `IFeedbackPlugin`), not through a single service. Plugins answer *which content is a site*, *which ratings/statuses/fields that site has*, *who the assignable users are*, and *whether the feedback workspace view shows*. `FeedbackService` and the controllers delegate to the collection and take the first plugin that returns `true`.

- Consumers subclass `FeedbackPluginBase` (all members virtual, dependencies injected via the single `FeedbackPluginDependencies` wrapper) and register with `builder.FeedbackPlugins().Append<T>()`.
- `DefaultFeedbackPlugin` is registered automatically unless `Limbo:Feedback:DisableDefaultPlugin` is true — read as a raw config value in `FeedbackComposer` because composers can't use DI.
- Default site detection = published content with an assigned Umbraco domain (`IDomainCache.HasAssigned`); `FeedbackSettings.SiteContentTypes` / `PageContentTypes` narrow it by alias.

When adding a capability, decide first whether it belongs on `IFeedbackPlugin` (per-site/overridable) or on `FeedbackService` (global). Adding to `IFeedbackPlugin` is a breaking change for implementors — add a virtual default on `FeedbackPluginBase` at the same time.

### Layers

`FeedbackService` (business logic, plugin events, logging) → `FeedbackDatabaseService` (NPoco + `IScopeProvider`) → `FeedbackEntryDto`. `FeedbackEntry` wraps a DTO; `FeedbackService.GetEntryByKey` is what resolves the DTO's raw GUIDs into `FeedbackRating` / `FeedbackStatus` / `IFeedbackUser` by asking the site's plugin — a bare `FeedbackEntry(dto)` (as used by `GetEntries`) does *not*. `FeedbackApiModelFactory` does the equivalent hydration when mapping lists for the management API.

Plugin hooks come in `-ing`/`-ed` pairs. Every plugin invocation is individually try/caught and logged so one bad plugin can't break a request; `-ing` hooks can cancel (`args.Cancel`, or returning `false` from `OnStatusChanging`/`OnUserAssigning`).

Ratings and statuses are identified by hardcoded GUIDs in `FeedbackConstants`. If a site later drops a configured rating/status, existing rows still reference the GUID — the lookup falls back to a synthetic `"notFound"` rating/status rather than throwing. Preserve that.

### Two APIs, two audiences

- `Controllers/Api/FeedbackController.cs` — public, plain `[ApiController]`, `POST api/feedback` and `POST api/feedback/{key}`. These URLs are a contract with consuming websites; don't change them.
- `Controllers/Management/*` — backoffice, all deriving from `FeedbackManagementControllerBase` (`BackOfficeRoute` + `MapToApi` + `AuthorizationPolicies.BackOfficeAccess`), served from `/umbraco/feedback/api/v1/*` and described by an OpenAPI document at `/umbraco/swagger/feedback/swagger.json` (registered in `FeedbackApiComposer`).

`GET workspace-view/{key}` is the one endpoint with no UI of its own: it's how the client condition asks the server whether the feedback tab applies to a document. It replaces the removed `IContentAppFactory`.

Response models live in `Models/Api`; they are plain `System.Text.Json` DTOs with no server-side localization.

### Backoffice client (`Client/`)

Registered through `Client/public/umbraco-package.json` → one bundle module → `Client/src/bundle.manifests.ts`. There is no `IManifestFilter`; adding an extension means adding a manifest to that bundle.

- `workspace-view/` — the feedback table, one element covering both the site-level and page-level cases (the server says which via `pageKey`).
- `conditions/has-feedback.condition.ts` — decides whether the tab shows.
- `api/feedback.repository.ts` — `UmbFeedbackRepository`, all HTTP via `umbHttpClient` with `security: [{ type: "http", scheme: "bearer" }]`. Never use raw `fetch()`; it 401s.
- `api/types.ts` — hand-written mirror of `Models/Api`. **Change both sides together.**
- `localization/files/{en-us,da-dk}.ts` — keys keep the `feedback_` prefix from the old Lang XML; add to both files.

`FEEDBACK_API_BASE` in `constants.ts` must match `FeedbackConstants.ApiRoute` plus Umbraco's backoffice path.

### Database & migrations

One table, `SkybrudFeedback` (name kept from the predecessor package — do not rename). Schema is created and upgraded by `MigrationComponent` (an `IAsyncComponent`), which runs an Umbraco `MigrationPlan` named `"Limbo.Umbraco.Feedback"` on startup. To change the schema, append a new `.To<T>("<state>")` at the end of that chain; never edit an existing step.

The `PageKey` property maps to a column literally spelled `PagKey` — a typo baked into shipped databases; leave it.

## Conventions

- The project builds with **zero warnings**, including no obsolete-API warnings. Keep it that way — if an Umbraco API is obsolete, find the replacement rather than suppressing.
- `#pragma warning disable 1591` at the top of files where XML docs are intentionally skipped; everything public elsewhere is fully XML-documented (`DocumentationFile` is enabled, so missing docs are warnings).
- Nullable reference types are on. `TryGetX(..., [NotNullWhen(true)] out X? y)` is the house pattern for lookups, not nullable returns.
- `#region Properties / Constructors / Public methods` blocks and Allman-with-blank-lines formatting are used consistently; match the surrounding file.
- Several filenames carry existing typos (`FeedbackExensions.cs`, `FeedbackCompositionExensions.cs`). Don't "fix" them incidentally — the types are public API.
