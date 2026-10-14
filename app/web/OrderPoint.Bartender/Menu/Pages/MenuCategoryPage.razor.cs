using Microsoft.AspNetCore.Components;
using OrderPoint.Bartender.Menu.Api;
using OrderPoint.Bartender.Menu.Api.Requests;
using OrderPoint.Bartender.Menu.Dtos;
using OrderPoint.Bartender.Menu.Enumerations;
using OrderPoint.Shared.Dtos;
using OrderPoint.Shared.Services;

namespace OrderPoint.Bartender.Menu.Pages;

// One category's items, opened from the menu's category grid
public sealed partial class MenuCategoryPage
{
    private const int PageSize = 20;

    private const int LoadingRowsCount = 3;

    // The search stays right under the dense app bar while scrolling, like the order tabs
    private const string SearchWrapperStyle =
        "position: sticky; top: 48px; z-index: 2; padding: 12px 0; margin-top: -12px; " +
        "background-color: var(--mud-palette-background);";

    [Inject]
    private ApiService ApiService { get; set; } = null!;

    [Inject]
    private MenuApiClient MenuApiClient { get; set; } = null!;

    [Inject]
    private NavigationManager NavigationManager { get; set; } = null!;

    [Parameter]
    public Guid CategoryId { get; set; }

    private CategoryDto? Category { get; set; }

    private bool IsLoadingCategory { get; set; } = true;

    private bool IsChangingStatus { get; set; }

    private List<ItemDto> Items { get; set; } = [];

    private PaginationDto<ItemDto>? LastItemsPage { get; set; }

    private bool IsLoadingItems { get; set; } = true;

    private bool IsLoadingMoreItems { get; set; }

    private string? SearchQuery { get; set; }

    private bool HasMoreItems => LastItemsPage?.HasNextPage ?? false;

    private string EmptyStateDetail => string.IsNullOrWhiteSpace(SearchQuery)
        ? "This category has no items yet."
        : "Try another search.";

    protected override async Task OnInitializedAsync()
    {
        await Task.WhenAll(GetCategoryAsync(), GetItemsAsync());
    }

    // A failed reload keeps what is shown
    private async Task GetCategoryAsync()
    {
        CategoryDto? category = await ApiService.ExecuteAsync(() => MenuApiClient.GetCategoryAsync(CategoryId));

        if (category is not null || IsLoadingCategory)
        {
            Category = category;
        }

        IsLoadingCategory = false;

        StateHasChanged();
    }

    private async Task GetItemsAsync()
    {
        IsLoadingItems = true;

        PaginationDto<ItemDto>? items = await ApiService.ExecuteAsync(
            () => MenuApiClient.GetItemsAsync(1, PageSize, SearchQuery, CategoryId));

        Items = items?.Items.ToList() ?? [];

        LastItemsPage = items;

        IsLoadingItems = false;

        StateHasChanged();
    }

    private async Task ShowMoreItemsAsync()
    {
        IsLoadingMoreItems = true;

        int nextPageNumber = LastItemsPage!.PageNumber + 1;

        PaginationDto<ItemDto>? items = await ApiService.ExecuteAsync(
            () => MenuApiClient.GetItemsAsync(nextPageNumber, PageSize, SearchQuery, CategoryId));

        if (items is not null)
        {
            Items.AddRange(items.Items);

            LastItemsPage = items;
        }

        IsLoadingMoreItems = false;
    }

    private async Task OnSearchChangedAsync()
    {
        await GetItemsAsync();
    }

    // Takes the whole category off the menu (and back on); its items keep their own status
    private async Task ChangeStatusAsync(bool isActive)
    {
        CategoryStatus status = isActive ? CategoryStatus.Active : CategoryStatus.Inactive;

        var request = new ChangeCategoriesStatusRequest
        {
            Ids = [CategoryId],
            Status = status
        };

        IsChangingStatus = true;

        bool isSuccess = await ApiService.ExecuteAsync(
            () => MenuApiClient.ChangeCategoriesStatusAsync(request),
            $"{Category!.Name} {(isActive ? "activated" : "deactivated")}");

        if (isSuccess)
        {
            await GetCategoryAsync();

            // The loaded items show the new category status too, without reloading and losing "Show more"
            Items = Items.Select(item => item with { Category = item.Category with { Status = status } }).ToList();
        }

        IsChangingStatus = false;
    }

    private void OpenItem(ItemDto item)
    {
        NavigationManager.NavigateTo($"/menu/{CategoryId}/items/{item.Id}");
    }
}