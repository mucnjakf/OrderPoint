using Microsoft.AspNetCore.Components;
using MudBlazor;
using OrderPoint.Admin.Categories.Api;
using OrderPoint.Admin.Categories.Dtos;
using OrderPoint.Admin.Items.Api;
using OrderPoint.Admin.Items.Api.Requests;
using OrderPoint.Admin.Items.Dialogs;
using OrderPoint.Admin.Items.Dtos;
using OrderPoint.Admin.Items.Enumerations;
using OrderPoint.Admin.Items.Sorting;
using OrderPoint.Admin.Shared.Dtos;
using OrderPoint.Admin.Shared.Extensions;
using OrderPoint.Admin.Shared.Services;

namespace OrderPoint.Admin.Items.Pages;

public sealed partial class ItemsPage
{
    private const int PageSize = 16;

    [Inject]
    private IDialogService DialogService { get; set; } = null!;

    [Inject]
    private ApiService ApiService { get; set; } = null!;

    [Inject]
    private ItemApiClient ItemApiClient { get; set; } = null!;

    [Inject]
    private CategoryApiClient CategoryApiClient { get; set; } = null!;

    private ItemDto? NewestItem { get; set; }

    private ItemDto? LastEditedItem { get; set; }

    private ItemDto? HighestPriceItem { get; set; }

    private ItemDto? LowestPriceItem { get; set; }

    private bool IsLoadingNewestItem { get; set; } = true;

    private bool IsLoadingLastEditedItem { get; set; } = true;

    private bool IsLoadingHighestPriceItem { get; set; } = true;

    private bool IsLoadingLowestPriceItem { get; set; } = true;

    private string? NewestItemCaption => NewestItem is null
        ? null
        : $"Added {NewestItem.CreatedAtUtc.ToRelativeTime()}";

    private string? LastEditedItemCaption => LastEditedItem?.UpdatedAtUtc is null
        ? null
        : $"Edited {LastEditedItem.UpdatedAtUtc.Value.ToRelativeTime()}";

    private PaginationDto<ItemDto>? Pagination { get; set; }

    private IReadOnlyList<ItemDto> Items { get; set; } = [];

    private string SelectedSortBy { get; set; } = nameof(ItemSortBy.CreatedAtUtcDesc);

    private string? SearchQuery { get; set; }

    private CategoryDto? SelectedCategory { get; set; }

    private MudAutocomplete<CategoryDto> CategoryAutocomplete { get; set; } = null!;

    private ItemStatus? SelectedStatus { get; set; }

    private bool IsLoading { get; set; } = true;

    private HashSet<ItemDto> SelectedItems { get; set; } = [];

    private IReadOnlyList<ItemDto> UndeletableItems =>
        [.. SelectedItems.Where(item => item.OrdersCount > 0)];

    private bool IsDeleteSelectedBlocked => UndeletableItems.Count > 0;

    private string UndeletableNamesText =>
        string.Join(", ", UndeletableItems.Select(item => item.Name));

    private string DeleteSelectedTooltipText => IsDeleteSelectedBlocked
        ? $"Items with orders can't be deleted: {UndeletableNamesText}"
        : "Delete selected";

    protected override async Task OnInitializedAsync()
    {
        await Task.WhenAll(
            GetSpotlightAsync(),
            GetItemsAsync(pageNumber: 1));
    }

    private async Task GetSpotlightAsync()
    {
        await Task.WhenAll(
            GetNewestItemAsync(),
            GetLastEditedItemAsync(),
            GetHighestPriceItemAsync(),
            GetLowestPriceItemAsync());
    }

    private async Task GetNewestItemAsync()
    {
        IsLoadingNewestItem = true;

        NewestItem = await GetSpotlightItemAsync(ItemSortBy.CreatedAtUtcDesc);

        IsLoadingNewestItem = false;

        StateHasChanged();
    }

    private async Task GetLastEditedItemAsync()
    {
        IsLoadingLastEditedItem = true;

        ItemDto? item = await GetSpotlightItemAsync(ItemSortBy.UpdatedAtUtcDesc);

        LastEditedItem = item?.UpdatedAtUtc is null ? null : item;

        IsLoadingLastEditedItem = false;

        StateHasChanged();
    }

    private async Task GetHighestPriceItemAsync()
    {
        IsLoadingHighestPriceItem = true;

        HighestPriceItem = await GetSpotlightItemAsync(ItemSortBy.PriceDesc);

        IsLoadingHighestPriceItem = false;

        StateHasChanged();
    }

    private async Task GetLowestPriceItemAsync()
    {
        IsLoadingLowestPriceItem = true;

        LowestPriceItem = await GetSpotlightItemAsync(ItemSortBy.PriceAsc);

        IsLoadingLowestPriceItem = false;

        StateHasChanged();
    }

    private async Task<ItemDto?> GetSpotlightItemAsync(ItemSortBy sortBy)
    {
        PaginationDto<ItemDto>? pagination = await ApiService.ExecuteAsync(
            () => ItemApiClient.GetItemsAsync(pageNumber: 1, pageSize: 1, sortBy.ToString()));

        return pagination?.Items.FirstOrDefault();
    }

    private async Task GetItemsAsync(int pageNumber)
    {
        SelectedItems = [];

        IsLoading = true;

        Pagination = await ApiService.ExecuteAsync(() => ItemApiClient.GetItemsAsync(
            pageNumber,
            PageSize,
            SelectedSortBy,
            SearchQuery,
            SelectedCategory?.Id,
            SelectedStatus));

        Items = Pagination?.Items ?? [];

        IsLoading = false;

        StateHasChanged();
    }

    private async Task OnSearchChangedAsync()
    {
        await GetItemsAsync(pageNumber: 1);
    }

    private async Task OnSortChangedAsync()
    {
        await GetItemsAsync(pageNumber: 1);
    }

    private async Task OnCategoryChangedAsync()
    {
        await GetItemsAsync(pageNumber: 1);
    }

    private async Task OnStatusChangedAsync()
    {
        await GetItemsAsync(pageNumber: 1);
    }

    private async Task OnPageChangedAsync(int pageNumber)
    {
        await GetItemsAsync(pageNumber);
    }

    private async Task<IEnumerable<CategoryDto>> OnCategorySearchAsync(
        string? value,
        CancellationToken cancellationToken)
    {
        IReadOnlyList<CategoryDto>? categories = await ApiService.ExecuteAsync(
            () => CategoryApiClient.SearchCategoriesAsync(value, cancellationToken),
            cancellationToken);

        return categories ?? [];
    }

    private async Task OnCategoryClearedAsync()
    {
        await CategoryAutocomplete.OpenMenuAsync();
    }

    private async Task ShowCreateItemDialogAsync()
    {
        var options = new DialogOptions
        {
            MaxWidth = MaxWidth.Medium,
            FullWidth = true
        };

        IDialogReference dialogReference = await DialogService
            .ShowAsync<CreateItemDialog>(string.Empty, options);

        DialogResult dialogResult = (await dialogReference.Result)!;

        if (dialogResult.Canceled)
        {
            return;
        }

        var request = (dialogResult.Data as CreateItemRequest)!;

        bool isSuccess = await ApiService.ExecuteAsync(
            () => CreateItemWithImageAsync(request),
            $"Item {request.Name} created successfully");

        if (isSuccess)
        {
            await Task.WhenAll(
                GetSpotlightAsync(),
                GetItemsAsync(pageNumber: 1));
        }
    }

    private async Task CreateItemWithImageAsync(CreateItemRequest request)
    {
        ItemDto item = await ItemApiClient.CreateItemAsync(request);

        if (request.Image is null)
        {
            return;
        }

        await ItemApiClient.UpdateItemImageAsync(item.Id, request.Image);
    }

    private async Task ShowItemDetailsDialogAsync(ItemDto item)
    {
        var parameters = new DialogParameters<ItemDetailsDialog>
        {
            { dialog => dialog.Item, item }
        };

        var options = new DialogOptions
        {
            MaxWidth = MaxWidth.Medium,
            FullWidth = true
        };

        await DialogService
            .ShowAsync<ItemDetailsDialog>(string.Empty, parameters, options);
    }

    private async Task ShowUpdateItemDialogAsync(ItemDto item)
    {
        var parameters = new DialogParameters<UpdateItemDialog>
        {
            { dialog => dialog.Item, item }
        };

        var options = new DialogOptions
        {
            MaxWidth = MaxWidth.Medium,
            FullWidth = true
        };

        IDialogReference dialogReference = await DialogService
            .ShowAsync<UpdateItemDialog>(string.Empty, parameters, options);

        DialogResult dialogResult = (await dialogReference.Result)!;

        if (dialogResult.Canceled)
        {
            return;
        }

        var request = (dialogResult.Data as UpdateItemRequest)!;

        bool isSuccess = await ApiService.ExecuteAsync(
            () => UpdateItemWithImageAsync(item.Id, request),
            $"Item {request.Name} edited successfully");

        if (isSuccess)
        {
            await Task.WhenAll(
                GetSpotlightAsync(),
                GetItemsAsync(pageNumber: 1));
        }
    }

    private async Task UpdateItemWithImageAsync(Guid id, UpdateItemRequest request)
    {
        await ItemApiClient.UpdateItemAsync(id, request);

        if (request.Image is not null)
        {
            await ItemApiClient.UpdateItemImageAsync(id, request.Image);
            return;
        }

        if (request.RemoveImage)
        {
            await ItemApiClient.DeleteItemImageAsync(id);
        }
    }

    private async Task ShowDeleteItemDialogAsync(ItemDto item)
    {
        var parameters = new DialogParameters<DeleteItemDialog>
        {
            { dialog => dialog.Item, item }
        };

        var options = new DialogOptions
        {
            MaxWidth = MaxWidth.Small,
            FullWidth = true
        };

        IDialogReference dialogReference = await DialogService
            .ShowAsync<DeleteItemDialog>(string.Empty, parameters, options);

        DialogResult dialogResult = (await dialogReference.Result)!;

        if (dialogResult.Canceled)
        {
            return;
        }

        bool isSuccess = await ApiService.ExecuteAsync(
            () => ItemApiClient.DeleteItemAsync(item.Id),
            $"Item {item.Name} deleted successfully");

        if (isSuccess)
        {
            await Task.WhenAll(
                GetSpotlightAsync(),
                GetItemsAsync(pageNumber: 1));
        }
    }

    private async Task ShowChangeItemsStatusDialogAsync(ItemStatus status)
    {
        IReadOnlyList<ItemDto> items = [.. SelectedItems];

        var parameters = new DialogParameters<ChangeItemsStatusDialog>
        {
            { dialog => dialog.Items, items },
            { dialog => dialog.Status, status }
        };

        var options = new DialogOptions
        {
            MaxWidth = MaxWidth.Small,
            FullWidth = true
        };

        IDialogReference dialogReference = await DialogService
            .ShowAsync<ChangeItemsStatusDialog>(string.Empty, parameters, options);

        DialogResult dialogResult = (await dialogReference.Result)!;

        if (dialogResult.Canceled)
        {
            return;
        }

        var request = new ChangeItemsStatusRequest
        {
            Ids = [.. items.Select(item => item.Id)],
            Status = status
        };

        string countText = items.Count.ToCountText("item", "items");
        string statusText = status is ItemStatus.Active ? "activated" : "deactivated";

        bool isSuccess = await ApiService.ExecuteAsync(
            () => ItemApiClient.ChangeItemsStatusAsync(request),
            $"{countText} {statusText} successfully");

        if (isSuccess)
        {
            await Task.WhenAll(
                GetSpotlightAsync(),
                GetItemsAsync(pageNumber: 1));
        }
    }

    private async Task ShowDeleteItemsDialogAsync()
    {
        IReadOnlyList<ItemDto> items = [.. SelectedItems];

        var parameters = new DialogParameters<DeleteItemsDialog>
        {
            { dialog => dialog.Items, items }
        };

        var options = new DialogOptions
        {
            MaxWidth = MaxWidth.Small,
            FullWidth = true
        };

        IDialogReference dialogReference = await DialogService
            .ShowAsync<DeleteItemsDialog>(string.Empty, parameters, options);

        DialogResult dialogResult = (await dialogReference.Result)!;

        if (dialogResult.Canceled)
        {
            return;
        }

        var request = new DeleteItemsRequest
        {
            Ids = [.. items.Select(item => item.Id)]
        };

        string countText = items.Count.ToCountText("item", "items");

        bool isSuccess = await ApiService.ExecuteAsync(
            () => ItemApiClient.DeleteItemsAsync(request),
            $"{countText} deleted successfully");

        if (isSuccess)
        {
            await Task.WhenAll(
                GetSpotlightAsync(),
                GetItemsAsync(pageNumber: 1));
        }
    }

    private async Task ShowChangeItemsCategoryDialogAsync()
    {
        IReadOnlyList<ItemDto> items = [.. SelectedItems];

        var parameters = new DialogParameters<ChangeItemsCategoryDialog>
        {
            { dialog => dialog.Items, items }
        };

        var options = new DialogOptions
        {
            MaxWidth = MaxWidth.Small,
            FullWidth = true
        };

        IDialogReference dialogReference = await DialogService
            .ShowAsync<ChangeItemsCategoryDialog>(string.Empty, parameters, options);

        DialogResult dialogResult = (await dialogReference.Result)!;

        if (dialogResult.Canceled)
        {
            return;
        }

        var request = (dialogResult.Data as ChangeItemsCategoryRequest)!;

        string countText = request.Ids.Count.ToCountText("item", "items");

        bool isSuccess = await ApiService.ExecuteAsync(
            () => ItemApiClient.ChangeItemsCategoryAsync(request),
            $"{countText} moved to {request.CategoryName} successfully");

        if (isSuccess)
        {
            await Task.WhenAll(
                GetSpotlightAsync(),
                GetItemsAsync(pageNumber: 1));
        }
    }

    private async Task ShowAdjustItemsPriceDialogAsync()
    {
        IReadOnlyList<ItemDto> items = [.. SelectedItems];

        var parameters = new DialogParameters<AdjustItemsPriceDialog>
        {
            { dialog => dialog.Items, items }
        };

        var options = new DialogOptions
        {
            MaxWidth = MaxWidth.Small,
            FullWidth = true
        };

        IDialogReference dialogReference = await DialogService
            .ShowAsync<AdjustItemsPriceDialog>(string.Empty, parameters, options);

        DialogResult dialogResult = (await dialogReference.Result)!;

        if (dialogResult.Canceled)
        {
            return;
        }

        var request = (dialogResult.Data as AdjustItemsPriceRequest)!;

        string countText = request.Ids.Count.ToCountText("item", "items");

        bool isSuccess = await ApiService.ExecuteAsync(
            () => ItemApiClient.AdjustItemsPriceAsync(request),
            $"Prices of {countText} adjusted successfully");

        if (isSuccess)
        {
            await Task.WhenAll(
                GetSpotlightAsync(),
                GetItemsAsync(pageNumber: 1));
        }
    }
}