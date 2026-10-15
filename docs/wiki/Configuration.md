# Configuration

Where every setting lives and what it does. Secrets are kept in **.NET user secrets** on the developer machine, never in `appsettings.json` (which lists the keys with empty values).

## API (`app/api/OrderPoint.Api`)

### User secrets (required)

| Key | Example | What it is |
| --- | --- | --- |
| `Jwt:SigningKey` | a random string of 32+ characters | Signs access tokens (HS256). The API refuses to start without it |
| `Admin:Email` | `owner@example.com` | The admin account created at startup if this email doesn't exist yet |
| `Admin:Password` | `Admin123!` | Its password: at least 8 characters with upper case, lower case and a digit |
| `MediatR:LicenseKey` | from [mediatr.io](https://mediatr.io) | MediatR 14 needs a license key (a free community key works) |

Set them from the repository root:

```bash
dotnet user-secrets set "Jwt:SigningKey" "<32+ random characters>" --project app/api/OrderPoint.Api
dotnet user-secrets set "Admin:Email" "owner@example.com" --project app/api/OrderPoint.Api
dotnet user-secrets set "Admin:Password" "<password>" --project app/api/OrderPoint.Api
dotnet user-secrets set "MediatR:LicenseKey" "<license key>" --project app/api/OrderPoint.Api
```

### `appsettings.json`

| Key | Default | What it is |
| --- | --- | --- |
| `Jwt:Issuer`, `Jwt:Audience` | `OrderPoint.Api`, `OrderPoint` | Written into and checked on every token |
| `Jwt:AccessTokenLifetime` | `00:15:00` | 15 minutes |
| `Jwt:RefreshTokenLifetime` | `7.00:00:00` | 7 days |
| `Cors:AllowedOrigins` | `[]` | Browser origins allowed to call the API directly. Empty, because every app calls from its server |
| `Logging:LogLevel` | Information; ASP.NET Core and EF commands at Warning | SQL shows up as trace spans instead of log lines |

### From Aspire (environment)

| Setting | Value | What it is |
| --- | --- | --- |
| `ConnectionStrings:order-point-db` | set by Aspire | The PostgreSQL database |
| `ConnectionStrings:images` | set by Aspire | The blob container |
| `ConnectionStrings:cache` | set by Aspire | Redis |
| `BusinessDay:StartHour` | `6` | When the bar's day starts (local time, 0 to 23). Missing means midnight; anything else stops the API |
| `services:<name>:...` | set by Aspire | Addresses of pgweb, Storage Explorer, RedisInsight and the four apps, only used for health checks |

## Web apps (`app/web/...`)

| Setting | Apps | What it is |
| --- | --- | --- |
| `services:order-point-api:https:0` | all | The API's address (Aspire service discovery); also used for the SignalR hub URL |
| `Images:StorageUrl` | all | The start of every stored image URL (`http://127.0.0.1:59287/devstoreaccount1/images`); the apps serve images under `/images` in its place |
| `BusinessDay:StartHour` | Bar, Bartender, Register | The same day start as the API, for "since 06:00" texts and the Done tab |
| `CustomerApp:PublicUrl` | Admin (user secrets, optional) | The customer app's public address for table QR codes, e.g. the dev tunnel address; without it the QR codes use Aspire's local address |
| `services:order-point-customer:https:0` | Admin | The customer app's local address, the fallback for QR codes |

```bash
dotnet user-secrets set "CustomerApp:PublicUrl" "https://orderpoint-7038.euw.devtunnels.ms" --project app/web/OrderPoint.Admin
```

Fixed in code rather than configured:
- Snackbars stay 2 seconds in every app (`snackbarVisibleMilliseconds` in each `Program.cs`).
- The customer app remembers a visit for 12 hours.

## AppHost (`app/host/OrderPoint.AppHost`)

| Setting | Where | Value |
| --- | --- | --- |
| Business day start hour | `AppHost.cs` (`businessDayStartHour`) | `"6"`, passed to the API and the staff apps |
| Blob port | `AppHost.cs` (`blobPort`) | `59287`, fixed because image URLs are stored with it |
| PostgreSQL port | `AppHost.cs` | `59286` |
| Postgres password and other generated parameters | AppHost user secrets | Created by Aspire on the first run |
| Dev tunnel | `AppHost.cs` | Tunnel id `orderpoint`, region West Europe, anonymous access, started by hand |

## Ports

| What | HTTPS | HTTP |
| --- | --- | --- |
| Aspire dashboard | 17092 | 15133 |
| API | 7180 | 5171 |
| Admin web | 7025 | 5130 |
| Register app | 7032 | 5132 |
| Bar app | 7034 | 5134 |
| Bartender app | 7036 | 5136 |
| Customer app | 7038 | 5138 |
| PostgreSQL | | 59286 |
| Blob storage (Azurite) | | 59287 |

Through the dev tunnel each app is `https://orderpoint-<https port>.euw.devtunnels.ms`. See [Testing on a phone](Testing-on-a-Phone).