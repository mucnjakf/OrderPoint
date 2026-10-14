using Microsoft.AspNetCore.Components;
using OrderPoint.Bartender.Menu.Api;
using OrderPoint.Bartender.Menu.Api.Requests;
using OrderPoint.Bartender.Menu.Dtos;
using OrderPoint.Bartender.Menu.Enumerations;
using OrderPoint.Shared.Orders.Services;
using OrderPoint.Shared.Services;

namespace OrderPoint.Bartender.Menu.Pages;

// Under its category's route, so the back arrow returns to that category
public sealed partial class MenuItemPage : IDisposable
{
    [Inject]
    private ApiService ApiService { get; set; } = null!;

    [Inject]
    private MenuApiClient MenuApiClient { get; set; } = null!;

    [Inject]
    private OrdersHubService OrdersHubService { get; set; } = null!;

    [Parameter]
    public Guid CategoryId { get; set; }

    [Parameter]
    public Guid ItemId { get; set; }

    private ItemDto? Item { get; set; }

    private bool IsLoading { get; set; } = true;

    private bool IsChangingStatus { get; set; }

    protected override async Task OnInitializedAsync()
    {
        OrdersHubService.MenuChanged += OnMenuChanged;

        await GetItemAsync();

        IsLoading = false;
    }

    public void Dispose()
    {
        OrdersHubService.MenuChanged -= OnMenuChanged;
    }

    private void OnMenuChanged()
    {
        _ = InvokeAsync(async () =>
        {
            await GetItemAsync();

            StateHasChanged();
        });
    }

    // A failed reload keeps what is shown
    private async Task GetItemAsync()
    {
        ItemDto? item = await ApiService.ExecuteAsync(() => MenuApiClient.GetItemAsync(ItemId));

        if (item is not null || IsLoading)
        {
            Item = item;
        }
    }

    // Takes the item off the menu when it runs out (and back on); easy to undo, so no confirmation
    private async Task ChangeStatusAsync(bool isActive)
    {
        var request = new ChangeItemsStatusRequest
        {
            Ids = [ItemId],
            Status = isActive ? ItemStatus.Active : ItemStatus.Inactive
        };

        IsChangingStatus = true;

        bool isSuccess = await ApiService.ExecuteAsync(
            () => MenuApiClient.ChangeItemsStatusAsync(request),
            $"{Item!.Name} {(isActive ? "activated" : "deactivated")}");

        if (isSuccess)
        {
            await GetItemAsync();
        }

        IsChangingStatus = false;
    }
}