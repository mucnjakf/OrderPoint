using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using MudBlazor;
using OrderPoint.Register.Orders.Api;
using OrderPoint.Register.Orders.Sorting;
using OrderPoint.Shared.Account.Services;
using OrderPoint.Shared.Auth.Services;
using OrderPoint.Shared.Dtos;
using OrderPoint.Shared.Layout;
using OrderPoint.Shared.Orders.Dtos;
using OrderPoint.Shared.Orders.Enumerations;
using OrderPoint.Shared.Orders.Services;
using OrderPoint.Shared.Services;

namespace OrderPoint.Register.Shared.Layout;

// The till's shell: the pages as tabs in the middle of the app bar, with what is waiting at the register, signed in as
// a bartender
public sealed partial class MainLayout : IDisposable
{
    // Only the counts are read
    private const int CountedOrdersPageSize = 1;



    // The brand and the live indicator take equal room on both sides, so the tabs sit in the middle
    private const string SideStyle = "flex: 1 1 0; min-width: 0;";

    // As tall as the app bar, so the underline of the current page sits on its bottom border
    private const string TabStyle = "height: 64px; border-radius: 0; border-bottom: 2px solid transparent;";

    private const string CurrentTabStyle =
        "height: 64px; border-radius: 0; border-bottom: 2px solid var(--mud-palette-primary); font-weight: 600;";

    [Inject]
    private AuthService AuthService { get; set; } = null!;

    [Inject]
    private NavigationManager NavigationManager { get; set; } = null!;

    [Inject]
    private TimeZoneService TimeZoneService { get; set; } = null!;

    [Inject]
    private DarkModeService DarkModeService { get; set; } = null!;

    [Inject]
    private AccountService AccountService { get; set; } = null!;

    [Inject]
    private OrdersHubService OrdersHubService { get; set; } = null!;

    [Inject]
    private ApiService ApiService { get; set; } = null!;

    [Inject]
    private OrderApiClient OrderApiClient { get; set; } = null!;

    private MudTheme Theme { get; } = AppTheme.Create();

    private int? ToPayCount { get; set; }

    private int? RefundsDueCount { get; set; }

    protected override async Task OnInitializedAsync()
    {
        AuthService.SessionChanged += OnSessionChanged;
        DarkModeService.Changed += OnDarkModeChanged;
        OrdersHubService.OrdersChanged += OnOrdersChanged;
        NavigationManager.LocationChanged += OnLocationChanged;

        await Task.WhenAll(
            DarkModeService.LoadAsync(),
            AuthService.LoadAsync(),
            TimeZoneService.LoadAsync());

        if (AuthService.IsSignedIn)
        {
            _ = OrdersHubService.StartAsync();

            await Task.WhenAll(AccountService.LoadAsync(), GetCountsAsync());
        }
    }

    public void Dispose()
    {
        AuthService.SessionChanged -= OnSessionChanged;
        DarkModeService.Changed -= OnDarkModeChanged;
        OrdersHubService.OrdersChanged -= OnOrdersChanged;
        NavigationManager.LocationChanged -= OnLocationChanged;
    }

    private void OnSessionChanged()
    {
        _ = InvokeAsync(async () =>
        {
            if (AuthService.IsSignedIn)
            {
                _ = OrdersHubService.StartAsync();

                await Task.WhenAll(AccountService.LoadAsync(), GetCountsAsync());
            }
            else
            {
                AccountService.Clear();
                ToPayCount = null;
                RefundsDueCount = null;

                await OrdersHubService.StopAsync();
            }

            StateHasChanged();
        });
    }

    private void OnDarkModeChanged()
    {
        _ = InvokeAsync(StateHasChanged);
    }

    private void OnOrdersChanged()
    {
        _ = InvokeAsync(GetCountsAsync);
    }

    private void OnLocationChanged(object? sender, LocationChangedEventArgs e)
    {
        _ = InvokeAsync(StateHasChanged);
    }

    // Today is the root, so it only matches itself
    private string GetTabStyle(string href)
    {
        string path = "/" + NavigationManager.ToBaseRelativePath(NavigationManager.Uri).Split('?')[0];
        bool isCurrent = href == "/" ? path == "/" : path.StartsWith(href, StringComparison.OrdinalIgnoreCase);

        return isCurrent ? CurrentTabStyle : TabStyle;
    }

    // A failed reload keeps what is shown
    private async Task GetCountsAsync()
    {
        PaginationDto<OrderDto>?[] pages = await Task.WhenAll(
            GetOrdersAsync(OrderPaymentFilter.ToPay),
            GetOrdersAsync(OrderPaymentFilter.RefundDue));

        if (pages[0] is not null)
        {
            ToPayCount = pages[0]!.TotalCount;
        }

        if (pages[1] is not null)
        {
            RefundsDueCount = pages[1]!.TotalCount;
        }

        StateHasChanged();
    }

    private Task<PaginationDto<OrderDto>?> GetOrdersAsync(OrderPaymentFilter payment)
    {
        return ApiService.ExecuteAsync(() => OrderApiClient.GetOrdersAsync(
            pageNumber: 1,
            CountedOrdersPageSize,
            nameof(OrderSortBy.CreatedAtUtcAsc),
            payment));
    }
}