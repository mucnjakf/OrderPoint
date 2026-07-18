using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using MudBlazor;
using OrderPoint.Admin.Categories.Api;
using OrderPoint.Admin.Categories.Dtos;
using OrderPoint.Admin.Items.Api;
using OrderPoint.Admin.Items.Api.Requests;
using OrderPoint.Admin.Items.Dialogs;
using OrderPoint.Admin.Items.Dtos;
using OrderPoint.Admin.Shared.Services;

namespace OrderPoint.Admin.Items.Pages.Update;

public sealed partial class UpdateItemPage
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
    private ItemApiClient ItemApiClient { get; set; } = null!;

    [Inject]
    private CategoryApiClient CategoryApiClient { get; set; } = null!;

    private List<BreadcrumbItem> Breadcrumbs { get; set; } = [];

    private bool IsLoading { get; set; } = true;

    private CategoryDto? SelectedCategory { get; set; }

    private bool IsFormSubmitted { get; set; }

    private UpdateItemRequest? Request { get; set; }

    private string? ItemName { get; set; }

    protected override async Task OnParametersSetAsync()
    {
        InitializeBreadcrumbs();
        await GetItemAsync();
    }

    private void InitializeBreadcrumbs()
    {
        Breadcrumbs =
        [
            new BreadcrumbItem("Dashboard", href: "/", icon: Icons.Material.Filled.Dashboard),
            new BreadcrumbItem("Items", href: "/items", icon: Icons.Material.Filled.MenuBook)
        ];

        if (ReturnUrl?.Contains("/items/") == true && ReturnUrl != "/items")
        {
            Breadcrumbs.Add(new BreadcrumbItem("Details", href: $"/items/{Id}",
                icon: Icons.Material.Filled.Notes));
        }

        Breadcrumbs.Add(new BreadcrumbItem("Edit", href: null, disabled: true, icon: Icons.Material.Filled.Edit));
    }

    private async Task GetItemAsync()
    {
        IsLoading = true;

        ItemDto item = await ApiService.ExecuteAsync(async ()
            => await ItemApiClient.GetItemAsync(Id));

        ItemName = item.Name;

        Request = new UpdateItemRequest
        {
            Name = item.Name,
            Description = item.Description,
            Portion = item.Portion,
            Price = item.Price,
            ImageUrl = item.ImageUrl,
            CategoryId = item.Category.Id
        };

        ItemCategoryDto category = item.Category;

        SelectedCategory = new CategoryDto(
            category.Id,
            category.Name,
            category.Description,
            category.Status,
            category.ImageUrl,
            0,
            category.CreatedAtUtc,
            category.UpdatedAtUtc);

        IsLoading = false;
    }

    private async Task<IEnumerable<CategoryDto>>? OnCategorySearchAsync(
        string? value,
        CancellationToken cancellationToken)
        => await CategoryApiClient.SearchCategoriesAsync(value, cancellationToken);

    private void OnInvalidSubmitAsync()
    {
        IsFormSubmitted = true;
        StateHasChanged();
    }

    private async Task ShowDeleteItemDialogAsync()
    {
        var parameters = new DialogParameters<DeleteItemDialog>
        {
            { dialog => dialog.ItemName, ItemName }
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
                => await ItemApiClient.DeleteItemAsync(Id));

            Snackbar.Add($"Item {ItemName} deleted successfully", Severity.Success);

            NavigationManager.NavigateTo("/items");
        }
    }

    private async Task OnValidSubmitAsync(EditContext editContext)
    {
        IsFormSubmitted = true;

        StateHasChanged();

        await ApiService.ExecuteAsync(async ()
            => await ItemApiClient.UpdateItemAsync(Id, Request!));

        Snackbar.Add($"Item {Request!.Name} updated successfully", Severity.Success);

        NavigationManager.NavigateTo(ReturnUrl ?? "/items");
    }
}