using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using MudBlazor;
using OrderPoint.Admin.Categories.Api;
using OrderPoint.Admin.Categories.Api.Requests;
using OrderPoint.Admin.Categories.Dialogs;
using OrderPoint.Admin.Categories.Dtos;
using OrderPoint.Admin.Shared.Services;

namespace OrderPoint.Admin.Categories.Pages.Update;

public sealed partial class UpdateCategoryPage
{
    [Parameter]
    public Guid Id { get; set; }

    [SupplyParameterFromQuery(Name = "returnUrl")]
    public string? ReturnUrl { get; set; }

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

    private List<BreadcrumbItem> Breadcrumbs { get; set; } = [];

    private bool IsLoading { get; set; } = true;

    private string? CategoryName { get; set; }

    private bool CategoryContainsItems { get; set; }

    private UpdateCategoryRequest? Request { get; set; }

    protected override async Task OnParametersSetAsync()
    {
        InitializeBreadcrumbs();
        await GetCategoryAsync();
    }

    private void InitializeBreadcrumbs()
    {
        Breadcrumbs =
        [
            new BreadcrumbItem("Dashboard", href: "/", icon: Icons.Material.Filled.Dashboard),
            new BreadcrumbItem("Categories", href: "/categories", icon: Icons.Material.Filled.Category)
        ];

        if (ReturnUrl?.Contains("/categories/") == true && ReturnUrl != "/categories")
        {
            Breadcrumbs.Add(new BreadcrumbItem("Details", href: $"/categories/{Id}",
                icon: Icons.Material.Filled.Notes));
        }

        Breadcrumbs.Add(new BreadcrumbItem("Edit", href: null, disabled: true, icon: Icons.Material.Filled.Edit));
    }

    private async Task GetCategoryAsync()
    {
        IsLoading = true;

        CategoryDto category = await ApiService.ExecuteAsync(async ()
            => await CategoryApiClient.GetCategoryAsync(Id));

        CategoryName = category.Name;
        CategoryContainsItems = category.ItemsCount > 0;

        Request = new UpdateCategoryRequest
        {
            Name = category.Name,
            Description = category.Description,
            Status = category.Status,
            ImageUrl = category.ImageUrl
        };

        IsLoading = false;
    }

    private async Task ShowDeleteCategoryDialogAsync()
    {
        var parameters = new DialogParameters<DeleteCategoryDialog>
        {
            { dialog => dialog.CategoryName, CategoryName }
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
                => await CategoryApiClient.DeleteCategoryAsync(Id));

            Snackbar.Add($"Category {CategoryName} deleted successfully", Severity.Success);

            NavigationManager.NavigateTo("/categories");
        }
    }

    private async Task OnValidSubmitAsync(EditContext editContext)
    {
        StateHasChanged();

        await ApiService.ExecuteAsync(async ()
            => await CategoryApiClient.UpdateCategoryAsync(Id, Request!));

        Snackbar.Add($"Category {Request!.Name} edited successfully", Severity.Success);

        NavigationManager.NavigateTo(ReturnUrl ?? "/categories");
    }
}