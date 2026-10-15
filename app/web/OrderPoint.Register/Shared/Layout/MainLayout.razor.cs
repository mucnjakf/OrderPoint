using System.Security.Cryptography;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using MudBlazor;
using OrderPoint.Register.Orders.Api;
using OrderPoint.Register.Orders.Sorting;
using OrderPoint.Register.Shared.Extensions;
using OrderPoint.Shared.Account.Services;
using OrderPoint.Shared.Auth.Services;
using OrderPoint.Shared.Dtos;
using OrderPoint.Shared.Layout;
using OrderPoint.Shared.Orders.Dtos;
using OrderPoint.Shared.Orders.Enumerations;
using OrderPoint.Shared.Orders.Services;
using OrderPoint.Shared.Services;

namespace OrderPoint.Register.Shared.Layout;

// The Admin's shell for the till: a drawer with what is waiting at the register, signed in as a bartender
public sealed partial class MainLayout : IDisposable
{
    private const string DrawerOpenStorageKey = "drawer-open";

    // Every unpaid order fits: a declined or cancelled one has nothing to pay, so they are counted here
    private const int CountedOrdersPageSize = 100;

    private const string DrawerToggleWrapperStyle =
        "position: fixed; left: 0; bottom: 72px; transform: translateX(-50%); " +
        "z-index: calc(var(--mud-zindex-drawer) + 1)";

    private const string DrawerToggleButtonStyle =
        "background-color: var(--mud-palette-surface); border: 1px solid var(--mud-palette-lines-default)";

    [Inject]
    private AuthService AuthService { get; set; } = null!;

    [Inject]
    private ProtectedLocalStorage ProtectedLocalStorage { get; set; } = null!;

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

    private bool DrawerOpen { get; set; } = true;

    private int? ToPayCount { get; set; }

    private int? RefundsDueCount { get; set; }

    private string DrawerToggleIcon => DrawerOpen
        ? Icons.Material.Filled.ChevronLeft
        : Icons.Material.Filled.ChevronRight;

    protected override async Task OnInitializedAsync()
    {
        AuthService.SessionChanged += OnSessionChanged;
        DarkModeService.Changed += OnDarkModeChanged;
        OrdersHubService.OrdersChanged += OnOrdersChanged;

        await Task.WhenAll(
            LoadLayoutPreferencesAsync(),
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

    // A failed reload keeps what is shown
    private async Task GetCountsAsync()
    {
        PaginationDto<OrderDto>?[] pages = await Task.WhenAll(
            GetOrdersAsync(PaymentStatus.Unpaid),
            GetOrdersAsync(PaymentStatus.RefundDue));

        if (pages[0] is not null)
        {
            ToPayCount = pages[0]!.Items.Count(order => order.HasSomethingToPay());
        }

        if (pages[1] is not null)
        {
            RefundsDueCount = pages[1]!.TotalCount;
        }

        StateHasChanged();
    }

    private Task<PaginationDto<OrderDto>?> GetOrdersAsync(PaymentStatus paymentStatus)
    {
        return ApiService.ExecuteAsync(() => OrderApiClient.GetOrdersAsync(
            pageNumber: 1,
            CountedOrdersPageSize,
            nameof(OrderSortBy.CreatedAtUtcAsc),
            paymentStatus));
    }

    private async Task LoadLayoutPreferencesAsync()
    {
        try
        {
            ProtectedBrowserStorageResult<bool> result =
                await ProtectedLocalStorage.GetAsync<bool>(DrawerOpenStorageKey);

            DrawerOpen = !result.Success || result.Value;
        }
        catch (CryptographicException)
        {
            await ProtectedLocalStorage.DeleteAsync(DrawerOpenStorageKey);
        }
    }

    private async Task ToggleDrawerAsync()
    {
        DrawerOpen = !DrawerOpen;

        await ProtectedLocalStorage.SetAsync(DrawerOpenStorageKey, DrawerOpen);
    }
}