# Roles and permissions

OrderPoint has three kinds of users. Two of them sign in; guests never do.

| Role | Who | Signs in to | How the account is made |
| --- | --- | --- | --- |
| **Guest** | Anyone at a table | Nothing: the customer app is anonymous | No account; the table comes from the QR code |
| **Bartender** | Bar staff | Bar app, Bartender app, Register app (one account for all three) | Created by the admin with a temporary password, which the bartender replaces at first sign-in |
| **Admin** | The owner | Admin web | Created at API startup from the configured email and password; only one |

Each app sends its own role when signing in, so a bartender can't sign in to the Admin web and the admin can't sign in to the staff apps; the API answers exactly as it does for a wrong password. Five wrong passwords lock an account for 5 minutes. See [Security](Security) for tokens and sessions.

## Bartender account states

| State | Effect |
| --- | --- |
| Must change password | After creation or a password reset; the next sign-in asks for a new password first |
| Active | Normal use |
| Inactive | Can't sign in; signed out everywhere; their accepted and active orders go back to the queue for someone else |
| Deleted | Only possible while they have no orders and took no payments; their login is deleted too |

Access tokens live for 15 minutes, so a bartender who was just deactivated or deleted could still hold one. Every order action therefore checks the bartender again, and the staff apps sign them out on that answer.

## What each role can do (API)

**Anonymous** endpoints are for guests (and for signing in). **Bartender** endpoints take the bartender from the token, so a bartender only ever acts as themselves.

| Area | Guest (anonymous) | Bartender | Admin |
| --- | --- | --- | --- |
| Sign in, refresh, sign out, first password | ✓ | ✓ | ✓ |
| Own account: view, change password, sign out everywhere | | ✓ | ✓ |
| Own profile picture | | | ✓ |
| Guests' menu | ✓ | | |
| Place a guest order, follow it, cancel it while pending | ✓ | | |
| Check a table by code, call a bartender, see the table's open calls | ✓ | | |
| Read orders and order details | | ✓ | ✓ |
| Accept, start, complete orders | | ✓ | |
| Decline an order | | ✓ (pending, or their own accepted) | ✓ (any open order) |
| Take orders at tables and the counter, withdraw their own pending ones | | ✓ | |
| Register: counter sale, pay an order, complete a refund, the day's takings | | ✓ | |
| Payments list | | ✓ | ✓ |
| My shift and My day | | ✓ | |
| Read categories and items | | ✓ | ✓ |
| Take items or categories off the menu (status) | | ✓ | ✓ |
| Create, edit, delete categories and items; pictures; menu order; prices; recipes | | | ✓ |
| Read the floor plan, an area, a table, a fixture | | ✓ | ✓ |
| Tables status (open orders per table), open calls, mark a call done | | ✓ | |
| Edit the floor plan | | | ✓ |
| Bartenders | | | ✓ |
| Dashboard | | | ✓ |
| Stock, suppliers, deliveries, counts | | | ✓ |
| Health | | | ✓ |

## Live connections

| Hub | Who may connect | Used by |
| --- | --- | --- |
| `hubs/orders` | Admin and bartenders | Admin web, Bar, Bartender and Register apps |
| `hubs/order-tracking` | Anyone | Customer app (a guest only hears about the orders they track) |

See [Real-time updates](Real-Time-Updates). The full endpoint list with routes is in the [API reference](API-Reference).