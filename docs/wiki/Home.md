# OrderPoint

OrderPoint is an order management system for a bar. Guests scan the QR code on their table and order from their phone, bartenders prepare the orders as they come in, the register takes payments at the counter, and the owner manages the menu, the team, the floor plan and the stock from an admin web app. Everything updates live: a new order shows up at the bar the moment it is placed, and a guest sees their order move from "Sent to the bar" to "Served" without refreshing.

## The five apps

| App | Who uses it | On | What it does |
| --- | --- | --- | --- |
| [Customer app](Customer-App) | Guests, no sign-in | Their phone | Menu, cart, card or cash payment, live order status, calling a bartender |
| [Bar app](Bar-App) | Bartenders behind the bar | Phone or tablet | The order queue: accept, start, complete or decline orders; tables map; their shift |
| [Bartender app](Bartender-App) | Bartenders on the floor | Phone or tablet | Taking orders at the tables and at the counter for guests who order in person |
| [Register app](Register-App) | Bartenders at the till | Desktop | Counter sales, cash and card payments, refunds, receipts, the day's takings |
| [Admin web](Admin-Web) | The owner | Desktop | Dashboard, orders, payments, menu, team, floor plan, stock, suppliers, deliveries, counts |

All five talk to one API, which keeps the data in PostgreSQL, the pictures in Azure Blob Storage and a read cache in Redis, and pushes live updates over SignalR. Aspire runs the whole system on a developer machine with one command.

## Highlights

- **Ordering from the table:** a QR code per table opens the menu with the table already chosen; guests pay by card or choose to pay in cash at the register.
- **Live everywhere:** orders, menu changes, sold-out items, table calls and payments appear in every app at once.
- **A real bar workflow:** an order is claimed by one bartender, prepared and served; open orders of a bartender who leaves go back to the queue.
- **Stock that follows sales:** recipes take ingredients off the stock when an order is served, items sell out on their own when an ingredient runs out, and the dashboard shows gross profit.
- **Floor plan editor:** areas, tables and fixtures dragged into place, with a QR code for every table.
- **Observability built in:** every request is traced from the app to the database in the Aspire dashboard.

## Where to start

- **Seeing what it does:** [Features overview](Features-Overview), then the page of each app.
- **Understanding how it is built:** [System architecture](System-Architecture), [Backend architecture](Backend-Architecture), [Frontend architecture](Frontend-Architecture).
- **Running it:** [Getting started](Getting-Started) takes you from installing the tools to signing in.
- **Unfamiliar words:** [Glossary](Glossary).

## Wiki map

- **Overview:** [Scope](Scope) · [Glossary](Glossary) · [Roadmap](Roadmap)
- **Product:** [Features overview](Features-Overview) · [Customer app](Customer-App) · [Bar app](Bar-App) · [Bartender app](Bartender-App) · [Register app](Register-App) · [Admin web](Admin-Web) · [Roles and permissions](Roles-and-Permissions) · [Order lifecycle](Order-Lifecycle) · [Payments and refunds](Payments-and-Refunds) · [Inventory](Inventory) · [Tables and floor plan](Tables-and-Floor-Plan)
- **Architecture:** [System architecture](System-Architecture) · [Backend architecture](Backend-Architecture) · [Frontend architecture](Frontend-Architecture) · [Real-time updates](Real-Time-Updates) · [Caching](Caching) · [Security](Security) · [Observability](Observability) · [Key flows](Key-Flows)
- **Data:** [Domain model](Domain-Model) · [ER diagram](ER-Diagram)
- **Technical specification:** [Technical specification](Technical-Specification) · [Tech stack](Tech-Stack) · [API reference](API-Reference) · [Error codes](Error-Codes) · [Validation rules](Validation-Rules) · [Configuration](Configuration) · [Architecture decisions](Architecture-Decisions)
- **Development:** [Getting started](Getting-Started) · [Testing on a phone](Testing-on-a-Phone) · [Database and migrations](Database-and-Migrations) · [Coding conventions](Coding-Conventions) · [Troubleshooting](Troubleshooting)