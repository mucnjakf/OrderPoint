# OrderPoint

Order management system for a bar: customers order from their phones via QR code, bartenders fulfil orders in real time, and the owner manages everything in an admin web app. Only the **API** and the **Admin web app** exist so far; the bartender and customer apps are planned.

## Ground rules

- **Never run `git commit`, `git push`, `git merge`, `git rebase`, `git reset`, `git stash` or `gh pr create`.** Leave every change uncommitted in the working tree. When a task is done, summarise the changed files and suggest a commit message; the owner reviews and commits himself. These rules are also enforced in `.claude/settings.json`.
- **Follow the existing patterns exactly.** Before writing a new file, open the closest existing equivalent (e.g. `Item*` when adding a new entity) and mirror its structure, naming, formatting and level of detail. Do not introduce new libraries, folders, abstractions or architectural styles without asking first.
- Do not add NuGet packages or upgrade versions without asking.
- If a request is ambiguous or would require breaking an existing convention, ask before implementing.
- For anything larger than a small change, propose a plan (files to add or change, per layer) and wait for approval before editing.
- **Keep visibility as low as possible.** Every type and member gets the narrowest access modifier that compiles (`private` > `internal` > `public`). Only make something `public` when another project or a public type genuinely needs it.
- Prefer readability: clear names, early returns, no magic numbers or strings (use `const` and `nameof`).

## Tech stack

.NET 10, C# (latest), Aspire 13 for local orchestration, PostgreSQL via EF Core (Npgsql), Azure Blob Storage for images (Azurite emulator locally), Minimal API, MediatR 14 (CQRS), FluentValidation 12, Scalar (API docs), OpenTelemetry. Admin web is Blazor Web App with Interactive Server render mode and MudBlazor 9.

## Repository layout

```
.claude/settings.json                 Claude Code permissions (allow/ask/deny)
.gitattributes                        line endings: LF in the repo, CRLF in the working tree
app/
  OrderPoint.slnx                     solution file
  .editorconfig                       formatting rules (UTF-8 BOM, 4 spaces, 120 columns)
  host/OrderPoint.AppHost             Aspire host: Postgres + Azure Storage emulator (blob container "images") + API + Admin
  host/OrderPoint.ServiceDefaults     shared Aspire defaults (telemetry, health, service discovery)
  api/OrderPoint.Domain               entities, errors, Result type (Outcomes/), enums, sort enums
  api/OrderPoint.Application          commands/queries + handlers, DTOs, mappers, repository interfaces
  api/OrderPoint.Infrastructure       EF Core DbContext, entity configs, migrations, repositories
  api/OrderPoint.Api                  Minimal API endpoints, request validators, exception handlers
  web/OrderPoint.Admin                Blazor admin app, organised by feature folder
design/                               static HTML mockups (admin-web, bartender-web, customer-web)
```

Dependency direction: Api -> Infrastructure -> Application -> Domain. Admin does not reference the API projects; it talks to the API over HTTP and keeps its own copies of DTOs and enums.

## Commands

Run from the repo root.

```bash
dotnet build app/OrderPoint.slnx -c Release            # verify every change with this
dotnet ef migrations add <Name> \
  --project app/api/OrderPoint.Infrastructure \
  --startup-project app/api/OrderPoint.Api \
  --output-dir EfCore/Migrations \
  --configuration Release
```

- **The owner keeps the app running** (`aspire run --project app/host/OrderPoint.AppHost`) and verifies changes himself. Never start, stop or restart it; after a change, say what changed and what to check.
- While Aspire runs, the Debug outputs (`bin/Debug`) are locked, so build and run `dotnet ef` with `-c Release` / `--configuration Release`. MSB3021/MSB3027 "file is being used by another process" errors mean a Debug build hit the lock, not a compile error.
- Migrations are applied automatically on API startup (`app.ApplyMigrations()`); do not run `dotnet ef database update`.
- Migration names describe the change as `<Verb>_<What>`: `Add_Item`, `Add_ImageUrl_To_Category`, `Add_Category_Description_And_Status`, `Remove_ShortDescription_From_Item`, `Restrict_Category_Delete`.
- There is no test project yet. Do not add one unless asked.
- "Done" means the build succeeds with no warnings. The AppHost suppresses `ASPIRE010` on purpose (DCP and the dashboard come from NuGet, not the Aspire CLI bundle).
- Secrets live in user secrets, not appsettings. The Api needs `MediatR:LicenseKey`, `Jwt:SigningKey` (at least 32 characters; the API refuses to start without it) and `Admin:Email` + `Admin:Password` (the seeded admin). `appsettings.json` lists these keys with empty values.
- Running the app needs Docker (Aspire starts Postgres on port 59286 and the Azurite blob endpoint on port 59287, both as persistent containers with data volumes; the blob port is fixed because image URLs are stored with it). The design-time `ApplicationDbContextFactory` points at `localhost:5432`; that is fine for `migrations add`, which does not connect.

## Backend conventions

### Domain (`OrderPoint.Domain`)

- Entities are `sealed`, inherit `Entity` (`Id`, `CreatedAtUtc`, `UpdatedAtUtc`), have `private set` properties and a private constructor.
- Creation goes through `public static Result<T> Create(...)` (failures: `Result.Failure<T>(...)`); changes through `public Result Update(...)` (failures: `Result.Failure(...)`). Both validate invariants instead of throwing. `Update` sets `UpdatedAtUtc = DateTimeOffset.UtcNow`.
- IDs are `Guid.CreateVersion7()`, timestamps are `DateTimeOffset.UtcNow`.
- Navigation collections: `private readonly List<T> _items = []` exposed as `IReadOnlyList<T>`.
- Errors live in `Errors/<Entity>Errors.cs` as `static readonly Error` fields built with `Error.NotFound/Validation/Conflict/Unauthorized/Forbidden/Failure` (401/403 are used by `AuthErrors` only), code format `"<Entity>.<Reason>"`. Errors used only inside Domain are `internal`, errors used by handlers are `public`. (`Error.RequestValidation` is reserved for the API's FluentValidation exception handler.)
- Sort options are enums in `Sorting/<Entity>SortBy.cs` (`NameAsc`, `NameDesc`, ..., `CreatedAtUtcDesc`). Other enums go in `Enumerations/`.

### Application (`OrderPoint.Application`)

- One file per use case holding both the message and its handler:
  - `Commands/<Feature>/<Action><Entity>Command.cs`: `public sealed record XCommand(...) : ICommand` or `ICommand<TDto>`, plus `internal sealed class XCommandHandler(...) : ICommandHandler<...>`.
  - `Queries/<Feature>/<Get...>Query.cs`: `public sealed record XQuery(...) : IQuery<TDto>`, plus `internal sealed class XQueryHandler`.
- Use the project's own `ICommand`/`IQuery`/`ICommandHandler`/`IQueryHandler` interfaces from `Mediator/` (all `internal`), never MediatR's `IRequest` directly.
- Handlers return `Result`/`Result<T>`, never throw for business failures. Typical flow: load → `Result.Failure(XErrors.NotFound)` if null → check related entities exist → call domain `Create`/`Update` → propagate failure → repository → `unitOfWork.SaveChangesAsync`.
- Create handlers that return a DTO with navigation data reload the entity after saving (`itemRepository.GetAsync(id)`, which includes `Category`) and map that. If nothing needs loading, map directly (a new category has `itemsCount: 0`).
- Uniqueness rules (e.g. bartender email) are checked in the handler before the domain `Create`/`Update` with `repository.ExistsBy<Field>Async(value)` (e.g. `ExistsByEmailAsync`) and return a `Conflict` error; on update the check only runs when the value changed. A unique index in the EF configuration backs them up. Values are stored as entered (no trimming or lower-casing).
- Orders are read-only in the Admin. `POST api/orders` exists for the future customer app. Status changes (accept, decline, activate, complete) and the `<Status>AtUtc` timestamps they set are deliberately not implemented yet; they come with the bartender app. Order lines (`OrderItem`) copy the item's price into `UnitPrice` when the order is placed, so later price changes do not rewrite history; the order total is calculated in the mapper, not stored.
- Items have their own `ItemStatus` (Active/Inactive), but an item in an inactive category counts as inactive too: `Item.IsAvailable` (needs `Category` loaded) is true only when both are Active, and `Order.Create` rejects unavailable items with `OrderErrors.ItemIsUnavailable` (409). Deactivating a category never changes its items' own statuses. The items `status` filter uses the same effective rule (`FilterItems`), and the Admin shows it via `ItemDto.IsActive` (chips, greyscale images); only the item details dialog and the edit form show the item's own status.
- Deleting a parent that still has children is blocked in the handler with a `Conflict` error (e.g. `CategoryErrors.CannotDeleteCategoryWithItems`), backed by `DeleteBehavior.Restrict` on the foreign key.
- DTOs: `public sealed record XDto(...)` in `Dtos/`. Mapping is manual via extension methods in `Dtos/Mappers/<Entity>Mapper.cs` (`ToXDto()`). No AutoMapper.
- Lists: paged lists return `PaginationDto<T>` (`Get<Plural>Query`). Unpaged lookups, such as autocomplete search, return `IReadOnlyList<T>` (`Search<Plural>Query`).
- Repository interfaces in `Repositories/` (`GetPaginatedAsync`, `SearchAsync`, `GetAsync`, `CreateAsync`, `Delete`, `ExistsAsync`...). Methods that look the entity up by its own id have no suffix (`GetAsync(Guid id)`, `GetAsync(IReadOnlyList<Guid> ids)`, `ICategoryRepository.ExistsAsync(Guid id)`). Methods filtering by anything else say what they filter by in the name, and the parameter is named after it: `IItemRepository.ExistsByCategoryAsync(Guid categoryId)` = "any item in this category", `CountByCategoryAsync(Guid categoryId)`, `IBartenderRepository.ExistsByEmailAsync(string email)`, `IOrderRepository.ExistsByItemAsync(Guid itemId)`, `CountByBartendersAsync(IReadOnlyList<Guid> bartenderIds)`. Saving goes through `IUnitOfWork`, never from a repository.
- Counts of children for a page of entities come from one grouped query that returns a dictionary (`IOrderRepository.CountByItemsAsync(itemIds)` for `GetItemsQuery`, `CountByBartendersAsync(bartenderIds)` for `GetBartendersQuery`), not from including the child collection. A single entity uses the scalar version (`CountByItemAsync(Guid itemId)`). Categories still include `Items` for their counts, which is fine while categories hold few items.
- Dashboard is the one read-model exception: `IDashboardRepository` (no entity of its own) returns DTOs straight from aggregations over orders, because loading entities to sum them would be wasteful. Each dashboard widget has its own query (`Queries/Dashboard/`) and endpoint (`api/dashboard/<widget>?period=Today|Last7Days|Last30Days&timeZone=<IANA id>`; busiest times and live take only `timeZone`) so widgets load in parallel. Periods are calendar based in the viewer's time zone, sent by the Admin and validated with the shared `MustBeValidTimeZone()` rule (`DashboardPeriodRange` computes local midnights with `LocalToUtc`/`UtcToLocal`; buckets are built from local dates, never by adding fixed lengths, so DST days stay correct): Today = since local midnight, Last7Days/Last30Days include today, and "previous" is the same-length window right before. Revenue and averages count only Completed orders; order counts include every status. Busiest times always cover the last 28 days and group by local weekday and hour.
- MediatR handlers are registered by assembly scan; no manual registration needed.

### Infrastructure (`OrderPoint.Infrastructure`)

- `ApplicationDbContext` implements `IUnitOfWork`; `DbSet`s are `internal`.
- One `IEntityTypeConfiguration<T>` per entity in `EfCore/EntityTypeConfiguration/` (table name, `ValueGeneratedNever()` on Id, `HasMaxLength`, `IsRequired`, relationships). Each property is configured as `builder` / `.Property(...)` / `...` on separate lines. Relationships use `.OnDelete(DeleteBehavior.Restrict)`. Picked up by assembly scan. The only database-generated value is `Order.Number` (`UseIdentityAlwaysColumn()` plus a unique index), the human-readable order number.
- Repositories are `internal sealed class <Entity>EfCoreRepository(ApplicationDbContext dbContext)` in `EfCore/Repositories/`, using private static `Search<X>`/`Filter<X>`/`Sort<X>` helpers for paginated queries (search is case-insensitive via `ToLower().Contains`; default sort `CreatedAtUtc` descending; unpaged search sorts by name ascending). `Sort<X>` builds an `IOrderedQueryable` and always ends with `.ThenBy(x => x.Id)` so paging is stable when sort values tie. Read-only queries (`GetPaginatedAsync`, `SearchAsync`) use `AsNoTracking()`; `GetAsync` stays tracked because commands update and delete what it returns, and it includes the navigations its DTO needs. Register in `InfrastructureModule`.

### API (`OrderPoint.Api`)

- `Program.cs` pipeline order: `ApplyMigrations` → `SeedAdminAsync` → `UseExceptionHandler` (first middleware) → `UseHttpsRedirection` → `UseCors` → `UseAuthentication` → `UseAuthorization` → endpoint and docs mapping. `launchSettings.json` URLs never contain a path (Kestrel refuses them); use `launchUrl` instead.
- CORS: one policy that allows only the origins in `Cors:AllowedOrigins` (empty by default in `appsettings.json`), any header, and GET/POST/PUT/DELETE. Nothing needs CORS today (the Admin and the planned bartender app call the API from their servers, Scalar is same-origin); add an origin only for a client that calls the API from the browser (e.g. a WebAssembly/JS customer app). Never go back to `AllowAnyOrigin`.
- One endpoint per file in `Endpoints/<Feature>/<Action><Entity>Endpoint.cs`: `internal sealed class XEndpoint : IEndpoint`, discovered automatically.
- Request and response records (`internal sealed record`) are declared in the same file above the endpoint; the FluentValidation validator is a nested `internal sealed class XRequestValidator` inside the endpoint class (auto-registered). Every endpoint that takes a body or query string has a request record and a validator.
- Query-string input is bound into a request record with `[AsParameters] XRequest request`; its constructor parameters carry `[FromQuery]` (see `GetItemsEndpoint`, `SearchCategoriesEndpoint`). Paged lists validate `PageNumber > 0` and `PageSize` between 1 and 100; search text is at most 100 characters (a search that cannot match returns no results, not an error); required search text uses `NotEmpty()` (also rejects whitespace). Route values (`{id:guid}`) stay as `[FromRoute]` parameters.
- `MapEndpoint` uses route `api/<plural>`, `api/<plural>/{id:guid}`, `api/<plural>/search?searchQuery=...` (unpaged lookup), plus `.WithName("<Action><Entity>")`, `.WithTags("<Plural>")` and, for every Admin endpoint, `.RequireAuthorization(AuthorizationPolicies.Admin)` as the last call. Endpoints without `RequireAuthorization` are anonymous (auth endpoints, `POST api/orders`, health checks, API docs), so never forget it on a new Admin endpoint.
- `private static async Task<Results<..., ProblemHttpResult>> HandleAsync(...)` with explicit `[FromBody]`/`[FromRoute]`/`[FromQuery]`/`[FromServices]` attributes and a `CancellationToken`.
- Flow: `await validator.ValidateAndThrowAsync(request, ct)` → build command/query → `sender.Send` → `result.IsSuccess ? TypedResults.X(...) : result.ToProblemDetails()`.
- Responses wrap payloads as `{ Data }`: create → `CreatedAtRoute` to the Get route, update/delete → `NoContent`, get → `Ok`.
- Errors are ProblemDetails with an `errors` array; status mapping lives in `Extensions/ResultExtensions.cs`. Exception handlers in `Exceptions/` (registered in `Program.cs`, in order): `RequestValidationExceptionHandler` (FluentValidation → 400), `BadHttpRequestExceptionHandler` (missing/unparsable parameters or malformed JSON → 400; `ThrowOnBadRequest` is on so this applies in every environment), `GlobalExceptionHandler` (logs the exception, → 500 without exposing exception details). In .NET 10 the exception middleware does not log exceptions a handler reports as handled, so any new handler for unexpected errors must log itself.

### Images

- Images live in the blob container `images` and the entity stores the full public URL in `ImageUrl` (UI uses it directly). The API gets a `BlobContainerClient` from `builder.AddAzureBlobContainerClient("images")` and `app.CreateImageContainer()` makes the container publicly readable at startup.
- `IImageStorage` (Application `Storage/`, implemented by Infrastructure `Storage/BlobImageStorage`): `UploadAsync(stream, contentType, folder)` stores `<folder>/<guid>` and returns the URL; `DeleteAsync(imageUrl)`. A new upload always gets a new name, so browsers never show a cached old image.
- Images are not part of create/update. Each entity with an image gets `PUT api/<plural>/{id}/image` (multipart form field `image`, `[FromForm]` request record, `.DisableAntiforgery()`) and `DELETE api/<plural>/{id}/image`, backed by `Update<Entity>ImageCommand`/`Delete<Entity>ImageCommand` and domain `SetImage(url)`/`RemoveImage()`. Replacing an image deletes the old blob after saving; deleting the entity deletes its blob. Limits: JPEG, PNG or WebP, at most 2 MB, checked by the API validators through the shared `RuleFor(request => request.Image).MustBeValidImage()` (`Extensions/ImageValidationExtensions.cs`) and by the Admin `ImageUploadField`.
- Admin: dialogs use the shared `ImageUploadField` (upload button, optional remove, inline error) which reads the file into an `ImageFileDto`; the preview paper shows it via `ImageFileDto.ToDataUrl()`. The request carries it as `[JsonIgnore] Image` (update also `[JsonIgnore] RemoveImage`), and the page sends create/update and then the image call inside one `ApiService.ExecuteAsync` (see `CategoriesPage`).
- Categories, Items and Bartenders all have uploads (blob folders `categories/`, `items/`, `bartenders/`). Bartenders without an image show coloured initials in `BartenderAvatar` (text size follows the avatar `Size`). Item cards in `ItemsPage` use `ItemCardImage`: the image at a fixed height, uncropped, centred over a blurred, faded copy of itself that fills the card's top area (so the background matches the image's colours), or the centred placeholder in an area of the same height. Small fixed-size thumbnails use `MudImage` with `ObjectFit.Cover` so photos are cropped, not stretched; never `MudCardMedia` inside a list row (it defaults to 300px tall).

### Authentication

- ASP.NET Core Identity, but only its user store: `UserManager` with `UserOnlyStore` on the plain `ApplicationDbContext` (password hashing, lockout, unique email, security stamp). No `SignInManager`, `MapIdentityApi`, Identity UI, cookies, claims, external logins, 2FA or Identity roles. Tables: `Users` (Infrastructure `Identity/ApplicationUser`, fully configured in `ApplicationUserTypeConfiguration`, unused Identity columns ignored) and `RefreshTokens`.
- Two roles in `UserRole` (a column on the user): `Admin` (only seeded at startup from `Admin:Email`/`Admin:Password` when that email does not exist yet) and `Bartender`. A bartender's user has the same `Id` as the `Bartender` row; creating a bartender creates the login (temporary password, `MustChangePassword = true`), changing the email updates it, deactivating revokes refresh tokens, deleting deletes it. A forgotten password is reset by the admin (`PUT api/bartenders/{id}/password`, "Reset password" button next to Orders): a new temporary password, `MustChangePassword = true` again, lockout lifted, refresh tokens revoked.
- Application talks to Identity only through `Identity/IIdentityService` and `Identity/ITokenService` (implemented in Infrastructure `Identity/`). The user store (`ApplicationUserStore`) runs with `AutoSaveChanges = false`, so user and token changes are saved by the handler's `IUnitOfWork` together with everything else; its `UpdateAsync` only marks the user modified, because the base store re-attaches the user, which breaks the concurrency check when one request makes several user updates before saving.
- Tokens: JWT access token (HS256, 15 minutes, claims `sub`, `email`, `role`) and a random refresh token (7 days, only its SHA-256 hash stored, replaced on every refresh). Lifetimes, issuer and audience are in `appsettings.json` (`Jwt`), the key in user secrets. `ClockSkew` is zero.
- Endpoints (anonymous): `POST api/auth/login` (`email`, `password`, `role`), `POST api/auth/initial-password` (first login only), `POST api/auth/refresh`, `POST api/auth/logout`. Each app sends its own role; a user of the other role gets `InvalidCredentials`, exactly like a wrong password. 5 failed attempts lock an account for 5 minutes. A bartender with `MustChangePassword` gets `PasswordChangeRequired` (403) and must use `initial-password`, which works only while the flag is set; an inactive bartender gets `AccountInactive`.
- Authorization: policy `AuthorizationPolicies.Admin` (role `Admin`) on every Admin endpoint. The bartender app's endpoints get a `Bartender` policy when they are built. Scalar sends a pasted bearer token (`BearerSecuritySchemeTransformer`).
- Admin: `Auth/` feature folder. `AuthService` (one per circuit) keeps the tokens in `ProtectedLocalStorage`, refreshes one minute before expiry under a lock (parallel calls refresh once) and raises `SessionChanged`. `AccessTokenHandler` on every typed API client attaches the token; it reaches the circuit's `AuthService` through `CircuitServicesAccessor` + `ServicesAccessorCircuitHandler` (the documented pattern, since `IHttpClientFactory` handlers live in another DI scope). `AuthApiClient` is registered without the handler. `MainLayout` is the gate: it renders nothing until the session is loaded, then `LoginForm` instead of the page when signed out, so no page needs `[Authorize]`. The signed-in email and Sign out sit at the bottom of the drawer (just a logout icon when it is collapsed). There is no hamburger button: a round chevron tab on the drawer's edge, just above that block, toggles it; it is a fixed div placed directly after `MudDrawer`, because MudBlazor gives such a div a left margin equal to the drawer width (see `MainLayout.razor.cs`). Prerendering is off (`App.razor`) because browser storage is readable only once the circuit is connected. When a refresh fails, calls throw `SessionExpiredException`, which `ApiService` ignores silently, and the login form says the session expired.
### Validation is duplicated on purpose; keep it in sync

Field rules live in several places. Changing one means changing all of them:
1. Domain entity `Create`/`Update`: required and positive checks only (no max lengths)
2. API request validator (FluentValidation, messages like `"Name is required"`): required, positive, max length, enums
3. EF entity configuration (`HasMaxLength`, `IsRequired`) plus a migration if the schema changes
4. Admin request class DataAnnotations (messages end with a period: `"Name is required."`): required, positive, max length

## Admin web conventions (`web/OrderPoint.Admin`)

### Structure

- Feature folders: `<Feature>/Api` (`<Feature>ApiClient`, `Requests/`, `Responses/`), `Components/`, `Dialogs/`, `Dtos/`, `Enumerations/`, `Pages/`, `Sorting/`. Cross-feature code is in `Shared/` (`Components/`, `Dtos/`, `Errors/`, `Extensions/`, `Layout/`, `Pages/`, `Services/`). Reusable formatting helpers are `internal static` extension methods in `Shared/Extensions/<Type>Extensions.cs` (e.g. `DateTimeOffset.ToRelativeTime()` → "2 h ago"), not private methods on a page.
- Every component is split into `X.razor` (markup only) and `X.razor.cs` (`public sealed partial class X`). No `@code` blocks.
- Visibility: components are `public` (Razor requires it), so DTOs, enums and `PaginationDto` used as component parameters are `public`. Everything else (API clients, requests, responses, sort enums, `XSorting` helpers, services) is `internal`.
- Inject with `[Inject] private T Name { get; set; } = null!;`. Parameters use `[Parameter]` and `[EditorRequired]` where mandatory.
- Add new namespaces that pages use to `_Imports.razor`.

### API calls

- API clients are `internal sealed`, use relative URIs without a leading slash (`api/items?...`), escape user text with `Uri.EscapeDataString`, are typed HttpClients (`internal sealed class XApiClient(HttpClient httpClient)`), call `ApiExceptionHelpers.ThrowApiExceptionAsync` on non-success, and unwrap `.Data` from the response record. Register them in `Program.cs` with `builder.Services.AddHttpClient<XApiClient>(ConfigureApiClient).AddHttpMessageHandler<AccessTokenHandler>()` (base address in `ConfigureApiClient`; the handler attaches the access token). Only `AuthApiClient` is registered without the handler. Typed clients are transient, so never store one in a singleton.
- Pages and dialogs call API clients **only** through `ApiService`, which owns all API error handling and snackbars:
  - Queries: `T? result = await ApiService.ExecuteAsync(() => client.GetXAsync(...))`. Returns `null` on failure (error snackbar already shown), so fall back: `Items = Pagination?.Items ?? [];`.
  - Mutations: `bool isSuccess = await ApiService.ExecuteAsync(() => client.CreateXAsync(request), $"Item {request.Name} created successfully");`. Shows the success snackbar on success and the error snackbar on failure; reload the list only when `isSuccess`.
  - Autocomplete `SearchFunc`s pass MudBlazor's `CancellationToken` to both the client and `ExecuteAsync`, so cancelled searches are silent: `return categories ?? [];`.
  - Pages never inject `ISnackbar` for API results.
  - `ApiService` logs unexpected exceptions (network, parsing); expected API errors (ProblemDetails), cancellations and `SessionExpiredException` are not logged.

### Pages and dialogs

- **Sections load independently and in parallel.** Each section of a page (a table, a highlight card, a list in a dialog) has its own `Get<Section>Async` method and its own `IsLoading<Section>` flag; the method ends with `StateHasChanged()` so the section renders as soon as its data arrives. `OnInitializedAsync` and post-mutation reloads start independent sections with `await Task.WhenAll(...)`; never await independent loads one after another. Only dependent steps stay sequential (mutation → reload).
- Highlight sections above a list (Categories: top 5 by item count; Items: spotlight cards for newest, last edited, highest and lowest price; Bartenders: team strip with active avatars, active/inactive balance and newest member; Orders: status pipeline of `OrderStatusCard`s, Pending → Accepted → Active → Completed plus Declined, where clicking a card filters the timeline and clicking it again clears the filter) reuse the list endpoint with `pageSize`/`sortBy`/filters (the pipeline makes one `pageSize: 1` call per status and reads `TotalCount`); they do not get their own endpoints.
- Dashboard (`Dashboard/Pages/DashboardPage`, route `/`): bento `MudGrid` (4 KPI cards; revenue trend area chart + busiest-times heat map + category donut on the left, tall "Right now" live panel on the right; top items, bartender leaderboard, recent orders below). Tiles use `DashboardTile` (icon, title, optional `HeaderContent`, `StatefulView`, optional `ContentHeight` that keeps loading/empty states as tall as the content; the revenue tile passes `RevenueTrendChart.ChartHeight` so the left column always matches the "Right now" panel) and `DashboardKpiCard` (value + ▲/▼ change chip vs previous period, `IsLowerBetter` for service time). A `MudToggleGroup` period switch in the `PageHeader` (no breadcrumbs on the dashboard) reloads the period sections; the live panel (open orders, longest wait, today so far: orders, completed, revenue, average order) refreshes every 30 s with a `PeriodicTimer` disposed with the page. Top items rows open `ItemDetailsDialog`, leaderboard rows open `BartenderOrdersDialog` (each fetches the full DTO first), recent orders open `OrderDetailsDialog`; all three lists highlight the hovered row with `var(--mud-palette-table-hover)`. All three charts are hand-built for a consistent hover experience (MudChart has no hover-anywhere crosshair and its tooltips are small): the revenue trend (SVG area + crosshair), the busiest-times heat map (HTML grid) and the top-5 category donut (SVG ring; hovering a segment or legend row shows the category in the centre). Revenue trend and heat map: transparent per-column slots set the hovered index and show a `MudTooltip` with `TooltipContent` (body2 label + h6 value), SVG coordinates are formatted with `CultureInfo.InvariantCulture`, colours come from palette CSS variables (`var(--mud-palette-primary)`, `rgba(var(--mud-palette-primary-rgb), …)`). In Razor loops, copy the loop variable into a local before using it inside child content or lambdas (`int rank = i + 1;`), otherwise every row renders the final value.
- List pages: `private const int PageSize` (10 for `DataTable`, `DataAccordion` and `DataTimeline`, 9 for the 3-column `DataGrid`, 5 for `DataList` in a dialog); one `Get<Plural>Async(int pageNumber)` that reads the current search/sort/filter properties; search, sort and filter changes call it with `pageNumber: 1`; `OnPageChangedAsync(int pageNumber)`.
- Sorting is passed as the sort enum's name: `SelectedSortBy = nameof(ItemSortBy.CreatedAtUtcDesc)`; labels and icons come from `<Feature>Sorting.GetSortByLabel/GetSortByIcon`.
- Create/update/delete/details are MudBlazor dialogs opened from the list page (not separate pages). Dialogs return the request object via `MudDialogInstance.Close(DialogResult.Ok(Request))`; the page performs the API call and reloads. Use early returns: `if (dialogResult.Canceled) { return; }`. Read-only features (Orders) only have a details dialog.
- A dialog that changes data the page shows (e.g. items inside `CategoryDetailsDialog`) exposes an `EventCallback On<Thing>Changed` parameter and invokes it after a successful mutation; the page passes `EventCallback.Factory.Create(this, ...)` in the `DialogParameters` and reloads. Do not rely on the dialog result for this: closing with X/Escape returns `Canceled`.
- Dialogs and components initialise state and load data in `OnInitialized`/`OnInitializedAsync` (parameters are already set). Do not use `OnParametersSet`: it runs again whenever the dialog provider re-renders, which resets forms and reloads lists. Exception: display-only chart components (`RevenueTrendChart`, `BusiestTimesHeatMap`, `CategoryRevenueDonut`) build their `ChartSeries`/labels from parameters in `OnParametersSet`, so charts are rebuilt only when their data changes, not on every render.
- Forms: `EditForm` + `DataAnnotationsValidator`, MudBlazor inputs with `Variant.Outlined`, `HelperText`, `Immediate="true"`. `OnValidSubmit` only closes the dialog. Add `OnInvalidSubmit` + `IsFormSubmitted` only for inputs not covered by DataAnnotations `For` (e.g. a `MudAutocomplete` showing `Error`/`ErrorText`).

### Components and styling

- Each page has exactly one `<h1>`: the `PageHeader` title (rendered as `h1`, styled `Typo.h4`). `Routes.razor` focuses it after navigation, so nothing else (e.g. drawer labels) may render as `h1`.
- Reuse shared components: `PageHeader` (title + optional breadcrumbs; optional `ChildContent` renders on the right of the title, e.g. the dashboard period switch), `DataTable` (tabular list, see Categories), `DataGrid` (card grid, see Items), `DataAccordion` (expandable rows with `HeaderTemplate`/`DetailsTemplate` and actions inside the expanded row; collapsed rows highlight on hover; optional `ActionsTemplate` adds page-specific buttons before Edit/Delete, e.g. the Orders button; see Bartenders), `DataTimeline` (read-only vertical timeline of clickable tickets with search and sort but no create/edit/delete; `GetItemColor`/`GetItemIcon` style each dot, `ItemTemplate` renders the ticket, `OnItemClick` opens details; see Orders), `DataList` (list inside a dialog; the create button only shows when `OnCreateClick` is set, so it also works read-only, see `ItemDetailsDialog`), `StatefulView` (loading/empty states for every independently loaded section; `Compact="true"` renders them without the surrounding paper, for use inside a card), `HighlightCard` (the shared card shell for highlight sections: centred label chip, hover lift, click, compact loading/empty state; optional `Height`, default 250, and `IsSelected`, which keeps the card raised and fills the chip; feature cards such as `TopCategoryCard`, `ItemSpotlightCard` and `OrderStatusCard` only supply the content), `TextDisplayRow` (label + `Text`, or `ChildContent` for richer values such as an avatar with name and email), `ChipDisplayRow`. Extend them rather than building parallel ones. `DataTable`/`DataGrid`/`DataAccordion` show the delete button when `OnDeleteClick` is set; `DeleteButtonDisabled` and `DeleteButtonDisabledTooltipText` are optional.
- Times are stored and sent as UTC. The Admin shows them in the browser's time zone: `TimeZoneService` (scoped, loaded once per circuit in `MainLayout` from `Intl.DateTimeFormat().resolvedOptions().timeZone`, UTC if unknown) and `dateTime.ToDisplayDateTime(TimeZoneService.TimeZone)` everywhere a date-time is displayed ("Today 14:30", "Yesterday 9:15", else the locale's short date and time, no seconds). For any other format convert with `ToTimeZone(...)` first; never call `ToString` on a `*AtUtc` value directly. `DashboardApiClient` sends `TimeZoneService.TimeZone.Id` as `timeZone`. Relative times (`ToRelativeTime`) need no conversion.
- Formatting follows the viewer's browser: `UseRequestLocalization` (all cultures, from `Accept-Language`) plus a middleware in `Program.cs` that swaps in `CultureInfo.ToDisplayCulture()` (`Shared/Extensions/CultureInfoExtensions.cs`): the locale's number, date and 12/24 h formats, but English day and month names to match the UI, and amounts always in euros with 2 decimals. So format with `CultureInfo.CurrentCulture` (`"C"` for money, `"t"`/`"g"` and `GetShortMonthDayPattern()` for dates, `Uses12HourClock()` for hour-only labels), never hardcode `HH:mm` or `dd.MM`, and use `InvariantCulture` only for CSS/SVG numbers.
- Order status colours and icons come from `OrderStatus.GetColor()`/`GetIcon()` in `Shared/Extensions/OrderStatusExtensions.cs`; use them everywhere a status is shown (pipeline, timeline dots, chips, details timeline).
- Admin DTOs, enums and sort enums mirror the API ones by hand. When an API contract changes, update the Admin copy too.
- Use MudBlazor components and `Icons.Material.Filled.*`; avoid custom CSS. Colours come from the theme in `Shared/Layout/AdminTheme.cs`, never hardcoded hex. The theme is tweakcn's "modern-minimal" (dark and light palettes, Inter font, 6px radius, soft shadows, buttons not uppercase; the light page uses `#f9fafb` so white cards stand out); the app bar and drawer are separated by `lines-default` borders, not elevation. Dark mode and an open drawer are the defaults; both toggles are remembered per browser in `ProtectedLocalStorage`. Elsewhere: dialogs use `Class="mud-background"`, borders use `var(--mud-palette-lines-default)`.
- Razor attribute style:
  - Plain for literals, enum values and type parameters: `Label="Sort by"`, `Clearable="true"`, `Variant="Variant.Outlined"`, `T="string"`, `TItem="ItemDto"`.
  - `@` for anything that references code, including directive attributes: `Items="@Items"`, `OnClick="@Cancel"`, `@bind-Value="@Request.Name"`, `Icon="@Icons.Material.Filled.Add"`.
  - `@(...)` for lambdas and expressions: `OnClick="@(() => ShowDeleteItemDialogAsync(item.Id, item.Name))"`.
- The mockups in `design/` are inspiration only; the owner decides layout and fields per screen. Do not add fields or screens from a mockup unless asked.

## C# style

- File-scoped namespaces matching the folder path; `using` directives at the top, outside the namespace.
- Classes are `sealed` by default and as little visible as possible (see Ground rules).
- Primary constructors for DI (handlers, repositories, API clients, exception handlers, services).
- Explicit types for locals (`Result<ItemDto> result = ...`, `Item? item = ...`, `string requestUri = ...`, `int i`). `var` only when the type is obvious from the right side: `new`, casts, mapper calls (`ToXDto()`), and generic calls naming the type (`GetRequiredService<T>()`, `Enum.Parse<T>()`). Never `var` for built-in types.
- Always braces, `is null` / `is not null`, collection expressions `[]`.
- Async methods end in `Async` and take a `CancellationToken`.
- Lines are at most 120 characters. When a call does not fit, break its arguments one per line; short calls stay on one line.
- Formatting follows `app/.editorconfig`: 4 spaces, UTF-8 **with BOM** for `.cs`, `.razor` and `.csproj` files, no final newline. Line endings are handled by git (`.gitattributes`: LF in the repo, CRLF in the working tree); keep CRLF when creating or editing files.

## Adding a new entity end-to-end (checklist)

Domain entity + errors (+ sort enum) → repository interface → EF config + EF repository + DbSet + registration → migration → DTO + mapper → commands/queries → endpoints with validators and `RequireAuthorization(AuthorizationPolicies.Admin)` → Admin DTOs/enums/sorting → API client + requests/responses + registration → page + dialogs → nav link in `Shared/Layout/MainLayout.razor` (add it, or check it already exists; Orders and Bartenders already have links) → `dotnet build app/OrderPoint.slnx -c Release`.
