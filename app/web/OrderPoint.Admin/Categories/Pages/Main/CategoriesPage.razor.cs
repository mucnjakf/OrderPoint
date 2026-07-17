using Microsoft.AspNetCore.Components;
using MudBlazor;
using OrderPoint.Admin.Categories.Api;
using OrderPoint.Admin.Categories.Dialogs;
using OrderPoint.Admin.Categories.Dtos;
using OrderPoint.Admin.Categories.Enumerations;
using OrderPoint.Admin.Shared.Dtos;
using OrderPoint.Admin.Shared.Services;

namespace OrderPoint.Admin.Categories.Pages.Main;

public sealed partial class CategoriesPage
{
    [Inject]
    private IDialogService DialogService { get; set; } = null!;

    [Inject]
    private ISnackbar Snackbar { get; set; } = null!;

    [Inject]
    private ApiService ApiService { get; set; } = null!;

    [Inject]
    private CategoryApiClient CategoryApiClient { get; set; } = null!;

    private List<BreadcrumbItem> Breadcrumbs { get; set; } =
    [
        new("Dashboard", href: "/", icon: Icons.Material.Filled.Dashboard),
        new("Categories", href: null, disabled: true, icon: Icons.Material.Filled.Category)
    ];

    private IReadOnlyList<CategoryDto> TopCategories { get; set; } = [];

    private PaginationDto<CategoryDto>? Pagination { get; set; }
    private IReadOnlyList<CategoryDto> Categories { get; set; } = [];

    private string SelectedSortBy { get; set; } = "CreatedAtUtcDesc";
    private string? SearchQuery { get; set; }
    private CategoryStatus? SelectedStatus { get; set; }

    private bool IsLoadingTop { get; set; } = true;
    private bool IsLoading { get; set; } = true;

    protected override async Task OnInitializedAsync()
    {
        await GetTopCategoriesAsync();

        await GetCategoriesAsync(
            1,
            10,
            SelectedSortBy,
            SearchQuery,
            SelectedStatus);
    }

    private async Task GetTopCategoriesAsync()
    {
        IsLoadingTop = true;

        PaginationDto<CategoryDto> pagination = await ApiService.ExecuteAsync(async ()
            => await CategoryApiClient.GetCategoriesAsync(1, 5, "ItemsCountDesc"));

        TopCategories = pagination.Items;

        IsLoadingTop = false;
    }

    private async Task GetCategoriesAsync(
        int pageNumber,
        int pageSize,
        string sortBy,
        string? searchQuery,
        CategoryStatus? status)
    {
        IsLoading = true;

        Pagination = await ApiService.ExecuteAsync(async ()
            => await CategoryApiClient.GetCategoriesAsync(
                pageNumber,
                pageSize,
                sortBy,
                searchQuery,
                status));

        Categories = Pagination.Items;

        IsLoading = false;
    }

    private async Task OnSearchChangedAsync()
    {
        await GetCategoriesAsync(
            1,
            10,
            SelectedSortBy,
            SearchQuery,
            SelectedStatus);
    }

    private async Task OnSortChangedAsync()
    {
        await GetCategoriesAsync(
            1,
            10,
            SelectedSortBy,
            SearchQuery,
            SelectedStatus);
    }

    private async Task OnStatusChangedAsync()
    {
        await GetCategoriesAsync(
            1,
            10,
            SelectedSortBy,
            SearchQuery,
            SelectedStatus);
    }

    private async Task OnPageChanged(int pageNumber)
    {
        await GetCategoriesAsync(
            pageNumber,
            10,
            SelectedSortBy,
            SearchQuery,
            SelectedStatus);
    }

    private async Task ShowDeleteCategoryDialogAsync(Guid categoryId, string categoryName)
    {
        var parameters = new DialogParameters<DeleteCategoryDialog>
        {
            { dialog => dialog.CategoryName, categoryName }
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
                => await CategoryApiClient.DeleteCategoryAsync(categoryId));

            Snackbar.Add($"Category {categoryName} deleted successfully", Severity.Success);

            await GetTopCategoriesAsync();

            await GetCategoriesAsync(
                1,
                10,
                SelectedSortBy,
                SearchQuery,
                SelectedStatus);
        }
    }
}