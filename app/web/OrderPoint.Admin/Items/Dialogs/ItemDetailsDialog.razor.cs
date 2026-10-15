using Microsoft.AspNetCore.Components;
using MudBlazor;
using OrderPoint.Admin.Items.Api;
using OrderPoint.Admin.Items.Api.Requests;
using OrderPoint.Admin.Items.Dtos;
using OrderPoint.Admin.Orders.Api;
using OrderPoint.Admin.Orders.Sorting;
using OrderPoint.Shared.Dtos;
using OrderPoint.Shared.Extensions;
using OrderPoint.Shared.Orders.Dtos;
using OrderPoint.Shared.Orders.Services;
using OrderPoint.Shared.Services;

namespace OrderPoint.Admin.Items.Dialogs;

public sealed partial class ItemDetailsDialog : IDisposable
{
    private const int PageSize = 5;

    [Parameter]
    public ItemDto Item { get; set; } = null!;

    [CascadingParameter]
    private IMudDialogInstance MudDialogInstance { get; set; } = null!;

    [Inject]
    private TimeZoneService TimeZoneService { get; set; } = null!;

    [Inject]
    private ApiService ApiService { get; set; } = null!;

    [Inject]
    private OrderApiClient OrderApiClient { get; set; } = null!;

    [Inject]
    private ItemApiClient ItemApiClient { get; set; } = null!;

    [Inject]
    private OrdersHubService OrdersHubService { get; set; } = null!;

    [Inject]
    private IDialogService DialogService { get; set; } = null!;

    // The parameter is reset whenever the dialog provider re-renders, so live updates go here
    private ItemDto CurrentItem { get; set; } = null!;

    private int OrdersCount { get; set; }

    // Null while loading
    private ItemRecipeDto? Recipe { get; set; }

    private PaginationDto<OrderDto>? Pagination { get; set; }

    private IReadOnlyList<OrderDto> Orders { get; set; } = [];

    private string UpdatedAtText => CurrentItem.UpdatedAtUtc?.ToDisplayDateTime(TimeZoneService.TimeZone) ?? "-";

    private string SelectedSortBy { get; set; } = nameof(OrderSortBy.CreatedAtUtcDesc);

    private string? SearchQuery { get; set; }

    private bool IsLoading { get; set; } = true;

    protected override async Task OnInitializedAsync()
    {
        CurrentItem = Item;
        OrdersCount = Item.OrdersCount;

        OrdersHubService.OrdersChanged += OnOrdersChanged;
        OrdersHubService.MenuChanged += OnMenuChanged;

        await Task.WhenAll(
            GetRecipeAsync(),
            GetOrdersAsync(pageNumber: 1));
    }

    public void Dispose()
    {
        OrdersHubService.OrdersChanged -= OnOrdersChanged;
        OrdersHubService.MenuChanged -= OnMenuChanged;
    }

    private void OnOrdersChanged()
    {
        _ = InvokeAsync(() => Task.WhenAll(
            GetItemAsync(),
            GetRecipeAsync(),
            GetOrdersAsync(Pagination?.PageNumber ?? 1, isLiveUpdate: true)));
    }

    // A completed order or a stock change may have sold an ingredient out
    private void OnMenuChanged()
    {
        _ = InvokeAsync(() => Task.WhenAll(GetItemAsync(), GetRecipeAsync()));
    }

    // A failed reload keeps what is shown
    private async Task GetRecipeAsync()
    {
        ItemRecipeDto? recipe = await ApiService.ExecuteAsync(() => ItemApiClient.GetItemRecipeAsync(Item.Id));

        if (recipe is not null)
        {
            Recipe = recipe;
        }

        StateHasChanged();
    }

    private async Task ShowEditRecipeDialogAsync()
    {
        var parameters = new DialogParameters<EditRecipeDialog>
        {
            { dialog => dialog.Item, CurrentItem },
            { dialog => dialog.Recipe, Recipe! }
        };

        var options = new DialogOptions
        {
            MaxWidth = MaxWidth.Medium,
            FullWidth = true
        };

        IDialogReference dialogReference = await DialogService
            .ShowAsync<EditRecipeDialog>(string.Empty, parameters, options);

        DialogResult dialogResult = (await dialogReference.Result)!;

        if (dialogResult.Canceled)
        {
            return;
        }

        var request = (dialogResult.Data as SetItemRecipeRequest)!;

        bool isSuccess = await ApiService.ExecuteAsync(
            () => ItemApiClient.SetItemRecipeAsync(Item.Id, request),
            $"Recipe for {CurrentItem.Name} saved");

        if (isSuccess)
        {
            await Task.WhenAll(GetItemAsync(), GetRecipeAsync());
        }
    }

    // Its orders count and, after a menu change, everything else; a failed reload keeps what is shown
    private async Task GetItemAsync()
    {
        ItemDto? item = await ApiService.ExecuteAsync(() => ItemApiClient.GetItemAsync(Item.Id));

        if (item is not null)
        {
            CurrentItem = item;
            OrdersCount = item.OrdersCount;
        }

        StateHasChanged();
    }

    private async Task GetOrdersAsync(int pageNumber, bool isLiveUpdate = false)
    {
        if (!isLiveUpdate)
        {
            IsLoading = true;
        }

        Pagination = await ApiService.ExecuteAsync(() => OrderApiClient.GetOrdersAsync(
            pageNumber,
            PageSize,
            SelectedSortBy,
            SearchQuery,
            itemId: Item.Id));

        Orders = Pagination?.Items ?? [];

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

    private async Task OnPageChangedAsync(int pageNumber)
    {
        await GetOrdersAsync(pageNumber);
    }

    private OrderItemDto GetOrderItem(OrderDto order)
    {
        return order.Items.Single(orderItem => orderItem.ItemId == Item.Id);
    }

    private void Close()
    {
        MudDialogInstance.Close(DialogResult.Ok(true));
    }
}