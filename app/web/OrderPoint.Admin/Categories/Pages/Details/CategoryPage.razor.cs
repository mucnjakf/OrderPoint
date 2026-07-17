using Microsoft.AspNetCore.Components;
using MudBlazor;
using OrderPoint.Admin.Categories.Api;
using OrderPoint.Admin.Categories.Dialogs;
using OrderPoint.Admin.Categories.Dtos;
using OrderPoint.Admin.Items.Api;
using OrderPoint.Admin.Items.Dtos;
using OrderPoint.Admin.Shared.Dtos;
using OrderPoint.Admin.Shared.Services;

namespace OrderPoint.Admin.Categories.Pages.Details;

public sealed partial class CategoryPage
{
    [Parameter]
    public Guid Id { get; set; }

    [Inject]
    private IDialogService DialogService { get; set; } = null!;

    [Inject]
    private ISnackbar Snackbar { get; set; } = null!;

    [Inject]
    private NavigationManager NavigationManager { get; set; } = null!;

    [Inject]
    private ApiService ApiService { get; set; } = null!;

    [Inject]
    private CategoryApiClient CategoryApiClient { get; set; } = null!;

    [Inject]
    private ItemApiClient ItemApiClient { get; set; } = null!;

    private List<BreadcrumbItem> Breadcrumbs { get; set; } =
    [
        new("Dashboard", href: "/", icon: Icons.Material.Filled.Dashboard),
        new("Categories", href: "/categories", icon: Icons.Material.Filled.Category),
        new("Details", href: null, disabled: true, icon: Icons.Material.Filled.Notes)
    ];

    private CategoryDto Category { get; set; } = null!;

    private IReadOnlyList<ItemDto> Items { get; set; } = [];
    private PaginationDto<ItemDto>? Pagination { get; set; }

    private string? SearchQuery { get; set; }
    private string SelectedSortBy { get; set; } = "CreatedAtUtcDesc";

    private bool IsLoading { get; set; } = true;
    private bool IsLoadingItems { get; set; } = true;

    protected override async Task OnParametersSetAsync()
    {
        await GetCategoryAsync();

        await GetItemsAsync(
            1,
            5,
            SelectedSortBy,
            null,
            Id
        );
    }

    private async Task GetCategoryAsync()
    {
        IsLoading = true;

        Category = await ApiService.ExecuteAsync(async ()
            => await CategoryApiClient.GetCategoryAsync(Id));

        IsLoading = false;
    }

    private async Task GetItemsAsync(
        int page,
        int pageSize,
        string sortBy,
        string? searchQuery,
        Guid categoryId
    )
    {
        IsLoadingItems = true;

        Pagination = await ApiService.ExecuteAsync(async ()
            => await ItemApiClient.GetItemsAsync(
                page,
                pageSize,
                sortBy,
                searchQuery,
                categoryId
            ));

        Items = Pagination.Items;

        IsLoadingItems = false;
    }

    private async Task OnSearchChangedAsync()
    {
        await GetItemsAsync(
            1,
            5,
            SelectedSortBy,
            SearchQuery,
            Id);
    }

    private async Task OnSortChangedAsync()
    {
        await GetItemsAsync(
            1,
            5,
            SelectedSortBy,
            SearchQuery,
            Id);
    }

    private async Task OnPageChanged(int pageNumber)
    {
        await GetItemsAsync(
            pageNumber,
            5,
            SelectedSortBy,
            SearchQuery,
            Id);
    }

    private async Task ShowDeleteCategoryDialogAsync()
    {
        var parameters = new DialogParameters<DeleteCategoryDialog>
        {
            { dialog => dialog.CategoryName, Category.Name }
        };

        var options = new DialogOptions
        {
            MaxWidth = MaxWidth.Small,
            FullWidth = true
        };

        IDialogReference dialogReference = await DialogService
            .ShowAsync<DeleteCategoryDialog>(string.Empty, parameters, options);

        DialogResult dialogResult = (await dialogReference.Result)!;

        if (!dialogResult.Canceled)
        {
            await ApiService.ExecuteAsync(async ()
                => await CategoryApiClient.DeleteCategoryAsync(Category.Id));

            Snackbar.Add($"Category {Category.Name} deleted successfully", Severity.Success);

            NavigationManager.NavigateTo("/categories");
        }
    }
}