# API reference

The API's full, interactive reference is **Scalar** at `https://localhost:7180/scalar` while the system runs (the API resource in the Aspire dashboard links to it); the OpenAPI document is at `/openapi/v1.json`. Every endpoint there has a summary, a description of its rules, its errors, and documented request and response fields. This page gives the conventions and a map of all endpoints.

## Conventions

- **Base path:** `api/...`; hubs under `hubs/...`.
- **Authentication:** `Authorization: Bearer <access token>` from `POST api/auth/login`; see [Security](Security). Scalar has a Bearer field to paste a token into, and it remembers the token.
- **Envelope:** successful responses wrap the payload: `{ "data": ... }`.
- **Status codes:** `200` with data, `201` with data and a `Location` header for creates, `204` for updates, deletes and bulk operations. Errors are problem details with codes; see [Error codes](Error-Codes).
- **Paged lists** take `PageNumber` (from 1), `PageSize` (1 to 100), `SearchQuery` (case-insensitive contains, up to 100 characters), `SortBy` and filters, and return `{ items, pageNumber, pageSize, totalCount, totalPages, hasPreviousPage, hasNextPage }`. Sorting always ends with the id, so pages never shift when values tie. `.../search` endpoints are unpaged lookups for autocompletes: every match, by name.
- **Ids** are UUID v7 strings; order **numbers** are the readable counterpart.
- **Times** are UTC ISO 8601 (`...AtUtc`). Dashboard, shift, register-day and payment-period requests take the viewer's IANA `TimeZone`, and a day starts at the business day's start hour.
- **Enums** are integers in JSON; query strings also accept names (`SortBy=NameAsc`).
- **Money** in euros with 2 decimals; order lines keep the price from when they were ordered; totals are calculated.
- **Images** are uploaded separately: `PUT .../image` as `multipart/form-data` with one `image` field (JPEG, PNG or WebP, at most 2 MB), `DELETE .../image` to remove.
- **Bulk operations** (`PUT .../status`, `POST .../delete`, `PUT items/category`, `PUT items/price`) take 1 to 100 unique ids and are all-or-nothing.
- **Caching:** reads are cached and every change clears what it affects, so a read after a write is current. See [Caching](Caching).

## Endpoints

118 endpoints in 20 groups. **Who** is the role the endpoint requires ("Anyone" means anonymous). See [Roles and permissions](Roles-and-Permissions).

### Authentication

**Auth**

| Method | Route | Who | Does |
| --- | --- | --- | --- |
| POST | `api/auth/initial-password` | Anyone | Set the first password |
| POST | `api/auth/login` | Anyone | Sign in |
| POST | `api/auth/logout` | Anyone | Sign out |
| POST | `api/auth/refresh` | Anyone | Refresh the tokens |

**Account**

| Method | Route | Who | Does |
| --- | --- | --- | --- |
| GET | `api/account` | Admin, Bartender | Get my account |
| PUT | `api/account/image` | Admin | Upload my profile image |
| DELETE | `api/account/image` | Admin | Remove my profile image |
| PUT | `api/account/password` | Admin, Bartender | Change my password |
| POST | `api/account/sign-out-everywhere` | Admin, Bartender | Sign out everywhere |

### Menu

**Categories**

| Method | Route | Who | Does |
| --- | --- | --- | --- |
| GET | `api/categories` | Admin, Bartender | List categories |
| POST | `api/categories` | Admin | Create a category |
| POST | `api/categories/delete` | Admin | Delete categories |
| PUT | `api/categories/display-order` | Admin | Reorder categories |
| GET | `api/categories/search` | Admin | Search categories |
| PUT | `api/categories/status` | Admin, Bartender | Change the status of categories |
| GET | `api/categories/{id}` | Admin, Bartender | Get a category |
| PUT | `api/categories/{id}` | Admin | Update a category |
| DELETE | `api/categories/{id}` | Admin | Delete a category |
| PUT | `api/categories/{id}/image` | Admin | Upload a category image |
| DELETE | `api/categories/{id}/image` | Admin | Remove a category image |

**Items**

| Method | Route | Who | Does |
| --- | --- | --- | --- |
| GET | `api/items` | Admin, Bartender | List items |
| POST | `api/items` | Admin | Create an item |
| PUT | `api/items/category` | Admin | Move items to a category |
| POST | `api/items/delete` | Admin | Delete items |
| PUT | `api/items/price` | Admin | Adjust the price of items |
| PUT | `api/items/status` | Admin, Bartender | Change the status of items |
| GET | `api/items/{id}` | Admin, Bartender | Get an item |
| PUT | `api/items/{id}` | Admin | Update an item |
| DELETE | `api/items/{id}` | Admin | Delete an item |
| PUT | `api/items/{id}/image` | Admin | Upload an item image |
| DELETE | `api/items/{id}/image` | Admin | Remove an item image |
| GET | `api/items/{id}/recipe` | Admin | Get an item's recipe |
| PUT | `api/items/{id}/recipe` | Admin | Set an item's recipe |

**Menu**

| Method | Route | Who | Does |
| --- | --- | --- | --- |
| GET | `api/menu` | Anyone | Get the menu |

### Staff

**Bartenders**

| Method | Route | Who | Does |
| --- | --- | --- | --- |
| GET | `api/bartenders` | Admin | List bartenders |
| POST | `api/bartenders` | Admin | Create a bartender |
| POST | `api/bartenders/delete` | Admin | Delete bartenders |
| GET | `api/bartenders/search` | Admin | Search bartenders |
| PUT | `api/bartenders/status` | Admin | Change the status of bartenders |
| GET | `api/bartenders/{id}` | Admin | Get a bartender |
| PUT | `api/bartenders/{id}` | Admin | Update a bartender |
| DELETE | `api/bartenders/{id}` | Admin | Delete a bartender |
| PUT | `api/bartenders/{id}/image` | Admin | Upload a bartender image |
| DELETE | `api/bartenders/{id}/image` | Admin | Remove a bartender image |
| PUT | `api/bartenders/{id}/password` | Admin | Reset a bartender's password |

### Orders

**Orders**

| Method | Route | Who | Does |
| --- | --- | --- | --- |
| GET | `api/orders` | Admin, Bartender | List orders |
| POST | `api/orders` | Anyone | Place an order |
| POST | `api/orders/bartender` | Bartender | Take an order |
| POST | `api/orders/register` | Bartender | Sell at the register |
| GET | `api/orders/{id}` | Admin, Bartender | Get an order |
| PUT | `api/orders/{id}/accept` | Bartender | Accept an order |
| PUT | `api/orders/{id}/activate` | Bartender | Start preparing an order |
| PUT | `api/orders/{id}/cancel` | Anyone | Cancel an order |
| PUT | `api/orders/{id}/complete` | Bartender | Complete an order |
| PUT | `api/orders/{id}/decline` | Admin, Bartender | Decline an order |
| PUT | `api/orders/{id}/pay` | Bartender | Pay an order at the register |
| PUT | `api/orders/{id}/refund` | Bartender | Hand back a refund |
| GET | `api/orders/{id}/tracking` | Anyone | Track an order |
| PUT | `api/orders/{id}/withdraw` | Bartender | Withdraw an order |

**Payments**

| Method | Route | Who | Does |
| --- | --- | --- | --- |
| GET | `api/payments` | Admin, Bartender | List payments |

**Shift**

| Method | Route | Who | Does |
| --- | --- | --- | --- |
| GET | `api/shift` | Bartender | Get my shift |
| GET | `api/shift/day` | Bartender | Get my day |

**Register**

| Method | Route | Who | Does |
| --- | --- | --- | --- |
| GET | `api/register/day` | Bartender | Get the register's day |

### Floor plan

**Areas**

| Method | Route | Who | Does |
| --- | --- | --- | --- |
| POST | `api/areas` | Admin | Create an area |
| GET | `api/areas/{id}` | Admin, Bartender | Get an area |
| PUT | `api/areas/{id}` | Admin | Update an area |
| DELETE | `api/areas/{id}` | Admin | Delete an area |
| GET | `api/floor-plan` | Admin, Bartender | Get the floor plan |

**Tables**

| Method | Route | Who | Does |
| --- | --- | --- | --- |
| POST | `api/tables` | Admin | Create a table |
| GET | `api/tables/by-code/{code}` | Anyone | Look up a table by its code |
| GET | `api/tables/status` | Bartender | Get the open orders per table |
| GET | `api/tables/{id}` | Admin, Bartender | Get a table |
| PUT | `api/tables/{id}` | Admin | Update a table |
| DELETE | `api/tables/{id}` | Admin | Delete a table |
| PUT | `api/tables/{id}/position` | Admin | Move a table |

**Fixtures**

| Method | Route | Who | Does |
| --- | --- | --- | --- |
| POST | `api/fixtures` | Admin | Create a fixture |
| GET | `api/fixtures/{id}` | Admin, Bartender | Get a fixture |
| PUT | `api/fixtures/{id}` | Admin | Update a fixture |
| DELETE | `api/fixtures/{id}` | Admin | Delete a fixture |
| PUT | `api/fixtures/{id}/position` | Admin | Move a fixture |

**Table requests**

| Method | Route | Who | Does |
| --- | --- | --- | --- |
| GET | `api/table-requests` | Bartender | Get the open requests |
| PUT | `api/table-requests/{id}/done` | Bartender | Mark a request done |
| GET | `api/tables/by-code/{code}/requests` | Anyone | Get a table's open requests |
| POST | `api/tables/by-code/{code}/requests` | Anyone | Call a bartender |

### Inventory

**Stock**

| Method | Route | Who | Does |
| --- | --- | --- | --- |
| GET | `api/stock-items` | Admin | List stock items |
| POST | `api/stock-items` | Admin | Create a stock item |
| GET | `api/stock-items/reorder` | Admin | Get the reorder list |
| GET | `api/stock-items/summary` | Admin | Get the stock summary |
| GET | `api/stock-items/{id}` | Admin | Get a stock item |
| PUT | `api/stock-items/{id}` | Admin | Update a stock item |
| DELETE | `api/stock-items/{id}` | Admin | Delete a stock item |
| POST | `api/stock-items/{id}/correction` | Admin | Correct the quantity |
| GET | `api/stock-items/{id}/movements` | Admin | List a stock item's movements |
| POST | `api/stock-items/{id}/waste` | Admin | Record waste |

**Deliveries**

| Method | Route | Who | Does |
| --- | --- | --- | --- |
| GET | `api/deliveries` | Admin | List deliveries |
| POST | `api/deliveries` | Admin | Record a delivery |
| GET | `api/deliveries/{id}` | Admin | Get a delivery |

**Suppliers**

| Method | Route | Who | Does |
| --- | --- | --- | --- |
| GET | `api/suppliers` | Admin | List suppliers |
| POST | `api/suppliers` | Admin | Create a supplier |
| GET | `api/suppliers/{id}` | Admin | Get a supplier |
| PUT | `api/suppliers/{id}` | Admin | Update a supplier |
| DELETE | `api/suppliers/{id}` | Admin | Delete a supplier |

**Stock counts**

| Method | Route | Who | Does |
| --- | --- | --- | --- |
| GET | `api/stock-counts` | Admin | List stock counts |
| POST | `api/stock-counts` | Admin | Start a stock count |
| GET | `api/stock-counts/{id}` | Admin | Get a stock count |
| DELETE | `api/stock-counts/{id}` | Admin | Discard a stock count |
| PUT | `api/stock-counts/{id}/close` | Admin | Close a stock count |
| PUT | `api/stock-counts/{id}/lines/{lineId}` | Admin | Enter a counted quantity |

### Insights

**Dashboard**

| Method | Route | Who | Does |
| --- | --- | --- | --- |
| GET | `api/dashboard/bartender-leaderboard` | Admin | Get the bartender leaderboard |
| GET | `api/dashboard/busiest-times` | Admin | Get the busiest times |
| GET | `api/dashboard/category-revenue` | Admin | Get revenue by category |
| GET | `api/dashboard/gross-profit` | Admin | Get the gross profit |
| GET | `api/dashboard/live` | Admin | Get the live figures |
| GET | `api/dashboard/payment-methods` | Admin | Get revenue by payment method |
| GET | `api/dashboard/revenue-trend` | Admin | Get the revenue trend |
| GET | `api/dashboard/summary` | Admin | Get the key figures |
| GET | `api/dashboard/top-items` | Admin | Get the best-selling items |

### System

**Health**

| Method | Route | Who | Does |
| --- | --- | --- | --- |
| GET | `api/health` | Admin | Get health |

## Hubs

| Hub | Who | Client calls | Server sends |
| --- | --- | --- | --- |
| `hubs/orders` | Admin, Bartender | nothing | `OrdersChanged`, `MenuChanged`, `TablesChanged` (no arguments) |
| `hubs/order-tracking` | Anyone | `TrackOrder(orderId)` | `OrderChanged`, `MenuChanged`, `TablesChanged` (no arguments) |

See [Real-time updates](Real-Time-Updates).

## Health

- `GET api/health` (Admin) returns every health check with a readable name, status, duration and description; the Admin's Health page shows it.
- `/health` and `/alive` (development only, anonymous) are Aspire's own checks.