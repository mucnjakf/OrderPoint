using Microsoft.AspNetCore.Components;
using MudBlazor;
using OrderPoint.Admin.Bartenders.Api;
using OrderPoint.Admin.Bartenders.Dtos;
using OrderPoint.Admin.Orders.Api;
using OrderPoint.Admin.Orders.Dialogs;
using OrderPoint.Admin.Orders.Sorting;
using OrderPoint.Shared.Dtos;
using OrderPoint.Shared.Orders.Dtos;
using OrderPoint.Shared.Orders.Enumerations;
using OrderPoint.Shared.Orders.Services;
using OrderPoint.Shared.Services;

namespace OrderPoint.Admin.Orders.Pages;

public sealed partial class OrdersPage : IDisposable
{
    private const int PageSize = 10;

    [Inject]
    private TimeZoneService TimeZoneService { get; set; } = null!;

    [Inject]
    private IDialogService DialogService { get; set; } = null!;

    [Inject]
    private ApiService ApiService { get; set; } = null!;

    [Inject]
    private OrderApiClient OrderApiClient { get; set; } = null!;

    [Inject]
    private BartenderApiClient BartenderApiClient { get; set; } = null!;

    [Inject]
    private OrdersHubService OrdersHubService { get; set; } = null!;

    private NewOrdersTracker NewOrders { get; } = new();

    private static readonly (OrderPaymentFilter Payment, string Label)[] PaymentOptions =
    [
        (OrderPaymentFilter.ToPay, "To pay"),
        (OrderPaymentFilter.NothingToPay, "Nothing to pay"),
        (OrderPaymentFilter.Paid, "Paid"),
        (OrderPaymentFilter.RefundDue, "Refund due"),
        (OrderPaymentFilter.Refunded, "Refunded")
    ];

    private Dictionary<OrderStatus, int> StatusCounts { get; } = [];

    private HashSet<OrderStatus> LoadingStatuses { get; } = [.. Enum.GetValues<OrderStatus>()];

    private PaginationDto<OrderDto>? Pagination { get; set; }

    private IReadOnlyList<OrderDto> Orders { get; set; } = [];

    private string SelectedSortBy { get; set; } = nameof(OrderSortBy.CreatedAtUtcDesc);

    private string? SearchQuery { get; set; }

    private OrderStatus? SelectedStatus { get; set; }

    private BartenderDto? SelectedBartender { get; set; }

    private OrderPaymentFilter? SelectedPayment { get; set; }

    private MudAutocomplete<BartenderDto> BartenderAutocomplete { get; set; } = null!;

    private bool IsLoading { get; set; } = true;

    protected override async Task OnInitializedAsync()
    {
        OrdersHubService.OrdersChanged += OnOrdersChanged;

        await Task.WhenAll(
            GetStatusCountsAsync(),
            GetOrdersAsync(pageNumber: 1));
    }

    public void Dispose()
    {
        OrdersHubService.OrdersChanged -= OnOrdersChanged;
    }

    // Live updates swap the data in place on the current page, without skeletons
    private void OnOrdersChanged()
    {
        _ = InvokeAsync(() => Task.WhenAll(
            GetStatusCountsAsync(isLiveUpdate: true),
            GetOrdersAsync(Pagination?.PageNumber ?? 1, isLiveUpdate: true)));
    }

    private async Task GetStatusCountsAsync(bool isLiveUpdate = false)
    {
        await Task.WhenAll(Enum.GetValues<OrderStatus>().Select(status => GetStatusCountAsync(status, isLiveUpdate)));
    }

    private async Task GetStatusCountAsync(OrderStatus status, bool isLiveUpdate)
    {
        if (!isLiveUpdate)
        {
            LoadingStatuses.Add(status);
        }

        PaginationDto<OrderDto>? pagination = await ApiService.ExecuteAsync(
            () => OrderApiClient.GetOrdersAsync(
                pageNumber: 1,
                pageSize: 1,
                nameof(OrderSortBy.CreatedAtUtcDesc),
                status: status,
                bartenderId: SelectedBartender?.Id,
                payment: SelectedPayment));

        StatusCounts[status] = pagination?.TotalCount ?? 0;

        LoadingStatuses.Remove(status);

        StateHasChanged();
    }

    private async Task GetOrdersAsync(int pageNumber, bool isLiveUpdate = false)
    {
        if (!isLiveUpdate)
        {
            IsLoading = true;
        }

        DateTimeOffset loadStartedAtUtc = DateTimeOffset.UtcNow;

        Pagination = await ApiService.ExecuteAsync(() => OrderApiClient.GetOrdersAsync(
            pageNumber,
            PageSize,
            SelectedSortBy,
            SearchQuery,
            SelectedStatus,
            bartenderId: SelectedBartender?.Id,
            payment: SelectedPayment));

        Orders = Pagination?.Items ?? [];

        NewOrders.Track(Orders, loadStartedAtUtc, () => InvokeAsync(StateHasChanged));

        IsLoading = false;

        StateHasChanged();
    }

    private async Task OnSearchChangedAsync()
    {
        await GetOrdersAsync(pageNumber: 1);
    }

    private async Task OnSortChangedAsync()
    {
        await GetOrdersAsync(pageNumber: 1);
    }

    private async Task OnStatusClickAsync(OrderStatus status)
    {
        SelectedStatus = SelectedStatus == status ? null : status;

        await GetOrdersAsync(pageNumber: 1);
    }

    private async Task OnBartenderChangedAsync()
    {
        await Task.WhenAll(
            GetStatusCountsAsync(),
            GetOrdersAsync(pageNumber: 1));
    }

    // The status counts follow it too, like the bartender filter
    private async Task OnPaymentChangedAsync()
    {
        await Task.WhenAll(
            GetStatusCountsAsync(),
            GetOrdersAsync(pageNumber: 1));
    }

    private async Task OnPageChangedAsync(int pageNumber)
    {
        await GetOrdersAsync(pageNumber);
    }

    private async Task<IEnumerable<BartenderDto>> OnBartenderSearchAsync(
        string? value,
        CancellationToken cancellationToken)
    {
        IReadOnlyList<BartenderDto>? bartenders = await ApiService.ExecuteAsync(
            () => BartenderApiClient.SearchBartendersAsync(value, cancellationToken),
            cancellationToken);

        return bartenders ?? [];
    }

    private async Task OnBartenderClearedAsync()
    {
        await BartenderAutocomplete.OpenMenuAsync();
    }

    private async Task ShowOrderDetailsDialogAsync(OrderDto order)
    {
        var parameters = new DialogParameters<OrderDetailsDialog>
        {
            { dialog => dialog.Order, order }
        };

        var options = new DialogOptions
        {
            MaxWidth = MaxWidth.Medium,
            FullWidth = true
        };

        await DialogService
            .ShowAsync<OrderDetailsDialog>(string.Empty, parameters, options);
    }

    // Why a declined or cancelled order ended, so the ticket says it without opening the details
    private static string? GetEndReason(OrderDto order)
    {
        return order.Status switch
        {
            OrderStatus.Declined when order.DeclineReason is not null => $"Declined: {order.DeclineReason}",
            OrderStatus.Cancelled => "Cancelled by the guest",
            _ => null
        };
    }
}