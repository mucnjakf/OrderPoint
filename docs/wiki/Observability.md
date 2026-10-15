# Observability

Everything reports to the **Aspire dashboard** over OpenTelemetry (OTLP): structured logs, traces and metrics from the API and all five apps. Opening an Admin page shows up as one trace: Admin → API → MediatR request → SQL.

## Shared defaults

`OrderPoint.ServiceDefaults` is added to every project with `AddServiceDefaults()`:

- **Instrumentation:**
  - ASP.NET Core, HttpClient and the .NET runtime.
  - Every `OrderPoint.*` activity source and meter.
  - Blazor Server's own sources and meters: navigation, events, circuits, rendering.
- **Not traced:** `/health` and `/alive` requests.
- **Resilience:** outgoing HTTP calls retry on transient errors, but only for GET, so a slow POST is never sent twice.
- **Health endpoints** in development: `/health` (every check) and `/alive` (the process itself). Every health check gets a 5-second timeout unless it sets its own.

The database's traces, metrics, health check and retries come from the Aspire Npgsql integration on the API's DbContext, the storage's from the Azure Blob integration, and Redis's from the Redis integration. SQL commands appear as trace spans, so EF's own command logging is reduced to warnings.

## Application telemetry

`ApplicationTelemetry` (source and meter `OrderPoint.Application`) and the `TelemetryBehavior` around every command and query provide:

| Signal | Name | Tags |
| --- | --- | --- |
| Span per request | the request's name, e.g. `AcceptOrderCommand` | `orderpoint.request.name`, `orderpoint.request.type`, `error.type` (the error code on failure) |
| Histogram | `orderpoint.request.duration` (seconds) | same as the span |
| Counter | `orderpoint.orders.placed` | |
| Histogram | `orderpoint.orders.value` (EUR) | |
| Counter | `orderpoint.auth.logins` | `orderpoint.user.role`, `error.type` |

Each request also writes one log line: Information on success, Warning with the error code on failure. The request itself is never logged, because it may hold a password or a card number.

## Event logs

Handlers whose outcome matters later write structured log lines with named values (`{OrderNumber}`, `{BartenderId}`, `{ItemCount}`). They cover:

- sign-ins, password changes and resets, signing out everywhere;
- bartenders created, (de)activated and deleted;
- orders placed and every status change;
- deletes and bulk changes;
- stock counts closed.

Failed request validations are logged by the validation exception handler, since they never reach MediatR.

## Health

`GET api/health` (Admin) runs every health check and returns each with a readable name. The Admin shows them on its **Health** page and in the app bar indicator, checking every 30 seconds:

| Section | Components |
| --- | --- |
| Core services | API, Database, Storage, Cache |
| Apps | Bar web, Bartender web, Customer web, Register web |
| Developer tools | Database browser (pgweb), Storage browser, Cache browser (RedisInsight) |

The cache, the apps and the developer tools report **Degraded** (amber) when down rather than Unhealthy, because the system still works without them. When the API itself can't be reached, the Admin shows that as a red result instead of an error message.

## Where to look

- **The Aspire dashboard** (`https://localhost:17092`): Resources, Console logs, Structured logs, Traces and Metrics. The API resource has an "API reference" link to Scalar.
- **Data:** pgweb (database), Storage Explorer (blobs) and RedisInsight (cache), each linked from its resource in the dashboard.