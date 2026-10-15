# Caching

Most reads in the API are cached with **HybridCache**: a fast in-memory cache in front of **Redis**. Writes clear exactly the cached reads they affect, by tag, so nothing shows stale data.

## Two levels

| Level | Lifetime | Notes |
| --- | --- | --- |
| In memory (per API process) | 5 minutes | First stop for every read |
| Redis (`cache` resource) | 30 minutes | Shared second level; it has no volume, so every run starts empty |

Redis is optional at runtime: its client is set to **fail fast** while Redis is down, so reads fall back to memory and the database at once instead of waiting for a timeout. Its health check then reports **Degraded** (amber in the Admin's Health page), not Unhealthy.

## Reads

A query handler wraps its work in `cache.GetOrCreateAsync`:

```csharp
ItemDto? itemDto = await cache.GetOrCreateAsync(
    CacheKeys.For(query),
    query,
    GetItemAsync,
    tags: [CacheTags.Items],
    cancellationToken: cancellationToken);
```

- The **DTO** is cached, never the `Result` (which isn't serialisable). A "not found" is cached as `null` and turned into the error by the handler.
- **Keys** come from `CacheKeys.For(<query>)`, one overload per query: `items:<id>`, `items:page:<every parameter>`, `items:search:<text>` (free text always last). Dashboard keys also hold the period, the viewer's time zone and the business date, so "today" moves on at the day start.
- **Tags** name the feature: `categories`, `items`, `bartenders`, `orders`, `dashboard`, `menu`, `tables`, `stock`, `suppliers`, `deliveries`, `stock-counts`.

**Cached:**
- Get, list and search for categories, items, bartenders and orders.
- Payments, the guests' menu, order tracking.
- The floor plan and single areas, tables and fixtures.
- Stock items, their movements and the stock summary; suppliers, deliveries, stock counts.
- Every dashboard widget except Right now.

**Not cached:** the dashboard's Right now, My shift and My day, the register's day, table requests and tables status, account, sign-in and health. These change every minute or are per user.

## Writes

A command whose change shows up in cached reads declares the tags to clear:

```csharp
public sealed record CancelOrderCommand(Guid Id)
    : ICommand, ICacheInvalidatingCommand, IOrderNotifyingCommand
{
    IReadOnlyList<string> ICacheInvalidatingCommand.InvalidatedCacheTags => CacheTags.OrderChanges;
}
```

`CacheInvalidationBehavior` calls `RemoveByTagAsync` once the command succeeds, with no cancellation token, so a caller who left can't leave stale data behind.

The tag groups follow what each DTO shows:

| Group | Clears | Why |
| --- | --- | --- |
| `CategoryChanges` | categories, items, dashboard, menu | Items show their category |
| `ItemChanges` | items, categories, orders, dashboard, menu | Categories count items, orders show item names |
| `BartenderChanges` | bartenders, orders, dashboard | Orders show the bartender |
| `OrderChanges` | orders, items, bartenders, dashboard | Items and bartenders show order counts |
| `CompletedOrderChanges` | the above + stock, menu, suppliers, stock-counts | Completing takes stock off |
| `TableChanges` | tables | |
| `StockChanges` | stock, items, menu, suppliers, stock-counts | Availability and cost come from stock |
| `SupplierChanges` | suppliers, stock | Stock items show their supplier |
| `DeliveryChanges` | deliveries, suppliers, stock, items, menu, stock-counts | A delivery adds stock and changes costs |
| `OpenStockCountChanges` | stock-counts | Entering figures changes only the count |
| `StockCountChanges` | stock-counts, stock, items, menu, suppliers | Closing applies the differences |

## Rules for new code

- A new query gets a `CacheKeys.For` overload and the tag of its feature.
- A new command that changes something a cached DTO shows implements `ICacheInvalidatingCommand`.
- A new DTO field that comes from another feature means adding that feature's changes to the group.

The cache is cleared **before** SignalR messages go out (see [Real-time updates](Real-Time-Updates)), so an app that reloads on a message always gets fresh data.

## Troubleshooting

To inspect the cache, open RedisInsight from the Aspire dashboard. To start clean, restart the `cache` resource (it has no volume) and the API (its memory cache).