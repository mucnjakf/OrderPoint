using System.Security.Cryptography;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using MudBlazor;
using OrderPoint.Admin.Bartenders.Api;
using OrderPoint.Admin.Bartenders.Dialogs;
using OrderPoint.Admin.Bartenders.Dtos;
using OrderPoint.Admin.Categories.Api;
using OrderPoint.Admin.Categories.Dialogs;
using OrderPoint.Admin.Categories.Dtos;
using OrderPoint.Admin.Items.Api;
using OrderPoint.Admin.Items.Dialogs;
using OrderPoint.Admin.Items.Dtos;
using OrderPoint.Admin.Orders.Api;
using OrderPoint.Admin.Orders.Dialogs;
using OrderPoint.Admin.Search.Dtos;
using OrderPoint.Admin.Search.Enumerations;
using OrderPoint.Shared.Orders.Dtos;
using OrderPoint.Shared.Services;

namespace OrderPoint.Admin.Search.Services;

internal sealed class SearchService(
    ProtectedLocalStorage protectedLocalStorage,
    IDialogService dialogService,
    NavigationManager navigationManager,
    ApiService apiService,
    ItemApiClient itemApiClient,
    CategoryApiClient categoryApiClient,
    BartenderApiClient bartenderApiClient,
    OrderApiClient orderApiClient)
{
    private const string RecentResultsStorageKey = "recent-search-results";

    private const int MaxRecentResultsCount = 5;

    private static readonly DialogOptions DetailsDialogOptions = new()
    {
        MaxWidth = MaxWidth.Medium,
        FullWidth = true
    };

    internal async Task<IReadOnlyList<SearchResultDto>> GetRecentResultsAsync()
    {
        try
        {
            ProtectedBrowserStorageResult<List<SearchResultDto>> result =
                await protectedLocalStorage.GetAsync<List<SearchResultDto>>(RecentResultsStorageKey);

            return result.Success && result.Value is not null ? result.Value : [];
        }
        catch (CryptographicException)
        {
            await protectedLocalStorage.DeleteAsync(RecentResultsStorageKey);

            return [];
        }
    }

    internal async Task OpenAsync(SearchResultDto result)
    {
        if (result.Kind is SearchResultKind.Page)
        {
            navigationManager.NavigateTo(result.Href!);
            return;
        }

        await AddRecentResultAsync(result);

        Guid id = result.Id!.Value;

        switch (result.Kind)
        {
            case SearchResultKind.Order:
                await ShowOrderAsync(id);
                break;
            case SearchResultKind.Item:
                await ShowItemAsync(id);
                break;
            case SearchResultKind.Category:
                await ShowCategoryAsync(id);
                break;
            case SearchResultKind.Bartender:
                await ShowBartenderAsync(id);
                break;
            case SearchResultKind.Table:
                navigationManager.NavigateTo(result.Href!);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(result), result.Kind, null);
        }
    }

    private async Task AddRecentResultAsync(SearchResultDto result)
    {
        IReadOnlyList<SearchResultDto> recentResults = await GetRecentResultsAsync();

        List<SearchResultDto> updatedResults =
        [
            result,
            .. recentResults.Where(recentResult => recentResult.Id != result.Id).Take(MaxRecentResultsCount - 1)
        ];

        await protectedLocalStorage.SetAsync(RecentResultsStorageKey, updatedResults);
    }

    private async Task ShowOrderAsync(Guid id)
    {
        OrderDto? order = await apiService.ExecuteAsync(() => orderApiClient.GetOrderAsync(id));

        if (order is null)
        {
            return;
        }

        var parameters = new DialogParameters<OrderDetailsDialog>
        {
            { dialog => dialog.Order, order }
        };

        await dialogService.ShowAsync<OrderDetailsDialog>(string.Empty, parameters, DetailsDialogOptions);
    }

    private async Task ShowItemAsync(Guid id)
    {
        ItemDto? item = await apiService.ExecuteAsync(() => itemApiClient.GetItemAsync(id));

        if (item is null)
        {
            return;
        }

        var parameters = new DialogParameters<ItemDetailsDialog>
        {
            { dialog => dialog.Item, item }
        };

        await dialogService.ShowAsync<ItemDetailsDialog>(string.Empty, parameters, DetailsDialogOptions);
    }

    private async Task ShowCategoryAsync(Guid id)
    {
        CategoryDto? category = await apiService.ExecuteAsync(() => categoryApiClient.GetCategoryAsync(id));

        if (category is null)
        {
            return;
        }

        var parameters = new DialogParameters<CategoryDetailsDialog>
        {
            { dialog => dialog.Category, category }
        };

        await dialogService.ShowAsync<CategoryDetailsDialog>(string.Empty, parameters, DetailsDialogOptions);
    }

    private async Task ShowBartenderAsync(Guid id)
    {
        BartenderDto? bartender = await apiService.ExecuteAsync(() => bartenderApiClient.GetBartenderAsync(id));

        if (bartender is null)
        {
            return;
        }

        var parameters = new DialogParameters<BartenderOrdersDialog>
        {
            { dialog => dialog.Bartender, bartender }
        };

        await dialogService.ShowAsync<BartenderOrdersDialog>(string.Empty, parameters, DetailsDialogOptions);
    }
}