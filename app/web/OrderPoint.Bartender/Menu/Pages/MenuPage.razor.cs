using Microsoft.AspNetCore.Components;
using MudBlazor;
using OrderPoint.Bartender.Menu.Api;
using OrderPoint.Bartender.Menu.Dialogs;
using OrderPoint.Bartender.Menu.Dtos;
using OrderPoint.Shared.Dtos;
using OrderPoint.Shared.Services;

namespace OrderPoint.Bartender.Menu.Pages;

// The menu as the admin set it up, to answer guests; it changes only in the Admin, so it is not live
public sealed partial class MenuPage
{
    private const int PageSize = 20;

    private const int MaxCategoriesCount = 100;

    private const int LoadingChipsCount = 4;

    private const int LoadingRowsCount = 3;

    // Search and categories stay right under the dense app bar while scrolling, like the order tabs
    private const string FiltersWrapperStyle =
        "position: sticky; top: 48px; z-index: 2; padding: 12px 0; margin-top: -12px; " +
        "background-color: var(--mud-palette-background);";

    private const string ChipsRowStyle = "display: flex; gap: 8px; overflow-x: auto; scrollbar-width: none;";

    [Inject]
    private ApiService ApiService { get; set; } = null!;

    [Inject]
    private MenuApiClient MenuApiClient { get; set; } = null!;

    [Inject]
    private IDialogService DialogService { get; set; } = null!;

    private IReadOnlyList<CategoryDto> Categories { get; set; } = [];

    private bool IsLoadingCategories { get; set; } = true;

    private List<ItemDto> Items { get; set; } = [];

    private PaginationDto<ItemDto>? LastItemsPage { get; set; }

    private bool IsLoadingItems { get; set; } = true;

    private bool IsLoadingMoreItems { get; set; }

    private string? SearchQuery { get; set; }

    private Guid? SelectedCategoryId { get; set; }

    private bool HasMoreItems => LastItemsPage?.HasNextPage ?? false;

    protected override async Task OnInitializedAsync()
    {
        await Task.WhenAll(GetCategoriesAsync(), GetItemsAsync());
    }

    private Variant GetChipVariant(Guid? categoryId)
    {
        return categoryId == SelectedCategoryId ? Variant.Filled : Variant.Outlined;
    }

    private async Task GetCategoriesAsync()
    {
        PaginationDto<CategoryDto>? categories =
            await ApiService.ExecuteAsync(() => MenuApiClient.GetCategoriesAsync(MaxCategoriesCount));

        Categories = categories?.Items ?? [];

        IsLoadingCategories = false;

        StateHasChanged();
    }

    private async Task GetItemsAsync()
    {
        IsLoadingItems = true;

        PaginationDto<ItemDto>? items = await ApiService.ExecuteAsync(
            () => MenuApiClient.GetItemsAsync(1, PageSize, SearchQuery, SelectedCategoryId));

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
            () => MenuApiClient.GetItemsAsync(nextPageNumber, PageSize, SearchQuery, SelectedCategoryId));

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

    private async Task SelectCategoryAsync(Guid? categoryId)
    {
        if (categoryId == SelectedCategoryId)
        {
            return;
        }

        SelectedCategoryId = categoryId;

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