using Microsoft.AspNetCore.Components;
using MudBlazor;
using OrderPoint.Customer.Cart.Services;
using OrderPoint.Customer.Menu.Dtos;
using OrderPoint.Customer.Menu.Services;
using OrderPoint.Customer.Orders.Api;
using OrderPoint.Customer.Orders.Dialogs;
using OrderPoint.Customer.Orders.Dtos;
using OrderPoint.Customer.Orders.Services;
using OrderPoint.Shared.Orders.Dtos;
using OrderPoint.Shared.Services;

namespace OrderPoint.Customer.Orders.Pages;

// The orders placed on this phone during the visit, newest first and live
public sealed partial class OrdersPage : IDisposable
{
    private const int LoadingCardsCount = 2;

    [Inject]
    private ApiService ApiService { get; set; } = null!;

    [Inject]
    private OrderApiClient OrderApiClient { get; set; } = null!;

    [Inject]
    private MyOrdersService MyOrdersService { get; set; } = null!;

    [Inject]
    private OrderTrackingHubService OrderTrackingHubService { get; set; } = null!;

    [Inject]
    private MenuService MenuService { get; set; } = null!;

    [Inject]
    private CartService CartService { get; set; } = null!;

    [Inject]
    private ISnackbar Snackbar { get; set; } = null!;

    [Inject]
    private IDialogService DialogService { get; set; } = null!;

    [Inject]
    private NavigationManager NavigationManager { get; set; } = null!;

    private IReadOnlyList<OrderTrackingDto> Orders { get; set; } = [];

    private bool IsLoading { get; set; } = true;

    private Guid? CancellingOrderId { get; set; }

    protected override async Task OnInitializedAsync()
    {
        OrderTrackingHubService.OrdersChanged += OnOrdersChanged;
        MyOrdersService.Changed += OnOrdersChanged;

        await GetOrdersAsync();
    }

    public void Dispose()
    {
        OrderTrackingHubService.OrdersChanged -= OnOrdersChanged;
        MyOrdersService.Changed -= OnOrdersChanged;
    }

    private void OnOrdersChanged()
    {
        _ = InvokeAsync(GetOrdersAsync);
    }

    // One call per order, in parallel; an order that no longer exists is left out
    private async Task GetOrdersAsync()
    {
        OrderTrackingDto?[] orders = await Task.WhenAll(MyOrdersService.OrderIds.Select(orderId =>
            ApiService.ExecuteAsync(() => OrderApiClient.GetOrderTrackingAsync(orderId))));

        Orders = orders.OfType<OrderTrackingDto>().ToList();

        IsLoading = false;

        StateHasChanged();
    }

    private void OpenOrder(OrderTrackingDto order)
    {
        NavigationManager.NavigateTo($"/orders/{order.Id}");
    }

    // Puts the lines that are still on the menu into the cart, at today's prices
    private async Task OrderAgainAsync(OrderTrackingDto order)
    {
        IReadOnlyList<MenuCategoryDto> menu = await MenuService.GetMenuAsync() ?? [];

        Dictionary<Guid, Guid> categoryIdsByItemId = menu
            .SelectMany(category => category.Items, (category, item) => (ItemId: item.Id, CategoryId: category.Id))
            .ToDictionary(pair => pair.ItemId, pair => pair.CategoryId);

        int addedCount = 0;

        foreach (OrderItemDto orderItem in order.Items)
        {
            if (!categoryIdsByItemId.TryGetValue(orderItem.ItemId, out Guid categoryId))
            {
                continue;
            }

            await CartService.AddAsync(orderItem.ItemId, categoryId, orderItem.Name, orderItem.Quantity);

            addedCount++;
        }

        if (addedCount == 0)
        {
            Snackbar.Add("None of these items are on the menu any more.", Severity.Warning);

            return;
        }

        if (addedCount < order.Items.Count)
        {
            Snackbar.Add("Some items are no longer on the menu, so they weren't added.", Severity.Warning);
        }

        NavigationManager.NavigateTo("/cart");
    }

    // Refused when a bartender took the order first; the reload then shows what happened
    private async Task ShowCancelOrderDialogAsync(OrderTrackingDto order)
    {
        var parameters = new DialogParameters<CancelOrderDialog>
        {
            { dialog => dialog.OrderNumber, order.Number }
        };

        var options = new DialogOptions
        {
            MaxWidth = MaxWidth.ExtraSmall,
            FullWidth = true
        };

        IDialogReference dialogReference = await DialogService
            .ShowAsync<CancelOrderDialog>(string.Empty, parameters, options);

        DialogResult dialogResult = (await dialogReference.Result)!;

        if (dialogResult.Canceled)
        {
            return;
        }

        CancellingOrderId = order.Id;

        await ApiService.ExecuteAsync(
            () => OrderApiClient.CancelOrderAsync(order.Id),
            $"Order #{order.Number} cancelled");

        CancellingOrderId = null;

        await GetOrdersAsync();
    }
}