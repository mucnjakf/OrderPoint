using Microsoft.AspNetCore.Components;
using MudBlazor;
using OrderPoint.Admin.Features.Categories.Api;
using OrderPoint.Admin.Features.Categories.Dialogs;
using OrderPoint.Admin.Features.Categories.Dtos;

namespace OrderPoint.Admin.Features.Categories.Pages;

public sealed partial class DetailsCategoryPage
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
    private CategoryApiClient CategoryApiClient { get; set; } = null!;

    private List<BreadcrumbItem> Breadcrumbs { get; set; } =
    [
        new("Dashboard", href: "/", icon: Icons.Material.Filled.Dashboard),
        new("Categories", href: "/categories", icon: Icons.Material.Filled.Category),
        new("Details", href: null, disabled: true, icon: Icons.Material.Filled.Notes)
    ];

    private CategoryDto Category { get; set; } = null!;

    private bool IsLoading { get; set; } = true;

    protected override async Task OnParametersSetAsync()
    {
        await GetCategoryAsync();
    }

    private async Task GetCategoryAsync()
    {
        try
        {
            IsLoading = true;

            Category = await CategoryApiClient.GetCategoryAsync(Id);
        }
        finally
        {
            IsLoading = false;
        }
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
            await CategoryApiClient.DeleteCategoryAsync(Category.Id);

            Snackbar.Add($"Category {Category.Name} deleted successfully", Severity.Success);

            NavigationManager.NavigateTo("/categories");
        }
    }
}