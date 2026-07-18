using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using MudBlazor;
using OrderPoint.Admin.Categories.Api;
using OrderPoint.Admin.Categories.Dtos;
using OrderPoint.Admin.Items.Api;
using OrderPoint.Admin.Items.Api.Requests;
using OrderPoint.Admin.Shared.Services;

namespace OrderPoint.Admin.Items.Pages.Create;

public sealed partial class CreateItemPage
{
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

    private List<BreadcrumbItem> Breadcrumbs { get; set; } =
    [
        new("Dashboard", href: "/", icon: Icons.Material.Filled.Dashboard),
        new("Items", href: "/items", icon: Icons.Material.Filled.MenuBook),
        new("New", href: null, disabled: true, icon: Icons.Material.Filled.Add)
    ];

    private CreateItemRequest Request { get; set; } = new();

    private CategoryDto? SelectedCategory { get; set; }

    private bool IsFormSubmitted { get; set; }

    private async Task<IEnumerable<CategoryDto>>? OnCategorySearchAsync(
        string? value,
        CancellationToken cancellationToken)
        => await CategoryApiClient.SearchCategoriesAsync(value, cancellationToken);

    private void OnInvalidSubmitAsync()
    {
        IsFormSubmitted = true;
        StateHasChanged();
    }

    private async Task OnValidSubmitAsync(EditContext editContext)
    {
        IsFormSubmitted = true;

        StateHasChanged();

        await ApiService.ExecuteAsync(async ()
            => await ItemApiClient.CreateItemAsync(Request));

        Snackbar.Add($"Item {Request.Name} created successfully", Severity.Success);

        NavigationManager.NavigateTo("/items");
    }
}