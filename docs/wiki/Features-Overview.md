# Features overview

Every feature, the app it lives in and who uses it. Each app has its own page with the details: [Customer app](Customer-App), [Bar app](Bar-App), [Bartender app](Bartender-App), [Register app](Register-App), [Admin web](Admin-Web).

**Roles:** Guest (anyone with the customer app, no account), Bartender (signs in to the Bar, Bartender and Register apps with the same account), Admin (the owner, signs in to the Admin web). See [Roles and permissions](Roles-and-Permissions).

## Ordering

| Feature | Customer | Bar | Bartender | Register | Admin |
| --- | :---: | :---: | :---: | :---: | :---: |
| Browse the menu by category, search, item details | ✓ | ✓ | ✓ | ✓ | ✓ |
| Order from a table by scanning its QR code | ✓ | | | | |
| Order at a table or the counter for a guest | | | ✓ | | |
| Sell at the counter and take the payment at once | | | | ✓ | |
| Follow an order's status live | ✓ | ✓ | ✓ | ✓ | ✓ |
| Cancel a pending order (guest) or withdraw it (bartender who entered it) | ✓ | | ✓ | | |
| Order again from a past order | ✓ | | | | |
| Accept, start, complete orders | | ✓ | | | |
| Decline an order with a reason for the guest | | ✓ | | | ✓ |
| Order history with filters, search and details | | ✓ | | | ✓ |

## Payments

| Feature | Customer | Bar | Bartender | Register | Admin |
| --- | :---: | :---: | :---: | :---: | :---: |
| Pay by card when ordering (simulated) | ✓ | | | | |
| Choose cash and pay at the register | ✓ | | | | |
| Take cash or card payments for orders from the tables | | | | ✓ | |
| Hand back refunds due | | | | ✓ | |
| Print a receipt | | | | ✓ | ✓ |
| Day's takings per method and per bartender | | | | ✓ | |
| All payments with filters and a receipt panel | | | | | ✓ |

See [Payments and refunds](Payments-and-Refunds).

## Tables

| Feature | Customer | Bar | Bartender | Register | Admin |
| --- | :---: | :---: | :---: | :---: | :---: |
| Call a bartender to the table | ✓ | | | | |
| See and answer guests' calls | | ✓ | ✓ | | |
| Live tables map with waiting times | | ✓ | | | |
| Floor plan editor (areas, tables, fixtures) and QR codes | | | | | ✓ |

See [Tables and floor plan](Tables-and-Floor-Plan).

## Menu and team

| Feature | Customer | Bar | Bartender | Register | Admin |
| --- | :---: | :---: | :---: | :---: | :---: |
| Take an item or category off the menu when it runs out | | ✓ | | | ✓ |
| Create and edit categories and items with pictures | | | | | ✓ |
| Menu order of categories | | | | | ✓ |
| Bulk status, category and price changes | | | | | ✓ |
| Bartender accounts, photos, password resets, deactivation | | | | | ✓ |

## Inventory

All in the Admin web; see [Inventory](Inventory).

| Feature | Admin |
| --- | :---: |
| Stock sheet by group with levels, values and history | ✓ |
| Waste and corrections | ✓ |
| Recipes per item with cost and margin | ✓ |
| Sold out when an ingredient runs out (live for guests and staff) | ✓ |
| Suppliers and deliveries with average cost | ✓ |
| Reorder list by supplier | ✓ |
| Stock counts | ✓ |

## Reporting and operations

| Feature | Bar | Register | Admin |
| --- | :---: | :---: | :---: |
| Dashboard: revenue, orders, average order, service time, revenue trend, busiest times, revenue by category, payments by method, gross profit, low stock, top items, bartender leaderboard, recent orders, right now | | | ✓ |
| My shift: queue, my orders, completed, average service time, my day by hour, most served | ✓ | | |
| The day at the register | | ✓ | |
| Global search (Ctrl+K) over orders, items, categories, bartenders, tables and pages | | | ✓ |
| Health of every component | | | ✓ |

## Everywhere

- **Live updates** over SignalR: lists, counts and details reload by themselves when something changes, and a connection indicator shows Live, Connecting or Offline in the staff apps. See [Real-time updates](Real-Time-Updates).
- **Light and dark mode**, remembered per browser; the customer app can also follow the phone.
- **Accounts:** change password, sign out everywhere, sign out (staff apps).
- **Local formatting:** dates, times and numbers follow the browser's locale, amounts are in euros.