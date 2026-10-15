using Microsoft.AspNetCore.Components;
using MudBlazor;
using OrderPoint.Admin.Bartenders.Api;
using OrderPoint.Admin.Bartenders.Dtos;
using OrderPoint.Admin.Dashboard.Api;
using OrderPoint.Admin.Dashboard.Dtos;
using OrderPoint.Admin.Dashboard.Enumerations;
using OrderPoint.Admin.Orders.Dialogs;
using OrderPoint.Admin.Payments.Api;
using OrderPoint.Shared.Dtos;
using OrderPoint.Shared.Orders.Dtos;
using OrderPoint.Shared.Orders.Enumerations;
using OrderPoint.Shared.Orders.Services;
using OrderPoint.Shared.Orders.Sorting;
using OrderPoint.Shared.Services;

namespace OrderPoint.Admin.Payments.Pages;

// Every payment of the period as a ledger, the selected one as its receipt beside it, and the totals by how it came in
public sealed partial class PaymentsPage : IDisposable
{
    private const int PageSize = 10;

    // Cash, card at the register and card online come from one call
    private const int ChannelCardsCount = 3;

    private static readonly (PaymentStatus Status, string Label)[] StatusOptions =
    [
        (PaymentStatus.Paid, "Paid"),
        (PaymentStatus.RefundDue, "Refund due"),
        (PaymentStatus.Refunded, "Refunded")
    ];

    [Inject]
    private TimeZoneService TimeZoneService { get; set; } = null!;

    [Inject]
    private IDialogService DialogService { get; set; } = null!;

    [Inject]
    private ApiService ApiService { get; set; } = null!;

    [Inject]
    private PaymentApiClient PaymentApiClient { get; set; } = null!;

    [Inject]
    private DashboardApiClient DashboardApiClient { get; set; } = null!;

    [Inject]
    private BartenderApiClient BartenderApiClient { get; set; } = null!;

    [Inject]
    private OrdersHubService OrdersHubService { get; set; } = null!;

    private DashboardPeriod SelectedPeriod { get; set; } = DashboardPeriod.Today;

    private PaymentMethodsDto? PaymentMethods { get; set; }

    private DashboardLiveDto? Live { get; set; }

    private PaginationDto<OrderDto>? Pagination { get; set; }

    private IReadOnlyList<OrderDto> Payments { get; set; } = [];

    private string SelectedSortBy { get; set; } = nameof(PaymentSortBy.PaidAtUtcDesc);

    private string? SearchQuery { get; set; }

    private PaymentChannel? SelectedChannel { get; set; }

    private PaymentStatus? SelectedStatus { get; set; }

    private BartenderDto? SelectedBartender { get; set; }

    private MudAutocomplete<BartenderDto> BartenderAutocomplete { get; set; } = null!;

    private OrderDto? SelectedPayment { get; set; }

    private bool IsLoading { get; set; } = true;

    protected override async Task OnInitializedAsync()
    {
        OrdersHubService.OrdersChanged += OnOrdersChanged;

        await Task.WhenAll(
            GetPaymentMethodsAsync(),
            GetLiveAsync(),
            GetPaymentsAsync(pageNumber: 1));
    }

    public void Dispose()
    {
        OrdersHubService.OrdersChanged -= OnOrdersChanged;
    }

    // Live updates swap the data in place on the current page, without skeletons
    private void OnOrdersChanged()
    {
        _ = InvokeAsync(() => Task.WhenAll(
            GetPaymentMethodsAsync(),
            GetLiveAsync(),
            GetPaymentsAsync(Pagination?.PageNumber ?? 1, isLiveUpdate: true)));
    }

    // A failed reload keeps what is shown
    private async Task GetPaymentMethodsAsync()
    {
        PaymentMethodsDto? paymentMethods = await ApiService.ExecuteAsync(
            () => DashboardApiClient.GetPaymentMethodsAsync(SelectedPeriod));

        if (paymentMethods is null)
        {
            return;
        }

        PaymentMethods = paymentMethods;

        StateHasChanged();
    }

    // Only for the refunds due, which come from any day
    private async Task GetLiveAsync()
    {
        DashboardLiveDto? live = await ApiService.ExecuteAsync(() => DashboardApiClient.GetLiveAsync());

        if (live is null)
        {
            return;
        }

        Live = live;

        StateHasChanged();
    }

    private async Task GetPaymentsAsync(int pageNumber, bool isLiveUpdate = false)
    {
        if (!isLiveUpdate)
        {
            IsLoading = true;
        }

        Pagination = await ApiService.ExecuteAsync(() => PaymentApiClient.GetPaymentsAsync(
            pageNumber,
            PageSize,
            SelectedSortBy,
            SelectedPeriod,
            TimeZoneService.TimeZone.Id,
            SearchQuery,
            SelectedChannel,
            SelectedBartender?.Id,
            SelectedStatus));

        Payments = Pagination?.Items ?? [];

        // The receipt follows the reloaded payment, e.g. once its refund is handed back
        SelectedPayment = Payments.FirstOrDefault(order => order.Id == SelectedPayment?.Id) ?? SelectedPayment;

        IsLoading = false;

        StateHasChanged();
    }

    private async Task OnPeriodChangedAsync()
    {
        PaymentMethods = null;

        await Task.WhenAll(
            GetPaymentMethodsAsync(),
            GetPaymentsAsync(pageNumber: 1));
    }

    private async Task OnFiltersChangedAsync()
    {
        await GetPaymentsAsync(pageNumber: 1);
    }

    private async Task OnPageChangedAsync(int pageNumber)
    {
        await GetPaymentsAsync(pageNumber);
    }

    private void SelectPayment(OrderDto order)
    {
        SelectedPayment = order;
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

        await DialogService.ShowAsync<OrderDetailsDialog>(string.Empty, parameters, options);
    }
}