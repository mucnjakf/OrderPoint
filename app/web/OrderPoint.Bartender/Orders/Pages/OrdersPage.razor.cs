using System.Globalization;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using OrderPoint.Bartender.Orders.Api;
using OrderPoint.Bartender.Orders.Dialogs;
using OrderPoint.Bartender.Orders.Sorting;
using OrderPoint.Shared.Auth.Services;
using OrderPoint.Shared.Dtos;
using OrderPoint.Shared.Extensions;
using OrderPoint.Shared.Orders.Dtos;
using OrderPoint.Shared.Orders.Services;
using OrderPoint.Shared.Services;

namespace OrderPoint.Bartender.Orders.Pages;

// What this bartender took since the day start, live, so they see when the bar takes it and when it is paid
public sealed partial class OrdersPage : IDisposable
{
    // A day's orders of one bartender fit on one page
    private const int PageSize = 50;

    private const int LoadingCardsCount = 3;

    // "Placed 3 min ago" moves on even when nothing changes
    private const int TimeRefreshIntervalInSeconds = 30;

    private readonly PeriodicTimer _timeRefreshTimer = new(TimeSpan.FromSeconds(TimeRefreshIntervalInSeconds));

    [Inject]
    private ApiService ApiService { get; set; } = null!;

    [Inject]
    private OrderApiClient OrderApiClient { get; set; } = null!;

    [Inject]
    private OrdersHubService OrdersHubService { get; set; } = null!;

    [Inject]
    private AuthService AuthService { get; set; } = null!;

    [Inject]
    private TimeZoneService TimeZoneService { get; set; } = null!;

    [Inject]
    private IConfiguration Configuration { get; set; } = null!;

    [Inject]
    private IDialogService DialogService { get; set; } = null!;

    private IReadOnlyList<OrderDto> Orders { get; set; } = [];

    private bool IsLoading { get; set; } = true;

    private Guid? WithdrawingOrderId { get; set; }

    private string DayStartText =>
        DateTime.Today.Add(Configuration.GetBusinessDayStart()).ToString("t", CultureInfo.CurrentCulture);

    protected override async Task OnInitializedAsync()
    {
        OrdersHubService.OrdersChanged += OnOrdersChanged;

        _ = RefreshTimesAsync();

        await GetOrdersAsync();
    }

    public void Dispose()
    {
        OrdersHubService.OrdersChanged -= OnOrdersChanged;
        _timeRefreshTimer.Dispose();
    }

    private void OnOrdersChanged()
    {
        _ = InvokeAsync(() => GetOrdersAsync(isLiveUpdate: true));
    }

    // A live update shows no skeleton, and a failed one keeps what is shown
    private async Task GetOrdersAsync(bool isLiveUpdate = false)
    {
        PaginationDto<OrderDto>? pagination = await ApiService.ExecuteAsync(() => OrderApiClient.GetOrdersAsync(
            pageNumber: 1,
            PageSize,
            nameof(OrderSortBy.CreatedAtUtcDesc),
            AuthService.UserId));

        if (pagination is not null || !isLiveUpdate)
        {
            DateTimeOffset todayStartUtc = TimeZoneService.TimeZone.GetTodayStartUtc(
                Configuration.GetBusinessDayStart());

            Orders = (pagination?.Items ?? []).Where(order => order.CreatedAtUtc >= todayStartUtc).ToList();
        }

        IsLoading = false;

        StateHasChanged();
    }

    // Refused once the bar accepted it; the reload then shows why
    private async Task WithdrawAsync(OrderDto order)
    {
        var parameters = new DialogParameters<WithdrawOrderDialog>
        {
            { dialog => dialog.OrderNumber, order.Number }
        };

        var options = new DialogOptions
        {
            MaxWidth = MaxWidth.ExtraSmall,
            FullWidth = true
        };

        IDialogReference dialogReference = await DialogService
            .ShowAsync<WithdrawOrderDialog>(string.Empty, parameters, options);

        DialogResult dialogResult = (await dialogReference.Result)!;

        if (dialogResult.Canceled)
        {
            return;
        }

        WithdrawingOrderId = order.Id;

        await ApiService.ExecuteAsync(
            () => OrderApiClient.WithdrawOrderAsync(order.Id),
            $"Order #{order.Number} withdrawn");

        WithdrawingOrderId = null;

        await GetOrdersAsync(isLiveUpdate: true);
    }

    private async Task RefreshTimesAsync()
    {
        while (await _timeRefreshTimer.WaitForNextTickAsync())
        {
            await InvokeAsync(StateHasChanged);
        }
    }
}