using Microsoft.AspNetCore.Components;
using MudBlazor;
using OrderPoint.Admin.Categories.Api;
using OrderPoint.Admin.Categories.Dtos;
using OrderPoint.Admin.Items.Api;
using OrderPoint.Admin.Items.Dialogs;
using OrderPoint.Admin.Items.Dtos;
using OrderPoint.Admin.Shared.Dtos;
using OrderPoint.Admin.Shared.Services;

namespace OrderPoint.Admin.Items.Pages.Main;

public sealed partial class ItemsPage
{
    [Inject]
    private IDialogService DialogService { get; set; } = null!;

    [Inject]
    private ISnackbar Snackbar { get; set; } = null!;

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

    private string SelectedSortBy { get; set; } = "CreatedAtUtcDesc";

    private string? SearchQuery { get; set; }

    private CategoryDto? SelectedCategory { get; set; }

    private bool IsLoading { get; set; } = true;

    protected override async Task OnInitializedAsync()
    {
        await GetItemsAsync(
            1,
            9,
            SelectedSortBy,
            SearchQuery,
            SelectedCategory?.Id);
    }

    private async Task GetItemsAsync(
        int pageNumber,
        int pageSize,
        string sortBy,
        string? searchQuery,
        Guid? categoryId)
    {
        IsLoading = true;

        Pagination = await ApiService.ExecuteAsync(async ()
            => await ItemApiClient.GetItemsAsync(
                pageNumber,
                pageSize,
                sortBy,
                searchQuery,
                categoryId));

        Items = Pagination.Items;

        IsLoading = false;
    }

    private async Task OnSearchChangedAsync()
    {
        await GetItemsAsync(
            1,
            9,
            SelectedSortBy,
            SearchQuery,
            SelectedCategory?.Id);
    }

    private async Task OnSortChangedAsync()
    {
        await GetItemsAsync(
            1,
            9,
            SelectedSortBy,
            SearchQuery,
            SelectedCategory?.Id);
    }

    private async Task<IEnumerable<CategoryDto>>? OnCategorySearchAsync(string? value,
        CancellationToken cancellationToken)
        => await CategoryApiClient.SearchCategoriesAsync(value, cancellationToken);

    private async Task OnCategoryChangedAsync()
    {
        await GetItemsAsync(
            1,
            9,
            SelectedSortBy,
            SearchQuery,
            SelectedCategory?.Id);
    }

    private async Task OnPageChanged(int pageNumber)
    {
        await GetItemsAsync(
            pageNumber,
            9,
            SelectedSortBy,
            SearchQuery,
            SelectedCategory?.Id);
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

        if (!dialogResult.Canceled)
        {
            await ApiService.ExecuteAsync(async ()
                => await ItemApiClient.DeleteItemAsync(id));

            Snackbar.Add($"Item {itemName} deleted successfully", Severity.Success);

            await GetItemsAsync(
                1,
                9,
                SelectedSortBy,
                SearchQuery,
                SelectedCategory?.Id);
        }
    }
}