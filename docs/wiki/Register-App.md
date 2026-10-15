# Register app

The till at the counter, on a desktop. Bartenders sell at the counter, take payments for orders from the tables, hand back refunds and print receipts. Whoever is signed in is recorded as the one who took each payment.

- **Project:** `app/web/OrderPoint.Register` (Blazor Server)
- **Local address:** `https://localhost:7032`
- **Sign-in:** bartender account, the same as in the Bar and Bartender apps
- **Navigation:** tabs in the app bar: **Today**, **Sale**, **To pay** and **Refunds due** (each with a live count), and **Account**

## Today

"What was paid since 06:00":

- Four tiles: **Cash in the till**, **Card at the register**, **Card online** (paid by guests in the customer app) and **Refunds due** (from any day).
- **Per bartender:** who took how many payments, in cash and by card. Clicking a bartender filters the payments below.
- **Payments:** a table of the day's payments with search, method and bartender filters and sorting; each row has a button that reopens its receipt.

## Sale

Selling at the counter, paid in one step:

- **Categories** as cards with picture, description, number of items and "from €3.50", with a search; a category opens its **items** as tiles. A tap adds one; an item on the sale gets a border and "2× on the sale".
- The **sale** is pinned to the bottom of the screen: the lines with quantity steppers and subtotals, an optional name to call and note for the bar, the total, and large **Cash** and **Card** buttons.
- The sale is kept while the bartender looks at another tab.

Paying creates the order (a counter order, already paid) and sends it to the bar, then shows the receipt.

## Taking a payment

- **Cash:** the total, the amount received with quick picks (the exact total and the total rounded up to 5, 10, 20, 50, 100), and the **change to give** in green (or "Still short" in red). Up to €1,000 can be taken in cash.
- **Card:** three steps: enter the amount on the terminal, the guest taps or inserts the card, confirm once it says approved. The bar's terminal charges the card; the register only records the payment.
- **Receipt:** "Paid", the total, how it was paid (with the change for cash), the place, the time, the lines and who served it. **Print** prints a till receipt (OrderPoint, order number, place, date, lines, total, payment, "Thank you!") on any printer, including a receipt printer.

## To pay

Orders still waiting for their payment: orders bartenders took at the tables and guests' cash orders, oldest first. Each card shows the order (number, place, where it came from, status, who took it, when, who makes it, lines, note) and **Cash** and **Card** buttons.

## Refunds due

Paid orders that were declined or cancelled afterwards: "Hand back €12.00 in cash" or "Refund €12.00 on the terminal". **Refunded** marks the money as handed back.

## Account

Profile, appearance, change password, sign out everywhere, sign out.