# Getting started

From a clean machine to all five apps running, signed in and taking a first order. It takes about 15 minutes, most of it downloads.

## 1. Install the tools

| Tool | Version | Why |
| --- | --- | --- |
| [.NET SDK](https://dotnet.microsoft.com/download/dotnet/10.0) | 10.0 (10.0.300 or later) | Builds and runs every project |
| [Docker Desktop](https://www.docker.com/products/docker-desktop/) | current | Runs PostgreSQL, the Azure Storage emulator and Redis |
| [Aspire CLI](https://aspire.dev/get-started/install-cli/) | 13.6 (matching the AppHost) | `aspire run` starts the whole system |
| [Git](https://git-scm.com/) | any | Getting the code |
| `dotnet-ef` (optional) | 10.0 | Only to create new migrations |

**Aspire CLI**, from a terminal:

```powershell
# Windows (PowerShell)
irm https://aspire.dev/install.ps1 | iex
```

```bash
# macOS and Linux
curl -sSL https://aspire.dev/install.sh | bash
```

Or as a .NET tool: `dotnet tool install -g Aspire.Cli`. Open a new terminal afterwards and check with `aspire --version`. The CLI must match the AppHost's Aspire version (13.6); `aspire update --self` updates it.

**Trust the HTTPS development certificate** (once per machine), so browsers accept `https://localhost`:

```bash
dotnet dev-certs https --trust
```

**Optional, for migrations:** `dotnet tool install -g dotnet-ef`.

Start Docker Desktop and leave it running.

## 2. Get the code

```bash
git clone https://github.com/mucnjakf/OrderPoint.git
cd OrderPoint
```

## 3. Set the secrets

The API needs four values in its user secrets (run from the repository root):

```bash
dotnet user-secrets set "Jwt:SigningKey" "<at least 32 random characters>" --project app/api/OrderPoint.Api
dotnet user-secrets set "Admin:Email" "owner@example.com" --project app/api/OrderPoint.Api
dotnet user-secrets set "Admin:Password" "<password>" --project app/api/OrderPoint.Api
dotnet user-secrets set "MediatR:LicenseKey" "<license key>" --project app/api/OrderPoint.Api
```

- **`Jwt:SigningKey`:** any long random string; the API refuses to start without one of at least 32 characters.
- **`Admin:Email` and `Admin:Password`:** the owner's account, created when the API first starts. The password needs at least 8 characters with an upper-case letter, a lower-case letter and a digit.
- **`MediatR:LicenseKey`:** MediatR 14 asks for a license key; get one (a free community key works) from [mediatr.io](https://mediatr.io).

Everything else has defaults; see [Configuration](Configuration).

## 4. Run it

```bash
aspire run --project app/host/OrderPoint.AppHost
```

The first run downloads the container images (PostgreSQL, Azurite, Redis, pgweb, Storage Explorer, RedisInsight) and builds the solution, so it takes a few minutes. The terminal prints the **Aspire dashboard** address with a sign-in token; open it.

In the dashboard's Resources page, wait until everything is **Running** and healthy. The API applies the database migrations and creates the admin account on startup. The `tunnel` resource stays stopped on purpose (see [Testing on a phone](Testing-on-a-Phone)).

| App | Address |
| --- | --- |
| Admin web | https://localhost:7025 |
| Register app | https://localhost:7032 |
| Bar app | https://localhost:7034 |
| Bartender app | https://localhost:7036 |
| Customer app | https://localhost:7038 |
| API reference (Scalar) | https://localhost:7180/scalar |
| Aspire dashboard | https://localhost:17092 |

Stop everything with Ctrl+C in the terminal. PostgreSQL and the storage emulator are persistent containers with volumes, so the data survives restarts; Redis starts empty every time.

## 5. A first order, end to end

1. **Admin web** (`https://localhost:7025`): sign in with `Admin:Email` and `Admin:Password`.
2. **Menu:** under Categories create a category (e.g. "Cocktails"), then under Items an item in it (name, description, portion, price, optionally a picture). Leave both Active.
3. **Floor plan:** under Tables add an area (e.g. "Inside"), then a table from the library strip (e.g. code `T1`).
4. **Team:** under Bartenders create a bartender with an email and a temporary password.
5. **Bar app** (`https://localhost:7034`): sign in as the bartender. The first sign-in asks for a new password.
6. **Customer app:** open `https://localhost:7038/t/T1` (or "Open" next to the table's QR code in the Admin). Add the item, Checkout, pay by **Card** with any number such as `4242 4242 4242 4242`, a future expiry and any CVC (`4000 0000 0000 0002` is declined).
7. **Back in the Bar app** the order is in the Queue already: Accept, Start, Complete. The guest's page changes with each step, and the Admin's dashboard and orders update live.
8. Try the **Register app** (same bartender account): pay for an order taken in the Bartender app, or sell at the counter.

## Next steps

- Use the apps on a real phone: [Testing on a phone](Testing-on-a-Phone).
- Look at the data: pgweb, Storage Explorer and RedisInsight are linked from their resources in the dashboard.
- Understand the code: [System architecture](System-Architecture), [Coding conventions](Coding-Conventions).
- Something didn't start: [Troubleshooting](Troubleshooting).