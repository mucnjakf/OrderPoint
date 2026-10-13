namespace OrderPoint.Api.OpenApi;

// The landing page of the API reference, rendered as Markdown by Scalar
internal static class ApiGuide
{
    internal const string Markdown = """
        OrderPoint is the backend of a bar ordering system: customers order from their phones through a QR code on
        the table, bartenders fulfil the orders, and the owner manages the menu, the staff and the numbers in the
        Admin web app.

        ## Authentication

        1. `POST /api/auth/login` with an email, a password and a role (`0` = Admin, `1` = Bartender) returns an
           **access token** (JWT, valid for 15 minutes) and a **refresh token** (valid for 7 days).
        2. Send the access token in every request: `Authorization: Bearer <access token>`. In this reference, paste
           it once into the **Bearer** authentication field; it is remembered between visits.
        3. Before the access token expires, call `POST /api/auth/refresh`. Every refresh token works once: the
           response contains a new pair.
        4. `POST /api/auth/logout` revokes the refresh token.

        - A wrong email, password or role returns `401 Auth.InvalidCredentials`. After 5 failed attempts the
          account is locked for 5 minutes (`401 Auth.LockedOut`).
        - A bartender created by the admin starts with a temporary password: login returns
          `403 Auth.PasswordChangeRequired` until `POST /api/auth/initial-password` sets a new one.
        - Every endpoint requires the **Admin** role, except Auth, `POST /api/orders` (used by the customer app)
          and the health checks (anonymous), the order status changes (`PUT /api/orders/{id}/accept`, `decline`,
          `activate`, `complete`), `GET /api/shift` and `GET /api/shift/day` (**Bartender**), and reading orders,
          the item and category lists, one category, `GET /api/account`, changing the password and signing out
          everywhere (**Admin** or **Bartender**). Each endpoint names its roles; those that need a token show a
          lock and list `401` and `403`.

        ## Responses

        Successful responses wrap the payload in `data`:

        ```json
        { "data": { "id": "0199b1c4-7f3a-7c2e-9d41-5a6b7c8d9e0f", "name": "Cocktails" } }
        ```

        - `200 OK` returns data, `201 Created` also returns a `Location` header with the URL of the new resource.
        - `204 No Content` is used by updates, deletes and bulk operations.

        ### Pagination, search and sorting

        Lists take `PageNumber` (from 1) and `PageSize` (1 to 100) and return a page:

        ```json
        {
          "data": {
            "items": [],
            "pageNumber": 1,
            "pageSize": 10,
            "totalCount": 42,
            "totalPages": 5,
            "hasPreviousPage": false,
            "hasNextPage": true
          }
        }
        ```

        `SearchQuery` is a case-insensitive "contains" search (at most 100 characters). `SortBy` defaults to
        the newest first; ties are broken by id, so paging is stable. `.../search` endpoints are unpaged lookups for
        autocompletes.

        ### Errors

        Every error is an [RFC 9457](https://www.rfc-editor.org/rfc/rfc9457) problem details object with an
        `errors` array of stable codes:

        ```json
        {
          "type": "NotFound",
          "title": "Not Found",
          "status": 404,
          "detail": "Known error occurred while processing your request",
          "errors": [ { "code": "Item.NotFound", "description": "Item not found", "type": 3 } ]
        }
        ```

        | Status | When |
        | --- | --- |
        | `400` | Validation failed (`RequestValidation.<Field>`), malformed input or a broken domain rule |
        | `401` | Missing, expired or invalid access token, or wrong credentials |
        | `403` | The token's role is not allowed, the account is inactive, or a password change is required |
        | `404` | A referenced resource does not exist |
        | `409` | A conflict: duplicate email, deleting something still in use, or an unavailable item |
        | `500` | An unexpected error; details are logged, never returned |

        ## Real time

        The SignalR hub at `/hubs/orders` (role **Admin** or **Bartender**, bearer token as for the endpoints)
        sends `OrdersChanged`, without arguments, after an order is placed or changes status. Clients then reload
        what they show; the cache is already cleared when the signal arrives. The hub has no methods to call.

        ## Conventions

        - **Ids** are UUID v7 strings. Order **numbers** are the human-readable, auto-incremented counterpart.
        - **Times** are UTC (`...AtUtc`, ISO 8601). Dashboard endpoints take an IANA `TimeZone` (e.g.
          `Europe/Zagreb`) and build periods and buckets from the viewer's local calendar.
        - **Enums** are integers in JSON; query strings also accept the name (e.g. `SortBy=NameAsc`). Every enum
          schema lists its values.
        - **Money** is in euros with 2 decimals. Order lines keep the `unitPrice` from the moment of ordering, so
          later price changes do not rewrite history; order totals are calculated, not stored.
        - **Images** are uploaded separately as `multipart/form-data` with one field, `image`: JPEG, PNG or WebP,
          at most 2 MB. Image URLs are public and change on every upload.
        - **Bulk operations** (`.../status`, `.../delete`, and `items/category` and `items/price`) take 1 to 100
          unique ids and are all-or-nothing: if any id is missing or blocked, nothing changes.
        - **Availability**: an item can be ordered only while both the item and its category are Active.
        - **Caching**: gets, lists, searches and dashboard widgets (except `dashboard/live`) are cached in memory
          and in Redis. Every successful change removes the reads it affects, so a read after a write is current.
        """;
}