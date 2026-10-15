# Error codes

Every error the API returns is a problem details object ([RFC 9457](https://www.rfc-editor.org/rfc/rfc9457)) with an `errors` array. Each error has a stable **code** (`<Entity>.<Reason>`), a description and a type that decides the HTTP status. Apps match on the code, never on the description.

```json
{
  "type": "Conflict",
  "title": "Conflict",
  "status": 409,
  "detail": "Known error occurred while processing your request",
  "errors": [
    { "code": "Order.InvalidStatusTransition", "description": "Order status does not allow this change", "type": 4 }
  ]
}
```

## Status codes

| Status | Error type | When |
| --- | --- | --- |
| 400 | Validation, RequestValidation | A request field is invalid (`RequestValidation.<Field>`), the request is malformed (`RequestValidation.BadHttpRequest`), or a domain rule is broken |
| 401 | Unauthorized | No, expired or invalid access token, or wrong credentials |
| 403 | Forbidden | The token's role isn't allowed, the account is inactive or deleted, or a password change is required |
| 404 | NotFound | The resource, or one it refers to, doesn't exist |
| 409 | Conflict | The change clashes with the current state: a duplicate name, something still in use, an invalid status change, an unavailable item |
| 500 | Failure | An unexpected error; it is logged and traced, and no details are returned |

## Request validation

Before a request reaches the use case, its FluentValidation validator checks the fields. Every failed rule becomes one error with the code `RequestValidation.<Field>` (e.g. `RequestValidation.Name`) and a message such as "Name is required", all in one 400 response. A body that isn't valid JSON, or a missing or unparsable route or query value, gives `RequestValidation.BadHttpRequest`. The rules are listed on [Validation rules](Validation-Rules).

## Domain errors

### Authentication and accounts

| Code | Status | Meaning |
| --- | --- | --- |
| `Auth.UserNotFound` | 404 | User not found |
| `Auth.InvalidCredentials` | 401 | Email or password is incorrect |
| `Auth.LockedOut` | 401 | Account is locked after too many failed login attempts, try again in a few minutes |
| `Auth.InvalidRefreshToken` | 401 | Refresh token is invalid or expired |
| `Auth.AccountInactive` | 403 | Account is inactive |
| `Auth.AccountDeleted` | 403 | Account no longer exists |
| `Auth.InvalidCurrentPassword` | 400 | Current password is incorrect |
| `Auth.PasswordChangeRequired` | 403 | Password must be changed at first login |
| `Auth.PasswordChangeNotAllowed` | 403 | Password was already changed at first login |
| `Auth.InvalidEmail` | 400 | Email is invalid |
| `Auth.InvalidPassword` | 400 | Password must be at least 8 characters and contain an uppercase letter, a lowercase letter and a digit |
| `Auth.EmailAlreadyExists` | 409 | User with this email already exists |

### Bartenders

| Code | Status | Meaning |
| --- | --- | --- |
| `Bartender.NotFound` | 404 | Bartender not found |
| `Bartender.FirstNameIsRequired` | 400 | Bartender first name is required |
| `Bartender.LastNameIsRequired` | 400 | Bartender last name is required |
| `Bartender.EmailIsRequired` | 400 | Bartender email is required |
| `Bartender.ImageUrlIsRequired` | 400 | Bartender image URL is required |
| `Bartender.EmailAlreadyExists` | 409 | Bartender with this email already exists |
| `Bartender.CannotDeleteBartenderWithOrders` | 409 | Cannot delete bartender with orders |

### Categories

| Code | Status | Meaning |
| --- | --- | --- |
| `Category.NotFound` | 404 | Category not found |
| `Category.NameIsRequired` | 400 | Category name is required |
| `Category.DescriptionIsRequired` | 400 | Category description is required |
| `Category.ImageUrlIsRequired` | 400 | Category image URL is required |
| `Category.DisplayOrderMustIncludeAllCategories` | 400 | Display order must include every category exactly once |
| `Category.CannotDeleteCategoryWithItems` | 409 | Cannot delete category with items |

### Items

| Code | Status | Meaning |
| --- | --- | --- |
| `Item.NotFound` | 404 | Item not found |
| `Item.NameIsRequired` | 400 | Item name is required |
| `Item.DescriptionIsRequired` | 400 | Item description is required |
| `Item.PortionMustBePositive` | 400 | Item portion must be positive |
| `Item.PriceMustBePositive` | 400 | Item price must be positive |
| `Item.ImageUrlIsRequired` | 400 | Item image URL is required |
| `Item.RecipeQuantityMustBePositive` | 400 | Every recipe quantity must be positive |
| `Item.RecipeHasDuplicateStockItems` | 400 | A stock item can be in a recipe only once |
| `Item.CannotDeleteItemWithOrders` | 409 | Cannot delete item with orders |

### Orders

| Code | Status | Meaning |
| --- | --- | --- |
| `Order.NotFound` | 404 | Order not found |
| `Order.ItemsAreRequired` | 400 | Order items are required |
| `Order.QuantityMustBePositive` | 400 | Order item quantity must be positive |
| `Order.ItemIsUnavailable` | 409 | Order contains an item that is not available |
| `Order.InvalidStatusTransition` | 409 | Order status does not allow this change |
| `Order.DeclineReasonIsRequired` | 400 | Order decline reason is required |
| `Order.AssignedToAnotherBartender` | 409 | Order is assigned to another bartender |
| `Order.EnteredByAnotherBartender` | 409 | Order was entered by another bartender |

### Payments

| Code | Status | Meaning |
| --- | --- | --- |
| `Payment.CardDeclined` | 400 | The card was declined |
| `Payment.AlreadyPaid` | 409 | Order is already paid |
| `Payment.NotEnoughCash` | 400 | The cash received is less than the total |
| `Payment.NothingToPay` | 409 | A declined or cancelled order has nothing to pay |
| `Payment.NoRefundDue` | 409 | Order has no refund to hand back |

### Areas

| Code | Status | Meaning |
| --- | --- | --- |
| `Area.NotFound` | 404 | Area not found |
| `Area.NameIsRequired` | 400 | Area name is required |
| `Area.GridSizeMustBePositive` | 400 | Area columns and rows must be positive |
| `Area.TooSmallForContents` | 409 | Area is too small for the tables and fixtures on it |
| `Area.NameAlreadyExists` | 409 | Area with this name already exists |
| `Area.CannotDeleteAreaWithTables` | 409 | Cannot delete area with tables |

### Tables

| Code | Status | Meaning |
| --- | --- | --- |
| `Table.NotFound` | 404 | Table not found |
| `Table.CodeIsRequired` | 400 | Table code is required |
| `Table.SeatsMustBePositive` | 400 | Table seats must be positive |
| `Table.OutsideArea` | 400 | Table must lie inside its area |
| `Table.IsInactive` | 409 | Table is not taking orders |
| `Table.CodeAlreadyExists` | 409 | Table with this code already exists |
| `Table.CannotDeleteTableWithOrders` | 409 | Cannot delete table with orders |

### Fixtures

| Code | Status | Meaning |
| --- | --- | --- |
| `Fixture.NotFound` | 404 | Fixture not found |
| `Fixture.OutsideArea` | 400 | Fixture must lie inside its area |

### Table requests

| Code | Status | Meaning |
| --- | --- | --- |
| `TableRequest.NotFound` | 404 | Table request not found |
| `TableRequest.AlreadyDone` | 409 | Table request is already done |

### Stock items

| Code | Status | Meaning |
| --- | --- | --- |
| `StockItem.NotFound` | 404 | Stock item not found |
| `StockItem.NameIsRequired` | 400 | Stock item name is required |
| `StockItem.PackNameIsRequired` | 400 | Pack name is required |
| `StockItem.PackSizeMustBePositive` | 400 | Pack size must be positive |
| `StockItem.PackCostMustNotBeNegative` | 400 | Pack cost must not be negative |
| `StockItem.ParLevelMustNotBeNegative` | 400 | Par level must not be negative |
| `StockItem.QuantityMustBePositive` | 400 | Quantity must be positive |
| `StockItem.QuantityMustNotBeNegative` | 400 | Quantity must not be negative |
| `StockItem.QuantityUnchanged` | 400 | The quantity is already this |
| `StockItem.NameAlreadyExists` | 409 | A stock item with this name already exists |
| `StockItem.UsedInRecipes` | 409 | Cannot delete a stock item that recipes use |
| `StockItem.UsedInCounts` | 409 | Cannot delete a stock item that is on a stock count |
| `StockItem.CannotDeleteWithMovements` | 409 | Cannot delete a stock item with stock movements |

### Suppliers

| Code | Status | Meaning |
| --- | --- | --- |
| `Supplier.NotFound` | 404 | Supplier not found |
| `Supplier.NameIsRequired` | 400 | Supplier name is required |
| `Supplier.NameAlreadyExists` | 409 | A supplier with this name already exists |
| `Supplier.CannotDeleteWithDeliveries` | 409 | Cannot delete a supplier with deliveries |

### Deliveries

| Code | Status | Meaning |
| --- | --- | --- |
| `Delivery.NotFound` | 404 | Delivery not found |
| `Delivery.LinesAreRequired` | 400 | A delivery needs at least one line |
| `Delivery.PacksMustBePositive` | 400 | Every line's packs must be positive |
| `Delivery.PackCostMustNotBeNegative` | 400 | Every line's pack cost must not be negative |
| `Delivery.HasDuplicateStockItems` | 400 | A stock item can be on a delivery only once |

### Stock counts

| Code | Status | Meaning |
| --- | --- | --- |
| `StockCount.NotFound` | 404 | Stock count not found |
| `StockCount.LineNotFound` | 404 | The stock item is not on this count |
| `StockCount.GroupsAreRequired` | 400 | Pick at least one group to count |
| `StockCount.QuantityMustNotBeNegative` | 400 | The counted quantity must not be negative |
| `StockCount.AlreadyOpen` | 409 | Another count is still open; close or discard it first |
| `StockCount.NoStockItems` | 409 | These groups have no active stock items to count |
| `StockCount.NotOpen` | 409 | The count is closed and can't be changed |
| `StockCount.NothingCounted` | 409 | Count at least one stock item before closing |

## In the apps

- The apps' `ApiService` shows each error's description in a snackbar; pages never handle API errors themselves.
- Some codes get special handling:
  - `Auth.PasswordChangeRequired`: the login form asks for a new password.
  - `Auth.AccountInactive` and `Auth.AccountDeleted`: the staff apps end the session.
  - `Order.ItemIsUnavailable` and `Item.NotFound` while ordering: the menu reloads and the lines are marked.
  - `Table.NotFound` and `Table.IsInactive` while ordering: the customer app asks for a new scan.
  - `Order.NotFound` on tracking: an old order is quietly dropped from the guest's list.
- A 401 from an expired session leads to the login form with "Your session expired", not to an error message.