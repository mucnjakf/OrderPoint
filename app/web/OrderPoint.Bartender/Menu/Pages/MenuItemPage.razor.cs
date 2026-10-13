using Microsoft.AspNetCore.Components;
using OrderPoint.Bartender.Menu.Api;
using OrderPoint.Bartender.Menu.Dtos;
using OrderPoint.Shared.Services;

namespace OrderPoint.Bartender.Menu.Pages;

// Under its category's route, so the back arrow returns to that category
public sealed partial class MenuItemPage
{
    [Inject]
    private ApiService ApiService { get; set; } = null!;

    [Inject]
    private MenuApiClient MenuApiClient { get; set; } = null!;

    [Parameter]
    public Guid CategoryId { get; set; }

    [Parameter]
    public Guid ItemId { get; set; }

    private ItemDto? Item { get; set; }

    private bool IsLoading { get; set; } = true;

    protected override async Task OnInitializedAsync()
    {
        Item = await ApiService.ExecuteAsync(() => MenuApiClient.GetItemAsync(ItemId));

        IsLoading = false;
    }
}