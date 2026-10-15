# Customer app

The guests' app. A guest scans the QR code on their table, browses the menu, orders and pays from their phone, and follows the order until it is served. There is no sign-in: the table comes from the QR code and the guest's orders are remembered on their phone.

- **Project:** `app/web/OrderPoint.Customer` (Blazor Server)
- **Local address:** `https://localhost:7038`; on a phone through the dev tunnel (see [Testing on a phone](Testing-on-a-Phone))
- **Navigation:** a bottom bar with **Menu**, **Cart**, **Orders** and **Visit**; the app bar shows the table (e.g. "Terrace · T7")

## Scanning a table

A table's QR code opens `/t/<code>`, for example `/t/T7`. The app checks the code with the API: an existing, open table is remembered on the phone and the guest lands on the menu. An unknown or closed table shows "Table not available - please ask the staff", and the guest keeps the table they had.

The table is remembered for **one visit (12 hours)**, so a guest coming back days later scans again instead of ordering to an old table. The cart and the list of the guest's orders expire the same way.

## Menu

- **Categories** in the bar's menu order, as cards with a picture and the number of items, with a search field.
- **A category** lists its items with picture, name, price, portion and description. A **+** button adds one to the cart and turns into **− n +**, so the guest sees the quantity without leaving the list.
- **An item** shows its full card: picture, description, price and portion, with "Add · €8.00" or, once it is in the cart, "In your cart" and "View cart".
- Only what can be ordered is shown: active items in active categories that aren't sold out. When the bar takes something off the menu or it sells out, it disappears from every guest's menu at once.

## Cart and paying

- **Cart:** each line with picture, "€8.00 each · €16.00" and a quantity stepper (up to 20 of an item), a note for the bartender (200 characters), the total and **Checkout**. A line whose item left the menu says "No longer on the menu" and blocks checkout until it is removed. **Clear** empties the cart.
- **Pay page:** a summary of the order and a choice between **Card** and **Cash**.
  - **Card:** card number, expiry (MM/YY) and CVC, formatted as the guest types, then "Pay €23.50". The payment is simulated: every card is approved except `4000 0000 0000 0002`, which is declined. Card details are never stored.
  - **Cash:** the order goes to the bar right away and is paid at the register (in person, or through a bartender who takes the cash there). The button reads "Place order · €23.50".
- If an item became unavailable while the guest was paying, the menu reloads, the line is marked and nothing is charged. If the table was closed in the meantime, the app asks for a new scan.

## Following an order

After ordering, the app opens the order's page: the status with an icon and the guest's wording, the steps with their times, the lines, the note, the total and how it is paid.

| Status | Guest sees |
| --- | --- |
| Pending | Sent to the bar |
| Accepted | Accepted |
| Active | Being prepared |
| Completed | Served |
| Declined | Declined, with the bartender's reason |
| Cancelled | Cancelled |

The page updates live. While the order is still pending, **Cancel order** takes it back (a card payment is refunded at once). Once a bartender has accepted it, it can no longer be cancelled.

Above the bottom bar, a slim **status bar** shows the latest order ("Order #128 · Being prepared"), with "+1" when more orders are open; it links to the order or to Orders.

## Orders

All orders placed on this phone in the last 12 hours, newest first, each with its status, its lines with pictures and, while unpaid, a "To pay" chip. **Order again** puts the lines that are still on the menu back in the cart at today's prices. Pending orders can be cancelled from here too. The Orders tab shows a badge with the number of open orders.

## Visit

- **Table:** the current table and how long orders go there.
- **Need something?** "Call bartender" sends a request to the bar; while it is open the card says "A bartender is on the way" with how long ago it was asked, and it turns back into a button once a bartender marks it done.
- **Settings:** Appearance (Auto follows the phone, or Light or Dark) and "Forget this phone's orders", which only clears the list on the phone.