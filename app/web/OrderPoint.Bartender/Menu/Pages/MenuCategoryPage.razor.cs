using Microsoft.AspNetCore.Components;
using MudBlazor;
using OrderPoint.Bartender.Menu.Api;
using OrderPoint.Bartender.Menu.Dialogs;
using OrderPoint.Bartender.Menu.Dtos;
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
    private IDialogService DialogService { get; set; } = null!;

    [Parameter]
    public Guid CategoryId { get; set; }

    private CategoryDto? Category { get; set; }

    private bool IsLoadingCategory { get; set; } = true;

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

    private async Task GetCategoryAsync()
    {
        Category = await ApiService.ExecuteAsync(() => MenuApiClient.GetCategoryAsync(CategoryId));

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

    private async Task ShowItemDialogAsync(ItemDto item)
    {
        var parameters = new DialogParameters<MenuItemDialog>
        {
            { dialog => dialog.Item, item }
        };

        var options = new DialogOptions
        {
            MaxWidth = MaxWidth.Small,
            FullWidth = true
        };

        await DialogService.ShowAsync<MenuItemDialog>(string.Empty, parameters, options);
    }
}