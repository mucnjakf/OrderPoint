using Microsoft.AspNetCore.Components;
using OrderPoint.Bartender.Orders.Enumerations;
using OrderPoint.Bartender.Orders.Services;
using OrderPoint.Bartender.Shift.Api;
using OrderPoint.Bartender.Shift.Dtos;
using OrderPoint.Shared.Orders.Services;
using OrderPoint.Shared.Services;

namespace OrderPoint.Bartender.Shift.Pages;

// Today's figures, live; the order tiles open the matching tab of the Orders page
public sealed partial class ShiftPage : IDisposable
{
    private const int TilesCount = 4;

    [Inject]
    private ApiService ApiService { get; set; } = null!;

    [Inject]
    private ShiftApiClient ShiftApiClient { get; set; } = null!;

    [Inject]
    private OrdersHubService OrdersHubService { get; set; } = null!;

    [Inject]
    private OrdersTabState OrdersTabState { get; set; } = null!;

    [Inject]
    private NavigationManager NavigationManager { get; set; } = null!;

    private ShiftDto? Shift { get; set; }

    private bool IsLoading { get; set; } = true;

    private string ServiceTimeText => Shift?.AverageServiceMinutes is null
        ? "-"
        : $"{Shift.AverageServiceMinutes.Value:0} min";

    protected override async Task OnInitializedAsync()
    {
        OrdersHubService.OrdersChanged += OnOrdersChanged;

        await GetShiftAsync();
    }

    public void Dispose()
    {
        OrdersHubService.OrdersChanged -= OnOrdersChanged;
    }

    // A failed reload keeps the figures shown
    private void OnOrdersChanged()
    {
        _ = InvokeAsync(GetShiftAsync);
    }

    private async Task GetShiftAsync()
    {
        ShiftDto? shift = await ApiService.ExecuteAsync(() => ShiftApiClient.GetShiftAsync());

        Shift = shift ?? Shift;

        IsLoading = false;

        StateHasChanged();
    }

    private void OpenOrders(OrdersTab tab)
    {
        OrdersTabState.Tab = tab;

        NavigationManager.NavigateTo("/");
    }
}