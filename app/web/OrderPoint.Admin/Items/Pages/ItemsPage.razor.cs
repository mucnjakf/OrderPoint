using Microsoft.AspNetCore.Components;
using MudBlazor;
using OrderPoint.Admin.Categories.Api;
using OrderPoint.Admin.Categories.Dtos;
using OrderPoint.Admin.Items.Api;
using OrderPoint.Admin.Items.Api.Requests;
using OrderPoint.Admin.Items.Dialogs;
using OrderPoint.Admin.Items.Dtos;
using OrderPoint.Admin.Items.Sorting;
using OrderPoint.Admin.Shared.Dtos;
using OrderPoint.Admin.Shared.Services;

namespace OrderPoint.Admin.Items.Pages;

public sealed partial class ItemsPage
{
    private const int PageSize = 9;

    [Inject]
    private IDialogService DialogService { get; set; } = null!;

    [Inject]
    private ApiService ApiService { get; set; } = null!;

    [Inject]
    private ItemApiClient ItemApiClient { get; set; } = null!;

    [Inject]
    private CategoryApiClient CategoryApiClient { get; set; } = null!;

    private List<BreadcrumbItem> Breadcrumbs { get; set; } =
    [
        new("Dashboard", href: "/", icon: Icons.Material.Filled.Dashboard),
        new("Items", href: null, disabled: true, icon: Icons.Material.Filled.MenuBook)
    ];

    private PaginationDto<ItemDto>? Pagination { get; set; }

    private IReadOnlyList<ItemDto> Items { get; set; } = [];

    private string SelectedSortBy { get; set; } = nameof(ItemSortBy.CreatedAtUtcDesc);

    private string? SearchQuery { get; set; }

    private CategoryDto? SelectedCategory { get; set; }

    private bool IsLoading { get; set; } = true;

    protected override async Task OnInitializedAsync()
    {
        await GetItemsAsync(pageNumber: 1);
    }

    private async Task GetItemsAsync(int pageNumber)
    {
        IsLoading = true;

        Pagination = await ApiService.ExecuteAsync(() => ItemApiClient.GetItemsAsync(
            pageNumber,
            PageSize,
            SelectedSortBy,
            SearchQuery,
            SelectedCategory?.Id));

        Items = Pagination?.Items ?? [];

        IsLoading = false;
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
            () => ItemApiClient.CreateItemAsync(request),
            $"Item {request.Name} created successfully");

        if (isSuccess)
        {
            await GetItemsAsync(pageNumber: 1);
        }
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
            () => ItemApiClient.UpdateItemAsync(item.Id, request),
            $"Item {request.Name} edited successfully");

        if (isSuccess)
        {
            await GetItemsAsync(pageNumber: 1);
        }
    }

    private async Task ShowDeleteItemDialogAsync(Guid id, string itemName)
    {
        var parameters = new DialogParameters<DeleteItemDialog>
        {
            { dialog => dialog.ItemName, itemName }
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
            () => ItemApiClient.DeleteItemAsync(id),
            $"Item {itemName} deleted successfully");

        if (isSuccess)
        {
            await GetItemsAsync(pageNumber: 1);
        }
    }
}