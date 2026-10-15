using System.Globalization;
using Microsoft.AspNetCore.Components;
using OrderPoint.Register.Orders.Services;
using OrderPoint.Register.Today.Api;
using OrderPoint.Register.Today.Dtos;
using OrderPoint.Shared.Dtos;
using OrderPoint.Shared.Extensions;
using OrderPoint.Shared.Orders.Dtos;
using OrderPoint.Shared.Orders.Enumerations;
using OrderPoint.Shared.Orders.Services;
using OrderPoint.Shared.Orders.Sorting;
using OrderPoint.Shared.Services;

namespace OrderPoint.Register.Today.Pages;

// The register's day: the totals, per bartender, and every payment with its receipt, live
public sealed partial class TodayPage : IDisposable
{
    private const int TilesCount = 4;

    private const int PageSize = 10;

    private const string HoveredBartenderRowStyle = "background-color: var(--mud-palette-table-hover);";

    private const string SelectedBartenderRowStyle =
        HoveredBartenderRowStyle + " border-color: var(--mud-palette-primary);";

    [Inject]
    private ApiService ApiService { get; set; } = null!;

    [Inject]
    private RegisterDayApiClient RegisterDayApiClient { get; set; } = null!;

    [Inject]
    private PaymentApiClient PaymentApiClient { get; set; } = null!;

    [Inject]
    private OrdersHubService OrdersHubService { get; set; } = null!;

    [Inject]
    private TimeZoneService TimeZoneService { get; set; } = null!;

    [Inject]
    private PaymentDialogService PaymentDialogService { get; set; } = null!;

    [Inject]
    private IConfiguration Configuration { get; set; } = null!;

    private RegisterDayDto? Day { get; set; }

    private PaginationDto<OrderDto>? Pagination { get; set; }

    private IReadOnlyList<OrderDto> Payments { get; set; } = [];

    private bool IsLoadingPayments { get; set; } = true;

    private string? SearchQuery { get; set; }

    private PaymentChannel? SelectedChannel { get; set; }

    private Guid? SelectedBartenderId { get; set; }

    private string SelectedSortBy { get; set; } = nameof(PaymentSortBy.PaidAtUtcDesc);

    private Guid? HoveredBartenderId { get; set; }

    private string DayStartText =>
        DateTime.Today.Add(Configuration.GetBusinessDayStart()).ToString("t", CultureInfo.CurrentCulture);

    private bool HasFilters =>
        !string.IsNullOrWhiteSpace(SearchQuery) || SelectedChannel is not null || SelectedBartenderId is not null;

    private string PaymentsEmptyText => HasFilters ? "No payments found" : "No payments yet";

    protected override async Task OnInitializedAsync()
    {
        OrdersHubService.OrdersChanged += OnOrdersChanged;

        await Task.WhenAll(GetDayAsync(), GetPaymentsAsync(pageNumber: 1));
    }

    public void Dispose()
    {
        OrdersHubService.OrdersChanged -= OnOrdersChanged;
    }

    private void OnOrdersChanged()
    {
        _ = InvokeAsync(() => Task.WhenAll(
            GetDayAsync(),
            GetPaymentsAsync(Pagination?.PageNumber ?? 1, isLiveUpdate: true)));
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

    private async Task GetPaymentsAsync(int pageNumber, bool isLiveUpdate = false)
    {
        if (!isLiveUpdate)
        {
            IsLoadingPayments = true;
        }

        PaginationDto<OrderDto>? pagination = await ApiService.ExecuteAsync(
            () => PaymentApiClient.GetTodayPaymentsAsync(
                pageNumber,
                PageSize,
                SelectedSortBy,
                TimeZoneService.TimeZone.Id,
                SearchQuery,
                SelectedChannel,
                SelectedBartenderId));

        if (pagination is not null || !isLiveUpdate)
        {
            Pagination = pagination;
            Payments = pagination?.Items ?? [];
        }

        IsLoadingPayments = false;

        StateHasChanged();
    }

    private async Task OnFiltersChangedAsync()
    {
        await GetPaymentsAsync(pageNumber: 1);
    }

    private async Task OnPageChangedAsync(int pageNumber)
    {
        await GetPaymentsAsync(pageNumber);
    }

    // A bartender's row shows only their payments; a second click shows everyone's again
    private async Task ToggleBartenderAsync(Guid bartenderId)
    {
        SelectedBartenderId = SelectedBartenderId == bartenderId ? null : bartenderId;

        await GetPaymentsAsync(pageNumber: 1);
    }

    private async Task ShowReceiptAsync(OrderDto order)
    {
        await PaymentDialogService.ShowReceiptAsync(order);
    }

    private string? GetBartenderRowStyle(RegisterBartenderDto bartender)
    {
        if (bartender.Id == SelectedBartenderId)
        {
            return SelectedBartenderRowStyle;
        }

        return bartender.Id == HoveredBartenderId ? HoveredBartenderRowStyle : null;
    }

    private string GetBartenderName(Guid? bartenderId)
    {
        RegisterBartenderDto? bartender = Day?.Bartenders.FirstOrDefault(bartender => bartender.Id == bartenderId);

        return bartender is null ? string.Empty : $"{bartender.FirstName} {bartender.LastName}";
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
}