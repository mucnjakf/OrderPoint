using System.Globalization;
using OrderPoint.Application.Queries.Bartenders;
using OrderPoint.Application.Queries.Categories;
using OrderPoint.Application.Queries.Dashboard;
using OrderPoint.Application.Queries.Items;
using OrderPoint.Application.Queries.Menu;
using OrderPoint.Application.Queries.Orders;
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

    internal static string For(GetOrdersQuery query)
        => $"orders:page:{query.PageNumber}:{query.PageSize}:{query.Status}:{query.ItemId}:{query.BartenderId}:" +
           $"{query.SortBy}:{query.SearchQuery}";

    internal static string For(GetDashboardSummaryQuery query)
        => ForDashboard("summary", query.Period, query.TimeZone);

    internal static string For(GetRevenueTrendQuery query)
        => ForDashboard("revenue-trend", query.Period, query.TimeZone);

    internal static string For(GetCategoryRevenueQuery query)
        => ForDashboard("category-revenue", query.Period, query.TimeZone);

    internal static string For(GetTopItemsQuery query)
        => ForDashboard("top-items", query.Period, query.TimeZone);

    internal static string For(GetBartenderLeaderboardQuery query)
        => ForDashboard("bartender-leaderboard", query.Period, query.TimeZone);

    internal static string For(GetBusiestTimesQuery query)
        => ForDashboard("busiest-times", period: null, query.TimeZone);


    private static string ForDashboard(string widget, DashboardPeriod? period, TimeZoneInfo timeZone)
    {
        string localToday = DashboardPeriodRange
            .GetLocalToday(DateTimeOffset.UtcNow, timeZone)
            .ToString(DateFormat, CultureInfo.InvariantCulture);

        return $"dashboard:{widget}:{period}:{timeZone.Id}:{localToday}";
    }
}