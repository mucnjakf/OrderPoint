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

.NET 10, C# (latest), Aspire 13 for local orchestration, PostgreSQL via EF Core (Npgsql), Minimal API, MediatR 14 (CQRS), FluentValidation 12, Scalar (API docs), OpenTelemetry. Admin web is Blazor Web App with Interactive Server render mode and MudBlazor 9.

## Repository layout

```
.claude/settings.json                 Claude Code permissions (allow/ask/deny)
.gitattributes                        line endings: LF in the repo, CRLF in the working tree
app/
  OrderPoint.slnx                     solution file
  .editorconfig                       formatting rules (UTF-8 BOM, 4 spaces, 120 columns)
  host/OrderPoint.AppHost             Aspire host: Postgres + API + Admin
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
- "Done" means the build succeeds with no new warnings. Known baseline: NuGet vulnerability warnings for `MessagePack` (AppHost, via Aspire) and `Microsoft.OpenApi` (Api); these will be fixed separately, so ignore them.
- Secrets live in user secrets, not appsettings (e.g. `MediatR:LicenseKey` for the Api).
- Running the app needs Docker (Aspire starts Postgres on port 59286). The design-time `ApplicationDbContextFactory` points at `localhost:5432`; that is fine for `migrations add`, which does not connect.

## Backend conventions

### Domain (`OrderPoint.Domain`)

- Entities are `sealed`, inherit `Entity` (`Id`, `CreatedAtUtc`, `UpdatedAtUtc`), have `private set` properties and a private constructor.
- Creation goes through `public static Result<T> Create(...)` (failures: `Result.Failure<T>(...)`); changes through `public Result Update(...)` (failures: `Result.Failure(...)`). Both validate invariants instead of throwing. `Update` sets `UpdatedAtUtc = DateTimeOffset.UtcNow`.
- IDs are `Guid.CreateVersion7()`, timestamps are `DateTimeOffset.UtcNow`.
- Navigation collections: `private readonly List<T> _items = []` exposed as `IReadOnlyList<T>`.
- Errors live in `Errors/<Entity>Errors.cs` as `static readonly Error` fields built with `Error.NotFound/Validation/Conflict/Failure`, code format `"<Entity>.<Reason>"`. Errors used only inside Domain are `internal`, errors used by handlers are `public`. (`Error.RequestValidation` is reserved for the API's FluentValidation exception handler.)
- Sort options are enums in `Sorting/<Entity>SortBy.cs` (`NameAsc`, `NameDesc`, ..., `CreatedAtUtcDesc`). Other enums go in `Enumerations/`.

### Application (`OrderPoint.Application`)

- One file per use case holding both the message and its handler:
  - `Commands/<Feature>/<Action><Entity>Command.cs`: `public sealed record XCommand(...) : ICommand` or `ICommand<TDto>`, plus `internal sealed class XCommandHandler(...) : ICommandHandler<...>`.
  - `Queries/<Feature>/<Get...>Query.cs`: `public sealed record XQuery(...) : IQuery<TDto>`, plus `internal sealed class XQueryHandler`.
- Use the project's own `ICommand`/`IQuery`/`ICommandHandler`/`IQueryHandler` interfaces from `Mediator/` (all `internal`), never MediatR's `IRequest` directly.
- Handlers return `Result`/`Result<T>`, never throw for business failures. Typical flow: load → `Result.Failure(XErrors.NotFound)` if null → check related entities exist → call domain `Create`/`Update` → propagate failure → repository → `unitOfWork.SaveChangesAsync`.
- Create handlers that return a DTO with navigation data reload the entity after saving (`itemRepository.GetAsync(id)`, which includes `Category`) and map that. If nothing needs loading, map directly (a new category has `itemsCount: 0`).
- Deleting a parent that still has children is blocked in the handler with a `Conflict` error (e.g. `CategoryErrors.CannotDeleteCategoryWithItems`), backed by `DeleteBehavior.Restrict` on the foreign key.
- DTOs: `public sealed record XDto(...)` in `Dtos/`. Mapping is manual via extension methods in `Dtos/Mappers/<Entity>Mapper.cs` (`ToXDto()`). No AutoMapper.
- Lists: paged lists return `PaginationDto<T>` (`Get<Plural>Query`). Unpaged lookups, such as autocomplete search, return `IReadOnlyList<T>` (`Search<Plural>Query`).
- Repository interfaces in `Repositories/` (`GetPaginatedAsync`, `SearchAsync`, `GetAsync`, `CreateAsync`, `Delete`, `ExistsAsync`, `CountAsync`...). Methods filtering by another entity name the parameter after it (`IItemRepository.ExistsAsync(Guid categoryId)` = "any item in this category"). Saving goes through `IUnitOfWork`, never from a repository.
- MediatR handlers are registered by assembly scan; no manual registration needed.

### Infrastructure (`OrderPoint.Infrastructure`)

- `ApplicationDbContext` implements `IUnitOfWork`; `DbSet`s are `internal`.
- One `IEntityTypeConfiguration<T>` per entity in `EfCore/EntityTypeConfiguration/` (table name, `ValueGeneratedNever()` on Id, `HasMaxLength`, `IsRequired`, relationships). Each property is configured as `builder` / `.Property(...)` / `...` on separate lines. Relationships use `.OnDelete(DeleteBehavior.Restrict)`. Picked up by assembly scan.
- Repositories are `internal sealed class <Entity>EfCoreRepository(ApplicationDbContext dbContext)` in `EfCore/Repositories/`, using private static `Search<X>`/`Filter<X>`/`Sort<X>` helpers for paginated queries (search is case-insensitive via `ToLower().Contains`; default sort `CreatedAtUtc` descending; unpaged search sorts by name ascending). `Sort<X>` builds an `IOrderedQueryable` and always ends with `.ThenBy(x => x.Id)` so paging is stable when sort values tie. Read-only queries (`GetPaginatedAsync`, `SearchAsync`) use `AsNoTracking()`; `GetAsync` stays tracked because commands update and delete what it returns, and it includes the navigations its DTO needs. Register in `InfrastructureModule`.

### API (`OrderPoint.Api`)

- `Program.cs` pipeline order: `ApplyMigrations` → `UseExceptionHandler` (first middleware) → `UseHttpsRedirection` → `UseCors` → endpoint and docs mapping. `launchSettings.json` URLs never contain a path (Kestrel refuses them); use `launchUrl` instead.
- One endpoint per file in `Endpoints/<Feature>/<Action><Entity>Endpoint.cs`: `internal sealed class XEndpoint : IEndpoint`, discovered automatically.
- Request and response records (`internal sealed record`) are declared in the same file above the endpoint; the FluentValidation validator is a nested `internal sealed class XRequestValidator` inside the endpoint class (auto-registered). Every endpoint that takes a body or query string has a request record and a validator.
- Query-string input is bound into a request record with `[AsParameters] XRequest request`; its constructor parameters carry `[FromQuery]` (see `GetItemsEndpoint`, `SearchCategoriesEndpoint`). Paged lists validate `PageNumber > 0` and `PageSize` between 1 and 100; search text is at most 100 characters (a search that cannot match returns no results, not an error); required search text uses `NotEmpty()` (also rejects whitespace). Route values (`{id:guid}`) stay as `[FromRoute]` parameters.
- `MapEndpoint` uses route `api/<plural>`, `api/<plural>/{id:guid}`, `api/<plural>/search?searchQuery=...` (unpaged lookup), plus `.WithName("<Action><Entity>")` and `.WithTags("<Plural>")`.
- `private static async Task<Results<..., ProblemHttpResult>> HandleAsync(...)` with explicit `[FromBody]`/`[FromRoute]`/`[FromQuery]`/`[FromServices]` attributes and a `CancellationToken`.
- Flow: `await validator.ValidateAndThrowAsync(request, ct)` → build command/query → `sender.Send` → `result.IsSuccess ? TypedResults.X(...) : result.ToProblemDetails()`.
- Responses wrap payloads as `{ Data }`: create → `CreatedAtRoute` to the Get route, update/delete → `NoContent`, get → `Ok`.
- Errors are ProblemDetails with an `errors` array; status mapping lives in `Extensions/ResultExtensions.cs`. Exception handlers in `Exceptions/` (registered in `Program.cs`, in order): `RequestValidationExceptionHandler` (FluentValidation → 400), `BadHttpRequestExceptionHandler` (missing/unparsable parameters or malformed JSON → 400; `ThrowOnBadRequest` is on so this applies in every environment), `GlobalExceptionHandler` (logs the exception, → 500 without exposing exception details). In .NET 10 the exception middleware does not log exceptions a handler reports as handled, so any new handler for unexpected errors must log itself.

### Validation is duplicated on purpose; keep it in sync

Field rules live in several places. Changing one means changing all of them:
1. Domain entity `Create`/`Update`: required and positive checks only (no max lengths)
2. API request validator (FluentValidation, messages like `"Name is required"`): required, positive, max length, enums
3. EF entity configuration (`HasMaxLength`, `IsRequired`) plus a migration if the schema changes
4. Admin request class DataAnnotations (messages end with a period: `"Name is required."`): required, positive, max length

## Admin web conventions (`web/OrderPoint.Admin`)

### Structure

- Feature folders: `<Feature>/Api` (`<Feature>ApiClient`, `Requests/`, `Responses/`), `Components/`, `Dialogs/`, `Dtos/`, `Enumerations/`, `Pages/`, `Sorting/`. Cross-feature code is in `Shared/` (`Components/`, `Dtos/`, `Errors/`, `Extensions/`, `Layout/`, `Pages/`, `Services/`). Reusable formatting helpers are `internal static` extension methods in `Shared/Extensions/<Type>Extensions.cs` (e.g. `DateTimeOffset.ToRelativeTime()` → "2 h ago"), not private methods on a page.
- Every component is split into `X.razor` (markup only) and `X.razor.cs` (`public sealed partial class X`). No `@code` blocks (`Dashboard/DashboardPage.razor` is an untouched template, not an example).
- Visibility: components are `public` (Razor requires it), so DTOs, enums and `PaginationDto` used as component parameters are `public`. Everything else (API clients, requests, responses, sort enums, `XSorting` helpers, services) is `internal`.
- Inject with `[Inject] private T Name { get; set; } = null!;`. Parameters use `[Parameter]` and `[EditorRequired]` where mandatory.
- Add new namespaces that pages use to `_Imports.razor`.

### API calls

- API clients are `internal sealed`, use relative URIs without a leading slash (`api/items?...`), escape user text with `Uri.EscapeDataString`, use `httpClientFactory.CreateClient("OrderPointApi")`, call `ApiExceptionHelpers.ThrowApiExceptionAsync` on non-success, and unwrap `.Data` from the response record. Register them in `Program.cs`.
- Pages and dialogs call API clients **only** through `ApiService`, which owns all API error handling and snackbars:
  - Queries: `T? result = await ApiService.ExecuteAsync(() => client.GetXAsync(...))`. Returns `null` on failure (error snackbar already shown), so fall back: `Items = Pagination?.Items ?? [];`.
  - Mutations: `bool isSuccess = await ApiService.ExecuteAsync(() => client.CreateXAsync(request), $"Item {request.Name} created successfully");`. Shows the success snackbar on success and the error snackbar on failure; reload the list only when `isSuccess`.
  - Autocomplete `SearchFunc`s pass MudBlazor's `CancellationToken` to both the client and `ExecuteAsync`, so cancelled searches are silent: `return categories ?? [];`.
  - Pages never inject `ISnackbar` for API results.
  - `ApiService` logs unexpected exceptions (network, parsing); expected API errors (ProblemDetails) and cancellations are not logged.

### Pages and dialogs

- **Sections load independently and in parallel.** Each section of a page (a table, a highlight card, a list in a dialog) has its own `Get<Section>Async` method and its own `IsLoading<Section>` flag; the method ends with `StateHasChanged()` so the section renders as soon as its data arrives. `OnInitializedAsync` and post-mutation reloads start independent sections with `await Task.WhenAll(...)`; never await independent loads one after another. Only dependent steps stay sequential (mutation → reload).
- Highlight sections above a list (Categories: top 5 by item count; Items: spotlight cards for newest, last edited, highest and lowest price) reuse the list endpoint with `pageSize`/`sortBy`; they do not get their own endpoints.
- List pages: `private const int PageSize` (10 for `DataTable`, 9 for the 3-column `DataGrid`, 5 for `DataList` in a dialog); one `Get<Plural>Async(int pageNumber)` that reads the current search/sort/filter properties; search, sort and filter changes call it with `pageNumber: 1`; `OnPageChangedAsync(int pageNumber)`.
- Sorting is passed as the sort enum's name: `SelectedSortBy = nameof(ItemSortBy.CreatedAtUtcDesc)`; labels and icons come from `<Feature>Sorting.GetSortByLabel/GetSortByIcon`.
- Create/update/delete/details are MudBlazor dialogs opened from the list page (not separate pages). Dialogs return the request object via `MudDialogInstance.Close(DialogResult.Ok(Request))`; the page performs the API call and reloads. Use early returns: `if (dialogResult.Canceled) { return; }`.
- A dialog that changes data the page shows (e.g. items inside `CategoryDetailsDialog`) exposes an `EventCallback On<Thing>Changed` parameter and invokes it after a successful mutation; the page passes `EventCallback.Factory.Create(this, ...)` in the `DialogParameters` and reloads. Do not rely on the dialog result for this: closing with X/Escape returns `Canceled`.
- Dialogs and components initialise state and load data in `OnInitialized`/`OnInitializedAsync` (parameters are already set). Do not use `OnParametersSet`: it runs again whenever the dialog provider re-renders, which resets forms and reloads lists.
- Forms: `EditForm` + `DataAnnotationsValidator`, MudBlazor inputs with `Variant.Outlined`, `HelperText`, `Immediate="true"`. `OnValidSubmit` only closes the dialog. Add `OnInvalidSubmit` + `IsFormSubmitted` only for inputs not covered by DataAnnotations `For` (e.g. a `MudAutocomplete` showing `Error`/`ErrorText`).

### Components and styling

- Each page has exactly one `<h1>`: the `PageHeader` title (rendered as `h1`, styled `Typo.h4`). `Routes.razor` focuses it after navigation, so nothing else (e.g. drawer labels) may render as `h1`.
- Reuse shared components: `PageHeader` (title + breadcrumbs), `DataTable` (tabular list, see Categories), `DataGrid` (card grid, see Items), `DataList` (list inside a dialog), `StatefulView` (loading/empty states for every independently loaded section; `Compact="true"` renders them without the surrounding paper, for use inside a card such as `ItemSpotlightCard`), `TextDisplayRow`, `ChipDisplayRow`. Extend them rather than building parallel ones. `DataTable`/`DataGrid` show the delete button when `OnDeleteClick` is set; `DeleteButtonDisabled` and `DeleteButtonDisabledTooltipText` are optional.
- Admin DTOs, enums and sort enums mirror the API ones by hand. When an API contract changes, update the Admin copy too.
- Use MudBlazor components and `Icons.Material.Filled.*`; avoid custom CSS. Colours come from the theme in `Shared/Layout/MainLayout.razor.cs` (`PaletteDark`), never hardcoded hex: dialogs use `Class="mud-background"`, borders use `var(--mud-palette-lines-default)`.
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
- Formatting follows `app/.editorconfig`: 4 spaces, UTF-8 **with BOM** for `.cs`, `.razor` and `.csproj` files. Line endings are handled by git (`.gitattributes`: LF in the repo, CRLF in the working tree); keep CRLF when creating or editing files.

## Adding a new entity end-to-end (checklist)

Domain entity + errors (+ sort enum) → repository interface → EF config + EF repository + DbSet + registration → migration → DTO + mapper → commands/queries → endpoints with validators → Admin DTOs/enums/sorting → API client + requests/responses + registration → page + dialogs → nav link in `Shared/Layout/MainLayout.razor` (add it, or check it already exists; Orders and Bartenders links are placeholders) → `dotnet build app/OrderPoint.slnx -c Release`.
