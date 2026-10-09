using Microsoft.AspNetCore.Components;
using MudBlazor;
using OrderPoint.Admin.Categories.Api;
using OrderPoint.Admin.Categories.Api.Requests;
using OrderPoint.Admin.Categories.Dialogs;
using OrderPoint.Admin.Categories.Dtos;
using OrderPoint.Admin.Categories.Enumerations;
using OrderPoint.Admin.Categories.Sorting;
using OrderPoint.Admin.Shared.Dtos;
using OrderPoint.Admin.Shared.Services;

namespace OrderPoint.Admin.Categories.Pages;

public sealed partial class CategoriesPage
{
    private const int PageSize = 10;

    private const int TopCategoriesCount = 5;

    [Inject]
    private TimeZoneService TimeZoneService { get; set; } = null!;

    [Inject]
    private IDialogService DialogService { get; set; } = null!;

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

    private string SelectedSortBy { get; set; } = nameof(CategorySortBy.CreatedAtUtcDesc);

    private string? SearchQuery { get; set; }

    private CategoryStatus? SelectedStatus { get; set; }

    private bool IsLoadingTop { get; set; } = true;

    private bool IsLoading { get; set; } = true;

    protected override async Task OnInitializedAsync()
    {
        await Task.WhenAll(
            GetTopCategoriesAsync(),
            GetCategoriesAsync(pageNumber: 1));
    }

    private async Task GetTopCategoriesAsync()
    {
        IsLoadingTop = true;

        PaginationDto<CategoryDto>? pagination = await ApiService.ExecuteAsync(
            () => CategoryApiClient.GetCategoriesAsync(
                1,
                TopCategoriesCount,
                nameof(CategorySortBy.ItemsCountDesc)));

        TopCategories = pagination?.Items ?? [];

        IsLoadingTop = false;

        StateHasChanged();
    }

    private async Task GetCategoriesAsync(int pageNumber)
    {
        IsLoading = true;

        Pagination = await ApiService.ExecuteAsync(() => CategoryApiClient.GetCategoriesAsync(
            pageNumber,
            PageSize,
            SelectedSortBy,
            SearchQuery,
            SelectedStatus));

        Categories = Pagination?.Items ?? [];

        IsLoading = false;

        StateHasChanged();
    }

    private async Task OnSearchChangedAsync()
    {
        await GetCategoriesAsync(pageNumber: 1);
    }

    private async Task OnSortChangedAsync()
    {
        await GetCategoriesAsync(pageNumber: 1);
    }

    private async Task OnStatusChangedAsync()
    {
        await GetCategoriesAsync(pageNumber: 1);
    }

    private async Task OnPageChangedAsync(int pageNumber)
    {
        await GetCategoriesAsync(pageNumber);
    }

    private async Task OnCategoryItemsChangedAsync()
    {
        await Task.WhenAll(
            GetTopCategoriesAsync(),
            GetCategoriesAsync(Pagination?.PageNumber ?? 1));
    }

    private async Task ShowCreateCategoryDialogAsync()
    {
        var options = new DialogOptions
        {
            MaxWidth = MaxWidth.Medium,
            FullWidth = true
        };

        IDialogReference dialogReference = await DialogService
            .ShowAsync<CreateCategoryDialog>(string.Empty, options);

        DialogResult dialogResult = (await dialogReference.Result)!;

        if (dialogResult.Canceled)
        {
            return;
        }

        var request = (dialogResult.Data as CreateCategoryRequest)!;

        bool isSuccess = await ApiService.ExecuteAsync(
            () => CreateCategoryWithImageAsync(request),
            $"Category {request.Name} created successfully");

        if (isSuccess)
        {
            await Task.WhenAll(
                GetTopCategoriesAsync(),
                GetCategoriesAsync(pageNumber: 1));
        }
    }

    private async Task CreateCategoryWithImageAsync(CreateCategoryRequest request)
    {
        CategoryDto category = await CategoryApiClient.CreateCategoryAsync(request);

        if (request.Image is null)
        {
            return;
        }

        await CategoryApiClient.UpdateCategoryImageAsync(category.Id, request.Image);
    }

    private async Task ShowCategoryDetailsDialogAsync(CategoryDto category)
    {
        var parameters = new DialogParameters<CategoryDetailsDialog>
        {
            { dialog => dialog.Category, category },
            { dialog => dialog.OnItemsChanged, EventCallback.Factory.Create(this, OnCategoryItemsChangedAsync) }
        };

        var options = new DialogOptions
        {
            MaxWidth = MaxWidth.Medium,
            FullWidth = true
        };

        await DialogService
            .ShowAsync<CategoryDetailsDialog>(string.Empty, parameters, options);
    }

    private async Task ShowUpdateCategoryDialogAsync(CategoryDto category)
    {
        var parameters = new DialogParameters<UpdateCategoryDialog>
        {
            { dialog => dialog.Category, category }
        };

        var options = new DialogOptions
        {
            MaxWidth = MaxWidth.Medium,
            FullWidth = true
        };

        IDialogReference dialogReference = await DialogService
            .ShowAsync<UpdateCategoryDialog>(string.Empty, parameters, options);

        DialogResult dialogResult = (await dialogReference.Result)!;

        if (dialogResult.Canceled)
        {
            return;
        }

        var request = (dialogResult.Data as UpdateCategoryRequest)!;

        bool isSuccess = await ApiService.ExecuteAsync(
            () => UpdateCategoryWithImageAsync(category.Id, request),
            $"Category {request.Name} edited successfully");

        if (isSuccess)
        {
            await Task.WhenAll(
                GetTopCategoriesAsync(),
                GetCategoriesAsync(pageNumber: 1));
        }
    }

    private async Task UpdateCategoryWithImageAsync(Guid id, UpdateCategoryRequest request)
    {
        await CategoryApiClient.UpdateCategoryAsync(id, request);

        if (request.Image is not null)
        {
            await CategoryApiClient.UpdateCategoryImageAsync(id, request.Image);
            return;
        }

        if (request.RemoveImage)
        {
            await CategoryApiClient.DeleteCategoryImageAsync(id);
        }
    }

    private async Task ShowDeleteCategoryDialogAsync(Guid id, string categoryName)
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

        if (dialogResult.Canceled)
        {
            return;
        }

        bool isSuccess = await ApiService.ExecuteAsync(
            () => CategoryApiClient.DeleteCategoryAsync(id),
            $"Category {categoryName} deleted successfully");

        if (isSuccess)
        {
            await Task.WhenAll(
                GetTopCategoriesAsync(),
                GetCategoriesAsync(pageNumber: 1));
        }
    }
}