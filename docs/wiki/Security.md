# Security

How users sign in, how the API decides who may do what, and what protects the anonymous parts used by guests.

## Accounts

- **ASP.NET Core Identity, user store only.** OrderPoint uses Identity for password hashing, lockout, unique emails and security stamps, but not its sign-in manager, cookies, UI, roles or external logins. The role is a column on the user (`Admin` or `Bartender`).
- **The admin** is created at API startup from the configured `Admin:Email` and `Admin:Password` when that email doesn't exist yet. Its email can't be changed, because the next startup would create the admin again.
- **A bartender's login** is created with the bartender, sharing their id, with a **temporary password** and "must change password" set. Changing the bartender's email updates the login; deactivating revokes their sessions; deleting deletes the login.
- **Passwords:** at least 8 characters with an upper-case letter, a lower-case letter and a digit.
- **Lockout:** 5 wrong passwords lock the account for 5 minutes.

## Tokens

| Token | Lifetime | Stored |
| --- | --- | --- |
| **Access token** (JWT, HS256) | 15 minutes | In the app's protected browser storage; sent as a bearer token by the app's server |
| **Refresh token** (random) | 7 days | Only its SHA-256 hash is stored in the database; replaced on every refresh |

- The access token carries `sub` (the user id; a bartender's is their bartender id), `email` and `role`.
- Issuer, audience and lifetimes are in `appsettings.json`.
- The signing key (at least 32 characters) is in user secrets, and the API refuses to start without it.
- Clock skew is zero, so a token is invalid the moment it expires.

**Sign-in flow:**

1. `POST api/auth/login` with email, password and the app's role. A user of the other role gets the same "invalid credentials" answer as a wrong password, so an app never reveals accounts of the other kind.
2. A bartender who must change their password gets `Auth.PasswordChangeRequired`. The login form then asks for a new password and calls `POST api/auth/initial-password`, which works only while that flag is set.
3. The app refreshes the access token a minute before it expires (`POST api/auth/refresh`); parallel calls share one refresh.
4. **Sign out** revokes the refresh token. **Sign out everywhere** and **changing the password** revoke every refresh token; changing the password returns a new pair for the current device.

A deactivated or deleted bartender may still hold a valid access token for up to 15 minutes. Every order action and the account endpoint therefore check the bartender again (`Auth.AccountInactive`, `Auth.AccountDeleted`, both 403), and the staff apps end the session on those answers.

## Authorization

Three policies cover every endpoint: **Admin**, **Bartender** and **AdminOrBartender**. Endpoints without a policy are anonymous on purpose. Bartender endpoints take the bartender from the token, never from the request, so a bartender can only act as themselves. The full matrix is in [Roles and permissions](Roles-and-Permissions).

The staff hub `hubs/orders` requires Admin or Bartender. The apps connect from their servers and send the token as a header, so tokens never appear in URLs.

## The anonymous parts

Guests have no accounts, so the anonymous endpoints are designed to be safe without one:

| Concern | Protection |
| --- | --- |
| Following or cancelling someone else's order | Orders are addressed by a random GUID returned only to the guest who placed it; the tracking hub only sends to the groups a connection joined |
| Flooding the bar with huge orders | At most 50 lines per order, each item once, 1 to 20 of each; a table code of up to 10 letters, digits or hyphens; notes limited to 200 characters |
| Ordering to a closed or invented table | The table is looked up by code and must exist and be active |
| Ordering what is off the menu | Every item is checked: active, in an active category, not sold out |
| Card data | Sent to the payment provider only; never stored or logged (`ToString` prints no digits) |
| Calling a bartender repeatedly | A table has at most one open call; asking again returns it |

## Transport and browser

- HTTPS everywhere locally (the .NET dev certificate), and through the dev tunnel for phones.
- **No CORS:** every web app calls the API from its own server. The API allows only the origins listed in `Cors:AllowedOrigins`, empty by default. A browser-based client added later would need its origin added, never "allow any origin".
- Tokens live in **protected** browser storage (encrypted with ASP.NET Core Data Protection) and only the app's server reads them.

## Errors and logs

- **Unexpected errors** return a 500 with no exception details; the exception is logged and traced.
- **Malformed requests** (bad JSON, missing or unparsable parameters) return 400 in every environment.
- **Logs** never contain passwords, tokens or card numbers. Request logging only records the request's name.

## Not covered

OrderPoint isn't deployed anywhere, so production concerns are out of scope: data protection key storage, secret management beyond user secrets, rate limiting, and exposing the API documentation only in development. See [Scope](Scope).