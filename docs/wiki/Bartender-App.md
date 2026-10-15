# Bartender app

The app for bartenders on the floor. Some guests prefer to order in person: the bartender takes the order on a phone at the table or at the counter and sends it to the bar. These orders are not paid in the app; the guest pays at the register.

- **Project:** `app/web/OrderPoint.Bartender` (Blazor Server)
- **Local address:** `https://localhost:7036`
- **Sign-in:** bartender account, the same as in the Bar and Register apps
- **Navigation:** a bottom bar with **Tables**, **Orders** and **Account**, and a live connection indicator in the app bar

## Tables: taking an order

The start page, "Take an order":

- **Calls** from guests on top ("T7 · Call bartender", asked how long ago); **Done** marks one answered, tapping it opens that table's order pad.
- **Counter:** an order at the bar counter, without a table.
- **Every area with its tables** as tiles showing the code and the seats; a bell marks a table whose guests are calling. A closed table is faded, says "Closed" and can't be used.

## The order pad

Three pages, like the guests' menu, each with the place ("Table T7" or "Counter") under the title:

1. **Categories** with a search.
2. **A category's items** with a search; **+** adds one and becomes **− n +**. Tapping an item opens it.
3. **An item** with its full card and "Add · €8.00", or "In the order" with a stepper and **Review**.

Only what guests can order is offered (active and not sold out). While something is added, a bar above the navigation shows "3 items · €20.50" and **Review**.

Each table and the counter keep their own **draft**, so a bartender can switch between tables without losing what they added. A draft disappears once it is sent.

## Review and send

The lines with pictures, prices and steppers; for the counter an optional **name to call** (30 characters); a **note for the bar** (200 characters); the total, and "Not paid yet: the guest pays at the register". **Send to the bar** places the order: it shows up in the Bar app's queue like a guest's order, and in the Register app's To pay list. If an item left the menu in the meantime, the line is marked and nothing is sent.

## Orders

"The orders you took since 06:00", newest first, each with its number, place, time, total, status, lines, note, decline reason and its payment: "To pay at the register" (amber), "Paid in cash", "Paid by card", "Refund due at the register", "Refunded" or "Nothing to pay". While an order is still pending, **Withdraw** takes it back after a confirmation. Everything updates live.

## Account

The same account page as in the Bar app: profile, appearance, password, sessions, sign out.