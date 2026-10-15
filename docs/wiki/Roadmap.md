# Roadmap

## Next: the API as a modular monolith

The API is one layered application today (Domain, Application, Infrastructure, Api). It will be split into modules that own their data and talk to each other through small public contracts:

| Module | Owns |
| --- | --- |
| Staff | Bartenders, users, sign-in, account |
| Catalog | Categories, items, the guests' menu |
| Floor | Areas, tables, fixtures, table requests |
| Inventory | Stock, recipes, suppliers, deliveries, counts |
| Ordering | Orders, payments, the register |
| Reporting | Dashboard, register day, My shift |

Each module gets its own database schema and DbContext; a change in one module that affects another (for example, completing an order takes stock off) travels as an in-process event in the same transaction. The apps don't change: routes, JSON and live messages stay the same.

The full plan, with the decisions and the eight phases, is in [`docs/modular-monolith-plan.md`](https://github.com/mucnjakf/OrderPoint/blob/main/docs/modular-monolith-plan.md). The [Backend architecture](Backend-Architecture) page describes the current design and will follow each phase.

## Later

- Screenshots of every app in this wiki.

## Ideas not planned yet

- A printable sheet with the QR codes of all tables.
- Orders and statistics per table in the Admin.
- Showing a guest's open "call bartender" request in the customer app's status bar.
- Pinch-to-zoom on the Bar app's tables map.

What was considered and left out is listed under [Scope](Scope).