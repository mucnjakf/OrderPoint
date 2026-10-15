# Tables and floor plan

The bar's layout as the owner draws it in the Admin web: areas with tables and fixtures on a grid. Guests order to a table through its QR code, bartenders see live table status on the same plan, and guests can call a bartender to their table.

## Areas, tables and fixtures

| Piece | What it is | Fields |
| --- | --- | --- |
| **Area** | A part of the bar, e.g. "Inside" or "Terrace" | Unique name, a grid of columns × rows, its position among the area tabs |
| **Table** | Somewhere guests sit and order | Unique **code** (capital letters, digits and hyphens, up to 10, e.g. T7), seats, status (Active or Inactive), shape (rectangle or round), position and size in cells |
| **Fixture** | Everything else on the plan | Type (bar counter, entrance or wall), optional label, position and size in cells |

Rules:

- Every piece lies inside its area's grid; an area can't shrink below what it holds.
- An area with tables can't be deleted (its fixtures go with it).
- A table with orders can't be deleted; deactivate it instead, and guests can't order to it.
- Orders keep the code their table had, so renaming a table doesn't rewrite past orders.

## The floor plan editor (Admin)

- **Area tabs** to switch areas, and "Add area".
- A **library strip** above the map adds pieces: square, round and long tables, a bar counter, an entrance and a wall. A new table asks for its code (the next free one is suggested), seats, status and shape, then goes to the first free spot. Fixtures are placed at once.
- **Drag and drop** moves pieces in whole cells, with mouse or touch, also onto free spots only; a refused drop jumps back. **Rotate** swaps a piece's width and height.
- The **side panel** shows the selected table (seats, shape, size, its QR code, edit, rotate, delete), the selected fixture, or the area with its tables.
- Global search finds a table by code or area and opens it on the plan.

## QR codes

Every table has a QR code that opens the customer app at `/t/<code>`, for example `https://.../t/T7`. The panel shows it with its address, an "Open" link for testing and "Download SVG" for printing. The codes are black on white whatever the theme, because scanners need the contrast. The customer app's public address is configured in the Admin (see [Configuration](Configuration)).

## Live tables in the Bar app

The Bar app shows the same plan full screen, pannable, with tables tinted by their open orders:

| Colour | Meaning |
| --- | --- |
| Amber | An order is waiting for a bartender |
| Blue | Orders are being handled |
| Red | An open order has waited 10 minutes or more |

A badge counts the open orders and a bell shows a guest's call. Tapping a table lists its calls and open orders. See [Bar app](Bar-App).

## Calling a bartender

A guest taps **Call bartender** on the customer app's Visit tab. The request appears:

- in the Bar app, above the orders and as a bell on the map;
- in the Bartender app, at the top of the tables page.

A table has at most one open call; asking again returns the same one. Any bartender marks it **Done**, and the guest's card turns back into a button. Who answered is not stored.