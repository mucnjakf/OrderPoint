# Tech stack

Everything OrderPoint is built with, with the versions in use and why each was chosen. The reasoning behind the bigger choices is on [Architecture decisions](Architecture-Decisions).

## Platform

| Technology | Version | Used for |
| --- | --- | --- |
| .NET / C# | .NET 10, C# latest | Every project |
| Aspire | 13.6.1 (AppHost SDK and hosting packages) | Starts the whole system locally: containers, projects, configuration, dashboard |
| Docker | Desktop | PostgreSQL, Azurite and Redis containers |
| PostgreSQL | 18 (Aspire's image) | The database |
| Azure Blob Storage | Azurite emulator | Item, category, bartender and profile pictures |
| Redis | Aspire's image | Second-level cache |
| Microsoft dev tunnels | devtunnel CLI | Public HTTPS addresses for phones |

## Backend

| Package | Version | Used for |
| --- | --- | --- |
| ASP.NET Core Minimal API | 10.0 | HTTP endpoints, one per file |
| MediatR | 14.1.0 | CQRS: commands, queries and pipeline behaviours |
| FluentValidation (DI extensions) | 12.1.1 | Request validators |
| Entity Framework Core + Npgsql provider | Npgsql.EntityFrameworkCore.PostgreSQL 10.0.3 | Data access and migrations |
| Aspire.Npgsql.EntityFrameworkCore.PostgreSQL | 13.6.1 | DbContext health check, tracing, metrics and retries |
| ASP.NET Core Identity (EF Core) | 10.0.12 | User store: password hashing, lockout |
| JWT bearer authentication | 10.0.12 | Access tokens (with Microsoft.IdentityModel.JsonWebTokens 8.19.2) |
| Microsoft.Extensions.Caching.Hybrid | 10.10.0 | HybridCache: memory + Redis with tags |
| Aspire.StackExchange.Redis.DistributedCaching | 13.6.1 | Redis as HybridCache's second level |
| Aspire.Azure.Storage.Blobs | 13.6.1 | Blob container client |
| ASP.NET Core SignalR | 10.0 | Live updates (`hubs/orders`, `hubs/order-tracking`) |
| Microsoft.AspNetCore.OpenApi | 10.0.12 | OpenAPI document with XML comments |
| Scalar.AspNetCore | 2.13.19 | Interactive API reference at `/scalar` |

## Frontend

| Package | Version | Used for |
| --- | --- | --- |
| Blazor Web App (Interactive Server) | .NET 10 | All five apps |
| MudBlazor | 9.6.0 | Components, layout, theme, icons |
| Microsoft.AspNetCore.SignalR.Client | 10.0.12 | The apps' live connection to the API hubs |
| QRCoder | 1.8.1 | Table QR codes as SVG (Admin) |
| Geist font | | Typography (from the "vercel" theme) |

## Observability and resilience

| Package | Version | Used for |
| --- | --- | --- |
| OpenTelemetry (hosting, OTLP exporter) | 1.19.1 | Sends logs, traces and metrics to the Aspire dashboard |
| OpenTelemetry instrumentation (ASP.NET Core, HTTP, runtime) | 1.19.0 | Automatic traces and metrics |
| Microsoft.Extensions.Http.Resilience | 10.2.0 | Retries and timeouts on outgoing HTTP (GET only) |
| Microsoft.Extensions.ServiceDiscovery | 10.2.0 | Resolving `https+http://order-point-api` |

## Aspire hosting

| Package | Version | Used for |
| --- | --- | --- |
| Aspire.Hosting.PostgreSQL | 13.6.1 | PostgreSQL container and pgweb |
| Aspire.Hosting.Azure.Storage | 13.6.1 | Azurite emulator and the `images` container |
| Aspire.Hosting.Redis | 13.6.1 | Redis and RedisInsight |
| Aspire.Hosting.DevTunnels | 13.6.1 | The `tunnel` resource |
| CommunityToolkit.Aspire.Hosting.Azure.Extensions | 13.6.0 | Azure Storage Explorer for the blobs |

## Tooling

| Tool | Used for |
| --- | --- |
| Aspire CLI (`aspire run`) | Running the AppHost |
| `dotnet ef` | Creating migrations |
| `dotnet user-secrets` | Secrets |
| `dotnet format` and `.editorconfig` | Formatting: UTF-8 with BOM, CRLF, 4 spaces, 120 columns |
| pgweb, Azure Storage Explorer, RedisInsight | Looking at the data during development |