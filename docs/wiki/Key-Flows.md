# Key flows

Sequence diagrams of the scenarios that cross the most parts of the system. "App" means the app's server, which calls the API; the browser only talks to its app (see [Frontend architecture](Frontend-Architecture)).

## A guest orders and pays by card

```mermaid
sequenceDiagram
    actor Guest
    participant C as Customer app
    participant A as API
    participant P as Payment provider (simulated)
    participant DB as PostgreSQL
    participant B as Bar app

    Guest->>C: scan QR code (/t/T7)
    C->>A: GET api/tables/by-code/T7
    A-->>C: table T7, Terrace, Active
    Guest->>C: add items, Checkout, Pay by card
    C->>A: POST api/orders (tableCode, lines, note, Card + card details)
    A->>DB: load table T7 and the items with category and recipe stock
    A->>A: Order.Create: table active? items available?
    A->>P: charge the total
    P-->>A: approved
    A->>A: order.PayByCard()
    A->>DB: save order (number #128 generated)
    A-->>C: 201 OrderDto
    A-->>B: OrdersChanged (hubs/orders)
    B->>A: GET api/orders?status=Pending
    C->>A: TrackOrder(id) on hubs/order-tracking
    C-->>Guest: Order #128 · Sent to the bar
```

A declined card returns `Payment.CardDeclined` and nothing is saved. An item that became unavailable returns `Order.ItemIsUnavailable`; the customer app reloads the menu and marks the line.

## The bar prepares and serves it

```mermaid
sequenceDiagram
    actor Bartender
    participant B as Bar app
    participant A as API
    participant DB as PostgreSQL
    participant C as Customer app
    participant Others as Other apps

    Bartender->>B: Accept
    B->>A: PUT api/orders/{id}/accept
    A->>DB: check the bartender is active, order.Accept(bartenderId), save
    A-->>C: OrderChanged → "Accepted"
    A-->>Others: OrdersChanged
    Bartender->>B: Start
    B->>A: PUT api/orders/{id}/activate
    A-->>C: OrderChanged → "Being prepared"
    Bartender->>B: Complete
    B->>A: PUT api/orders/{id}/complete
    A->>DB: order.Complete(bartenderId)
    A->>DB: for each line's recipe: StockItem.RecordSale → Sale movements
    A->>A: did any recipe line drop below one serving?
    A->>DB: save order and movements together
    A-->>C: OrderChanged → "Served"
    A-->>Others: OrdersChanged, and MenuChanged if something sold out
    Others->>A: reload, a sold-out item disappears from guests' menus
```

If another bartender accepted the order first, Accept returns `Order.InvalidStatusTransition` (409) and the Bar app shows why and reloads.

## A counter sale at the register, paid in cash

```mermaid
sequenceDiagram
    actor Bartender
    participant R as Register app
    participant A as API
    participant DB as PostgreSQL
    participant B as Bar app

    Bartender->>R: tap items, Cash
    R->>R: Cash dialog: received €50, change €26.50
    R->>A: POST api/orders/register (lines, name, note, Cash, cashReceived 50)
    A->>A: Order.CreateAtRegister (counter, source Register)
    A->>A: order.PayAtRegister(Cash, 50, bartenderId)
    A->>DB: save
    A-->>R: 201 OrderDto
    A-->>B: OrdersChanged (it appears in the queue)
    R-->>Bartender: receipt with change, Print
```

Orders from the tables are paid later the same way with `PUT api/orders/{id}/pay` from the To pay list.

## A bartender is deactivated

```mermaid
sequenceDiagram
    actor Owner
    participant AW as Admin web
    participant A as API
    participant DB as PostgreSQL
    participant B as Bar app

    Owner->>AW: Deactivate Ana
    AW->>A: PUT api/bartenders/status (ids, Inactive)
    A->>DB: bartender.ChangeStatus(Inactive)
    A->>DB: revoke Ana's refresh tokens
    A->>DB: Ana's accepted and active orders: order.Release() → Pending
    A->>DB: save everything together
    A-->>B: OrdersChanged (the released orders return to the queue)
    Note over B: Ana's next order action gets Auth.AccountInactive (403), and her app signs her out
```

## Signing in and staying signed in

```mermaid
sequenceDiagram
    actor User
    participant App as Staff app
    participant A as API
    participant DB as PostgreSQL

    User->>App: email, password
    App->>A: POST api/auth/login (email, password, role Bartender)
    A->>DB: check password, lockout, role, active
    alt must change password
        A-->>App: 403 Auth.PasswordChangeRequired
        User->>App: new password
        App->>A: POST api/auth/initial-password
    end
    A->>DB: store the hash of a new refresh token
    A-->>App: access token (15 min) + refresh token (7 days)
    App->>App: keep tokens in protected browser storage
    loop every call
        App->>A: request with Authorization: Bearer
    end
    App->>A: POST api/auth/refresh (a minute before expiry)
    A->>DB: replace the refresh token
    A-->>App: new pair
```

When the refresh fails (expired, revoked by "sign out everywhere"), the app shows the login form with "Your session expired".