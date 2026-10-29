# Upgrade recap: Umbraco 13 → Umbraco 17

This document records what was changed when `Limbo.Umbraco.Feedback` was upgraded from Umbraco 13 (.NET 8) to
Umbraco 17 (.NET 10), released as **17.0.0-alpha000**.

Umbraco 14 replaced the AngularJS back office with a Web Components / Lit back office served by a new Management
API. Because this package's entire editor experience lived in a content app built on AngularJS, the upgrade is a
rewrite of the presentation layer rather than a version bump. The business logic, database schema and public
front-end API are unchanged.

---

## 1. Project & packaging

| | Before | After |
|---|---|---|
| Target framework | `net8.0` | `net10.0` |
| Version | `13.0.0-alpha001` | `17.0.0-alpha000` |
| Umbraco packages | `Umbraco.Cms.Core`, `Umbraco.Cms.Web.Website`, `Umbraco.Cms.Web.BackOffice` @ `[13.0.0,13.999)` | `Umbraco.Cms.Core`, `Umbraco.Cms.Web.Common`, `Umbraco.Cms.Web.Website`, `Umbraco.Cms.Api.Common`, `Umbraco.Cms.Api.Management` @ `[17.0.0,17.9.9)` |
| Static assets | `wwwroot/**` with `StaticWebAssetBasePath=App_Plugins/$(AssemblyName)` | `wwwroot/App_Plugins/Limbo.Umbraco.Feedback/**` (Vite output) with `StaticWebAssetBasePath=/` |
| Front-end build | Web Compiler (`compilerconfig.json`, LESS → CSS) | Vite + TypeScript in `src/Limbo.Umbraco.Feedback/Client` |

`Umbraco.Cms.Web.BackOffice` no longer exists — its server-side surface moved to `Umbraco.Cms.Api.Management`.

The npm build is wired into MSBuild but **opt-in**, so a plain `dotnet build` does not require Node.js:

```bash
dotnet build src/Limbo.Umbraco.Feedback.sln                              # C# only
dotnet pack  src/Limbo.Umbraco.Feedback.sln -c Release \
             -p:BuildClientAssets=true                                    # C# + client, for release
cd src/Limbo.Umbraco.Feedback/Client && npm install && npm run build      # client only
```

The produced NuGet package contains the compiled assembly plus
`staticwebassets/App_Plugins/Limbo.Umbraco.Feedback/**`, including `umbraco-package.json`.

---

## 2. Extension registration: `IManifestFilter` → `umbraco-package.json`

`Manifests/FeedbackManifestFilter.cs` was deleted. Umbraco no longer discovers package assets through
`IManifestFilter`; it reads `App_Plugins/<package>/umbraco-package.json`, which points at a single bundle module:

```
Client/public/umbraco-package.json  →  /App_Plugins/Limbo.Umbraco.Feedback/limbo-feedback.js
                                        └── Client/src/bundle.manifests.ts
```

All extensions (localization, condition, modals, workspace view) are declared as TypeScript manifests in that
bundle.

---

## 3. Content app → workspace view

Content apps were removed in Umbraco 14. The two AngularJS content apps (`ContentApp.html` for sites,
`ContentAppPage.html` for a single page) became **one** workspace view element, and the decision about *whether*
to show it moved from a server-rendered object to a client-side condition backed by a server endpoint.

**Plugin API change (breaking for anyone implementing `IFeedbackPlugin`):**

```csharp
// Before
bool TryGetContentApp(IContent content, IEnumerable<IReadOnlyUserGroup> userGroups, out ContentApp? result);
protected virtual ContentApp? GetContentAppForSite(IContent site);
protected virtual ContentApp? GetContentAppForPage(IContent site, IContent page);

// After
bool TryGetWorkspaceView(IContent content, out FeedbackWorkspaceView? result);
protected virtual FeedbackWorkspaceView? GetWorkspaceViewForSite(IContent site);
protected virtual FeedbackWorkspaceView? GetWorkspaceViewForPage(FeedbackSiteSettings site, IContent page);
```

`FeedbackWorkspaceView` carries only data (`siteKey`, `pageKey`, `icon`) — no view path, no view model. The
selection logic is unchanged: site content types get the site view, page content types get the page view when a
parent site can be resolved, and content with `Id == 0` (being created) gets nothing.

The client side registers:

* `Limbo.Feedback.Condition.HasFeedback` — a custom condition that calls
  `GET /umbraco/feedback/api/v1/workspace-view/{key}` and permits the view only on a `200`.
* `Limbo.Feedback.WorkspaceView` — a `workspaceView` manifest conditioned on
  `Umb.Condition.WorkspaceAlias == Umb.Workspace.Document` **and** the condition above.

`ContentApps/FeedbackContentApp.cs` (the `IContentAppFactory`) was deleted, and
`FeedbackPluginCollection.TryGetContentApp` became `TryGetWorkspaceView`.

---

## 4. Back office API: `UmbracoAuthorizedApiController` → Management API

`Controllers/Api/Backoffice/FeedbackController.cs` (class `FeedbackAdminController`, `[PluginController("Limbo")]`)
was replaced by `Controllers/Management/*`, based on `BackOfficeRoute` + `MapToApi` + the back office
authorization policy.

**Routes moved and the verbs were corrected** (`Archive` and `Delete` were previously `GET`):

| Before | After |
|---|---|
| `GET /umbraco/backoffice/Limbo/FeedbackAdmin/GetEntriesForSite?key=` | `GET /umbraco/feedback/api/v1/entries/site/{key}` |
| `GET /umbraco/backoffice/Limbo/FeedbackAdmin/GetEntriesForPage?key=` | `GET /umbraco/feedback/api/v1/entries/page/{key}` |
| `GET /umbraco/backoffice/Limbo/FeedbackAdmin/GetUsers` | `GET /umbraco/feedback/api/v1/users` |
| `POST /umbraco/backoffice/Limbo/FeedbackAdmin/SetStatus` (JObject body) | `POST /umbraco/feedback/api/v1/entries/{key}/status` |
| `POST /umbraco/backoffice/Limbo/FeedbackAdmin/SetResponsible` (JObject body) | `POST /umbraco/feedback/api/v1/entries/{key}/responsible` |
| `GET /umbraco/backoffice/Limbo/FeedbackAdmin/Archive?key=` | `POST /umbraco/feedback/api/v1/entries/{key}/archive` |
| `GET /umbraco/backoffice/Limbo/FeedbackAdmin/Delete?key=` | `DELETE /umbraco/feedback/api/v1/entries/{key}` |
| — | `GET /umbraco/feedback/api/v1/workspace-view/{key}` (new) |

A new `FeedbackApiComposer` registers an OpenAPI document at `/umbraco/swagger/feedback/swagger.json`, with back
office security requirements and short operation IDs. Anonymous `JObject` request bodies and anonymous response
objects were replaced with typed models (`SetStatusModel`, `SetResponsibleModel`, `FeedbackResultApiModel`,
`EntryListApiModel`, `PaginationApiModel`, `SortingApiModel`), so the endpoints describe themselves properly.

Response mapping moved out of the controller into `Services/FeedbackApiModelFactory.cs`.

---

## 5. Public front-end API — unchanged URLs

`UmbracoApiController` was obsoleted in Umbraco 15 and removed since. `Controllers/Api/FeedbackController.cs` is
now a plain `[ApiController]` with `[Route("api/feedback")]`. **The two endpoints keep their exact URLs**, so
existing website implementations do not need to change:

* `POST /api/feedback`
* `POST /api/feedback/{key}`

---

## 6. Localization moved to the client

Umbraco no longer loads `~/App_Plugins/*/Lang/*.xml`. `wwwroot/Lang/en-US.xml` and `da-DK.xml` were deleted and
their contents ported to `localization` extension manifests (`Client/src/localization/files/en-us.ts` and
`da-dk.ts`). **Key names are unchanged** — `feedback_labelRating` still resolves the same string.

Consequences on the server:

* `ILocalizedTextService` and `IBackOfficeSecurityAccessor` are no longer used for rendering labels.
* `RatingApiModel.Name` / `StatusApiModel.Name` now return `null` when the rating/status has no explicit name; the
  client falls back to `feedback_rating{Alias}` / `feedback_status{Alias}`.
* `EntryApiModel.CreateDateDiff` / `UpdateDateDiff` were removed. Relative times ("2 hours ago") are computed in
  the browser with `Intl.RelativeTimeFormat` (`Client/src/utils/format.ts`).

New keys were added for messages that used to be hard-coded Danish strings in the AngularJS templates
(`noEntriesForSite`, `confirmArchive`, `archiveSuccess`, …).

---

## 7. JSON: Newtonsoft.Json → System.Text.Json

Umbraco 14+ serializes both the Management API and front-end APIs with `System.Text.Json`. Every
`[JsonProperty]` became `[JsonPropertyName]`, and `using Newtonsoft.Json` was removed throughout.

`Skybrud.Essentials.Json.Newtonsoft.Converters.Enums.EnumCamelCaseConverter` on `EntriesSortField` and
`EntriesSortOrder` was replaced with `Json/CamelCaseEnumConverter<T>`, a thin `JsonStringEnumConverter<T>`
subclass that applies the camel case naming policy — so the wire format (`"createDate"`, `"asc"`) is unchanged.

`Skybrud.Essentials.Json.Extensions` (`JObject.GetGuid`) is no longer used; those request bodies are now typed.

---

## 8. Removed and replaced Umbraco APIs

| Umbraco 13 API | Umbraco 17 replacement | Where |
|---|---|---|
| `IUmbracoContextAccessor` → `UmbracoContext.Content.GetById` | `IPublishedContentCache.GetById` (injected directly) | `FeedbackPluginDependencies`, both controllers, `FeedbackApiModelFactory` |
| `IDomainService.GetAssignedDomains(int, bool)` *(obsolete)* | `IDomainCache.HasAssigned(int, bool)` | `FeedbackPluginBase.TryGetSite` |
| `IPublishedContent.Parent` *(obsolete)* | `Parent<IPublishedContent>(IDocumentNavigationQueryService, IPublishedContentStatusFilteringService)` | `FeedbackPluginBase.TryGetSite` |
| `IComponent` *(obsolete)* | `IAsyncComponent` | `MigrationComponent` |
| `Upgrader.Execute` *(obsolete)* | `Upgrader.ExecuteAsync` | `MigrationComponent` |
| `MigrationBase` *(obsolete)* | `AsyncMigrationBase` (`MigrateAsync`) | both migrations |
| `IPublishedContent.Url()` (needs an ambient context) | `IPublishedUrlProvider.GetUrl(..., UrlMode.Absolute)` inside `IUmbracoContextFactory.EnsureUmbracoContext()` | `FeedbackApiModelFactory` |

`FeedbackPluginDependencies` therefore gained `PublishedContentCache`, `DomainCache`,
`DocumentNavigationQueryService` and `PublishedContentStatusFilteringService`, and lost `UmbracoContextAccessor`
and `UmbracoContext` — a breaking change for plugins that used them.

**The project builds with zero warnings**, including no obsolete-API warnings.

---

## 9. Unchanged on purpose

* **Database.** Table `SkybrudFeedback`, the `PagKey` column name (a long-standing typo baked into shipped
  databases), and the migration plan `Limbo.Umbraco.Feedback` with its existing steps. New installs and existing
  installs both keep working; no new migration step was added.
* **Configuration.** `Limbo:Feedback:DisableDefaultPlugin`, `SiteContentTypes`, `PageContentTypes`.
* **Rating/status GUIDs** in `FeedbackConstants`, and the `"notFound"` fallback for ratings/statuses that a site
  has since dropped.
* **Plugin event model.** `OnEntryAdding/Added`, `OnEntryUpdating/Updated`, `OnStatusChanging/Changed`,
  `OnUserAssigning/Assigned`, and the per-plugin try/catch so one failing plugin cannot break a request.
* **`FeedbackService` / `FeedbackDatabaseService`** public surface.

---

## 10. New front-end project layout

```
src/Limbo.Umbraco.Feedback/Client/
├── package.json, tsconfig.json, vite.config.ts
├── public/umbraco-package.json          # extension registration
└── src/
    ├── bundle.manifests.ts              # aggregates every manifest
    ├── constants.ts                     # API base URL, aliases, empty GUID
    ├── api/
    │   ├── types.ts                     # mirrors Models/Api on the server
    │   └── feedback.repository.ts       # UmbFeedbackRepository (umbHttpClient)
    ├── conditions/                      # HasFeedback condition
    ├── localization/files/{en-us,da-dk}.ts
    ├── modals/                          # select status / select responsible
    ├── utils/format.ts                  # names, dates, relative dates, initials
    └── workspace-view/                  # the feedback table
```

Notes on the implementation:

* All HTTP goes through `umbHttpClient` from `@umbraco-cms/backoffice/http-client`, passing
  `security: [{ type: "http", scheme: "bearer" }]`. Raw `fetch()` would return `401`. No entry point is needed,
  because `umbHttpClient` is pre-configured by the back office.
* `api/types.ts` is hand-written rather than generated with `@hey-api/openapi-ts`, because generation requires a
  running Umbraco instance. The OpenAPI document exists (`/umbraco/swagger/feedback/swagger.json`), so it can be
  switched to generation later — keep the types in sync with `Models/Api` until then.
* Data access sits in `UmbFeedbackRepository` (an `UmbControllerBase`) rather than in the element, and
  confirmations use `umbConfirmModal` rather than `window.confirm`, per the Umbraco extension review checks
  (AR-1, UI-1).
* The `editorService` overlays became real `modal` extensions with `UmbModalToken`s.
* UI is built from UUI components (`uui-box`, `uui-table`, `uui-select`, `uui-button`, `uui-pagination`,
  `uui-loader`), replacing the Bootstrap-ish markup and the deleted `Default.less`.

`FEEDBACK_API_BASE` in `constants.ts` is `/umbraco/feedback/api/v1`. If a site customises
`Umbraco:CMS:Global:UmbracoPath`, that constant needs to change too.

---

## 11. Files deleted

```
src/Limbo.Umbraco.Feedback/compilerconfig.json
src/Limbo.Umbraco.Feedback/ContentApps/FeedbackContentApp.cs
src/Limbo.Umbraco.Feedback/Manifests/FeedbackManifestFilter.cs
src/Limbo.Umbraco.Feedback/Controllers/Api/Backoffice/FeedbackController.cs
src/Limbo.Umbraco.Feedback/wwwroot/Lang/{en-US,da-DK}.xml
src/Limbo.Umbraco.Feedback/wwwroot/Scripts/Controllers/*.js
src/Limbo.Umbraco.Feedback/wwwroot/Styles/Default.{less,css,min.css}
src/Limbo.Umbraco.Feedback/wwwroot/Views/*.html
```

---

## 12. Not verified

The upgrade was verified by compilation only — `dotnet build` (0 errors, 0 warnings), `dotnet pack` (package
contents confirmed) and `npm run build` (TypeScript type-check + Vite bundle). There is no test project and no
consuming Umbraco site in this repository, so the following still need a manual pass in a real Umbraco 17
install:

1. The workspace view appears on configured site and page nodes, and nowhere else.
2. Filtering, sorting, pagination, expanding a row.
3. Archive, delete, change status, change responsible.
4. Localized labels in both `en-US` and `da-DK`.
5. Page URLs resolve in the entry details (the `IUmbracoContextFactory` path).
6. Submitting feedback from a front-end site through `POST /api/feedback`.
7. Upgrading a site with an existing `SkybrudFeedback` table (migration plan should be a no-op).
