using System.Globalization;
using OrderPoint.Application.Queries.Areas;
using OrderPoint.Application.Queries.Bartenders;
using OrderPoint.Application.Queries.Categories;
using OrderPoint.Application.Queries.Dashboard;
using OrderPoint.Application.Queries.Fixtures;
using OrderPoint.Application.Queries.Items;
using OrderPoint.Application.Queries.Menu;
using OrderPoint.Application.Queries.Orders;
using OrderPoint.Application.Queries.Tables;
using OrderPoint.Domain.Enumerations;

namespace OrderPoint.Application.Caching;

internal static class CacheKeys
{
    private const string DateFormat = "yyyy-MM-dd";

    internal static string For(GetCategoryQuery query) => $"categories:{query.Id}";

    internal static string For(GetCategoriesQuery query)
        => $"categories:page:{query.PageNumber}:{query.PageSize}:{query.Status}:{query.SortBy}:{query.SearchQuery}";

    internal static string For(SearchCategoriesQuery query) => $"categories:search:{query.SearchQuery}";

    internal static string For(GetItemQuery query) => $"items:{query.Id}";

    internal static string For(GetItemsQuery query)
        => $"items:page:{query.PageNumber}:{query.PageSize}:{query.CategoryId}:{query.Status}:{query.SortBy}:" +
           $"{query.SearchQuery}";

    internal static string For(GetBartenderQuery query) => $"bartenders:{query.Id}";

    internal static string For(GetBartendersQuery query)
        => $"bartenders:page:{query.PageNumber}:{query.PageSize}:{query.Status}:{query.SortBy}:{query.SearchQuery}";

    internal static string For(SearchBartendersQuery query) => $"bartenders:search:{query.SearchQuery}";

    internal static string For(GetOrderQuery query) => $"orders:{query.Id}";

    internal static string For(GetOrderTrackingQuery query) => $"orders:{query.Id}:tracking";

    internal static string For(GetMenuQuery query) => "menu";

    internal static string For(GetFloorPlanQuery query) => "tables:floor-plan";

    internal static string For(GetAreaQuery query) => $"tables:areas:{query.Id}";

    internal static string For(GetTableQuery query) => $"tables:{query.Id}";

    internal static string For(GetTableByCodeQuery query) => $"tables:code:{query.Code}";

    internal static string For(GetFixtureQuery query) => $"tables:fixtures:{query.Id}";

    internal static string For(GetOrdersQuery query)
        => $"orders:page:{query.PageNumber}:{query.PageSize}:{query.Status}:{query.ItemId}:{query.BartenderId}:" +
           $"{query.TableId}:{query.TakenByBartenderId}:{query.PaymentStatus}:{query.SortBy}:{query.SearchQuery}";

    internal static string For(GetDashboardSummaryQuery query, TimeSpan dayStart)
        => ForDashboard("summary", query.Period, query.TimeZone, dayStart);

    internal static string For(GetRevenueTrendQuery query, TimeSpan dayStart)
        => ForDashboard("revenue-trend", query.Period, query.TimeZone, dayStart);

    internal static string For(GetCategoryRevenueQuery query, TimeSpan dayStart)
        => ForDashboard("category-revenue", query.Period, query.TimeZone, dayStart);

    internal static string For(GetTopItemsQuery query, TimeSpan dayStart)
        => ForDashboard("top-items", query.Period, query.TimeZone, dayStart);

    internal static string For(GetBartenderLeaderboardQuery query, TimeSpan dayStart)
        => ForDashboard("bartender-leaderboard", query.Period, query.TimeZone, dayStart);

    // Calendar days, like the heat map itself
    internal static string For(GetBusiestTimesQuery query)
        => ForDashboard("busiest-times", period: null, query.TimeZone, TimeSpan.Zero);


    private static string ForDashboard(
        string widget,
        DashboardPeriod? period,
        TimeZoneInfo timeZone,
        TimeSpan dayStart)
    {
        string localToday = DashboardPeriodRange
            .GetLocalToday(DateTimeOffset.UtcNow, timeZone, dayStart)
            .ToString(DateFormat, CultureInfo.InvariantCulture);

        return $"dashboard:{widget}:{period}:{timeZone.Id}:{localToday}";
    }
}