# Technical specification

What OrderPoint must do (functional requirements) and how well it must do it (non-functional requirements), with links to the pages that hold the details.

## 1. Purpose

OrderPoint manages orders in one bar:
- guests order and pay from their table;
- bartenders take, prepare and serve orders and handle payments at the register;
- the owner manages the menu, staff, floor plan and stock, and follows the business on a dashboard.

See [Scope](Scope) for what is in and out.

## 2. System overview

Five web apps (Customer, Bar, Bartender, Register, Admin) on one API, with PostgreSQL, Blob Storage and Redis, run together by Aspire. Details: [System architecture](System-Architecture).

## 3. Users

| Role | Apps | Authentication |
| --- | --- | --- |
| Guest | Customer app | None; the table comes from the QR code |
| Bartender | Bar, Bartender, Register | Email and password, JWT |
| Admin | Admin web | Email and password, JWT |

See [Roles and permissions](Roles-and-Permissions).

## 4. Functional requirements

### 4.1 Guest ordering

| Id | Requirement |
| --- | --- |
| FR-G1 | Scanning a table's QR code opens the menu for that table; an unknown or closed table is refused. |
| FR-G2 | Guests browse categories and items in menu order, search them and see each item's details; only orderable items appear. |
| FR-G3 | Guests build a cart (up to 20 of an item) with a note, kept on the phone for one visit (12 hours). |
| FR-G4 | Guests pay by card (simulated) or choose cash, paid later at the register. A declined card creates no order. |
| FR-G5 | Guests follow each order's status live and may cancel it while it is pending (a card payment is refunded). |
| FR-G6 | Guests see the orders placed on their phone in the last 12 hours and can order one again. |
| FR-G7 | Guests can call a bartender to their table and see when the call is answered. |

### 4.2 Bar workflow

| Id | Requirement |
| --- | --- |
| FR-B1 | Bartenders see pending orders oldest first, their own open orders, and their finished orders of the business day, all updating live. |
| FR-B2 | Any bartender can accept a pending order, which assigns it to them; only that bartender can start and complete it. |
| FR-B3 | A bartender can decline a pending order or their own accepted one with a reason the guest sees; the admin can decline any open order. |
| FR-B4 | Completing an order takes its recipes' ingredients off the stock. |
| FR-B5 | Waiting times are highlighted after 5 and 10 minutes. |
| FR-B6 | A live map shows every table's open orders and calls; bartenders mark calls done. |
| FR-B7 | Bartenders can take items or categories off the menu instantly. |
| FR-B8 | Bartenders see their shift: queue, open orders, completed today, average service time, orders per hour, most served items. |

### 4.3 Floor ordering

| Id | Requirement |
| --- | --- |
| FR-F1 | Bartenders take orders for a table or the counter (with an optional name), with a separate draft per table. |
| FR-F2 | These orders go to the bar unpaid and appear in the register's To pay list. |
| FR-F3 | A bartender can withdraw an order they entered while it is pending, and sees the orders they took that day with their payment state. |

### 4.4 Register

| Id | Requirement |
| --- | --- |
| FR-R1 | Bartenders sell at the counter and take the payment in the same step. |
| FR-R2 | Cash payments record the amount received (at least the total, at most €1,000) and show the change; card payments are recorded after the terminal approves them. |
| FR-R3 | Unpaid orders from the tables can be paid at the register. |
| FR-R4 | Paid orders that were declined, cancelled or withdrawn appear as refunds due until marked refunded. |
| FR-R5 | Every payment has a printable receipt. |
| FR-R6 | The register shows the business day's takings per method and per bartender, and every payment. |

### 4.5 Management (Admin)

| Id | Requirement |
| --- | --- |
| FR-A1 | The admin manages categories and items with pictures, statuses, prices and menu order, including bulk changes. |
| FR-A2 | The admin manages bartender accounts: create with a temporary password, edit, picture, reset password, activate or deactivate (releasing their open orders), delete when unused. |
| FR-A3 | The admin draws the floor plan (areas, tables, fixtures) by drag and drop and gets a QR code per table. |
| FR-A4 | The admin browses orders and payments with search, filters and details. |
| FR-A5 | The dashboard shows revenue, orders, average order, service time, trends, busiest times, category revenue, payment methods, gross profit, low stock, top items, a leaderboard, recent orders and live figures, for today, 7 or 30 days, compared with the previous period. |
| FR-A6 | A global search finds orders, items, categories, bartenders, tables and pages. |
| FR-A7 | The admin sees the health of every component. |

### 4.6 Inventory

| Id | Requirement |
| --- | --- |
| FR-I1 | Stock items are kept in base units with packs, costs, par levels, suppliers and a full movement history. |
| FR-I2 | Waste and corrections are recorded with a note. |
| FR-I3 | Items have recipes with cost and margin; an item sells out automatically when an ingredient has less than one serving. |
| FR-I4 | Deliveries add stock and update the average cost; suppliers are managed with contact details. |
| FR-I5 | A reorder list suggests whole packs per supplier back to twice the par level. |
| FR-I6 | Stock counts compare the shelves with the system while the bar stays open and apply the differences when closed. |

Details: [Features overview](Features-Overview), the app pages, [Order lifecycle](Order-Lifecycle), [Payments and refunds](Payments-and-Refunds), [Inventory](Inventory), [Tables and floor plan](Tables-and-Floor-Plan).

## 5. Non-functional requirements

| Id | Area | Requirement | How it is met |
| --- | --- | --- | --- |
| NFR-1 | Responsiveness | Changes appear in every app within about a second. | SignalR messages after every change; pages reload silently |
| NFR-2 | Performance | Lists and dashboards load quickly under repeated live reloads. | HybridCache (memory + Redis) with tag invalidation; paged queries; aggregations in SQL |
| NFR-3 | Consistency | No app shows stale data after a change. | The cache is cleared before notifications are sent |
| NFR-4 | Integrity | Data can't be lost or orphaned by accident. | Restrictive foreign keys, blockers checked before deletes, all-or-nothing bulk operations, order lines and stock movements that are never edited |
| NFR-5 | Security | Only the right role can act; guests can't reach others' orders. | JWT with roles, re-checks for deactivated staff, unguessable order ids, input limits; see [Security](Security) |
| NFR-6 | Privacy | Card data is never stored or logged. | The provider receives it; logs only name the request |
| NFR-7 | Availability | The system keeps working without its optional parts. | Redis fails fast; developer tools and apps report Degraded; lost connections reconnect and reload |
| NFR-8 | Observability | Every request can be traced end to end. | OpenTelemetry to the Aspire dashboard; health page; see [Observability](Observability) |
| NFR-9 | Usability | Each app suits its device: phones for guests and floor staff, tablets at the bar, desktops for the till and the owner. | Separate apps with their own layouts; skeletons; stable list heights |
| NFR-10 | Localisation | Dates, times and numbers follow the viewer; amounts in euros. | Per-request culture from the browser, viewer's time zone, UTC storage |
| NFR-11 | Maintainability | Features follow one pattern end to end. | Layered API, one file per use case and endpoint, feature folders; see [Coding conventions](Coding-Conventions) |
| NFR-12 | Portability | The whole system runs on one developer machine with one command. | Aspire with containers; see [Getting started](Getting-Started) |

## 6. Interfaces

- **HTTP API:** 118 endpoints with OpenAPI and Scalar; see [API reference](API-Reference) and [Error codes](Error-Codes).
- **Real time:** two SignalR hubs; see [Real-time updates](Real-Time-Updates).
- **Payment provider:** `IPaymentProvider`, simulated.
- **Image storage:** Azure Blob Storage container `images`.

## 7. Data

19 tables in PostgreSQL; see [Domain model](Domain-Model) and [ER diagram](ER-Diagram). Field limits: [Validation rules](Validation-Rules).

## 8. Constraints and assumptions

- .NET 10, Blazor Server, PostgreSQL, Aspire; Docker on the developer machine.
- One bar, euros, English interface, one payment per order, business day from 06:00.
- No deployment, no real payment processing, no automated tests yet.

See [Tech stack](Tech-Stack), [Configuration](Configuration) and [Architecture decisions](Architecture-Decisions).