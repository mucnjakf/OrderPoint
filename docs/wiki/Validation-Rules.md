# Validation rules

Every field limit in one place. The rules are enforced in several layers on purpose, so a mistake in one app never reaches the data.

## Where rules live

| # | Layer | Checks | Message style |
| --- | --- | --- | --- |
| 1 | **Domain entity** (`Create`, `Update`, behaviour methods) | Required values, positive numbers, state rules | Error codes such as `Item.PriceMustBePositive` |
| 2 | **API request validator** (FluentValidation, one per endpoint) | Required, lengths, ranges, enums, formats, list sizes | `"Name is required"` as `RequestValidation.Name` |
| 3 | **Database** (EF configuration + migration) | Column lengths, `NOT NULL`, precision, unique indexes, foreign keys | Last line of defence |
| 4 | **Web app forms** (DataAnnotations on the request classes) | The same required, length and range rules, shown next to the field | `"Name is required."` |

A rule changed in one place must be changed in all of them. Rules that depend on other data (a unique name, an item still being available, a blocker before deleting) are checked by the command handler, see [Error codes](Error-Codes).

## Common rules

| Applies to | Rule |
| --- | --- |
| Paged lists | `PageNumber` ≥ 1, `PageSize` 1 to 100 |
| Search text | At most 100 characters (a search that can't match returns no results, not an error); autocomplete `.../search` endpoints require non-blank text |
| Sort options and enums | Must be a defined value (`IsInEnum`) |
| Bulk operations | 1 to 100 ids, no empty id, no duplicates |
| Images | JPEG, PNG or WebP, at most 2 MB, one multipart field named `image` |
| Time zones | A valid IANA id (e.g. `Europe/Zagreb`); required by every dashboard, shift and register-day request, and by payments when a period is given |
| Ids in routes | GUIDs (`{id:guid}`); anything else is a 404 from routing |

## Menu

| Field | Rule |
| --- | --- |
| Category name | Required, at most 30 |
| Category description | Required, at most 100 |
| Category menu order | Must list every category exactly once |
| Item name | Required, at most 30 |
| Item description | Required, at most 200 |
| Item portion | Greater than 0 |
| Item price | Greater than 0 (money, 2 decimals) |
| Price adjustment | Set: value > 0. Change by amount or percent: value ≠ 0. Percent: value > −100. No resulting price may be 0 or less, otherwise nothing is saved |
| Recipe | At most 30 lines, each stock item once, quantity > 0 and at most 1,000,000 |

## Orders

| Field | Rule |
| --- | --- |
| Table code (guest) | Required, at most 10, letters, digits and hyphens; must be an existing, active table |
| Table code (bartender) | Optional (none means a counter order); if given, as above |
| Customer name (counter) | At most 30 |
| Note | At most 200 |
| Lines | 1 to 50, each item once |
| Quantity | 1 to 20 per line |
| Items | Each must exist and be available (active, active category, not sold out) |
| Payment method (guest) | Required: Card or Cash |
| Card (guest, card only) | Required with Card, refused with Cash. Number: digits and spaces, 13 to 19 digits. Expiry: `MM/YY`, not expired. CVC: 3 or 4 digits |
| Decline reason | Required, at most 200 |
| Cash received (register) | Required for cash, more than 0, at most 1,000, at least the total; refused for card |

## Staff and accounts

| Field | Rule |
| --- | --- |
| First and last name | Required, at most 30 each |
| Email | Required, at most 100, a valid email, unique |
| Phone number | At most 20 |
| Notes | At most 500 |
| Password (new, temporary, reset) | 8 to 100 characters, with an upper-case letter, a lower-case letter and a digit |
| Sign-in | Email and password required (at most 100), role must be Admin or Bartender |
| Refresh token | Required, at most 200 |

## Floor plan

| Field | Rule |
| --- | --- |
| Area name | Required, at most 30, unique |
| Area grid | 4 to 48 columns and rows; can't shrink below its tables and fixtures |
| Table code | Required, at most 10, capital letters, digits and hyphens (`^[A-Z0-9-]+$`), unique |
| Seats | 1 to 20 |
| Table size | 1 to 12 cells wide and high |
| Fixture label | At most 30 |
| Fixture size | 1 to 48 cells wide and high |
| Position | X and Y ≥ 0, and the piece must lie inside its area |

## Inventory

| Field | Rule |
| --- | --- |
| Stock item name | Required, at most 100, unique |
| Pack name | Required, at most 30 |
| Pack size | Greater than 0, at most 1,000,000 base units |
| Cost per pack | 0 or more, at most €100,000 |
| Par level | 0 or more, at most 1,000,000 |
| Opening quantity | 0 or more, at most 1,000,000 |
| Waste amount | Greater than 0, at most 1,000,000 |
| Correction | 0 or more, at most 1,000,000, and different from the current quantity |
| Notes (stock, deliveries, counts) | At most 200 |
| Supplier name | Required, at most 100, unique |
| Supplier contact and email | At most 100 each; the email must be valid when given |
| Supplier phone | At most 30 |
| Delivery | Supplier required; date not later than tomorrow; invoice number at most 50; 1 to 50 lines, each stock item once; packs > 0 and at most 1,000,000; price per pack 0 to €100,000 |
| Stock count | At least one group; only one open count at a time; counted quantities 0 to 1,000,000 |

## Customer app limits

The customer app follows the API's order limits (20 per item, a 200-character note) and also forgets its table, cart and order list one visit (12 hours) after the last scan or change.