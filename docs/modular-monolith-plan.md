# Modular monolith plan

Agreed on 2026-10-16. The API (`app/api`) is split into modules. Nothing outside the API changes: the routes, the JSON and the SignalR messages stay the same, so the Admin, Bar, Bartender, Register and customer apps are untouched.

**Status:** plan agreed; phase 0 not started.

## Decisions

| Question | Decision |
| --- | --- |
| Projects | Two per module: `OrderPoint.<Module>` (everything `internal`) and `OrderPoint.<Module>.Contracts` (what other modules may use), plus `OrderPoint.SharedKernel` and the `OrderPoint.Api` host |
| Database | One Postgres database, one schema per module, each module with its own DbContext and migrations history. No foreign keys across modules |
| Writes across modules | In-process integration events, handled in the same request and the same database transaction (no outbox) |
| Reporting | Through the module contracts: each module aggregates in its own SQL, Reporting combines the results and adds names and pictures from Catalog and Staff |

## Modules

| Module | Owns | Uses (through contracts) |
| --- | --- | --- |
| **Staff** | Bartender, users, refresh tokens, auth, account | Ordering: a bartender with orders can't be deleted; deactivating releases their open orders |
| **Catalog** | Category, Item, the guests' menu | Inventory: sold out and cost. Ordering: order counts, delete blockers, images order lines still show |
| **Floor** | Area, Table, Fixture, TableRequest | Ordering: a table with orders can't be deleted |
| **Inventory** | StockItem, StockMovement, RecipeLine (keyed by item id), Supplier, Delivery, StockCount | Catalog: item names and prices for recipes |
| **Ordering** | Order, OrderItem, payments, register sales, payments list, tables status | Catalog: prices and availability. Floor: table by code. Staff: bartender checks, names and photos. Inventory: stock taken off on Complete (event) |
| **Reporting** | Dashboard widgets, register day, My shift, My day | Reads only, through every module's contract |

## Where the code is coupled today

- **Domain navigations across modules:** `Item.RecipeLines` (and through it `StockItem`) for `IsAvailable`, `IsSoldOut` and `Cost`; `Order.Bartender`, `TakenByBartender`, `PaidByBartender`; `OrderItem.Item`; `Item.OrderItems`; `Bartender.Orders`; `Order.TableId`.
- **Handlers using other features' repositories:**
  - Placing an order reads items, recipes, tables and bartenders.
  - `CompleteOrderCommand` writes stock movements in the same save.
  - Bartender commands release orders.
  - Items, categories and tables check orders before deleting.
  - Deliveries and counts set `ChangedMenu`.
- **Cross-cutting:**
  - Cache tag groups like `CompletedOrderChanges` clear five features at once.
  - `OrderDto` carries the bartender's name and photo.
  - `IDashboardRepository` joins over everything.

## Target layout

```
app/api/
  OrderPoint.Api                    host: Program, hubs and notifiers, OpenApi, exception handlers, health
  OrderPoint.SharedKernel           Entity, Result/Error, Mediator interfaces and behaviours, caching helpers,
                                    telemetry, IModule, integration event plumbing, IImageStorage
  modules/
    Staff/       OrderPoint.Staff      OrderPoint.Staff.Contracts
    Catalog/     OrderPoint.Catalog    OrderPoint.Catalog.Contracts
    Floor/       OrderPoint.Floor      OrderPoint.Floor.Contracts
    Inventory/   OrderPoint.Inventory  OrderPoint.Inventory.Contracts
    Ordering/    OrderPoint.Ordering   OrderPoint.Ordering.Contracts
    Reporting/   OrderPoint.Reporting
```

- **Inside a module:** folders `Domain/`, `Application/` (commands, queries, DTOs), `Infrastructure/` (DbContext, entity configurations, migrations, repositories) and `Endpoints/`. Everything is `internal`; the module exposes only `Add<Module>Module()` and `Map<Module>Endpoints()`.
- **Contracts:** `I<Module>Module` (e.g. `IOrderingModule.HasOrdersForTableAsync(tableId)`, `ICatalogModule.GetOrderableItemsAsync(ids)`), small DTOs and integration events. A module references other modules' Contracts only, never their implementation, so the compiler enforces the boundaries.

## How modules work together

- **Integration events** (e.g. `OrderCompleted(orderId, number, lines)`, `BartenderDeactivated`, `ItemDeleted`, `StockServingsChanged`):
  - A command raises them.
  - A `TransactionBehavior` runs them in the same transaction. Each request has one Npgsql connection, shared by every module DbContext. The behavior begins the transaction inside EF's execution strategy (required by the Aspire retries), runs the handler, dispatches the collected events to the other modules' handlers, then commits.
  - Cache invalidation and SignalR notifications run after the commit, as today.
- **Caching:** each module clears only its own tags, in its own event handlers (Inventory clears `stock` on `OrderCompleted`). This replaces the cross-feature tag groups.
- **Cross-module data in DTOs:** looked up through contracts, cached in the module that owns the data. Examples: bartender names and photos in `OrderDto`, current item names in top items.

## Database migration

- While the old `ApplicationDbContext` still exists, every extraction adds a hand-written migration to it, `Move_<Module>_To_Schema`. It moves the module's tables with `ALTER TABLE ... SET SCHEMA <module>` and drops the foreign keys that cross modules.
- The module's own first migration is an empty baseline: its model snapshot, no SQL.
- On startup the old context migrates first, then the modules. This works for the existing database (data kept) and for an empty one.

## Phases

Each phase builds without warnings and leaves every app working; the owner tests and commits after each.

| # | Phase | Content |
| --- | --- | --- |
| 0 | Groundwork | SharedKernel; the module registration pattern (MediatR handlers, validators, endpoints and OpenAPI XML comments scanned from every module assembly); shared connection, `TransactionBehavior` and integration events; today's Domain, Application and Infrastructure become the legacy module. No behaviour change. Starts with a spike proving the shared connection, the transaction and the Aspire retries with two DbContexts |
| 1 | Floor (pilot) | Areas, tables, fixtures, table requests. `OrderPoint.Ordering.Contracts` is created now, implemented by the legacy code until phase 5 |
| 2 | Staff | Bartenders, Identity (`UserManager` and its store on the Staff DbContext), auth, account. `BartenderDeactivated` releases orders |
| 3 | Catalog | Categories, items, menu. Sold out and cost from Inventory's contract (legacy implementation until phase 4) |
| 4 | Inventory | All of stock. Handles `OrderCompleted` and `ItemDeleted`, raises `StockServingsChanged` for live sold out |
| 5 | Ordering | Orders, payments, register, payments list, tables status, and the reporting methods that aggregate in SQL. Implements the contracts the legacy code held |
| 6 | Reporting | Dashboard, register day, My shift, My day, built from the module contracts |
| 7 | Clean-up | Delete the legacy projects and `ApplicationDbContext` (optionally fold the old migrations into one initial migration per module); rewrite the backend sections of CLAUDE.md |

## Risks

- **Transactions:** EF's execution strategy allows explicit transactions only inside `strategy.ExecuteAsync`, and every DbContext must use the same `NpgsqlConnection`. Phase 0's spike proves this before anything moves.
- **DTOs that join across modules** lose those joins and need contract lookups (bartender names and photos, current item names).
- **Size:** about 490 files are touched, mostly moves and namespace changes. Phases 3 to 5 are the largest.
- **No test project:** the build and manual testing after each phase are the safety net. An architecture test project that checks the module boundaries would catch slips, but is only added if asked.