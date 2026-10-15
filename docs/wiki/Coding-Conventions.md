# Coding conventions

How code is written in OrderPoint, so every feature reads like the others. The full, detailed rule book is [`CLAUDE.md`](https://github.com/mucnjakf/OrderPoint/blob/main/CLAUDE.md) in the repository; this page is the readable summary.

## General

- **Follow the existing pattern.** Before adding a file, open the closest existing one (e.g. `Item*` for a new entity) and mirror its structure, naming and level of detail.
- **Keep visibility low:** `private` over `internal` over `public`. Only DTOs, entities and components that other projects or Razor need are public.
- **Readable over clever:** clear names, early returns, no magic numbers or strings (`const`, `nameof`).
- **No new libraries, folders or architectural styles** without a reason agreed first.

## C# style

- File-scoped namespaces matching the folder; `using` directives at the top.
- Classes are `sealed` by default; primary constructors for dependency injection.
- **Explicit types** for locals (`Result<ItemDto> result = ...`); `var` only when the type is obvious (`new`, casts, `ToXDto()`, generic calls naming the type), never for built-in types.
- Always braces; `is null` / `is not null`; collection expressions `[]`.
- Async methods end in `Async` and take a `CancellationToken`.
- Lines are at most 120 characters; a call that doesn't fit gets one argument per line.
- Formatting follows `app/.editorconfig`: 4 spaces, UTF-8 with BOM for `.cs`, `.razor` and `.csproj`, no final newline, CRLF in the working tree (LF in the repository through `.gitattributes`). Local constants in top-level `Program.cs` files are camelCase.
- Comments are rare and short: one line, only for something surprising.

## Backend

| Thing | Convention |
| --- | --- |
| Entity | `sealed`, inherits `Entity`, private setters and constructor, `static Result<T> Create(...)`, `Result Update(...)` |
| Errors | `Errors/<Entity>Errors.cs`, code `"<Entity>.<Reason>"`; `internal` when only the domain uses it |
| Use case | One file: `Commands/<Feature>/<Action><Entity>Command.cs` with the record and its `internal sealed` handler (same for queries) |
| Handler result | `Result` / `Result<T>`, never an exception for a business failure |
| DTO and mapping | `public sealed record XDto(...)` in `Dtos/`, hand-written `ToXDto()` mappers; no AutoMapper |
| Repository | Interface in Application `Repositories/`, `internal sealed class <Entity>EfCoreRepository` in Infrastructure; methods named after what they filter by (`ExistsByCategoryAsync(Guid categoryId)`) |
| Endpoint | One file `Endpoints/<Feature>/<Action><Entity>Endpoint.cs`: request record, the endpoint class, a nested validator; route `api/<plural>`; `.WithName`, `.WithTags`, `.WithSummary`, `.WithDescription` listing its errors, `.ProducesProblem`, then `.RequireAuthorization(...)` last |
| Documentation | Every request, response and DTO record has `/// <summary>` and one `/// <param>` per field |
| Caching | A new query gets a `CacheKeys.For` overload and a tag; a command that changes cached data implements `ICacheInvalidatingCommand` |
| Live updates | Commands that change orders, the menu or tables implement the matching notifying marker interface |
| Logging | Outcomes worth keeping use source-generated `[LoggerMessage]` methods with named placeholders; never passwords, tokens or card numbers |

## Frontend

| Thing | Convention |
| --- | --- |
| Structure | Feature folders: `Api/`, `Components/`, `Dialogs/`, `Dtos/`, `Enumerations/`, `Pages/`, `Sorting/`; shared code in `Shared/` or the `OrderPoint.Shared` library |
| Components | `X.razor` for markup only and `X.razor.cs` for `public sealed partial class X`; no `@code` blocks |
| Injection | `[Inject] private T Name { get; set; } = null!;` |
| API calls | Only through `ApiService`; typed clients are `internal sealed`, use relative URLs and escape user text |
| Loading | Each section loads on its own (`Get<Section>Async` + `IsLoading<Section>`), in parallel, with a skeleton that mirrors the real layout |
| Live updates | Subscribe in `OnInitialized(Async)`, unsubscribe in `Dispose`, reload silently |
| Dialogs vs pages | Admin: dialogs for create, edit, delete and details. Phone apps: pages with a back arrow, dialogs only for confirmations |
| Styling | MudBlazor components and Material icons; colours from the theme; no custom CSS beyond the few scoped styles in the shared library |
| Razor attributes | Literals plain (`Label="Sort by"`), code with `@` (`Items="@Items"`), expressions with `@(...)` |
| Times and numbers | Show times in the viewer's time zone (`ToDisplayDateTime`), format with the current culture, amounts as `"C"` |

## Adding a feature end to end

1. **Domain:** entity, errors, sort enum.
2. **Application:** repository interface, DTO and mapper, commands and queries (with caching and invalidation).
3. **Infrastructure:** EF configuration, repository, `DbSet`, registration, migration.
4. **API:** endpoints with validators and authorization, documented.
5. **Admin (or app):** DTO and enum copies, API client and requests, page, dialogs or pages, skeletons, the nav link.
6. **Validation in sync:** domain, API validator, database, form.
7. **Build:** `dotnet build app/OrderPoint.slnx -c Release` with no warnings.

## Building and checking

- **"Done" means a clean build:** `dotnet build app/OrderPoint.slnx -c Release` with no warnings. Use Release while Aspire runs; the Debug outputs are locked by the running apps.
- **Formatting:** `dotnet format whitespace app/OrderPoint.slnx --verify-no-changes` (it doesn't look at `.razor` files). Some style suggestions are ignored on purpose, such as rewriting `.ToList()` as collection expressions.
- **Tests:** there is no automated test project yet; changes are verified by using the apps.

## Commits

- Describe what changed for the user in one line, e.g. `Inventory phase 5: dashboard gross profit ... and a low stock tile under Right now with reorder`.
- One feature or fix per commit; migrations are committed with the code that needs them.

## Documentation

- `CLAUDE.md` is the detailed rule book and must follow every change.
- This wiki lives in `docs/wiki/` in the repository. Page files are named like their titles with hyphens (`Bar-App.md` is "Bar App"), links use the page name without `.md` (`[Bar app](Bar-App)`), and `_Sidebar.md` and `_Footer.md` frame every page. Diagrams are Mermaid; keep semicolons out of them.

### Publishing the wiki

The GitHub wiki is a separate Git repository. Push the pages from `docs/wiki` to it by hand:

```bash
# once: the wiki needs a first page created in the browser (Wiki tab, Create the first page, Save)
git clone https://github.com/mucnjakf/OrderPoint.wiki.git ../OrderPoint.wiki

# every time
cd ../OrderPoint.wiki
git pull
rm -f *.md                                        # removed pages disappear too
cp ../OrderPoint/docs/wiki/*.md .
git add -A
git commit -m "Update wiki from docs/wiki"
git push
```

Edit pages in `docs/wiki`, never in the GitHub editor, or the next push overwrites the change.