using System.Globalization;
using Microsoft.AspNetCore.Components;
using OrderPoint.Register.Orders.Api;
using OrderPoint.Register.Orders.Services;
using OrderPoint.Register.Orders.Sorting;
using OrderPoint.Register.Today.Api;
using OrderPoint.Register.Today.Dtos;
using OrderPoint.Shared.Dtos;
using OrderPoint.Shared.Extensions;
using OrderPoint.Shared.Orders.Dtos;
using OrderPoint.Shared.Orders.Enumerations;
using OrderPoint.Shared.Orders.Services;
using OrderPoint.Shared.Services;

namespace OrderPoint.Register.Today.Pages;

// The register's day: the totals, per bartender, and every payment with its receipt, live
public sealed partial class TodayPage : IDisposable
{
    private const int TilesCount = 4;

    // A day's payments fit on one page
    private const int PageSize = 100;

    [Inject]
    private ApiService ApiService { get; set; } = null!;

    [Inject]
    private RegisterDayApiClient RegisterDayApiClient { get; set; } = null!;

    [Inject]
    private OrderApiClient OrderApiClient { get; set; } = null!;

    [Inject]
    private OrdersHubService OrdersHubService { get; set; } = null!;

    [Inject]
    private TimeZoneService TimeZoneService { get; set; } = null!;

    [Inject]
    private PaymentDialogService PaymentDialogService { get; set; } = null!;

    [Inject]
    private IConfiguration Configuration { get; set; } = null!;

    private RegisterDayDto? Day { get; set; }

    private IReadOnlyList<OrderDto> Payments { get; set; } = [];

    private bool IsLoadingPayments { get; set; } = true;

    private string DayStartText =>
        DateTime.Today.Add(Configuration.GetBusinessDayStart()).ToString("t", CultureInfo.CurrentCulture);

    protected override async Task OnInitializedAsync()
    {
        OrdersHubService.OrdersChanged += OnOrdersChanged;

        await Task.WhenAll(GetDayAsync(), GetPaymentsAsync());
    }

    public void Dispose()
    {
        OrdersHubService.OrdersChanged -= OnOrdersChanged;
    }

    private void OnOrdersChanged()
    {
        _ = InvokeAsync(() => Task.WhenAll(GetDayAsync(), GetPaymentsAsync()));
    }

    // A failed reload keeps what is shown
    private async Task GetDayAsync()
    {
        RegisterDayDto? day = await ApiService.ExecuteAsync(
            () => RegisterDayApiClient.GetRegisterDayAsync(TimeZoneService.TimeZone.Id));

        if (day is null)
        {
            return;
        }

        Day = day;

        StateHasChanged();
    }

    // Paid since the day start, the newest first; refunded ones are left out like in the totals
    private async Task GetPaymentsAsync()
    {
        PaginationDto<OrderDto>? pagination = await ApiService.ExecuteAsync(() => OrderApiClient.GetOrdersAsync(
            pageNumber: 1,
            PageSize,
            nameof(OrderSortBy.CreatedAtUtcDesc),
            OrderPaymentFilter.Paid));

        if (pagination is not null)
        {
            DateTimeOffset todayStartUtc = TimeZoneService.TimeZone.GetTodayStartUtc(
                Configuration.GetBusinessDayStart());

            Payments = pagination.Items
                .Where(order => order.PaidAtUtc >= todayStartUtc)
                .OrderByDescending(order => order.PaidAtUtc)
                .ToList();
        }

        IsLoadingPayments = false;

        StateHasChanged();
    }

    private static string GetBartenderText(RegisterBartenderDto bartender)
    {
        string cash = bartender.CashTotal.ToString("C", CultureInfo.CurrentCulture);
        string card = bartender.CardTotal.ToString("C", CultureInfo.CurrentCulture);

        return $"{bartender.PaymentsCount.ToCountText("payment", "payments")} · cash {cash} · card {card}";
    }

    private static string GetBartenderTotalText(RegisterBartenderDto bartender)
    {
        return (bartender.CashTotal + bartender.CardTotal).ToString("C", CultureInfo.CurrentCulture);
    }

    private static string GetMethodText(OrderDto order)
    {
        if (order.PaymentMethod == PaymentMethod.Cash)
        {
            return "Cash";
        }

        return order.PaidByBartender is null ? "Card online" : "Card";
    }

    private static string GetPaidByText(OrderDto order)
    {
        return order.PaidByBartender is { } paidBy ? $"{paidBy.FirstName} {paidBy.LastName}" : "Guest";
    }
}