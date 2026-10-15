# Troubleshooting

Problems seen while developing OrderPoint, and what fixes them.

## Building and starting

| Symptom | Cause | Fix |
| --- | --- | --- |
| `MSB3021` / `MSB3027` "file is being used by another process" | A Debug build while Aspire runs: the running apps lock `bin/Debug` | Build with `-c Release` (and `dotnet ef ... --configuration Release`) |
| A component shows up as a raw tag (`<MyComponent>`) and an RZ10012 warning appears only sometimes | A missing `@using` for the component's namespace; incremental builds can hide it | Add the namespace to `_Imports.razor`; check with `dotnet build -c Release --no-incremental` |
| The API stops at startup mentioning the signing key | `Jwt:SigningKey` missing or shorter than 32 characters | Set it in the API's user secrets (see [Configuration](Configuration)) |
| The API stops: "BusinessDay:StartHour must be an hour from 0 to 23" | A wrong start hour in the AppHost | Use a number from 0 to 23 |
| A MediatR license warning or error | `MediatR:LicenseKey` missing | Set it in the API's user secrets |
| `aspire run` hangs on "connecting to apphost" | The Aspire CLI version doesn't match the AppHost's Aspire version | `aspire update --self` |
| Containers don't start | Docker Desktop isn't running | Start it and run again |
| You can't sign in to the Admin with your configured email | The admin is only created if that email doesn't exist yet; a changed `Admin:Email` creates a second admin, and a changed password doesn't update the existing one | Use the original credentials, or reset the database ([Database and migrations](Database-and-Migrations#resetting)) |

## While running

| Symptom | Cause | Fix |
| --- | --- | --- |
| An API change seems to have no effect | The running API is the old build | Restart the API resource in the Aspire dashboard (AppHost changes need a full Aspire restart) |
| A new endpoint returns 403 | The API wasn't restarted, or the endpoint's policy doesn't include the caller's role | Restart the API; check `RequireAuthorization` |
| Health shows **Cache** amber (Degraded) | Redis is stopped | Start the `cache` resource; reads keep working from memory and the database meanwhile |
| An app or developer tool is amber on the Health page | That resource is stopped | Start it in the dashboard; nothing else depends on it |
| A list shows old data after a fix made directly in the database | The cache doesn't know about changes made outside the API | Restart the `cache` resource (it has no volume) and the API (its memory cache) |
| The staff apps show **Offline** | The API is down or restarting | They reconnect by themselves (after 0, 2, 10 and then every 30 seconds) and reload everything |
| "Reconnecting" dialog in an app | The browser lost its circuit to the app (app restarted, network dropped) | It retries by itself; Retry or reload the page |
| Signed out with "Your session expired" | The refresh token expired (7 days), or "Sign out everywhere" or a password change revoked it | Sign in again |
| A bartender is signed out in the middle of the shift | The admin deactivated or deleted them | Expected: their open orders went back to the queue |

## Ordering

| Symptom | Cause | Fix |
| --- | --- | --- |
| "Table not available" in the customer app | Unknown code, or the table is inactive | Check the table in the Admin's floor plan |
| An item doesn't appear for guests | The item or its category is inactive, or it's sold out (an ingredient has less than one serving) | Check its status, its category, and its recipe's stock |
| Placing an order fails with "Order contains an item that is not available" | The item became unavailable while the guest was paying | Expected; the app reloads the menu and marks the line |
| "The card was declined" | The test number `4000 0000 0000 0002` | Use any other card number |
| Can't delete a category, item, bartender, table or stock item | It is still in use (items, orders, payments, movements, recipes, counts) | Deactivate it instead; the error says what blocks it |

## Phones and the tunnel

| Symptom | Cause | Fix |
| --- | --- | --- |
| The tunnel address doesn't open | The `tunnel` resource isn't started | Start it in the dashboard (▶) |
| The tunnel resource reports a sign-in error | The devtunnel CLI login expired | `devtunnel user login -g` |
| QR codes open a tunnel address that doesn't work | `CustomerApp:PublicUrl` is set but the tunnel is stopped | Start the tunnel, or remove the setting ([Testing on a phone](Testing-on-a-Phone)) |
| Pictures missing on the phone | The app's image proxy isn't configured | Run through Aspire, which passes `Images__StorageUrl` to every app |