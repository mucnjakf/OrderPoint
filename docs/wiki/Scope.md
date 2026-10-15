# Scope

What OrderPoint covers, what it deliberately leaves out, and the assumptions behind it.

## In scope

| Area | What is covered |
| --- | --- |
| Guest ordering | QR code per table, menu with categories and search, cart, card or cash payment, live order status, cancelling while pending, ordering again, calling a bartender |
| Bar workflow | Order queue, claiming an order, preparing and serving it, declining with a reason for the guest, a live tables map, the bartender's own shift numbers |
| Floor ordering | Bartenders taking orders at a table or the counter for guests who order in person; those orders are paid at the register |
| Register | Counter sales paid in one step, paying orders from the tables, refunds handed back, printable receipts, the day's takings per method and per bartender |
| Menu management | Categories and items with pictures, prices, portions, statuses, menu order, bulk changes (status, category, price) |
| Team | Bartender accounts with photos, temporary passwords, password resets, deactivation that hands their open orders back to the queue |
| Floor plan | Areas, tables and fixtures on a grid, drag and drop, QR codes per table |
| Inventory | Stock items in packs and base units, waste and corrections, recipes, sold out from stock, suppliers, deliveries with average cost, reorder suggestions, stock counts |
| Reporting | Dashboard with revenue, orders, service time, trends, busiest times, categories, payment methods, gross profit, low stock, top items, bartender leaderboard, recent orders |
| Operations | Health page for every component, tracing, metrics and logs in the Aspire dashboard |

## Out of scope

These were considered and left out on purpose:

- **Deployment.** OrderPoint runs on a developer machine through Aspire, and phones reach it through a dev tunnel. There is no production hosting, so concerns such as data protection key storage, public blob access or exposing the API docs outside development are not addressed.
- **Real payments.** Card payments are simulated: a test provider approves every card except one test number. Card details are never stored or logged. At the register, the bar's own card terminal charges the card and OrderPoint only records it.
- **More than one venue.** One bar, one menu, one currency (euros). No multi-tenancy.
- **Features ruled out for now:**
  - running the till: cash drawer close, tips, happy hour prices
  - menu and service: item options or modifiers, reservations
  - staff: time clock, audit log
  - Admin lists: CSV export of orders, per-item and per-bartender insights, item order within a category, undoing deletes
  - Bar app: a "Right now" view and a team leaderboard
  - phones: sound or vibration for new orders, keeping the screen awake
- **Admin on small screens.** The Admin web is built for desktop screens only.
- **Bills.** Guests pay when they order (card) or at the register (cash), so there is no "bill please".
- **Waste and counting at the bar.** Inventory is managed in the Admin only.

## Assumptions

- A **business day** starts at 06:00 local time, so a night that goes past midnight counts as one day (configurable).
- Each order has **one payment**, for its full total.
- Times are stored in UTC and shown in the viewer's time zone; numbers, dates and the 12 or 24-hour clock follow the viewer's browser, while the interface text is English.
- Stock leaves the shelf when an order is **completed**, not when it is accepted.

## Constraints

- .NET 10 and C# throughout, with Blazor Server for every web app.
- Docker on the developer machine, for PostgreSQL, the Azure Storage emulator and Redis.
- No automated tests yet; changes are verified with a clean build and by using the apps.