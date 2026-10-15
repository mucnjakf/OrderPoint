# Bar app

The app behind the bar. Bartenders see the queue of new orders, claim them, prepare and serve them, and keep an eye on the tables. It works on a phone and, with a split view, on a tablet.

- **Project:** `app/web/OrderPoint.Bar` (Blazor Server)
- **Local address:** `https://localhost:7034`
- **Sign-in:** bartender account (the same one as in the Bartender and Register apps). A new bartender first chooses their own password.
- **Navigation:** a bottom bar with **Orders**, **Tables**, **My shift**, **Menu** and **Account**; the app bar shows a live connection indicator (green Live, amber Connecting, red Offline)

## Orders

Three tabs, each with a live count:

| Tab | Shows |
| --- | --- |
| **Queue** | Every pending order, oldest first; new arrivals are highlighted |
| **Mine** | The bartender's active orders, then their accepted ones |
| **Done** | The bartender's completed and declined orders since the business day started, newest first |

Each ticket shows the order number, the place ("Table T7", "Counter · Ana" or "Counter"), how long ago it was placed, the total, the status, every line with a picture, the note, and who took it if a bartender entered it. **Waiting times stand out:** an open order's time turns amber after 5 minutes and red after 10.

Above the tabs:
- **Search** by order number or table finds orders of any status and any bartender.
- **Guest calls** ("T7 · Call bartender"), each with **Done**; tapping one opens the table on the map.

On a phone a ticket opens its own page; from 600 px wide the screen splits into the list and a details panel.

## Order details and actions

The details show the lines, the note, the total, the status steps with their times and who handles the order. The buttons at the bottom match what this bartender may do:

| Order | Buttons |
| --- | --- |
| Pending | **Decline** and **Accept** (any bartender) |
| Accepted by me | **Decline** and **Start** |
| Active, mine | **Complete** |
| Someone else's | none |

**Decline** opens a page with the ticket and a required reason for the guest, with quick picks ("Out of stock", ...). If someone else acted first, the app shows why and reloads. Completing an order takes its ingredients off the stock (see [Inventory](Inventory)).

## Tables

The floor plan the owner drew, full screen and pannable with a finger, with tabs per area. Tables are tinted by their orders: **amber** while an order waits for a bartender, **blue** while one is in progress, **red** once an open order has waited 10 minutes or more. A badge shows the number of open orders, and a bell shows a guest's call. Tapping a table shows its open orders and calls (a bottom sheet on a phone, a side panel on a tablet). "Show on map" on an order jumps here and centres the table.

## My shift

"Your day since 06:00": tiles for the **queue**, **my open orders**, **completed today** and my **average service time** (placed to completed), then **My day** with what I served, items served, a bar chart of completed orders per hour and my three most served items. The order tiles open the matching Orders tab.

## Menu

The menu as the guests see it, to answer questions and to take things off when they run out: categories, their items and each item's card. An **"Available to guests"** switch on an item (and an Active switch on a category) takes it off or puts it back at once, without confirmation. Unavailable and sold-out items stay visible here with a chip saying so.

## Account

The bartender's profile as the admin set it (photo, name, email, phone), and settings for appearance, password, signing out everywhere and signing out.